using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A parser for bencoded dictionaries.
    /// </summary>
    public class BDictionaryParser : BObjectParser<BDictionary>
    {
        /// <summary>
        /// The minimum stream length in bytes for a valid dictionary ('de').
        /// </summary>
        protected const int MinimumLength = 2;

        /// <summary>
        /// Creates an instance using the specified <see cref="IBencodeParser"/> for parsing contained keys and values.
        /// </summary>
        /// <param name="bencodeParser">The parser used for contained keys and values.</param>
        public BDictionaryParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        /// <summary>
        /// The parser used for parsing contained keys and values.
        /// </summary>
        protected IBencodeParser BencodeParser { get; set; }

        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        protected override Encoding Encoding => BencodeParser.Encoding;

        /// <summary>
        /// Parses the next <see cref="BDictionary"/> from the stream and its contained keys and values.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BDictionary"/>.</returns>
        /// <exception cref="InvalidBencodeException{BDictionary}">Invalid bencode</exception>
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
