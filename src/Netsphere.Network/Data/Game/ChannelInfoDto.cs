using BlubLib.Serialization;
using Netsphere.Network.Serializers;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Data.Game
{
    [BlubContract]
    public class ChannelInfoDto
    {
        [BlubMember(0)]
        public ushort ChannelId { get; set; }

        [BlubMember(1)]
        public ushort PlayerCount { get; set; }

        [BlubMember(2)]
        public ushort PlayerLimit { get; set; }

#if CLIENT_1162
        // 1162 client reads 4 raw bytes here (see FUN_00b6f180), not a 1-byte
        // bool - otherwise every field after this one (Name, Rank,
        // Description, levels) shifts by 3 bytes.
        [BlubMember(3, typeof(IntBooleanSerializer))]
#else
        [BlubMember(3)]
#endif
        public bool IsClanChannel { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string Rank { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string Description { get; set; }

        [BlubMember(7)]
        public uint Color { get; set; }

        [BlubMember(8)]
        public uint MinLevel { get; set; }

        [BlubMember(9)]
        public uint MaxLevel { get; set; }

#if CLIENT_1162
        // 1162 client reads these as floats (see FUN_00b72da0), not uints.
        [BlubMember(10)]
        public float MinRankedLevel { get; set; }

        [BlubMember(11)]
        public float MaxRankedLevel { get; set; }
#else
        [BlubMember(10)]
        public uint MinRankedLevel { get; set; }

        [BlubMember(11)]
        public uint MaxRankedLevel { get; set; }
#endif

        public ChannelInfoDto()
        {
            Name = "";
            Rank = "FREE";
            Description = "";
            MaxRankedLevel = 999;
        }
    }
}
