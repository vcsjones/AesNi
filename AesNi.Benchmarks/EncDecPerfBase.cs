using System;
using System.IO;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace AesNi.Benchmarks
{
    public abstract class EncDecPerfBase : TestKeysBase
    {
        protected  ICryptoTransform frameworkEncryptTransform;
        protected  ICryptoTransform frameworkDecryptTransform;
        protected  byte[] input;
        protected  byte[] iv;
        protected  byte[] output;

        [Params(CipherMode.ECB, CipherMode.CBC)]
        public CipherMode CipherMode { get; set; }

        [Params(PaddingMode.None/*, PaddingMode.Zeros, PaddingMode.PKCS7, PaddingMode.ANSIX923*/)]
        public PaddingMode PaddingMode { get; set; }

        [Params(128, 192, 256)] public int KeySize { get; set; }

        [Params(16, 1024, 1024 * 1024)] public int DataSize { get; set; }

        protected  byte[] KeyBytes
        {
            get
            {
                switch (KeySize)
                {
                    case 128: return KeyArray128;
                    case 192: return KeyArray192;
                    case 256: return KeyArray256;
                    default: throw new InvalidDataException();
                }
            }
        }

        protected  AesKey AesKey => AesKey.Create(KeyBytes);

        [GlobalSetup]
        public void Setup()
        {
            aesKey = AesKey;
            input = new byte[DataSize];
            output = new byte[DataSize];
            iv = new byte[16];

            var r = new Random(42);
            r.NextBytes(input);
            r.NextBytes(output);
            r.NextBytes(iv);

            var aesFw = System.Security.Cryptography.Aes.Create();
            aesFw.Key = KeyBytes;
            aesFw.Mode = CipherMode;
            aesFw.Padding = PaddingMode;
            aesFw.IV = iv;
            frameworkEncryptTransform = aesFw.CreateEncryptor();
            frameworkDecryptTransform = aesFw.CreateDecryptor();
        }
    }
}