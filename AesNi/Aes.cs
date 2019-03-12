using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace AesNi
{
    public static partial class Aes
    {
        // TODO: harmonize default parameter values (e.g. paddingMode)

        public static void Encrypt(ReadOnlySpan<byte> plaintext,
            Span<byte> ciphertext,
            ReadOnlySpan<byte> iv,
            AesKey key,
            CipherMode cipherMode = CipherMode.CBC,
            PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (cipherMode != CipherMode.ECB && cipherMode != CipherMode.CBC)
                ThrowHelper.ThrowNotImplementedException();
            if (paddingMode == PaddingMode.None && plaintext.Length % BlockSize != 0)
                ThrowHelper.ThrowInputNotMultipleOfBlockSizeException(nameof(plaintext));
            if (cipherMode == CipherMode.CBC && iv == null)
                ThrowHelper.ThrowArgumentNullException(nameof(iv));
            // TODO: correctly validate ciphertext length
            if (ciphertext.Length < plaintext.Length)
                ThrowHelper.ThrowDestinationBufferTooSmallException(nameof(ciphertext));
            // TODO: moar validation

            switch (cipherMode)
            {
                case CipherMode.ECB:
                    DispatchEncryptEcb(plaintext, ciphertext, key, paddingMode);
                    return;
                case CipherMode.CBC:
                    DispatchEncryptCbc(plaintext, ciphertext, iv, key, paddingMode);
                    return;
            }

            ThrowHelper.ThrowNotImplementedException();
        }

        private static void DispatchEncryptEcb(ReadOnlySpan<byte> plaintext,
            Span<byte> ciphertext,
            AesKey key,
            PaddingMode paddingMode)
        {
            switch (key)
            {
                case Aes128Key aes128Key:
                    EncryptEcb(plaintext, ciphertext, aes128Key, paddingMode);
                    return;
                case Aes192Key aes192Key:
                    EncryptEcb(plaintext, ciphertext, aes192Key, paddingMode);
                    return;
                case Aes256Key aes256Key:
                    EncryptEcb(plaintext, ciphertext, aes256Key, paddingMode);
                    return;
            }
        }

        private static void DispatchEncryptCbc(ReadOnlySpan<byte> plaintext,
            Span<byte> ciphertext,
            ReadOnlySpan<byte> iv,
            AesKey key,
            PaddingMode paddingMode)
        {
            switch (key)
            {
                case Aes128Key aes128Key:
                    EncryptCbc(plaintext, ciphertext, iv, aes128Key, paddingMode);
                    return;
                case Aes192Key aes192Key:
                    EncryptCbc(plaintext, ciphertext, iv, aes192Key, paddingMode);
                    return;
                case Aes256Key aes256Key:
                    EncryptCbc(plaintext, ciphertext, iv, aes256Key, paddingMode);
                    return;
            }
        }

        public static void Decrypt(ReadOnlySpan<byte> ciphertext,
            Span<byte> plaintext,
            ReadOnlySpan<byte> iv,
            AesKey key,
            CipherMode cipherMode = CipherMode.CBC,
            PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (cipherMode != CipherMode.ECB && cipherMode != CipherMode.CBC)
                ThrowHelper.ThrowNotImplementedException();
            if (cipherMode == CipherMode.CBC && iv == null)
                ThrowHelper.ThrowArgumentNullException(nameof(iv));
            // TODO: correctly validate plaintext length
            if (plaintext.Length < ciphertext.Length)
                ThrowHelper.ThrowDestinationBufferTooSmallException(nameof(plaintext));
            // TODO: moar validation

            switch (cipherMode)
            {
                case CipherMode.ECB:
                    DispatchDecryptEcb(ciphertext, plaintext, key, paddingMode);
                    return;
                case CipherMode.CBC:
                    DispatchDecryptCbc(ciphertext, plaintext, iv, key, paddingMode);
                    return;
            }

            ThrowHelper.ThrowNotImplementedException();
        }

        private static void DispatchDecryptEcb(
            ReadOnlySpan<byte> ciphertext,
            Span<byte> plaintext,
            AesKey key,
            PaddingMode paddingMode)
        {
            switch (key)
            {
                case Aes128Key aes128Key:
                    DecryptEcb(ciphertext, plaintext, aes128Key);
                    break;
                case Aes192Key aes192Key:
                    DecryptEcb(ciphertext, plaintext, aes192Key);
                    break;
                case Aes256Key aes256Key:
                    DecryptEcb(ciphertext, plaintext, aes256Key);
                    break;
            }
        }

        private static void DispatchDecryptCbc(
            ReadOnlySpan<byte> ciphertext,
            Span<byte> plaintext,
            ReadOnlySpan<byte> iv,
            AesKey key,
            PaddingMode paddingMode)
        {
            switch (key)
            {
                case Aes128Key aes128Key:
                    DecryptCbc(ciphertext, plaintext, iv, aes128Key);
                    break;
                case Aes192Key aes192Key:
                    DecryptCbc(ciphertext, plaintext, iv, aes192Key);
                    break;
                case Aes256Key aes256Key:
                    DecryptCbc(ciphertext, plaintext, iv, aes256Key);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyPadding(ReadOnlySpan<byte> remainingBytes, Span<byte> lastBlock,
            PaddingMode paddingMode)
        {
            remainingBytes.CopyTo(lastBlock); // fill last block with remainder of message
            var remainingBytesLength = (byte) remainingBytes.Length;

            switch (paddingMode)
            {
                case PaddingMode.ANSIX923: // fill with zeroes, length of padding in last byte
                    lastBlock[BlockSize - 1] = remainingBytesLength;
                    break;
                case PaddingMode.ISO10126: // fill with random, length of padding in last byte
                    RandomHelper.NextBytes(lastBlock.Slice(remainingBytes.Length)); // fill rest with random bytes
                    lastBlock[BlockSize - 1] = (byte) remainingBytes.Length; // set last byte to length
                    break;
                case PaddingMode.PKCS7: // fill with length of padding
                    lastBlock.Slice(remainingBytes.Length).Fill(remainingBytesLength);
                    break;
                case PaddingMode.Zeros: // fill with zeroes
                    break; // lastBlock assumed to be already zeroed out
                default:
                    ThrowHelper.ThrowPaddingNotSupportedException(paddingMode);
                    break; // unreachable
            }
        }
    }
}