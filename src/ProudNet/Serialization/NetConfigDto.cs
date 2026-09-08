using BlubLib.Serialization;

namespace ProudNet.Serialization
{
    [BlubContract]
    internal class NetConfigDto
    {
        [BlubMember(0)]
        public bool EnableServerLog { get; set; }

        [BlubMember(1)]
        public FallbackMethod FallbackMethod { get; set; }

        [BlubMember(2)]
        public uint MessageMaxLength { get; set; }

        [BlubMember(3)]
        public double TimeoutTimeMs { get; set; }

        [BlubMember(4)]
        public DirectP2PStartCondition DirectP2PStartCondition { get; set; }

        [BlubMember(5)]
        public uint OverSendSuspectingThresholdInBytes { get; set; }

        [BlubMember(6)]
        public bool EnableNagleAlgorithm { get; set; }

        [BlubMember(7)]
        public int EncryptedMessageKeyLength { get; set; }

        // Season 1 has a bool here. Season 2 has a second key length, in bits,
        // which it divides by eight to size a CryptoPP key. Zero throws
        // InvalidKeyLength inside the client and kills the connection.
        [BlubMember(8)]
        public uint FastEncryptedMessageKeyLength { get; set; }

        [BlubMember(9)]
        public bool EnableP2PEncryptedMessaging { get; set; }

        [BlubMember(10)]
        public bool UpnpDetectNatDevice { get; set; }

        [BlubMember(11)]
        public bool UpnpTcpAddrPortMapping { get; set; }

        [BlubMember(12)]
        public bool EnablePingTest { get; set; }

        [BlubMember(13)]
        public uint EmergencyLogLineCount { get; set; }

        // Season 2 reads two more bytes here. Names unknown; the client only
        // needs them present, it does not look at the values.
        [BlubMember(14)]
        public bool Unk1 { get; set; }

        [BlubMember(15)]
        public bool Unk2 { get; set; }
    }
}
