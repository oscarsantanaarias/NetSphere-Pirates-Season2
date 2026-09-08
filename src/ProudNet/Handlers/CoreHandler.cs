using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using BlubLib.DotNetty;
using BlubLib.DotNetty.Handlers.MessageHandling;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ProudNet.Codecs;
using ProudNet.Serialization;
using ProudNet.Serialization.Messages;
using ProudNet.Serialization.Messages.Core;

namespace ProudNet.Handlers
{
    internal class CoreHandler : ProudMessageHandler //[p2p] peer to peer connections are primarily handled in this file
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ProudServer _server;
        private readonly Lazy<DateTime> _startTime = new Lazy<DateTime>(() => Process.GetCurrentProcess().StartTime);

        public CoreHandler(ProudServer server)
        {
            _server = server;
        }

        [MessageHandler(typeof(RmiMessage))] // normal packet message
        public void RmiMessage(IChannelHandlerContext context, RmiMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Data);
            context.FireChannelRead(buffer);
        }

        [MessageHandler(typeof(UserMessageMessage))] // Season 2 wrapper: unwrap and read what is inside
        public void UserMessageMessage(IChannelHandlerContext context, UserMessageMessage message)
        {
            Logger.Info($"UserMessage unk={message.Unk}, {message.Data?.Length ?? 0} bytes dentro");
            var buffer = Unpooled.WrappedBuffer(message.Data);
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(buffer);
        }

        [MessageHandler(typeof(CompressedMessage))] //message inside message/big message, compressed, decond and re-read
        public void CompressedMessage(IChannelHandlerContext context, CompressedMessage message)
        {
            var decompressed = message.Data.DecompressZLib();
            var buffer = Unpooled.WrappedBuffer(decompressed);
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(buffer);
        }

        [MessageHandler(typeof(EncryptedReliableMessage))]
        public void EncryptedReliableMessage(IChannelHandlerContext context, ProudSession session, EncryptedReliableMessage message)//reads proudnet message and decodes them to the server message (message inside message)
        {
            Crypt crypt;
            crypt = session.Crypt;
            var buffer = context.Allocator.Buffer(message.Data.Length);
            using (var src = new MemoryStream(message.Data))
            using (var dst = new WriteOnlyByteBufferStream(buffer, false))
                crypt.Decrypt(src, dst, true);
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(buffer);
        }

        [MessageHandler(typeof(NotifyCSEncryptedSessionKeyMessage))]
        public void NotifyCSEncryptedSessionKeyMessage(IChannelHandlerContext context, ProudSession session, NotifyCSEncryptedSessionKeyMessage message) //send client session encryption key
        {
            // Season 1 sends its own public key here and expects the server to
            // pick the session key. Season 2 already has the server's public key
            // from the hint, so this carries the session key encrypted with it.
            Logger.Info($"[hs] llego la clave de sesion: Key={message.Key?.Length ?? 0}B FastKey={message.FastKey?.Length ?? 0}B");

            if (session.Rsa == null)
                throw new ProudException("No RSA key for this session");

            var rsaSize = session.Rsa.KeySize / 8;
            // A CryptoAPI SIMPLEBLOB carries a 12 byte header before the
            // ciphertext, and CryptoPP writes the ciphertext the other way
            // round. Season 1 built exactly that shape on the way out.
            var cipher = message.Key.Length > rsaSize
                ? message.Key.Skip(message.Key.Length - rsaSize).ToArray()
                : message.Key;

            byte[] sessionKey = null;
            foreach (var candidate in new[] { Enumerable.Reverse(cipher).ToArray(), cipher })
            {
                foreach (var oaep in new[] { false, true })
                {
                    try
                    {
                        sessionKey = session.Rsa.Decrypt(candidate, oaep);
                        break;
                    }
                    catch (CryptographicException)
                    {
                    }
                }
                if (sessionKey != null)
                    break;
            }

            if (sessionKey == null)
                throw new ProudException($"No pude descifrar la clave de sesion ({message.Key.Length} bytes)");

            Logger.Info($"[hs] clave de sesion descifrada, {sessionKey.Length} bytes");

            var fastKey = sessionKey;
            if (message.FastKey != null && message.FastKey.Length > 0)
            {

                // Wrapped with the AES key we just recovered. Its first block is
                // the RC4 key; the rest is padding.
                using (var aes = new AesCryptoServiceProvider
                {
                    Key = sessionKey,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None
                })
                using (var dec = aes.CreateDecryptor())
                {
                    var plain = dec.TransformFinalBlock(message.FastKey, 0, message.FastKey.Length);
                    fastKey = plain.Take(sessionKey.Length).ToArray();
                }
            }

            session.Crypt = new Crypt(sessionKey, fastKey);
            Logger.Info("[hs] cifrado listo, mandando NotifyCSSessionKeySuccess");
            session.SendAsync(new NotifyCSSessionKeySuccessMessage(Array.Empty<byte>()));
        }

        [MessageHandler(typeof(NotifyServerConnectionRequestDataMessage))]//client connects to server, proudapi checks version n' shit
        public void NotifyServerConnectionRequestDataMessage(IChannelHandlerContext context, ProudSession session, NotifyServerConnectionRequestDataMessage message)
        {
            if (message.InternalNetVersion != Constants.NetVersion || message.Version != _server.Configuration.Version)
            {
                Logger.Error(
                    $"handshake RECHAZADO desde {session.RemoteEndPoint}: " +
                    $"NetVersion cliente={message.InternalNetVersion} servidor={Constants.NetVersion}, " +
                    $"Guid cliente={message.Version} servidor={_server.Configuration.Version}");
                session.SendAsync(new NotifyProtocolVersionMismatchMessage());
                session.CloseAsync();
                return;
            }
            Logger.Info($"handshake OK con {session.RemoteEndPoint}, hostId={session.HostId}");
            _server.AddSession(session);
            session.HandhsakeEvent.Set();
            session.SendAsync(new NotifyServerConnectSuccessMessage(session.HostId, _server.Configuration.Version, session.RemoteEndPoint)); // send success if everything is okay
        }

        [MessageHandler(typeof(UnreliablePingMessage))] //unreliable(udp) ping --> udp never tells u if peer got the message or not, --> "workaround" with client/server time, not accurate
        public void UnreliablePingHandler(IChannelHandlerContext context, ProudSession session, UnreliablePingMessage message)
        {
            session.UnreliablePing = TimeSpan.FromSeconds(message.Ping).TotalMilliseconds;
            var ts = DateTime.Now - _startTime.Value;
            session.SendUdpIfAvailableAsync(new UnreliablePongMessage(message.ClientTime, ts.TotalSeconds));
        }

        [MessageHandler(typeof(SpeedHackDetectorPingMessage))] 
        public void SpeedHackDetectorPingHandler(ProudSession session)
        {
            session.LastSpeedHackDetectorPing = DateTime.Now;
        }

        [MessageHandler(typeof(ReliableRelay1Message))] //Handles RT/Relay'd TCP messages --> client2server2client
        public void ReliableRelayHandler(IChannel channel, ProudSession session, ReliableRelay1Message message)
        {
            if (session.P2PGroup == null){return;}
            foreach (var destination in message.Destination)//this is bugging --> destinations are fucked for ex damage and other weird stuff like hp display
            {
                if (session.P2PGroup == null){return;}
                if (!session.P2PGroup.Members.ContainsKey(destination.HostId)){return;}       
                var target = _server.Sessions.GetValueOrDefault(destination.HostId);
                target?.SendAsync(new ReliableRelay2Message(new RelayDestinationDto(session.HostId, destination.FrameNumber), message.Data));
            }
        }

        [MessageHandler(typeof(UnreliableRelay1Message))] //Handles RT/Relay'd UDP messages --> client2server2client
        public void UnreliableRelayHandler(IChannel channel, ProudSession session, UnreliableRelay1Message message)
        {
            foreach (var destination in message.Destination)//this is bugging --> destinations are fucked for ex chat/damage/hp display
            {
                if (session.P2PGroup == null){return;}          
                if (!session.P2PGroup.Members.ContainsKey(destination)){return;}
                var target = _server.Sessions.GetValueOrDefault(destination);
                target?.SendUdpIfAvailableAsync(new UnreliableRelay2Message(session.HostId, message.Data));
            }
        }

        [MessageHandler(typeof(ServerHolepunchMessage))] //client trys to connect to server
        public void NotifyHolepunchSuccess(ProudServer server, ProudSession session, ServerHolepunchMessage message)
        {
            if (session.P2PGroup == null || !_server.UdpSocketManager.IsRunning || session.HolepunchMagicNumber != message.MagicNumber){return; }
            //Console.WriteLine($"{session.RemoteEndPoint} ServerHolepunchMessage!! {session.UdpEndPoint}");
            session.SendUdpAsync(new ServerHolepunchAckMessage(session.HolepunchMagicNumber, session.UdpEndPoint));// udp connection succeeded, sending return info
        }

        [MessageHandler(typeof(NotifyHolepunchSuccessMessage))] //client says return info is right
        public void NotifyHolepunchSuccess(ProudServer server, ProudSession session, NotifyHolepunchSuccessMessage message)
        {
            if (session.P2PGroup == null || !_server.UdpSocketManager.IsRunning || session.HolepunchMagicNumber != message.MagicNumber){return;}

            //Console.WriteLine($"{session.RemoteEndPoint} NotifyHolepunchSuccessMessage!! {message.LocalEndPoint} {message.EndPoint}");

            session.UdpEnabled = true;
            session.UdpLocalEndPoint = message.LocalEndPoint;
            session.SendUdpAsync(new NotifyClientServerUdpMatchedMessage(message.MagicNumber)); //matches, right client, right connection
        }

        [MessageHandler(typeof(PeerUdp_ServerHolepunchMessage))] //prepare p2p #3
        public void PeerUdp_ServerHolepunch(IChannel channel, ProudSession session, PeerUdp_ServerHolepunchMessage message)
        {
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
                return;
            var target = session.P2PGroup?.Members.GetValueOrDefault(message.HostId)?.Session;
            if (target == null || !target.UdpEnabled)
                return;
            session.SendUdpAsync(new PeerUdp_ServerHolepunchAckMessage(message.MagicNumber, session.UdpEndPoint, target.HostId));
        }

        [MessageHandler(typeof(PeerUdp_NotifyHolepunchSuccessMessage))] //request holepunch --> results in ServerHandler.cs
        public void PeerUdp_NotifyHolepunchSuccess(IChannel channel, ProudSession session, PeerUdp_NotifyHolepunchSuccessMessage message)
        {
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
                return;

            var peer = session.P2PGroup?.Members.GetValueOrDefault(session.HostId);
            var connectionState = peer?.ConnectionStates?.GetValueOrDefault(message.HostId);
            if (connectionState == null)
                return;

            connectionState.PeerUdpHolepunchSuccess = true;
            connectionState.LocalEndPoint = message.LocalEndPoint;
            connectionState.EndPoint = message.EndPoint;

            var otherState = connectionState.RemotePeer?.ConnectionStates.GetValueOrDefault(session.HostId);
            if (otherState == null || !otherState.PeerUdpHolepunchSuccess)
                return;

            peer.SendAsync(new RequestP2PHolepunchMessage(message.HostId, otherState.LocalEndPoint, otherState.EndPoint));
            connectionState.RemotePeer.SendAsync(new RequestP2PHolepunchMessage(session.HostId, connectionState.LocalEndPoint, connectionState.EndPoint));
        }
    }
}
