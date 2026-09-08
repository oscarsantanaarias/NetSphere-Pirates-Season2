using System;
using BlubLib.Serialization;
using Netsphere.Network.Serializers;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Data.GameRule
{
    // Season 2 sends the room settings flat, the same shape it uses for
    // MakeRoom: no packed match key and no friendly/balanced flags.
    [BlubContract]
    public class ChangeRuleDto
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
        public uint Unk1 { get; set; }

        [BlubMember(5)]
        public byte TimeLimitMinutes { get; set; }

        [BlubMember(6)]
        public uint WeaponLimit { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(9)]
        public bool HasSpectator { get; set; }

        [BlubMember(10)]
        public byte SpectatorLimit { get; set; }

        [BlubMember(11)]
        public byte Unk3 { get; set; }

        [BlubMember(12)]
        public uint Unk4 { get; set; }

        public TimeSpan TimeLimit
        {
            get { return TimeSpan.FromMinutes(TimeLimitMinutes); }
            set { TimeLimitMinutes = (byte)value.TotalMinutes; }
        }

        public byte ItemLimit
        {
            get { return (byte)WeaponLimit; }
            set { WeaponLimit = value; }
        }

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

        public ChangeRuleDto()
        {
            Name = "";
            Password = "";
        }
    }
}
