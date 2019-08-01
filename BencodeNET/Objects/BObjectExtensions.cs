using System;
using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
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
            using (var stream = bobject.EncodeTo(new MemoryStream(size)))
            {
#if NETCOREAPP2_1
                if (stream.TryGetBuffer(out var buffer) && stream.Length <=  int.MaxValue)
                {
                    return encoding.GetString(buffer);
                }
#endif
                return encoding.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Encodes the object and returns the raw bytes.
        /// </summary>
        /// <returns>The raw bytes of the bencoded object.</returns>
        public static ReadOnlySpan<byte> EncodeAsBytes(this IBObject bobject)
        {
            var size = bobject.GetSizeInBytes();
            using (var stream = new MemoryStream(size))
            {
                bobject.EncodeTo(stream);
                if (stream.TryGetBuffer(out var buffer) && stream.Length <= int.MaxValue)
                {
                    return buffer;
                }
                return stream.ToArray();
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

#if !NETSTANDARD1_3
        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="bobject"></param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="bufferSize">The buffer size to use. Uses default size of <see cref="BufferedStream"/> if null.</param>
        /// <returns>The used stream.</returns>
        public static BufferedStream EncodeToBuffered(this IBObject bobject, Stream stream, int? bufferSize = null)
        {
            var bufferedStream = bufferSize == null
                ? new BufferedStream(stream)
                : new BufferedStream(stream, bufferSize.Value);
            bobject.EncodeTo(bufferedStream);
            return bufferedStream;
        }
#endif
    }
}
