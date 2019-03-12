using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace AesNi.Tests
{
    public class Aes128Tests
    {
        // TODO: test all padding modes
        private const int DataSize = 1040;

        [Fact]
        public void ReferenceTest()
        {
            var r = new Random(42);
            var bytes = new byte[DataSize + 2];
            var key = new byte[16];
            r.NextBytes(bytes);
            r.NextBytes(key);

            var managed = new AesManaged {Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros};
            var managedResult = managed.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);

            var niResult = new byte[DataSize + 16];
            Aes.Encrypt(bytes, niResult, null, new Aes128Key(key), CipherMode.ECB, PaddingMode.Zeros);

            Assert.Equal(managedResult, niResult);
        }

        [Fact]
        public void ReferenceTestCbc()
        {
            var r = new Random(42);
            var bytes = new byte[DataSize];
            var key = new byte[16];
            var iv = new byte[16];
            r.NextBytes(bytes);
            r.NextBytes(key);
            r.NextBytes(iv);

            var managed = new AesManaged {Key = key, IV = iv, Mode = CipherMode.CBC, Padding = PaddingMode.None};
            var managedResult = managed.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);

            var niResult = new byte[DataSize];
            Aes.EncryptCbc(bytes, niResult, iv, new Aes128Key(key));

            Assert.Equal(managedResult, niResult);
        }

        [Fact]
        public void SelfTest()
        {
            var r = new Random(42);
            var plain = new byte[DataSize];
            var key = new byte[16];
            r.NextBytes(plain);
            r.NextBytes(key);

            var cipher = new byte[DataSize];
            var plainAgain = new byte[DataSize];
            var k = new Aes128Key(key);

            Aes.EncryptEcb(plain, cipher, k);
            Aes.DecryptEcb(cipher, plainAgain, k);

            Assert.Equal(plain, plainAgain);
        }

        [Fact]
        public void SelfTestCbc()
        {
            var r = new Random(42);
            var plain = new byte[DataSize];
            var key = new byte[16];
            var iv = new byte[16];
            r.NextBytes(plain);
            r.NextBytes(key);
            r.NextBytes(iv);

            var cipher = new byte[DataSize];
            var plainAgain = new byte[DataSize];
            var k = new Aes128Key(key);

            Aes.EncryptCbc(plain, cipher, iv, k);
            Aes.DecryptCbc(cipher, plainAgain, iv, k);

            Assert.Equal(plain, plainAgain);
        }
    }
}