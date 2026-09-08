using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ProudNet
{
    // Season 2's client hands the server key to Proud::CRsaKey::FromBlob, which
    // is an ASN.1 decoder, not the Windows CryptoAPI. It wants X.509
    // SubjectPublicKeyInfo DER:
    //
    //   SEQUENCE {
    //     SEQUENCE { OID 1.2.840.113549.1.1.1, NULL }
    //     BIT STRING { SEQUENCE { INTEGER modulus, INTEGER exponent } }
    //   }
    //
    // .NET Framework has no ExportSubjectPublicKeyInfo, so write it by hand.
    internal static class RsaDer
    {
        private static readonly byte[] RsaEncryptionOid =
            { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01 };

        public static byte[] SubjectPublicKeyInfo(RSAParameters key)
        {
            var rsaPublicKey = Sequence(
                Integer(key.Modulus).Concat(Integer(key.Exponent)).ToArray());

            var algorithm = Sequence(
                RsaEncryptionOid.Concat(new byte[] { 0x05, 0x00 }).ToArray());

            var bitString = TagLengthValue(0x03,
                new byte[] { 0x00 }.Concat(rsaPublicKey).ToArray());

            return Sequence(algorithm.Concat(bitString).ToArray());
        }

        public static byte[] Pkcs1PublicKey(RSAParameters key)
        {
            return Sequence(Integer(key.Modulus).Concat(Integer(key.Exponent)).ToArray());
        }

        private static byte[] Sequence(byte[] content)
        {
            return TagLengthValue(0x30, content);
        }

        private static byte[] Integer(byte[] value)
        {
            var trimmed = value.SkipWhile(b => b == 0).ToArray();
            if (trimmed.Length == 0)
                trimmed = new byte[] { 0x00 };

            // DER integers are signed, so a leading high bit needs a zero byte.
            if ((trimmed[0] & 0x80) != 0)
                trimmed = new byte[] { 0x00 }.Concat(trimmed).ToArray();

            return TagLengthValue(0x02, trimmed);
        }

        private static byte[] TagLengthValue(byte tag, byte[] content)
        {
            using (var ms = new MemoryStream())
            {
                ms.WriteByte(tag);
                WriteLength(ms, content.Length);
                ms.Write(content, 0, content.Length);
                return ms.ToArray();
            }
        }

        private static void WriteLength(Stream stream, int length)
        {
            if (length < 0x80)
            {
                stream.WriteByte((byte)length);
                return;
            }

            var bytes = new List<byte>();
            var value = length;
            while (value > 0)
            {
                bytes.Insert(0, (byte)(value & 0xFF));
                value >>= 8;
            }

            stream.WriteByte((byte)(0x80 | bytes.Count));
            foreach (var b in bytes)
                stream.WriteByte(b);
        }
    }
}
