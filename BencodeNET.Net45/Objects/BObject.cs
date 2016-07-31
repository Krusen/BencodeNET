using System;
using System.IO;
using System.Linq;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

namespace BencodeNET.Objects
{
    public abstract class BObject : IBObject
    {

        internal BObject()
        { }

        // <summary>
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
            using (var ms = EncodeToStream(new MemoryStream()))
            {
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        public abstract TStream EncodeToStream<TStream>(TStream stream) where TStream : Stream;

#if !NET35
        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        public abstract Task<TStream> EncodeToStreamAsync<TStream>(TStream stream) where TStream : Stream;
#endif

        public virtual void EncodeToFile(string path)
        {
            using (var stream = File.OpenWrite(path))
            {
                EncodeToStream(stream);
            }
        }

        public static bool operator ==(BObject first, BObject second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(BObject first, BObject second)
        {
            return !(first == second);
        }

        public override bool Equals(object other)
        {
            var obj = other as BObject;
            if (obj == null)
                return false;

            using (var ms1 = EncodeToStream(new MemoryStream()))
            using (var ms2 = obj.EncodeToStream(new MemoryStream()))
            {
                var bytes1 = ms1.ToArray();
                var bytes2 = ms2.ToArray();

                return bytes1.SequenceEqual(bytes2);
            }
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class BObject<TY> : BObject
    {
        internal BObject()
        { }

        /// <summary>
        /// The underlying value of the BObject.
        /// </summary>
        public TY Value { get; protected set; }
    }
}
