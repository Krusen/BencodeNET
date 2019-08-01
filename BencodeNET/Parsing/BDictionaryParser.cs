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
            BencodeParser = bencodeParser ?? throw new ArgumentNullException(nameof(bencodeParser));
        }

        /// <summary>
        /// The parser used for parsing contained keys and values.
        /// </summary>
        protected IBencodeParser BencodeParser { get; set; }

        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        public override Encoding Encoding => BencodeParser.Encoding;

        /// <summary>
        /// Parses the next <see cref="BDictionary"/> and its contained keys and values from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <returns>The parsed <see cref="BDictionary"/>.</returns>
        /// <exception cref="InvalidBencodeException{BDictionary}">Invalid bencode.</exception>
        public override BDictionary Parse(BencodeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (reader.Length < MinimumLength)
                throw InvalidBencodeException<BDictionary>.BelowMinimumLength(MinimumLength, reader.Length.Value, reader.Position);

            var startPosition = reader.Position;

            // Dictionaries must start with 'd'
            if (reader.ReadChar() != 'd')
                throw InvalidBencodeException<BDictionary>.UnexpectedChar('d', reader.PreviousChar, startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (reader.PeekChar() != 'e' && reader.PeekChar() != null)
            {
                BString key;
                try
                {
                    // Decode next string in stream as the key
                    key = BencodeParser.Parse<BString>(reader);
                }
                catch (BencodeException ex)
                {
                    throw InvalidException("Could not parse dictionary key. Keys must be strings.", ex, startPosition);
                }

                IBObject value;
                try
                {
                    // Decode next object in stream as the value
                    value = BencodeParser.Parse(reader);
                }
                catch (BencodeException ex)
                {
                    throw InvalidException(
                        $"Could not parse dictionary value for the key '{key}'. There needs to be a value for each key.",
                        ex, startPosition);
                }

                if (dictionary.ContainsKey(key))
                {
                    throw InvalidException(
                        $"The dictionary already contains the key '{key}'. Duplicate keys are not supported.", startPosition);
                }

                dictionary.Add(key, value);
            }

            if (reader.ReadChar() != 'e')
                throw InvalidBencodeException<BDictionary>.MissingEndChar(startPosition);

            return dictionary;
        }

        private static InvalidBencodeException<BDictionary> InvalidException(string message, long startPosition)
        {
            return new InvalidBencodeException<BDictionary>(
                $"{message} The dictionary starts at position {startPosition}.", startPosition);
        }

        private static InvalidBencodeException<BDictionary> InvalidException(string message, Exception inner, long startPosition)
        {
            return new InvalidBencodeException<BDictionary>(
                $"{message} The dictionary starts at position {startPosition}.",
                inner, startPosition);
        }
    }
}
