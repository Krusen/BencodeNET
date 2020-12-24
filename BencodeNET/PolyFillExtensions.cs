using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BencodeNET
{
#if NETSTANDARD2_0
    internal static class PolyFillExtensions
    {
        public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            {
                return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
            }
        }

        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            {
                return encoding.GetString(bytesPtr, bytes.Length);
            }
        }

        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(array);
                stream.Write(array, 0, buffer.Length);
            }
            finally { ArrayPool<byte>.Shared.Return(array); }
        }
    }
#endif
}
