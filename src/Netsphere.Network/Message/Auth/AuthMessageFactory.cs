using ProudNet.Serialization;

namespace Netsphere.Network.Message.Auth
{
    public interface IAuthMessage
    { }

    public class AuthMessageFactory : MessageFactory<AuthOpCode, IAuthMessage>
    {
        public AuthMessageFactory()
        {
            // S2C
            Register<SAuthInEuAckMessage>(AuthOpCode.SAuthInEuAck);
            Register<SServerListAckMessage>(AuthOpCode.SServerListAck);
            Register<SOptionBinaryAckMessage>(AuthOpCode.SOptionBinaryAck);

            // C2S
            Register<CAuthInEUReqMessage>(AuthOpCode.CAuthInEuReq);
            Register<CServerListReqMessage>(AuthOpCode.CServerListReq);
            Register<CCheckOptionVersionReqMessage>(AuthOpCode.CCheckOptionVersionReq);
        }
    }
}