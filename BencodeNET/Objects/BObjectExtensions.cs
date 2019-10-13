using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Extensions to simplify encoding directly as a string or byte array.
    /// </summary>
    public static class BObjectExtensions
    {
        /// <summary>
        /// Encodes the object and returns the result as a string using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <returns>The object bencoded and converted to a string using <see cref="Encoding.UTF8"/>.</returns>
        public static string EncodeAsString(this IBObject bobject) => EncodeAsString(bobject, Encoding.UTF8);

        /// <summary>
        /// Encodes the byte-string as bencode and returns the encoded string.
        /// Uses the current value of the <see cref="Encoding"/> property.
        /// </summary>
        /// <returns>The byte-string as a bencoded string.</returns>
        public static string EncodeAsString(this BString bstring) => EncodeAsString(bstring, bstring.Encoding);

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="bobject"></param>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>The object bencoded and converted to a string using the specified encoding.</returns>
        public static string EncodeAsString(this IBObject bobject, Encoding encoding)
        {
            var size = bobject.GetSizeInBytes();
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                using (var stream = new MemoryStream(buffer))
                {
                    bobject.EncodeTo(stream);
                    return encoding.GetString(buffer.AsSpan().Slice(0, size));
                }
            }
            finally { ArrayPool<byte>.Shared.Return(buffer); }
        }

        /// <summary>
        /// Encodes the object and returns the raw bytes.
        /// </summary>
        /// <returns>The raw bytes of the bencoded object.</returns>
        public static byte[] EncodeAsBytes(this IBObject bobject)
        {
            var size = bobject.GetSizeInBytes();
            var bytes = new byte[size];
            using (var stream = new MemoryStream(bytes))
            {
                bobject.EncodeTo(stream);
                return bytes;
            }
        }

        /// <summary>
        /// Writes the object as bencode to the specified file path.
        /// </summary>
        /// <param name="bobject"></param>
        /// <param name="filePath">The file path to write the encoded object to.</param>
        public static void EncodeTo(this IBObject bobject, string filePath)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                bobject.EncodeTo(stream);
            }
        }
    }
}
