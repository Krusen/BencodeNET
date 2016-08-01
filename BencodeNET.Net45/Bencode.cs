using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Torrents;

namespace BencodeNET
{
    public static class Bencode
    {
        private const int Int64MaxDigits = 19;

        // TODO: Move to relevant classes instead?
        private const int BStringMinLength = 2;
        private const int BNumberMinLength = 3;
        private const int BListMinLength = 2;
        private const int BDictionaryMinLength = 2;

        private static Encoding _defaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the default encoding used to convert strings to and from bytes
        /// when encoding/decoding bencode and no encoding is explicitly specified.
        /// </summary>
        public static Encoding DefaultEncoding
        {
            get { return _defaultEncoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "DefaultEncoding may not be set to null");
                _defaultEncoding = value;
            }
        }

        public static IBObject DecodeFromFile(string path)
        {
            return DecodeFromFile(path, DefaultEncoding);
        }

        public static IBObject DecodeFromFile(string path, Encoding encoding)
        {
            using (var stream = File.OpenRead(path))
            {
                return Decode(stream, encoding);
            }
        }

        /// <summary>
        /// Decodes the specified bencoded string using the default encoding.
        /// </summary>
        /// <param name="bencodedString">The bencoded string.</param>
        /// <returns>An <see cref="IBObject"/> representing the bencoded string.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IBObject Decode(string bencodedString)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));

