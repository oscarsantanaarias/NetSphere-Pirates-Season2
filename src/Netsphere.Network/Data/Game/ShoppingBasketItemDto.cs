using BlubLib.Serialization;

namespace Netsphere.Network.Data.Game
{
    // 50056 / 60099 / 60100. Layout del cliente: u32,u32,u32,u32,u16,u8,u32
    [BlubContract]
    public class ShoppingBasketItemDto
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }

        [BlubMember(4)]
        public ushort Unk5 { get; set; }

        [BlubMember(5)]
        public byte Unk6 { get; set; }

        [BlubMember(6)]
        public uint Unk7 { get; set; }
    }

    // 60104. Layout del cliente: u32,u32,u32,u8,u32,u8
    [BlubContract]
    public class RandomShopRollingDto
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public byte Unk4 { get; set; }

        [BlubMember(4)]
        public uint Unk5 { get; set; }

        [BlubMember(5)]
        public byte Unk6 { get; set; }
    }
}
