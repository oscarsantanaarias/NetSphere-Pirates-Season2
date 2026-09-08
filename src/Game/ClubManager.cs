using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Netsphere.Database.Game;
using System.Data;
using Dapper.FastCrud;
using Netsphere.Network;

namespace Netsphere
{
    internal class ClubManager : IEnumerable<Club>
    {
        private readonly ConcurrentDictionary<int, Club> _clubs = new ConcurrentDictionary<int, Club>();

        public Club this[int id] => GetClub(id);

        public int Count => _clubs.Count;

        public ClubManager()
        {
            using (var db = GameDatabase.Open())
            {
                foreach (var dto in db.Find<ClubDto>())
                    _clubs[dto.Id] = new Club(dto);

                foreach (var member in db.Find<ClubPlayerDto>())
                {
                    Club club;
                    if (!_clubs.TryGetValue(member.ClubId, out club))
                        continue;

                    club.AddPlayer(new ClubPlayerInfo
                    {
                        PlayerId = member.PlayerId,
                        Rank = (ClubRank)member.MemberRank,
                        JoinedDate = member.JoinedDate
                    });
                }
            }
        }

        public Club GetClub(int id)
        {
            Club club;
            return _clubs.TryGetValue(id, out club) ? club : null;
        }

        public Club GetClubByName(string name)
        {
            return _clubs.Values.FirstOrDefault(club => club.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public Club GetClubByPlayer(int playerId)
        {
            return _clubs.Values.FirstOrDefault(club => club.Players.ContainsKey(playerId));
        }

        public Club Create(string name, string icon, int masterId)
        {
            var club = new Club
            {
                Id = ClubIdGenerator.GetNextId(),
                Name = name,
                Icon = string.IsNullOrEmpty(icon) ? "1-1-1" : icon,
                MasterId = masterId,
                Notice = "",
                CreatedDate = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
                Level = 1
            };

            using (var db = GameDatabase.Open())
            {
                db.Insert(new ClubDto
                {
                    Id = club.Id,
                    Name = club.Name,
                    Icon = club.Icon,
                    MasterId = club.MasterId,
                    Notice = club.Notice,
                    CreatedDate = club.CreatedDate,
                    Level = club.Level
                });
            }

            _clubs[club.Id] = club;
            AddPlayer(club, masterId, ClubRank.Master);
            return club;
        }

        public void Remove(Club club)
        {
            using (var db = GameDatabase.Open())
            {
                db.BulkDelete<ClubPlayerDto>(statement => statement.Where($"{nameof(ClubPlayerDto.ClubId):C} = {club.Id}"));
                db.Delete(new ClubDto { Id = club.Id });
            }

            Club removed;
            _clubs.TryRemove(club.Id, out removed);
        }

        public void AddPlayer(Club club, int playerId, ClubRank rank)
        {
            var info = new ClubPlayerInfo
            {
                PlayerId = playerId,
                Rank = rank,
                JoinedDate = DateTimeOffset.Now.ToString("yyyy-MM-dd")
            };

            using (var db = GameDatabase.Open())
            {
                db.Insert(new ClubPlayerDto
                {
                    Id = ClubPlayerIdGenerator.GetNextId(),
                    ClubId = club.Id,
                    PlayerId = playerId,
                    MemberRank = (byte)rank,
                    JoinedDate = info.JoinedDate
                });
            }

            club.AddPlayer(info);
        }

        public void RemovePlayer(Club club, int playerId)
        {
            using (var db = GameDatabase.Open())
                db.BulkDelete<ClubPlayerDto>(statement => statement.Where($"{nameof(ClubPlayerDto.PlayerId):C} = {playerId}"));

            club.RemovePlayer(playerId);
        }

        public void Save(Club club)
        {
            using (var db = GameDatabase.Open())
            {
                db.Update(new ClubDto
                {
                    Id = club.Id,
                    Name = club.Name,
                    Icon = club.Icon,
                    MasterId = club.MasterId,
                    Notice = club.Notice,
                    CreatedDate = club.CreatedDate,
                    Level = club.Level
                });
            }
        }

        public IEnumerator<Club> GetEnumerator()
        {
            return _clubs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
