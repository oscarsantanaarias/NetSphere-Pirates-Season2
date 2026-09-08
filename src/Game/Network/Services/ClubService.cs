using System.Threading.Tasks;
using BlubLib.DotNetty.Handlers.MessageHandling;
using ExpressMapper.Extensions;
using Netsphere.Network.Data.Game;
using Netsphere.Network.Message.Game;
using NLog;
using NLog.Fluent;
using ProudNet.Handlers;

namespace Netsphere.Network.Services
{
    internal class ClubService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Season 2 has no create/disband/kick/invite messages, so a clan is
        // built and administered from the console; the client only asks about
        // it, joins it and leaves it.
        [MessageHandler(typeof(CClubInfoReqMessage))]
        public async Task CClubInfoReq(GameSession session)
        {
            await session.SendAsync(new SClubInfoAckMessage { ClubInfo = session.Player.Map<Player, PlayerClubInfoDto>() })
                .ConfigureAwait(false);
        }

        [MessageHandler(typeof(CGetClubInfoReqMessage))]
        public async Task CGetClubInfoReq(GameSession session, CGetClubInfoReqMessage message)
        {
            int id;
            var club = int.TryParse(message.Unk, out id)
                ? GameServer.Instance.ClubManager.GetClub(id)
                : GameServer.Instance.ClubManager.GetClubByName(message.Unk);

            await SendClubInfo(session, club).ConfigureAwait(false);
        }

        [MessageHandler(typeof(CGetClubInfoByNameReqMessage))]
        public async Task CGetClubInfoByNameReq(GameSession session, CGetClubInfoByNameReqMessage message)
        {
            await SendClubInfo(session, GameServer.Instance.ClubManager.GetClubByName(message.Unk))
                .ConfigureAwait(false);
        }

        private static async Task SendClubInfo(GameSession session, Club club)
        {
            if (club == null)
            {
                await session.SendAsync(new SServerResultInfoAckMessage(ServerResult.CantReadClanInfo))
                    .ConfigureAwait(false);
                return;
            }

            await session.SendAsync(new SGetClubInfoAckMessage { ClubInfo = new ClubInfoDto
            {
                Unk1 = club.Name,
                Unk2 = club.Icon,
                Unk3 = club.GetMasterName(),
                Unk4 = club.Notice,
                Unk5 = club.CreatedDate,
                Unk6 = (ushort)club.Count,
                Unk7 = (uint)club.Id,
                Unk8 = (uint)club.Level
            } }).ConfigureAwait(false);
        }

        [MessageHandler(typeof(CClubJoinReqMessage))]
        public async Task CClubJoinReq(GameSession session, CClubJoinReqMessage message)
        {
            var plr = session.Player;

            if (plr.Club != null)
            {
                await session.SendAsync(new SClubJoinAckMessage { Unk = 1, Message = "You are already in a clan" })
                    .ConfigureAwait(false);
                return;
            }

            var club = GameServer.Instance.ClubManager.GetClubByName(message.Unk2);
            if (club == null)
            {
                Logger.Error()
                    .Account(session)
                    .Message($"Clan {message.Unk2} does not exist")
                    .Write();
                await session.SendAsync(new SClubJoinAckMessage { Unk = 1, Message = "That clan does not exist" })
                    .ConfigureAwait(false);
                return;
            }

            GameServer.Instance.ClubManager.AddPlayer(club, (int)plr.Account.Id, ClubRank.Member);

            await session.SendAsync(new SClubJoinAckMessage { Unk = 0, Message = club.Name }).ConfigureAwait(false);
            await session.SendAsync(new SClubInfoAckMessage { ClubInfo = plr.Map<Player, PlayerClubInfoDto>() })
                .ConfigureAwait(false);
        }

        [MessageHandler(typeof(CClubUnJoinReqMessage))]
        public async Task CClubUnJoinReq(GameSession session)
        {
            var plr = session.Player;
            var club = plr.Club;

            if (club == null)
            {
                await session.SendAsync(new SClubUnJoinAckMessage { Unk = 1 }).ConfigureAwait(false);
                return;
            }

            GameServer.Instance.ClubManager.RemovePlayer(club, (int)plr.Account.Id);

            await session.SendAsync(new SClubUnJoinAckMessage { Unk = 0 }).ConfigureAwait(false);
            await session.SendAsync(new SClubInfoAckMessage { ClubInfo = plr.Map<Player, PlayerClubInfoDto>() })
                .ConfigureAwait(false);
        }

        [MessageHandler(typeof(CClubNoticeChangeReqMessage))]
        public async Task CClubNoticeChangeReq(GameSession session, CClubNoticeChangeReqMessage message)
        {
            var plr = session.Player;
            var club = plr.Club;

            if (club == null)
                return;

            var member = club[(int)plr.Account.Id];
            if (member == null || member.Rank > ClubRank.Staff)
                return;

            club.Notice = message.Unk ?? "";
            GameServer.Instance.ClubManager.Save(club);

            foreach (var other in club.GetOnlinePlayers())
                await other.ChatSession.SendAsync(new Message.Chat.SClubNoticeChangeAckMessage(club.Notice))
                    .ConfigureAwait(false);
        }

        [MessageHandler(typeof(CClubHistoryReqMessage))]
        public async Task CClubHistoryReq(GameSession session)
        {
            var club = session.Player.Club;

            await session.SendAsync(new SClubHistoryAckMessage { History = new ClubHistoryDto
            {
                Unk1 = (uint)(club?.Id ?? 0),
                Unk2 = (uint)(club?.Level ?? 0),
                Unk3 = club?.Name ?? "",
                Unk4 = club?.CreatedDate ?? "",
                Unk5 = "",
                Unk6 = "",
                Unk7 = "",
                Unk8 = ""
            } }).ConfigureAwait(false);
        }

        [MessageHandler(typeof(CClubAddressReqMessage))]
        public async Task CClubAddressReq(GameSession session, CClubAddressReqMessage message)
        {
            Logger.Debug()
                .Account(session)
                .Message($"RequestId:{message.RequestId} LanguageId:{message.LanguageId} Command:{message.Command}")
                .Write();

            await session.SendAsync(new SClubAddressAckMessage("", 0))
                .ConfigureAwait(false);
        }
    }
}
