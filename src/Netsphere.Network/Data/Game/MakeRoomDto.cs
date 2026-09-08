using BlubLib.Serialization;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Data.Game
{
    [BlubContract]
    public class MakeRoomDto
    {
        [BlubMember(0)]
        public uint GameRule { get; set; }

        [BlubMember(1)]
        public byte Map { get; set; }

        [BlubMember(2)]
        public byte PlayerLimit { get; set; }

        [BlubMember(3)]
        public ushort ScoreLimit { get; set; }

        [BlubMember(4)]
        public byte TimeLimit { get; set; }

        [BlubMember(5)]
        public uint WeaponLimit { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(8)]
        public bool IsFriendly { get; set; }

        [BlubMember(9)]
        public bool IsBalanced { get; set; }

        [BlubMember(10)]
        public bool IsNoIntrusion { get; set; }

        [BlubMember(11)]
        public uint Unk2 { get; set; }

        public byte MinLevel { get; set; }
        public byte MaxLevel { get; set; }

        public MatchKey MatchKey
        {
            get
            {
                byte limitCode;
                switch (PlayerLimit)
                {
                    case 12: limitCode = 8; break;
                    case 10: limitCode = 7; break;
                    case 8: limitCode = 6; break;
                    case 6: limitCode = 5; break;
                    case 4: limitCode = 3; break;
                    default: limitCode = 8; break;
                }

                return new MatchKey((GameRule << 4) | ((uint)Map << 8) | ((uint)limitCode << 16));
            }
        }

        public MakeRoomDto()
        {
            Name = "";
            Password = "";
        }
    }
}
