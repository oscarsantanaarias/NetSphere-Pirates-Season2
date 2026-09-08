using BlubLib.Serialization;

namespace Netsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ChangeAvatarItemDto
    {
        [BlubMember(0)]
        public ItemNumber ItemNumber { get; set; }

        [BlubMember(1)]
        public uint Effect { get; set; }

        public ChangeAvatarItemDto()
        { }

        public ChangeAvatarItemDto(ItemNumber itemNumber, uint effect)
        {
            ItemNumber = itemNumber;
            Effect = effect;
        }
    }
}
