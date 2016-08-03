using System;
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
        private Encoding _encoding;

        public BencodeParser()
            : this(Encoding.UTF8)
        { }

        public BencodeParser(Encoding encoding)
        {
            Encoding = encoding;

            StringParser = new StringParser(encoding);
            NumberParser = new NumberParser();
            // TODO: Just use parser.Encoding inside contructor instead of passing encoding variable?
            ListParser = new ListParser(this, encoding);
            DictionaryParser = new DictionaryParser(this, encoding);
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                _encoding = value;

                StringParser.Encoding = value;
                ListParser.Encoding = value;
                DictionaryParser.Encoding = value;
            }
        }

        private StringParser StringParser { get; set; }

        private NumberParser NumberParser { get; set; }

        private ListParser ListParser { get; set; }

        private DictionaryParser DictionaryParser { get; set; }

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

        // TODO: Maybe leave this out, as we have BList.As<T>()
        public BList<T> ParseList<T>(BencodeStream stream) where T : IBObject
        {
            return ListParser.Parse(stream).As<T>();
        }

        public BDictionary ParseDictionary(BencodeStream stream)
        {
            return DictionaryParser.Parse(stream);
        }

        // TODO: Make torrent parser?
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
