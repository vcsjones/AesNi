using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using static AesNi.Utils;
using static System.Runtime.Intrinsics.X86.Sse2;
using AesIntrin = System.Runtime.Intrinsics.X86.Aes;

namespace AesNi
{
    public static partial class Aes
    {
        public static void EncryptCbc(
            ReadOnlySpan<byte> plaintext,
            Span<byte> ciphertext,
            ReadOnlySpan<byte> iv,
            Aes128Key key,
            PaddingMode paddingMode = PaddingMode.Zeros)
        {
            ref var expandedKey = ref MemoryMarshal.GetReference(key.ExpandedKey);
            ref var inputRef = ref MemoryMarshal.GetReference(plaintext);
            ref var outputRef = ref MemoryMarshal.GetReference(ciphertext);

            var position = 0;
            var left = plaintext.Length;

            var key0 = ReadUnalignedOffset(ref expandedKey, Kn * 0);
            var key1 = ReadUnalignedOffset(ref expandedKey, Kn * 1);
            var key2 = ReadUnalignedOffset(ref expandedKey, Kn * 2);
            var key3 = ReadUnalignedOffset(ref expandedKey, Kn * 3);
            var key4 = ReadUnalignedOffset(ref expandedKey, Kn * 4);
            var key5 = ReadUnalignedOffset(ref expandedKey, Kn * 5);
            var key6 = ReadUnalignedOffset(ref expandedKey, Kn * 6);
            var key7 = ReadUnalignedOffset(ref expandedKey, Kn * 7);
            var key8 = ReadUnalignedOffset(ref expandedKey, Kn * 8);
            var key9 = ReadUnalignedOffset(ref expandedKey, Kn * 9);
            var key10 = ReadUnalignedOffset(ref expandedKey, Kn * 10);

            var feedback = ReadUnalignedOffset(ref MemoryMarshal.GetReference(iv), 0);

            while (left >= BlockSize)
            {
                var block = ReadUnalignedOffset(ref inputRef, position);

                feedback = Xor(block, feedback);
                feedback = Xor(feedback, key0);

                feedback = AesIntrin.Encrypt(feedback, key1);
                feedback = AesIntrin.Encrypt(feedback, key2);
                feedback = AesIntrin.Encrypt(feedback, key3);
                feedback = AesIntrin.Encrypt(feedback, key4);
                feedback = AesIntrin.Encrypt(feedback, key5);
                feedback = AesIntrin.Encrypt(feedback, key6);
                feedback = AesIntrin.Encrypt(feedback, key7);
                feedback = AesIntrin.Encrypt(feedback, key8);
                feedback = AesIntrin.Encrypt(feedback, key9);
                feedback = AesIntrin.EncryptLast(feedback, key10);

                WriteUnalignedOffset(ref outputRef, position, feedback);

                position += BlockSize;
                left -= BlockSize;
            }

            if (paddingMode == PaddingMode.None)
            {
                Debug.Assert(left == 0);
                return;
            }

            Span<byte> lastBlock = stackalloc byte[BlockSize];
            var remainingPlaintext =
                left != 0 ? plaintext.Slice(plaintext.Length - left) : ReadOnlySpan<byte>.Empty;

            ApplyPadding(remainingPlaintext, lastBlock, paddingMode);

            var lBlock = ReadUnalignedOffset(ref MemoryMarshal.GetReference(lastBlock), 0);

            feedback = Xor(lBlock, feedback);
            feedback = Xor(feedback, key0);

            feedback = AesIntrin.Encrypt(feedback, key1);
            feedback = AesIntrin.Encrypt(feedback, key2);
            feedback = AesIntrin.Encrypt(feedback, key3);
            feedback = AesIntrin.Encrypt(feedback, key4);
            feedback = AesIntrin.Encrypt(feedback, key5);
            feedback = AesIntrin.Encrypt(feedback, key6);
            feedback = AesIntrin.Encrypt(feedback, key7);
            feedback = AesIntrin.Encrypt(feedback, key8);
            feedback = AesIntrin.Encrypt(feedback, key9);
            feedback = AesIntrin.EncryptLast(feedback, key10);

            WriteUnalignedOffset(ref outputRef, position, feedback);
        }

