using BlubLib.Serialization;

namespace Netsphere.Network.Data.Game
{
    [BlubContract]
    public class GiveItemResultDto
    {
        [BlubMember(0)]
        public uint ItemNumber { get; set; }

        [BlubMember(1)]
        public uint Unk { get; set; }

        public GiveItemResultDto()
        { }

        public GiveItemResultDto(uint itemNumber)
        {
            ItemNumber = itemNumber;
        }
    }
}
