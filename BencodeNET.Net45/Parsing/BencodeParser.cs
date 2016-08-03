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

        protected StringParser StringParser => Parsers[typeof (BString)] as StringParser;

        protected NumberParser NumberParser => Parsers[typeof(BNumber)] as NumberParser;

        protected ListParser ListParser => Parsers[typeof(BList)] as ListParser;

        protected DictionaryParser DictionaryParser => Parsers[typeof(BDictionary)] as DictionaryParser;

        private TorrentParser TorrentParser => Parsers[typeof(Torrent)] as TorrentParser;

        public IBObject Parse(string bencodedString)
        {
            using (var ms = new MemoryStream(Encoding.GetBytes(bencodedString)))
            {
                return Parse(ms);
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
                case '9': return StringParser.Parse(stream);
                case 'i': return NumberParser.Parse(stream);
                case 'l': return ListParser.Parse(stream);
                case 'd': return DictionaryParser.Parse(stream);
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

        public T Parse<T>(BencodeStream stream) where T : class, IBObject
        {
            if (typeof (BString) == typeof (T))
                return StringParser.Parse(stream) as T;

            if (typeof (BNumber) == typeof (T))
                return NumberParser.Parse(stream) as T;

            if (typeof (BList) == typeof (T))
                return ListParser.Parse(stream) as T;

            if (typeof (BList<BString>) == typeof (T))
                return ListParser.Parse<BString>(stream) as T;

            if (typeof (BList<BNumber>) == typeof (T))
                return ListParser.Parse<BNumber>(stream) as T;

            if (typeof (BList<BList>) == typeof (T))
                return ListParser.Parse<BList>(stream) as T;

            if (typeof (BList<BDictionary>) == typeof (T))
                return ListParser.Parse<BDictionary>(stream) as T;

            if (typeof (BDictionary) == typeof (T))
                return DictionaryParser.Parse(stream) as T;

            if (typeof (Torrent) == typeof (T))
                return TorrentParser.Parse(stream) as T;

            throw new BencodeParsingException($"Missing parser for the type '{typeof (T).FullName}'");
        }

        public Task<T> ParseAsync<T>(string bencodedString) where T : class, IBObject
        {
            using (var ms = new MemoryStream(Encoding.GetBytes(bencodedString)))
            {
                return ParseAsync<T>(ms);
            }
        }

        public Task<T> ParseAsync<T>(Stream stream) where T : class, IBObject
        {
            return ParseAsync<T>(new BencodeStream(stream));
        }

        public async Task<T> ParseAsync<T>(BencodeStream stream) where T : class, IBObject
        {
            if (typeof (BString) == typeof (T))
                return await StringParser.ParseAsync(stream) as T;

            if (typeof (BNumber) == typeof (T))
                return await NumberParser.ParseAsync(stream) as T;

            if (typeof (BList) == typeof (T))
                return await ListParser.ParseAsync(stream) as T;

            if (typeof (BList<BString>) == typeof (T))
                return await ListParser.ParseAsync<BString>(stream) as T;

            if (typeof (BList<BNumber>) == typeof (T))
                return await ListParser.ParseAsync<BNumber>(stream) as T;

            if (typeof (BList<BList>) == typeof (T))
                return await ListParser.ParseAsync<BList>(stream) as T;

            if (typeof (BList<BDictionary>) == typeof (T))
                return await ListParser.ParseAsync<BDictionary>(stream) as T;

            if (typeof (BDictionary) == typeof (T))
                return await DictionaryParser.ParseAsync(stream) as T;

            if (typeof (Torrent) == typeof (T))
                return await TorrentParser.ParseAsync(stream) as T;

            throw new BencodeParsingException($"Missing parser for the type '{typeof (T).FullName}'");
        }

        public Task<T> ParseFromFileAsync<T>(string path) where T : class, IBObject
        {
            using (var stream = File.OpenRead(path))
            {
                return ParseAsync<T>(stream);
            }
        }

        public BString ParseString(BencodeStream stream)
        {
            return StringParser.Parse(stream);
        }

        public BNumber ParseNumber(BencodeStream stream)
        {
            return NumberParser.Parse(stream);
        }

        public BList ParseList(BencodeStream stream)
        {
            return ListParser.Parse(stream);
        }

        public BList<T> ParseList<T>(BencodeStream stream) where T : IBObject
        {
            return ListParser.Parse(stream).As<T>();
        }

        public BDictionary ParseDictionary(BencodeStream stream)
        {
            return DictionaryParser.Parse(stream);
        }

        public Torrent ParseTorrent(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Torrent ParseTorrent(BencodeStream stream)
        {
            throw new NotImplementedException();
        }

        public Torrent ParseTorrentFromFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IBObject> ParseAsync(string bencodedString)
        {
            using (var ms = new MemoryStream(Encoding.GetBytes(bencodedString)))
            {
                return ParseAsync(ms);
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
                case '9': return StringParser.ParseAsync(stream).FromDerived<IBObject, BString>();
                case 'i': return NumberParser.ParseAsync(stream).FromDerived<IBObject, BNumber>();
                case 'l': return ListParser.ParseAsync(stream).FromDerived<IBObject, BList>();
                case 'd': return DictionaryParser.ParseAsync(stream).FromDerived<IBObject, BDictionary>();
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

        public Task<BString> ParseStringAsync(BencodeStream stream)
        {
            return StringParser.ParseAsync(stream);
        }

        public Task<BNumber> ParseNumberAsync(BencodeStream stream)
        {
            return NumberParser.ParseAsync(stream);
        }

        public Task<BList> ParseListAsync(BencodeStream stream)
        {
            return ListParser.ParseAsync(stream);
        }

        public async Task<BList<T>> ParseListAsync<T>(BencodeStream stream) where T : IBObject
        {
            return (await ListParser.ParseAsync(stream)).As<T>();
        }

        public Task<BDictionary> ParseDictionaryAsync(BencodeStream stream)
        {
            return DictionaryParser.ParseAsync(stream);
        }

        public Task<Torrent> ParseTorrentAsync(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<Torrent> ParseTorrentAsync(BencodeStream stream)
        {
            throw new NotImplementedException();
        }

        public Task<Torrent> ParseTorrentFromFileAsync(string path)
        {
            throw new NotImplementedException();
        }
    }
}
