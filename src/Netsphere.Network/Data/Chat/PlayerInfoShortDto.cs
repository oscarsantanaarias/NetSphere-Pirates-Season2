using BlubLib.Serialization;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Data.Chat
{
    // What Season 2 actually reads for a player in the channel list, the
    // channel join broadcast and the clan member list.
    [BlubContract]
    public class PlayerInfoShortDto
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public int Unk { get; set; }

        [BlubMember(3)]
        public int TotalExp { get; set; }

        [BlubMember(4)]
        public bool IsGM { get; set; }

        public PlayerInfoShortDto()
        {
            Nickname = "";
        }
    }

    [BlubContract]
    public class PlayerLocationDto
    {
        [BlubMember(0)]
        public int ServerGroupId { get; set; }

        [BlubMember(1)]
        public int GameServerId { get; set; }

        [BlubMember(2)]
        public int ChannelId { get; set; }

        [BlubMember(3)]
        public int RoomId { get; set; }

        [BlubMember(4)]
        public int Unk { get; set; }

        [BlubMember(5)]
        public int ChatServerId { get; set; }
    }

    [BlubContract]
    public class PlayerInfoDto
    {
        [BlubMember(0)]
        public PlayerInfoShortDto Info { get; set; }

        [BlubMember(1)]
        public PlayerLocationDto Location { get; set; }

        public PlayerInfoDto()
        {
            Info = new PlayerInfoShortDto();
            Location = new PlayerLocationDto();
        }
    }
}