        public static void DecryptCbc(
            ReadOnlySpan<byte> ciphertext,
            Span<byte> plaintext,
            ReadOnlySpan<byte> iv,
            Aes128Key key,
            PaddingMode paddingMode = PaddingMode.Zeros)
        {
            ref var expandedKey = ref MemoryMarshal.GetReference(key.ExpandedKey);
            ref var inputRef = ref MemoryMarshal.GetReference(ciphertext);
            ref var outputRef = ref MemoryMarshal.GetReference(plaintext);

            var position = 0;
            var left = ciphertext.Length;

            var key0 = ReadUnalignedOffset(ref expandedKey, Kn * 10);
            var key1 = ReadUnalignedOffset(ref expandedKey, Kn * 11);
            var key2 = ReadUnalignedOffset(ref expandedKey, Kn * 12);
            var key3 = ReadUnalignedOffset(ref expandedKey, Kn * 13);
            var key4 = ReadUnalignedOffset(ref expandedKey, Kn * 14);
            var key5 = ReadUnalignedOffset(ref expandedKey, Kn * 15);
            var key6 = ReadUnalignedOffset(ref expandedKey, Kn * 16);
            var key7 = ReadUnalignedOffset(ref expandedKey, Kn * 17);
            var key8 = ReadUnalignedOffset(ref expandedKey, Kn * 18);
            var key9 = ReadUnalignedOffset(ref expandedKey, Kn * 19);
            var key10 = ReadUnalignedOffset(ref expandedKey, Kn * 0);

            var feedback0 = ReadUnalignedOffset(ref MemoryMarshal.GetReference(iv), 0);
            Vector128<byte> feedback1;
            Vector128<byte> feedback2;
            Vector128<byte> feedback3;
            Vector128<byte> feedback4;
            Vector128<byte> feedback5;
            Vector128<byte> feedback6;
            Vector128<byte> feedback7;
            Vector128<byte> lastIn;

            while (left >= BlockSize * 8)
            {
                var block0 = ReadUnalignedOffset(ref inputRef, position + 0 * BlockSize);
                var block1 = ReadUnalignedOffset(ref inputRef, position + 1 * BlockSize);
                var block2 = ReadUnalignedOffset(ref inputRef, position + 2 * BlockSize);
                var block3 = ReadUnalignedOffset(ref inputRef, position + 3 * BlockSize);
                var block4 = ReadUnalignedOffset(ref inputRef, position + 4 * BlockSize);
                var block5 = ReadUnalignedOffset(ref inputRef, position + 5 * BlockSize);
                var block6 = ReadUnalignedOffset(ref inputRef, position + 6 * BlockSize);
                var block7 = ReadUnalignedOffset(ref inputRef, position + 7 * BlockSize);

                feedback1 = block0;
                feedback2 = block1;
                feedback3 = block2;
                feedback4 = block3;
                feedback5 = block4;
                feedback6 = block5;
                feedback7 = block6;
                lastIn = block7;

                block0 = Xor(block0, key0);
                block1 = Xor(block1, key0);
                block2 = Xor(block2, key0);
                block3 = Xor(block3, key0);
                block4 = Xor(block4, key0);
                block5 = Xor(block5, key0);
                block6 = Xor(block6, key0);
                block7 = Xor(block7, key0);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key1);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key1);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key1);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key1);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key1);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key1);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key1);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key1);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key2);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key2);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key2);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key2);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key2);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key2);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key2);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key2);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key3);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key3);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key3);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key3);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key3);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key3);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key3);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key3);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key4);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key4);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key4);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key4);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key4);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key4);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key4);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key4);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key5);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key5);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key5);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key5);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key5);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key5);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key5);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key5);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key6);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key6);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key6);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key6);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key6);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key6);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key6);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key6);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key7);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key7);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key7);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key7);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key7);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key7);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key7);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key7);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key8);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key8);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key8);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key8);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key8);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key8);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key8);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key8);

                block0 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block0, key9);
                block1 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block1, key9);
                block2 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block2, key9);
                block3 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block3, key9);
                block4 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block4, key9);
                block5 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block5, key9);
                block6 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block6, key9);
                block7 = System.Runtime.Intrinsics.X86.Aes.Decrypt(block7, key9);

                block0 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block0, key10);
                block1 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block1, key10);
                block2 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block2, key10);
                block3 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block3, key10);
                block4 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block4, key10);
                block5 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block5, key10);
                block6 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block6, key10);
                block7 = System.Runtime.Intrinsics.X86.Aes.DecryptLast(block7, key10);

                block0 = Xor(block0, feedback0);
                block1 = Xor(block1, feedback1);
                block2 = Xor(block2, feedback2);
                block3 = Xor(block3, feedback3);
                block4 = Xor(block4, feedback4);
                block5 = Xor(block5, feedback5);
                block6 = Xor(block6, feedback6);
                block7 = Xor(block7, feedback7);

                WriteUnalignedOffset(ref outputRef, position + 0 * BlockSize, block0);
                WriteUnalignedOffset(ref outputRef, position + 1 * BlockSize, block1);
                WriteUnalignedOffset(ref outputRef, position + 2 * BlockSize, block2);
                WriteUnalignedOffset(ref outputRef, position + 3 * BlockSize, block3);
                WriteUnalignedOffset(ref outputRef, position + 4 * BlockSize, block4);
                WriteUnalignedOffset(ref outputRef, position + 5 * BlockSize, block5);
                WriteUnalignedOffset(ref outputRef, position + 6 * BlockSize, block6);
                WriteUnalignedOffset(ref outputRef, position + 7 * BlockSize, block7);

                feedback0 = lastIn;

                position += BlockSize * 8;
                left -= BlockSize * 8;
            }

            while (left >= BlockSize)
            {
                var block = ReadUnalignedOffset(ref inputRef, position);
                lastIn = block;
                var data = Xor(block, key0);

                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key1);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key2);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key3);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key4);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key5);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key6);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key7);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key8);
                data = System.Runtime.Intrinsics.X86.Aes.Decrypt(data, key9);
                data = System.Runtime.Intrinsics.X86.Aes.DecryptLast(data, key10);

                data = Xor(data, feedback0);

                WriteUnalignedOffset(ref outputRef, position, data);

                feedback0 = lastIn;

                position += BlockSize;
                left -= BlockSize;
            }
        }
    }
}