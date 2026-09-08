using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Netsphere.Database.Game;
using Netsphere.Network;

namespace Netsphere
{
    internal class ClubPlayerInfo
    {
        public int PlayerId { get; set; }
        public ClubRank Rank { get; set; }
        public string JoinedDate { get; set; }
    }

    internal class Club : IEnumerable<ClubPlayerInfo>
    {
        private readonly ConcurrentDictionary<int, ClubPlayerInfo> _players =
            new ConcurrentDictionary<int, ClubPlayerInfo>();

        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int MasterId { get; set; }
        public string Notice { get; set; }
        public string CreatedDate { get; set; }
        public int Level { get; set; }

        public IReadOnlyDictionary<int, ClubPlayerInfo> Players => _players;

        public int Count => _players.Count;

        public ClubPlayerInfo this[int playerId]
        {
            get
            {
                ClubPlayerInfo info;
                return _players.TryGetValue(playerId, out info) ? info : null;
            }
        }

        public Club()
        {
            Name = "";
            Icon = "1-1-1";
            Notice = "";
            CreatedDate = "";
            Level = 1;
        }

        public Club(ClubDto dto)
            : this()
        {
            Id = dto.Id;
            Name = dto.Name ?? "";
            Icon = string.IsNullOrEmpty(dto.Icon) ? "1-1-1" : dto.Icon;
            MasterId = dto.MasterId;
            Notice = dto.Notice ?? "";
            CreatedDate = dto.CreatedDate ?? "";
            Level = dto.Level;
        }

        public void AddPlayer(ClubPlayerInfo info)
        {
            _players[info.PlayerId] = info;
        }

        public bool RemovePlayer(int playerId)
        {
            ClubPlayerInfo removed;
            return _players.TryRemove(playerId, out removed);
        }

        public string GetMasterName()
        {
            var master = GameServer.Instance.PlayerManager.FirstOrDefault(plr => plr.Account.Id == (ulong)MasterId);
            return master?.Account.Nickname ?? "";
        }

        public IEnumerable<Player> GetOnlinePlayers()
        {
            return GameServer.Instance.PlayerManager.Where(plr => _players.ContainsKey((int)plr.Account.Id));
        }

        public void Broadcast(object message)
        {
            foreach (var plr in GetOnlinePlayers())
                plr.Session?.SendAsync(message);
        }

        public IEnumerator<ClubPlayerInfo> GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
