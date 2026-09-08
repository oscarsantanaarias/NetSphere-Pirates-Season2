using BlubLib.Serialization;

namespace Netsphere.Network.Data.GameRule
{
    // 21072. Layout del cliente: u32,u32,u8,u8,u16,u16,u32,u32,u32,u64 + {u32,u64}
    [BlubContract]
    public class SeizeInfoDto
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public byte Unk3 { get; set; }

        [BlubMember(3)]
        public byte Unk4 { get; set; }

        [BlubMember(4)]
        public ushort Unk5 { get; set; }

        [BlubMember(5)]
        public ushort Unk6 { get; set; }

        [BlubMember(6)]
        public uint Unk7 { get; set; }

        [BlubMember(7)]
        public uint Unk8 { get; set; }

        [BlubMember(8)]
        public uint Unk9 { get; set; }

        [BlubMember(9)]
        public ulong Unk10 { get; set; }

        [BlubMember(10)]
        public uint Unk11 { get; set; }

        [BlubMember(11)]
        public ulong Unk12 { get; set; }
    }

    // 21073. Layout del cliente: u32,u8,u16,u16
    [BlubContract]
    public class SeizeIntrudeInfoDto
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }

        [BlubMember(2)]
        public ushort Unk3 { get; set; }

        [BlubMember(3)]
        public ushort Unk4 { get; set; }
    }

    // 21076. Layout del cliente: u64,u8
    [BlubContract]
    public class SeizeBuffDropDto
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }
}