            return Decode(bencodedString, DefaultEncoding);
        }

        /// <summary>
        /// Decodes the specified bencoded string using the specified encoding.
        /// </summary>
        /// <param name="bencodedString">The bencoded string.</param>
        /// <param name="encoding">The encoding used to convert the string to bytes.</param>
        /// <returns>An <see cref="IBObject"/> representing the bencoded string.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IBObject Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        /// <summary>
        /// Decodes the specified stream using the default encoding.
        /// </summary>
        /// <param name="stream">The stream to decode.</param>
        /// <returns>An <see cref="IBObject"/> representing the bencoded stream.</returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public static IBObject Decode(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return Decode(stream, DefaultEncoding);
        }

        /// <summary>
        /// Decodes the specified stream using the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to decode.</param>
        /// <param name="encoding">The encoding used by <see cref="BString"/> when calling <c>ToString()</c> with no arguments.</param>
        /// <returns>An <see cref="IBObject"/> representing the bencoded stream.</returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public static IBObject Decode(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return Decode(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        /// <summary>
        /// Decodes the specified stream using the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to decode.</param>
        /// <param name="encoding">The encoding used by <see cref="BString"/> when calling <c>ToString()</c> with no arguments.</param>
        /// <returns>An <see cref="IBObject"/> representing the bencoded stream.</returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public static IBObject Decode(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            switch (stream.PeekChar())
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return DecodeString(stream, encoding);
                case 'i': return DecodeNumber(stream);
                case 'l': return DecodeList(stream, encoding);
                case 'd': return DecodeDictionary(stream, encoding);
            }

            // TODO: Throw BencodeDecodingException because next char was not a valid start of a BObject?
            return null;
        }

        public static Task<IBObject> DecodeFromFileAsync(string path)
        {
            return DecodeFromFileAsync(path, DefaultEncoding);
        }

        public static Task<IBObject> DecodeFromFileAsync(string path, Encoding encoding)
        {
            using (var stream = File.OpenRead(path))
            {
                return DecodeAsync(stream, encoding);
            }
        }

        public static Task<IBObject> DecodeAsync(string bencodedString)
        {
            return DecodeAsync(bencodedString, DefaultEncoding);
        }

        public static Task<IBObject> DecodeAsync(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return DecodeAsync(ms, encoding);
            }
        }

        public static Task<IBObject> DecodeAsync(Stream stream)
        {
            return DecodeAsync(stream, DefaultEncoding);
        }

        public static Task<IBObject> DecodeAsync(Stream stream, Encoding encoding)
        {
            return DecodeAsync(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static async Task<IBObject> DecodeAsync(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            switch (await stream.PeekCharAsync().ConfigureAwait(false))
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return await DecodeStringAsync(stream, encoding).ConfigureAwait(false);
                case 'i': return await DecodeNumberAsync(stream).ConfigureAwait(false);
                case 'l': return await DecodeListAsync(stream, encoding).ConfigureAwait(false);
                case 'd': return await DecodeDictionaryAsync(stream, encoding).ConfigureAwait(false);
            }

            // TODO: Throw BencodeDecodingException because next char was not a valid start of a BObject?
            return null;
        }

        public static BString DecodeString(string bencodedString)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));

            return DecodeString(bencodedString, DefaultEncoding);
        }

        public static BString DecodeString(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return DecodeString(ms, encoding);
            }
        }

        public static BString DecodeString(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return DecodeString(stream, DefaultEncoding);
        }

        public static BString DecodeString(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return DecodeString(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static BString DecodeString(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < BStringMinLength)
                throw new BencodeDecodingException<BString>("Minimum valid stream length is 2 (an empty string: '0:')", stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = stream.ReadChar(); c != ':' && c != default(char); c = stream.ReadChar())
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits)
                {
                    throw new UnsupportedBencodeException(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                        stream.Position);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!TryParseLongFast(lengthString.ToString(), out stringLength))
            {
                throw new BencodeDecodingException<BString>($"Invalid length of string '{lengthString}'", startPosition);
            }

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw new UnsupportedBencodeException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    stream.Position);
            }

            var bytes = stream.Read((int)stringLength);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength)
            {
                throw new BencodeDecodingException<BString>(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes.",
                    stream.Position);
            }

            return new BString(bytes, encoding);
        }

        public static Task<BString> DecodeStringAsync(Stream stream)
        {
            return DecodeStringAsync(stream, DefaultEncoding);
        }

        public static Task<BString> DecodeStringAsync(Stream stream, Encoding encoding)
        {
            return DecodeStringAsync(new BencodeStream(stream, leaveOpen:true), encoding);
        }

        public static async Task<BString> DecodeStringAsync(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < BStringMinLength)
                throw new BencodeDecodingException<BString>("Minimum valid stream length is 2 (an empty string: '0:')", stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = await stream.ReadCharAsync().ConfigureAwait(false);
                c != ':' && c != default(char);
                c = await stream.ReadCharAsync().ConfigureAwait(false))
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits)
                {
                    throw new UnsupportedBencodeException(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                        stream.Position);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!TryParseLongFast(lengthString.ToString(), out stringLength))
            {
                throw new BencodeDecodingException<BString>($"Invalid length of string '{lengthString}'", startPosition);
            }

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw new UnsupportedBencodeException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    stream.Position);
            }

            var bytes = await stream.ReadAsync((int)stringLength).ConfigureAwait(false);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength)
            {
                throw new BencodeDecodingException<BString>(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes.",
                    stream.Position);
            }

            return new BString(bytes, encoding);
        }

        public static BNumber DecodeNumber(string bencodedString)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));

            using (var ms = new MemoryStream(DefaultEncoding.GetBytes(bencodedString)))
            {
                return DecodeNumber(ms);
            }
        }

        public static BNumber DecodeNumber(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return DecodeNumber(new BencodeStream(stream, leaveOpen: true));
        }

        public static BNumber DecodeNumber(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < BNumberMinLength)
                throw new BencodeDecodingException<BNumber>("Minimum valid length of stream is 3 ('i0e').", stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            if (stream.ReadChar() != 'i')
                throw new BencodeDecodingException<BNumber>(
                    $"Must begin with 'i' but began with '{stream.ReadPreviousChar()}'.", stream.Position);

            var digits = new StringBuilder();
            char c;
            for (c = stream.ReadChar(); c != 'e' && c != default(char); c = stream.ReadChar())
            {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e')
                throw new BencodeDecodingException<BNumber>("Missing end character 'e'.", stream.Position);

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw new UnsupportedBencodeException(
                    $"The number '{digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    stream.Position);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw new BencodeDecodingException<BNumber>("It contains no digits.", startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw new BencodeDecodingException<BNumber>("Leading '0's are not valid.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw new BencodeDecodingException<BNumber>("'-0' is not a valid number.", startPosition);

            long number;
            if (!TryParseLongFast(digits.ToString(), out number))
            {
                throw new BencodeDecodingException<BNumber>(
                    $"The value '{digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
                    stream.Position);
            }

            return new BNumber(number);
        }

        public static Task<BNumber> DecodeNumberAsync(Stream stream)
        {
            return DecodeNumberAsync(new BencodeStream(stream, leaveOpen: true));
        }

        public static async Task<BNumber> DecodeNumberAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < BNumberMinLength)
                throw new BencodeDecodingException<BNumber>("Minimum valid length of stream is 3 ('i0e').", stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'i')
                throw new BencodeDecodingException<BNumber>(
                    $"Must begin with 'i' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'.", stream.Position);

            var digits = new StringBuilder();
            char c;
            for (c = await stream.ReadCharAsync().ConfigureAwait(false);
                c != 'e' && c != default(char);
                c = await stream.ReadCharAsync().ConfigureAwait(false))
            {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e')
                throw new BencodeDecodingException<BNumber>("Missing end character 'e'.", stream.Position);

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw new UnsupportedBencodeException(
                    $"The number '{digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    stream.Position);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw new BencodeDecodingException<BNumber>("It contains no digits.", startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw new BencodeDecodingException<BNumber>("Leading '0's are not valid.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw new BencodeDecodingException<BNumber>("'-0' is not a valid number.", startPosition);

            long number;
            if (!TryParseLongFast(digits.ToString(), out number))
            {
                throw new BencodeDecodingException<BNumber>(
                    $"The value '{digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
                    stream.Position);
            }

            return new BNumber(number);
        }

        public static BList DecodeList(string bencodedString)
        {
            return DecodeList(bencodedString, DefaultEncoding);
        }

        public static BList DecodeList(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return DecodeList(ms, encoding);
            }
        }

        public static BList DecodeList(Stream stream)
        {
            return DecodeList(stream, DefaultEncoding);
        }

        public static BList DecodeList(Stream stream, Encoding encoding)
        {
            return DecodeList(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static BList DecodeList(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            if (stream.Length < BListMinLength)
                throw new BencodeDecodingException<BList>("Minimum valid length is 2 (an empty list: 'le')", stream.Position);

            // Lists must start with 'l'
            if (stream.ReadChar() != 'l')
                throw new BencodeDecodingException<BList>(
                    $"Must begin with 'l' but began with '{stream.ReadPreviousChar()}'.", stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // Decode next object in stream
                var bObject = Decode(stream, encoding);
                if (bObject == null)
                    throw new BencodeDecodingException<BList>($"Invalid object beginning with '{stream.PeekChar()}'", stream.Position);

                list.Add(bObject);
            }

            if (stream.ReadChar() != 'e')
                throw new BencodeDecodingException<BList>("Missing end character 'e'.", stream.Position);

            return list;
        }

        public static Task<BList> DecodeListAsync(Stream stream)
        {
            return DecodeListAsync(stream, DefaultEncoding);
        }

        public static Task<BList> DecodeListAsync(Stream stream, Encoding encoding)
        {
            return DecodeListAsync(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static async Task<BList> DecodeListAsync(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            if (stream.Length < BListMinLength)
                throw new BencodeDecodingException<BList>("Minimum valid length is 2 (an empty list: 'le')", stream.Position);

            // Lists must start with 'l'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'l')
                throw new BencodeDecodingException<BList>(
                    $"Must begin with 'l' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'.", stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (await stream.PeekAsync().ConfigureAwait(false) != 'e' && await stream.PeekAsync().ConfigureAwait(false) != -1)
            {
                // Decode next object in stream
                var bObject = await DecodeAsync(stream, encoding).ConfigureAwait(false);
                if (bObject == null)
                    throw new BencodeDecodingException<BList>($"Invalid object beginning with '{await stream.PeekCharAsync().ConfigureAwait(false)}'", stream.Position);

                list.Add(bObject);
            }

            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'e')
                throw new BencodeDecodingException<BList>("Missing end character 'e'.", stream.Position);

            return list;
        }

        public static BDictionary DecodeDictionary(string bencodedString)
        {
            return DecodeDictionary(bencodedString, DefaultEncoding);
        }

        public static BDictionary DecodeDictionary(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException(nameof(bencodedString));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return DecodeDictionary(ms, encoding);
            }
        }

        public static BDictionary DecodeDictionary(Stream stream)
        {
            return DecodeDictionary(stream, DefaultEncoding);
        }

        public static BDictionary DecodeDictionary(Stream stream, Encoding encoding)
        {
            return DecodeDictionary(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static BDictionary DecodeDictionary(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            var startPosition = stream.Position;

            if (stream.Length < BDictionaryMinLength)
                throw new BencodeDecodingException<BDictionary>("Minimum valid length is 2 (an empty dictionary: 'de')", startPosition);

            // Dictionaries must start with 'd'
            if (stream.ReadChar() != 'd')
                throw new BencodeDecodingException<BDictionary>($"Must begin with 'd' but began with '{stream.ReadPreviousChar()}'", startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // Decode next string in stream as the key
                BString key;
                try
                {
                    key = DecodeString(stream, encoding);
                }
                catch (BencodeDecodingException<BString> ex)
                {
                    throw new BencodeDecodingException<BDictionary>("Dictionary keys must be strings.", stream.Position);
                }

                // Decode next object in stream as the value
                var value = Decode(stream, encoding);
                if (value == null)
                    throw new BencodeDecodingException<BDictionary>("All keys must have a corresponding value.", stream.Position);

                dictionary.Add(key, value);
            }

            if (stream.ReadChar() != 'e')
                throw new BencodeDecodingException<BDictionary>("Missing end character 'e'.", stream.Position);

            return dictionary;
        }

        public static Task<BDictionary> DecodeDictionaryAsync(Stream stream)
        {
            return DecodeDictionaryAsync(stream, DefaultEncoding);
        }

        public static Task<BDictionary> DecodeDictionaryAsync(Stream stream, Encoding encoding)
        {
            return DecodeDictionaryAsync(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static async Task<BDictionary> DecodeDictionaryAsync(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            var startPosition = stream.Position;

            if (stream.Length < BDictionaryMinLength)
                throw new BencodeDecodingException<BDictionary>("Minimum valid length is 2 (an empty dictionary: 'de')", startPosition);

            // Dictionaries must start with 'd'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'd')
                throw new BencodeDecodingException<BDictionary>($"Must begin with 'd' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'", startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (await stream.PeekAsync().ConfigureAwait(false) != 'e' && await stream.PeekAsync().ConfigureAwait(false) != -1)
            {
                // Decode next string in stream as the key
                BString key;
                try
                {
                    key = await DecodeStringAsync(stream, encoding).ConfigureAwait(false);
                }
                catch (BencodeDecodingException<BString> ex)
                {
                    throw new BencodeDecodingException<BDictionary>("Dictionary keys must be strings.", stream.Position);
                }

                // Decode next object in stream as the value
                var value = await DecodeAsync(stream, encoding).ConfigureAwait(false);
                if (value == null)
                    throw new BencodeDecodingException<BDictionary>("All keys must have a corresponding value.", stream.Position);

                dictionary.Add(key, value);
            }

            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'e')
                throw new BencodeDecodingException<BDictionary>("Missing end character 'e'.", stream.Position);

            return dictionary;
        }

        public static Torrent DecodeTorrent(string path)
        {
            return DecodeTorrent(path, DefaultEncoding);
        }

        public static Torrent DecodeTorrent(string path, Encoding encoding)
        {
            using (var stream = File.OpenRead(path))
            {
                return DecodeTorrent(stream, encoding);
            }
        }

        public static Torrent DecodeTorrent(Stream stream)
        {
            return DecodeTorrent(stream, DefaultEncoding);
        }

        public static Torrent DecodeTorrent(Stream stream, Encoding encoding)
        {
            return DecodeTorrent(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static Torrent DecodeTorrent(BencodeStream stream, Encoding encoding)
        {
            var bdictionary = DecodeDictionary(stream, encoding);
            return Torrent.FromBDictionary(bdictionary);
        }

        public static Task<Torrent> DecodeTorrentAsync(Stream stream)
        {
            return DecodeTorrentAsync(stream, DefaultEncoding);
        }

        public static Task<Torrent> DecodeTorrentAsync(Stream stream, Encoding encoding)
        {
            return DecodeTorrentAsync(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static async Task<Torrent> DecodeTorrentAsync(BencodeStream stream, Encoding encoding)
        {
            var bdictionary = await DecodeDictionaryAsync(stream, encoding);
            return Torrent.FromBDictionary(bdictionary);
        }

        [Obsolete("Use DecodeTorrent(string) instead. Will be removed in a future version.")]
        public static TorrentFile DecodeTorrentFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return DecodeTorrentFile(stream);
            }
        }

        [Obsolete("Use DecodeTorrent(Stream) instead. Will be removed in a future version.")]
        public static TorrentFile DecodeTorrentFile(Stream stream)
        {
            return DecodeTorrentFile(stream, DefaultEncoding);
        }

        [Obsolete("Use DecodeTorrent(Stream, Encoding) instead. Will be removed in a future version.")]
        public static TorrentFile DecodeTorrentFile(Stream stream, Encoding encoding)
        {
            return DecodeTorrentFile(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        [Obsolete("Use DecodeTorrent(BencodeStream, Encoding) instead. Will be removed in a future version.")]
        public static TorrentFile DecodeTorrentFile(BencodeStream stream, Encoding encoding)
        {
            var bdictionary = DecodeDictionary(stream, encoding);
            return new TorrentFile(bdictionary);
        }

        // TODO: Unit tests
        /// <summary>
        /// A faster implementation than <see cref="long.TryParse(string, out long)"/>
        /// because we skip some checks that are not needed.
        /// </summary>
        private static bool TryParseLongFast(string value, out long result)
        {
            result = 0;

            if (value == null)
                return false;

            var length = value.Length;

            // Cannot parse empty string
            if (length == 0)
                return false;

            var isNegative = value[0] == '-';
            var startIndex = isNegative ? 1 : 0;

            // Cannot parse just '-'
            if (isNegative && length == 1)
                return false;

            // Cannot parse string longer than long.MaxValue
            if (length - startIndex > Int64MaxDigits)
                return false;

            long parsedLong = 0;
            for (var i = startIndex; i < length; i++)
            {
                var character = value[i];
                if (!character.IsDigit())
                    return false;

                var digit = character - '0';

                if (isNegative)
                    parsedLong = 10 * parsedLong - digit;
                else
                    parsedLong = 10 * parsedLong + digit;
            }

            // Negative - should be less than zero
            if (isNegative && parsedLong >= 0)
                return false;

            // Positive - should be equal to or greater than zero
            if (!isNegative && parsedLong < 0)
                return false;

            result = parsedLong;
            return true;
        }
    }
}
