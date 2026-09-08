using System;
using System.Threading;
using DotNetty.Transport.Channels;
using NLog;
using ProudNet.Serialization;
using ProudNet.Serialization.Messages.Core;

namespace ProudNet.Handlers
{
    internal class SessionHandler : ChannelHandlerAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ProudServer _server;

        public SessionHandler(ProudServer server)
        {
            _server = server;
        }

        public override async void ChannelActive(IChannelHandlerContext context)
        {
            var hostId = _server.Configuration.HostIdFactory.New();
            var session = _server.Configuration.SessionFactory.Create(hostId, context.Channel);
            context.Channel.GetAttribute(ChannelAttributes.Session).Set(session);

            Logger.Info($"conexion entrante de {context.Channel.RemoteAddress} hostId={hostId}, mandando el hint");

            var config = new NetConfigDto
            {
                EnableServerLog = _server.Configuration.EnableServerLog,
                FallbackMethod = _server.Configuration.FallbackMethod,
                MessageMaxLength = _server.Configuration.MessageMaxLength,
                TimeoutTimeMs = _server.Configuration.IdleTimeout.TotalMilliseconds,
                DirectP2PStartCondition = _server.Configuration.DirectP2PStartCondition,
                OverSendSuspectingThresholdInBytes = _server.Configuration.OverSendSuspectingThresholdInBytes,
                EnableNagleAlgorithm = _server.Configuration.EnableNagleAlgorithm,
                EncryptedMessageKeyLength = _server.Configuration.EncryptedMessageKeyLength,
                FastEncryptedMessageKeyLength = (uint)_server.Configuration.EncryptedMessageKeyLength,
                EnableP2PEncryptedMessaging = _server.Configuration.EnableP2PEncryptedMessaging,
                UpnpDetectNatDevice = _server.Configuration.UpnpDetectNatDevice,
                UpnpTcpAddrPortMapping = _server.Configuration.UpnpTcpAddrPortMapping,
                EnablePingTest = _server.Configuration.EnablePingTest,
                EmergencyLogLineCount = _server.Configuration.EmergencyLogLineCount
            };
            byte[] publicKey;
            try
            {
                session.Rsa = new System.Security.Cryptography.RSACryptoServiceProvider(1024);
                var parameters = session.Rsa.ExportParameters(false);
                publicKey = RsaDer.SubjectPublicKeyInfo(parameters);
                Logger.Info($"[hs] clave RSA generada: KeySize={session.Rsa.KeySize} modulo={parameters.Modulus?.Length ?? 0}B der={publicKey.Length}B");
            }
            catch (Exception ex)
            {
                Logger.Error($"[hs] FALLO generando la clave RSA: {ex}");
                await session.CloseAsync();
                return;
            }

            Logger.Info($"[hs] mandando hint a {context.Channel.RemoteAddress}");
            await session.SendAsync(new NotifyServerConnectionHintMessage(config, publicKey)); //client connected, send first packet, server information and parameters for client

            Logger.Info("[hs] hint enviado, esperando la clave de sesion del cliente");

            using (var cts = new CancellationTokenSource(_server.Configuration.ConnectTimeout))
            {
                try
                {
                    Console.WriteLine("Entro en el catch y envio handshake await");
                    await session.HandhsakeEvent.WaitAsync(cts.Token);
                    Console.WriteLine("paso en el catch y envio handshake await");
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("Dio error y entro en el catch");
                    Console.WriteLine($"Exception: {ex}");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine($"Type: {ex.GetType().FullName}");
                    Console.WriteLine($"Token canceled: {cts.IsCancellationRequested}");
                    Console.WriteLine($"Handshake IsSet: {session.HandhsakeEvent.IsSet}");

                    if (!session.IsConnected)
                        return;

                    Logger.Error(
                        $"{context.Channel.RemoteAddress} nunca contesto al hint, se acabo el tiempo del handshake"
                    );

                    await session.SendAsync(new ConnectServerTimedoutMessage());
                    await session.CloseAsync();
                    return;
                }
            }
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var session = context.Channel.GetAttribute(ChannelAttributes.Session).Get();
            Logger.Info($"se desconecto {context.Channel.RemoteAddress} hostId={session?.HostId}");
            session.Dispose();
            _server.RemoveSession(session);
            _server.Configuration.HostIdFactory.Free(session.HostId);
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            var session = context.Channel.GetAttribute(ChannelAttributes.Session).Get();
            Logger.Error(exception, $"excepcion en {context.Channel.RemoteAddress}, cerrando la conexion");
            _server.RaiseError(new ErrorEventArgs(session, exception));
            session.CloseAsync();
        }
    }
}
