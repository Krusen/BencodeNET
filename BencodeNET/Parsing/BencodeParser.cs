using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        protected BObjectParserList Parsers { get; }

        /// <summary>
        /// The encoding use for parsing.
        /// </summary>
        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value ?? throw new ArgumentNullException(nameof(value));
                Parsers.AddOrReplace(new BStringParser(value));
            }
        }
        private Encoding _encoding;

        /// <summary>
        /// Creates an instance using the specified encoding and the default parsers.
        /// Encoding defaults to <see cref="System.Text.Encoding.UTF8"/> if not specified.
        /// </summary>
        /// <param name="encoding">The encoding to use when parsing.</param>
        public BencodeParser(Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.UTF8;

            Parsers = new BObjectParserList
            {
                new BNumberParser(),
                new BStringParser(_encoding),
                new BListParser(this),
                new BDictionaryParser(this)
            };
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public virtual IBObject Parse(Stream stream)
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
        public virtual T Parse<T>(Stream stream) where T : class, IBObject
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
        public virtual IBObject Parse(BencodeReader reader)
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
                case default(char): return null;
            }

            throw InvalidBencodeException<IBObject>.InvalidBeginningChar(reader.PeekChar(), reader.Position);
        }

        /// <summary>
        /// Parse an <see cref="IBObject"/> of type <typeparamref name="T"/> from the reader.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="reader"></param>
        public virtual T Parse<T>(BencodeReader reader) where T : class, IBObject
        {
            var parser = Parsers.Get<T>();

            if (parser == null)
                throw new BencodeException($"Missing parser for the type '{typeof(T).FullName}'. Stream position: {reader.Position}");

            return parser.Parse(reader);
        }

        public virtual ValueTask<IBObject> ParseAsync(PipeReader pipeReader, CancellationToken cancellationToken = default)
        {
            var reader = new PipeBencodeReader(pipeReader);
            return ParseAsync(reader, cancellationToken);
        }

        public virtual ValueTask<T> ParseAsync<T>(PipeReader pipeReader, CancellationToken cancellationToken = default) where T : class, IBObject
        {
            var reader = new PipeBencodeReader(pipeReader);
            return ParseAsync<T>(reader, cancellationToken);
        }

        public virtual async ValueTask<IBObject> ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default)
        {
            if (pipeReader == null) throw new ArgumentNullException(nameof(pipeReader));

            switch (await pipeReader.PeekCharAsync(cancellationToken).ConfigureAwait(false))
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
                case '9': return await ParseAsync<BString>(pipeReader, cancellationToken).ConfigureAwait(false);
                case 'i': return await ParseAsync<BNumber>(pipeReader, cancellationToken).ConfigureAwait(false);
                case 'l': return await ParseAsync<BList>(pipeReader, cancellationToken).ConfigureAwait(false);
                case 'd': return await ParseAsync<BDictionary>(pipeReader, cancellationToken).ConfigureAwait(false);
                case default(char): return null;
            }

            throw InvalidBencodeException<IBObject>.InvalidBeginningChar(
                await pipeReader.PeekCharAsync(cancellationToken).ConfigureAwait(false),
                pipeReader.Position);
        }

        public virtual async ValueTask<T> ParseAsync<T>(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default) where T : class, IBObject
        {
            var parser = Parsers.Get<T>();

            if (parser == null)
                throw new BencodeException($"Missing parser for the type '{typeof(T).FullName}'. Stream position: {pipeReader.Position}");

            return await parser.ParseAsync(pipeReader, cancellationToken).ConfigureAwait(false);
        }
    }
}
