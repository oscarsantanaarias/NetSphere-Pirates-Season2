using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Extensions;
using Netsphere.Network;
using Netsphere.Network.Data.Chat;
using Netsphere.Network.Message.Chat;
using Netsphere.Network.Message.Game;

namespace Netsphere
{
    internal class Channel
    {
        private readonly IDictionary<ulong, Player> _players = new ConcurrentDictionary<ulong, Player>();
        public uint Id { get; set; }
        public ChannelCategory Category { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Description { get; set; }
        public int PlayerLimit { get; set; }
        public byte Type { get; set; }
        public uint Color { get; set; }
        public bool IsClanChannel { get; set; }

        #region Events

        public event EventHandler<ChannelPlayerJoinedEventArgs> PlayerJoined;
        public event EventHandler<ChannelPlayerLeftEventArgs> PlayerLeft;
        public event EventHandler<ChannelMessageEventArgs> Message;

        protected virtual void OnPlayerJoined(ChannelPlayerJoinedEventArgs e)
        {
            PlayerJoined?.Invoke(this, e);
        }

        protected virtual void OnPlayerLeft(ChannelPlayerLeftEventArgs e)
        {
            PlayerLeft?.Invoke(this, e);
        }

        protected virtual void OnMessage(ChannelMessageEventArgs e)
        {
            Message?.Invoke(this, e);
        }

        #endregion

        public IReadOnlyDictionary<ulong, Player> Players => (IReadOnlyDictionary<ulong, Player>)_players;
        public RoomManager RoomManager { get; }

        public Channel()
        {
            RoomManager = new RoomManager(this);
        }

        public void Update(TimeSpan delta)
        {
            RoomManager.Update(delta);
        }

        public void Join(Player plr)
        {

            Console.WriteLine("Le diste click a join");
            if (plr.Channel != null)
                throw new ChannelException("Player is already inside a channel");

            if (Players.Count >= PlayerLimit)
                throw new ChannelLimitReachedException();

            _players.Add(plr.Account.Id, plr);
            plr.Channel = this;

            plr.Session.SendAsync(new SServerResultInfoAckMessage(ServerResult.ChannelEnter));

            foreach (var other in Players.Values.Where(p => p.Account.Id != plr.Account.Id))
                other.ChatSession?.SendAsync(new SChannelEnterPlayerAckMessage(plr.Map<Player, PlayerInfoShortDto>()));

            plr.ChatSession?.SendAsync(new SChannelPlayerListAckMessage(Players.Values
                .Where(p => p.Room == null)
                .Select(p => p.Map<Player, PlayerInfoShortDto>()).ToArray()));

            OnPlayerJoined(new ChannelPlayerJoinedEventArgs(this, plr));

            var club = plr.Club;
            if (club != null)
            {
                plr.ChatSession.SendAsync(new Network.Message.Chat.SClanMemberListAckMessage(
                    club.GetOnlinePlayers().Select(member => member.Map<Player, Network.Data.Chat.PlayerInfoDto>()).ToArray()));
            }

            plr.Mailbox.UpdateReminderAsync();
        }

        public void Leave(Player plr)
        {
            if (plr.Channel != this)
                throw new ChannelException("Player is not in this channel");

            _players.Remove(plr.Account.Id);
            plr.Channel = null;

            Broadcast(new SChannelLeavePlayerAckMessage(plr.Account.Id));

            OnPlayerLeft(new ChannelPlayerLeftEventArgs(this, plr));
            plr.Session?.SendAsync(new SServerResultInfoAckMessage(ServerResult.ChannelLeave));
        }

        public void SendChatMessage(Player plr, string message)
        {
            OnMessage(new ChannelMessageEventArgs(this, plr, message));

            foreach (var p in Players.Values.Where(p => !p.DenyManager.Contains(plr.Account.Id) && p.Room == null))
                p.ChatSession.SendAsync(new SChatMessageAckMessage(ChatType.Channel, plr.Account.Id, plr.Account.Nickname, message));
        }

        public void Broadcast(IGameMessage message, bool excludeRooms = false)
        {
            foreach (var plr in Players.Values.Where(plr => !excludeRooms || plr.Room == null))
                plr.Session.SendAsync(message);
        }

        public void Broadcast(IChatMessage message, bool excludeRooms = false)
        {
            foreach (var plr in Players.Values.Where(plr => !excludeRooms || plr.Room == null))
                plr.ChatSession.SendAsync(message);
        }
    }
}
