using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Torrents;

namespace BencodeNET.Parsing
{
    public class BencodeParser : IBencodeParser
    {
        public BencodeParser()
            : this(Encoding.UTF8)
        { }

        public BencodeParser(Encoding encoding)
        {
            Encoding = encoding;

            var listParser = new ListParser(this);
            Parsers = new Dictionary<Type, IBObjectParser>
            {
                [typeof (BString)] = new StringParser(this),
                [typeof (BNumber)] = new NumberParser(),
                [typeof (BList)] = listParser,
                [typeof (BList<BString>)] = listParser,
                [typeof (BList<BNumber>)] = listParser,
                [typeof (BList<BList>)] = listParser,
                [typeof (BList<BDictionary>)] = listParser,
                [typeof(BDictionary)] = new DictionaryParser(this),
                [typeof(Torrent)] = new TorrentParser(this),
            };
        }

        public Encoding Encoding { get; set; }

        public IDictionary<Type, IBObjectParser> Parsers { get; }

        public IBObject Parse(string bencodedString)
        {
            using (var stream = bencodedString.AsStream(Encoding))
            {
                return Parse(stream);
            }
        }

        public IBObject Parse(Stream stream)
        {
            return Parse(new BencodeStream(stream));
        }

        public IBObject Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

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
                case '9': return Parse<BString>(stream);
                case 'i': return Parse<BNumber>(stream);
                case 'l': return Parse<BList>(stream);
                case 'd': return Parse<BDictionary>(stream);
            }

            throw new BencodeParsingException("Invalid first character - valid characters are: 0-9, 'i', 'l' and 'd'", stream.Position);
        }

        public IBObject ParseFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        public Task<IBObject> ParseAsync(Stream stream)
        {
            return ParseAsync(new BencodeStream(stream));
        }

        public Task<IBObject> ParseAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

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
                case '9': return ParseAsync<BString>(stream).FromDerived<IBObject, BString>();
                case 'i': return ParseAsync<BNumber>(stream).FromDerived<IBObject, BNumber>();
                case 'l': return ParseAsync<BList>(stream).FromDerived<IBObject, BList>();
                case 'd': return ParseAsync<BDictionary>(stream).FromDerived<IBObject, BDictionary>();
            }

            throw new BencodeParsingException("Invalid first character - valid characters are: 0-9, 'i', 'l' and 'd'", stream.Position);
        }

        public Task<IBObject> ParseFromFileAsync(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return ParseAsync(stream);
            }
        }

        public T Parse<T>(string bencodedString) where T : class, IBObject
        {
            using (var stream = bencodedString.AsStream(Encoding))
            {
                return Parse<T>(stream);
            }
        }

        public T Parse<T>(Stream stream) where T : class, IBObject
        {
            return Parse<T>(new BencodeStream(stream));
        }

        public T Parse<T>(BencodeStream stream) where T : class, IBObject
        {
            var parser = Parsers.GetValueOrDefault(typeof (T));

            if (parser == null)
                throw new BencodeParsingException($"Missing parser for the type '{typeof(T).FullName}'");

            return parser.Parse(stream) as T;
        }

        public T ParseFromFile<T>(string path) where T : class, IBObject
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse<T>(stream);
            }
        }

        public Task<T> ParseAsync<T>(Stream stream) where T : class, IBObject
        {
            return ParseAsync<T>(new BencodeStream(stream));
        }

        public async Task<T> ParseAsync<T>(BencodeStream stream) where T : class, IBObject
        {
            var parser = Parsers.GetValueOrDefault(typeof(T));

            if (parser == null)
                throw new BencodeParsingException($"Missing parser for the type '{typeof(T).FullName}'");

            return await parser.ParseAsync(stream) as T;
        }

        public Task<T> ParseFromFileAsync<T>(string path) where T : class, IBObject
        {
            using (var stream = File.OpenRead(path))
            {
                return ParseAsync<T>(stream);
            }
        }
    }
}
