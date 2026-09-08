using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using BlubLib.IO;
using BlubLib.Security.Cryptography;

namespace ProudNet
{
    internal class Crypt : IDisposable
    {
        private static readonly RNGCryptoServiceProvider s_random = new RNGCryptoServiceProvider();

        private int _encryptCounter;
        private int _decryptCounter;

        public RC4 RC4 { get; private set; }

        public Crypt(int keySize)
        {
            RC4 = new RC4 { KeySize = keySize };
            RC4.GenerateKey();
        }

        // Season 2 runs two ciphers: AES for the reliable channel and RC4 for
        // the unreliable one. The client picks both keys and sends them over,
        // the AES one wrapped with RSA and the RC4 one wrapped with the AES key.
        public Crypt(byte[] aesKey, byte[] rc4Key)
        {
            Aes = new AesCryptoServiceProvider
            {
                Key = aesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };

            RC4 = new RC4 { KeySize = rc4Key.Length * 8 };
            RC4.Key = rc4Key;
        }

        public AesCryptoServiceProvider Aes { get; private set; }

        public void Encrypt(Stream src, Stream dst, bool reliable)
        {
            if (RC4 == null)
                throw new ObjectDisposedException(GetType().FullName);

            if (reliable && Aes != null)
            {
                using (var body = new MemoryStream())
                {
                    src.CopyTo(body);
                    var data = body.ToArray();

                    // Same shape the client sends: padding length, four random
                    // bytes, the counter, the message, then the block padding.
                    var counter = (ushort)(Interlocked.Increment(ref _encryptCounter) - 1);
                    var used = 7 + data.Length;
                    var padding = (16 - used % 16) % 16;
                    var plain = new byte[used + padding];

                    plain[0] = (byte)padding;
                    var salt = new byte[4];
                    s_random.GetBytes(salt);
                    Buffer.BlockCopy(salt, 0, plain, 1, 4);
                    plain[5] = (byte)(counter & 0x00FF);
                    plain[6] = (byte)(counter >> 8);
                    Buffer.BlockCopy(data, 0, plain, 7, data.Length);

                    using (var encryptor = Aes.CreateEncryptor())
                    {
                        var cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                        dst.Write(cipher, 0, cipher.Length);
                    }
                }
                return;
            }

            using (var encryptor = RC4.CreateEncryptor())
            using (var cs = new CryptoStream(new NonClosingStream(dst), encryptor, CryptoStreamMode.Write))
            {
                if (reliable)
                {
                    var counter = (ushort)(Interlocked.Increment(ref _encryptCounter) - 1);
                    cs.WriteByte((byte)(counter & 0x00FF));
                    cs.WriteByte((byte)(counter >> 8));
                }
                src.CopyTo(cs);
            }
        }

        public void Decrypt(Stream src, Stream dst, bool reliable)
        {
            if (RC4 == null)
                throw new ObjectDisposedException(GetType().FullName);

            if (reliable && Aes != null)
            {
                using (var ms = new MemoryStream())
                {
                    src.CopyTo(ms);
                    var cipher = ms.ToArray();
                    var usable = cipher.Length - cipher.Length % 16;

                    using (var decryptor = Aes.CreateDecryptor())
                    {
                        var plain = decryptor.TransformFinalBlock(cipher, 0, usable);
                        Interlocked.Increment(ref _decryptCounter);

                        // Season 2 header: padding length, four random bytes,
                        // then the counter. The message starts at byte seven and
                        // the block padding hangs off the end.
                        var padding = plain[0];
                        var start = 7;
                        var length = plain.Length - padding - start;

                        if (length <= 0)
                        {
                            NLog.LogManager.GetLogger("Crypt").Warn(
                                $"cabecera rara: {plain.Length} bytes, relleno {padding}");
                            return;
                        }

                        dst.Write(plain, start, length);
                    }
                }
                return;
            }

            using (var decryptor = RC4.CreateDecryptor())
            using (var cs = new CryptoStream(src, decryptor, CryptoStreamMode.Read))
            {
                if (reliable)
                {
                    var counter = (ushort)(Interlocked.Increment(ref _decryptCounter) - 1);
                    var messageCounter = cs.ReadByte() | cs.ReadByte() << 8;

                    // Not fatal while the Season 2 stream is still being worked
                    // out: a mismatch tells us the key is wrong, but throwing
                    // here hides whatever the rest of the message decodes to.
                    if (counter != messageCounter)
                        NLog.LogManager.GetLogger("Crypt").Warn(
                            $"contador de descifrado distinto, remoto {messageCounter} local {counter}");
                }

                cs.CopyTo(dst);
            }
        }

        public void Dispose()
        {
            if (RC4 != null)
            {
                RC4.Dispose();
                RC4 = null;
            }
        }
    }
}
