using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// Main class used for parsing bencode.
    /// </summary>
    public class BencodeParser : IBencodeParser
    {
        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> and the default parsers.
        /// </summary>
        public BencodeParser()
            : this(Encoding.UTF8)
        { }

        /// <summary>
        /// Creates an instance using the specified encoding and the default parsers.
        /// </summary>
        /// <param name="encoding">The encoding to use when parsing.</param>
        public BencodeParser(Encoding encoding)
        {
            Encoding = encoding;

            Parsers = new BObjectParserList
            {
                new BStringParser(encoding),
                new BNumberParser(),
                new BListParser(this),
                new BDictionaryParser(this),
                new TorrentParser(this)
            };
        }

        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> and the default parsers plus the specified parsers.
        /// Existing default parsers for the same type will be replaced by the new parsers.
        /// </summary>
        /// <param name="parsers">The new parsers to add or replace.</param>
        public BencodeParser(IEnumerable<KeyValuePair<Type, IBObjectParser>> parsers)
            : this(parsers, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> and the default parsers plus the specified parsers.
        /// Existing default parsers for the same type will be replaced by the new parsers.
        /// </summary>
        /// <param name="parsers">The new parsers to add or replace.</param>
        public BencodeParser(IDictionary<Type, IBObjectParser> parsers)
            : this(parsers, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates an instance using the specified encoding and the default parsers plus the specified parsers.
        /// Existing default parsers for the same type will be replaced by the new parsers.
        /// </summary>
        /// <param name="parsers">The new parsers to add or replace.</param>
        /// <param name="encoding">The encoding to use when parsing.</param>
        public BencodeParser(IEnumerable<KeyValuePair<Type, IBObjectParser>> parsers, Encoding encoding)
        {
            Encoding = encoding;

            foreach (var entry in parsers)
            {
                Parsers.AddOrReplace(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Creates an instance using the specified encoding and the default parsers plus the specified parsers.
        /// Existing default parsers for the same type will be replaced by the new parsers.
        /// </summary>
        /// <param name="parsers">The new parsers to add or replace.</param>
        /// <param name="encoding">The encoding to use when parsing.</param>
        public BencodeParser(IDictionary<Type, IBObjectParser> parsers, Encoding encoding)
            : this((IEnumerable<KeyValuePair<Type, IBObjectParser>>)parsers, encoding)
        { }

        /// <summary>
        /// The encoding use for parsing.
        /// </summary>
        public Encoding Encoding { get; protected set; }

        /// <summary>
        /// The parsers used by this instance when parsing bencoded.
        /// </summary>
        public BObjectParserList Parsers { get; }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public IBObject Parse(Stream stream)
        {
            using (var reader = new BencodeReader(stream, leaveOpen: true))
            {
                return Parse(reader);
            }
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public T Parse<T>(Stream stream) where T : class, IBObject
        {
            using (var reader = new BencodeReader(stream, leaveOpen: true))
            {
                return Parse<T>(reader);
            }
        }

        /// <summary>
        ///  Parses an <see cref="IBObject"/> from the reader.
        /// </summary>
        /// <param name="reader"></param>
        public IBObject Parse(BencodeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            switch (reader.PeekChar())
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
                case '9': return Parse<BString>(reader);
                case 'i': return Parse<BNumber>(reader);
                case 'l': return Parse<BList>(reader);
                case 'd': return Parse<BDictionary>(reader);
                case null: return null;
            }

            throw InvalidBencodeException<IBObject>.InvalidBeginningChar(reader.PeekChar(), reader.Position);
        }

        /// <summary>
        /// Parse an <see cref="IBObject"/> of type <typeparamref name="T"/> from the reader.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="reader"></param>
        public T Parse<T>(BencodeReader reader) where T : class, IBObject
        {
            var parser = Parsers.Get<T>();

            if (parser == null)
                throw new BencodeException($"Missing parser for the type '{typeof(T).FullName}'. Stream position: {reader.Position}");

            return parser.Parse(reader);
        }
    }
}
