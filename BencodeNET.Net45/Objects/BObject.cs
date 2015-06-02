using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    public abstract class BObject<TY> : IBObject
    {
        internal BObject()
        { }

        /// <summary>
        /// The underlying value of the BObject.
        /// </summary>
        public TY Value { get; protected set; }


        /// <summary>
        /// Encodes the object and returns the result as a string using 
        /// the default encoding from <c>Bencode.DefaultEncoding</c>.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using
        /// the encoding of <c>Bencode.DefaultEncoding</c>.
        /// </returns>
        public virtual string Encode()
        {
            return Encode(Bencode.DefaultEncoding);
        }

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>
        /// The object bencoded and converted to a string using the specified encoding.
        /// </returns>
        public virtual string Encode(Encoding encoding)
        {
            var ms = new MemoryStream();
            EncodeToStream(ms).Position = 0;
            return new StreamReader(ms, encoding).ReadToEnd();
        }

        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        public abstract TStream EncodeToStream<TStream>(TStream stream) where TStream : Stream;
    }
}
