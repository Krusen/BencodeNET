using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class BDictionaryParser : BObjectParser<BDictionary>
    {
        protected const int MinimumLength = 2;

        public BDictionaryParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        protected IBencodeParser BencodeParser { get; set; }

        protected override Encoding Encoding => BencodeParser.Encoding;

        public override BDictionary Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var startPosition = stream.Position;

            if (stream.Length < MinimumLength)
                throw InvalidBencodeException<BDictionary>.BelowMinimumLength(MinimumLength, stream.Length, startPosition);

            // Dictionaries must start with 'd'
            if (stream.ReadChar() != 'd')
                throw InvalidBencodeException<BDictionary>.UnexpectedChar('d', stream.ReadPreviousChar(), startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // TODO: try/catch this and throw more telling exception message?
                // Decode next string in stream as the key
                var key = BencodeParser.Parse<BString>(stream);
                // TODO: try/catch exception and tell that we need a value for each key?
                // Decode next object in stream as the value
                var value = BencodeParser.Parse(stream);

                dictionary.Add(key, value);
            }

            if (stream.ReadChar() != 'e')
                throw InvalidBencodeException<BDictionary>.InvalidEndChar(stream.ReadPreviousChar(), stream.Position);

            return dictionary;
        }
    }
}
