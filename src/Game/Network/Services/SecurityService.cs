using System.Threading.Tasks;
using BlubLib.DotNetty.Handlers.MessageHandling;
using Netsphere.Network.Message.Game;
using NLog;
using NLog.Fluent;
using ProudNet.Handlers;

namespace Netsphere.Network.Services
{
    // Requests the client sends that had an opcode but no class, so they were
    // dropped before reaching a handler. The anti cheat ones are answered with
    // an echo because the client waits for the reply; the shop basket and the
    // random shop are only logged until those features exist.
    internal class SecurityService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [MessageHandler(typeof(CGameGuardAuthReqMessage))]
        public async Task CGameGuardAuthReq(GameSession session, CGameGuardAuthReqMessage message)
        {
            await session.SendAsync(new SGameGuardAuthAckMessage
            {
                Unk1 = message.Unk1,
                Unk2 = message.Unk2,
                Unk3 = message.Unk3,
                Unk4 = message.Unk4
            }).ConfigureAwait(false);
        }

        [MessageHandler(typeof(CHShieldMakeResponseReqMessage))]
        public async Task CHShieldMakeResponseReq(GameSession session, CHShieldMakeResponseReqMessage message)
        {
            await session.SendAsync(new SHShieldMakeResponseAckMessage
            {
                Unk1 = message.Unk1,
                Unk2 = ""
            }).ConfigureAwait(false);
        }

        [MessageHandler(typeof(CShoppingBasketActionReqMessage))]
        public void CShoppingBasketActionReq(GameSession session)
        {
            Logger.Debug()
                .Account(session)
                .Message("Shopping basket is not implemented")
                .Write();
        }

        [MessageHandler(typeof(CShoppingBasketDeleteReqMessage))]
        public void CShoppingBasketDeleteReq(GameSession session)
        {
            Logger.Debug()
                .Account(session)
                .Message("Shopping basket is not implemented")
                .Write();
        }

        [MessageHandler(typeof(CRandomShopGetNiceItemReqMessage))]
        public void CRandomShopGetNiceItemReq(GameSession session)
        {
            Logger.Debug()
                .Account(session)
                .Message("Random shop nice item is not implemented")
                .Write();
        }
    }
}
