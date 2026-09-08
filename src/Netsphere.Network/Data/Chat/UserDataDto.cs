using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Data.Chat
{
    [BlubContract]
    public class UserDataDto
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2)]
        public uint TotalExp { get; set; }

        [BlubMember(3)]
        public int UnkInt1 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string UnkStr1 { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string UnkStr2 { get; set; }

        [BlubMember(6)]
        public uint Level { get; set; }

        [BlubMember(7)]
        public uint PlayTime { get; set; }

        [BlubMember(8)]
        public uint TotalGames { get; set; }

        [BlubMember(9)]
        public uint GamesWon { get; set; }

        [BlubMember(10)]
        public uint GamesLost { get; set; }

        [BlubMember(11)]
        public uint Unk10 { get; set; }

        [BlubMember(12)]
        public uint Unk11 { get; set; }

        [BlubMember(13)]
        public uint Unk12 { get; set; }

        [BlubMember(14)]
        public uint Unk13 { get; set; }

        [BlubMember(15)]
        public uint TDScore { get; set; }

        [BlubMember(16)]
        public uint DMScore { get; set; }

        [BlubMember(17)]
        public uint ChaserSurvivability { get; set; }

        [BlubMember(18)]
        public uint BRScore { get; set; }

        [BlubMember(19)]
        public uint CaptainScore { get; set; }

        [BlubMember(20)]
        public uint SiegeScore { get; set; }

        [BlubMember(21)]
        public uint Stat1 { get; set; }

        [BlubMember(22)]
        public uint Stat2 { get; set; }

        [BlubMember(23)]
        public uint Stat3 { get; set; }

        [BlubMember(24)]
        public uint Stat4 { get; set; }

        [BlubMember(25)]
        public uint Stat5 { get; set; }

        [BlubMember(26)]
        public uint Stat6 { get; set; }

        [BlubMember(27)]
        public uint Stat7 { get; set; }

        [BlubMember(28)]
        public uint Stat8 { get; set; }

        [BlubMember(29)]
        public uint Stat9 { get; set; }

        [BlubMember(30)]
        public uint Stat10 { get; set; }

        [BlubMember(31)]
        public uint Stat11 { get; set; }

        [BlubMember(32)]
        public uint Stat12 { get; set; }

        [BlubMember(33)]
        public uint Stat13 { get; set; }

        [BlubMember(34)]
        public uint Stat14 { get; set; }

        [BlubMember(35)]
        public uint Stat15 { get; set; }

        [BlubMember(36)]
        public uint Stat16 { get; set; }

        [BlubMember(37)]
        public uint Stat17 { get; set; }

        [BlubMember(38)]
        public uint Stat18 { get; set; }

        [BlubMember(39)]
        public uint Stat19 { get; set; }

        [BlubMember(40)]
        public uint Stat20 { get; set; }

        [BlubMember(41)]
        public uint Stat21 { get; set; }

        [BlubMember(42)]
        public uint Stat22 { get; set; }

        [BlubMember(43)]
        public uint Stat23 { get; set; }

        [BlubMember(44)]
        public uint Stat24 { get; set; }

        public uint Stat25 { get; set; }

        public byte Unk1 { get; set; }

        public byte Unk2 { get; set; }

        public short ServerId { get; set; }

        public short ChannelId { get; set; }

        public uint RoomId { get; set; }

        public byte Unk3 { get; set; }

        public TDUserDataDto TDStats { get; set; }

        public DMUserDataDto DMStats { get; set; }

        public ChaserUserDataDto ChaserStats { get; set; }

        public BRUserDataDto BattleRoyalStats { get; set; }

        public CPTUserDataDto CaptainStats { get; set; }

        public CommunitySetting AllowCombiInvite { get; set; }

        public CommunitySetting AllowFriendRequest { get; set; }

        public CommunitySetting AllowRoomInvite { get; set; }

        public CommunitySetting AllowInfoRequest { get; set; }

        public Team Team { get; set; }

        public int Unk4 { get; set; }

        public byte Unk5 { get; set; }

        public short Unk6 { get; set; }

        public byte[] Unk7 { get; set; }

        public UserDataDto()
        {
            Nickname = "";
            UnkStr1 = "";
            UnkStr2 = "";
            Unk7 = new byte[9];
            TDStats = new TDUserDataDto();
            DMStats = new DMUserDataDto();
            ChaserStats = new ChaserUserDataDto();
            BattleRoyalStats = new BRUserDataDto();
            CaptainStats = new CPTUserDataDto();
        }
    }

    [BlubContract]
    public class UserDataWithNickDto
    {
        [BlubMember(0)]
        public uint AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public UserDataDto Data { get; set; }

        public UserDataWithNickDto()
        {
            Nickname = "";
            Data = new UserDataDto();
        }
    }

    [BlubContract]
    public class UserDataWithNickLongDto
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public UserDataDto Data { get; set; }

        public UserDataWithNickLongDto()
        {
            Nickname = "";
            Data = new UserDataDto();
        }
    }
}
