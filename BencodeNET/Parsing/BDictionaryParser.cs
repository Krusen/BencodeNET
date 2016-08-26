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
                BString key;
                try
                {
                    // Decode next string in stream as the key
                    key = BencodeParser.Parse<BString>(stream);
                }
                catch (BencodeException<BString> ex)
                {
                    throw InvalidException("Could not parse dictionary key. Keys must be strings.", ex, startPosition);
                }

                IBObject value;
                try
                {
                    // Decode next object in stream as the value
                    value = BencodeParser.Parse(stream);
                }
                catch (BencodeException ex)
                {
                    throw InvalidException(
                        "Could not parse dictionary value. There needs to be a value for each key.",
                        ex, startPosition);
                }

                dictionary.Add(key, value);
            }

            if (stream.ReadChar() != 'e')
            {
                if (stream.EndOfStream) throw InvalidBencodeException<BDictionary>.MissingEndChar();
                throw InvalidBencodeException<BDictionary>.InvalidEndChar(stream.ReadPreviousChar(), stream.Position);
            }

            return dictionary;
        }

        private static InvalidBencodeException<BDictionary> InvalidException(string message, Exception inner, long startPosition)
        {
            return new InvalidBencodeException<BDictionary>(
                $"{message} The dictionary starts at position {startPosition}.",
                inner, startPosition);
        }
    }
}
