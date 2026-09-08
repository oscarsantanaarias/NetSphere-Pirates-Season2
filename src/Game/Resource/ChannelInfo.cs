namespace Netsphere.Resource
{
    public class ChannelInfo
    {
        public uint Id { get; set; }
        public ChannelCategory Category { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Description { get; set; }
        public int PlayerLimit { get; set; }
        public byte Type { get; set; }
        public uint Color { get; set; }
        public bool IsClanChannel { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
