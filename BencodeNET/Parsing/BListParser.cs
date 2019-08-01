using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A parser for bencoded lists.
    /// </summary>
    public class BListParser : BObjectParser<BList>
    {
        /// <summary>
        /// The minimum stream length in bytes for a valid list ('le').
        /// </summary>
        protected const int MinimumLength = 2;

        /// <summary>
        /// Creates an instance using the specified <see cref="IBencodeParser"/> for parsing contained objects.
        /// </summary>
        /// <param name="bencodeParser">The parser used for parsing contained objects.</param>
        public BListParser(IBencodeParser bencodeParser)
        {
            BencodeParser = bencodeParser ?? throw new ArgumentNullException(nameof(bencodeParser));
        }

        /// <summary>
        /// The parser used for parsing contained objects.
        /// </summary>
        protected IBencodeParser BencodeParser { get; set; }

        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        public override Encoding Encoding => BencodeParser.Encoding;

        /// <summary>
        /// Parses the next <see cref="BList"/> from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <returns>The parsed <see cref="BList"/>.</returns>
        /// <exception cref="InvalidBencodeException{BList}">Invalid bencode.</exception>
        public override BList Parse(BencodeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (reader.Length < MinimumLength)
                throw InvalidBencodeException<BList>.BelowMinimumLength(MinimumLength, reader.Length.Value, reader.Position);

            // Lists must start with 'l'
            if (reader.ReadChar() != 'l')
                throw InvalidBencodeException<BList>.UnexpectedChar('l', reader.ReadPreviousChar(), reader.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (reader.PeekChar() != 'e' && reader.PeekChar() != null)
            {
                // Decode next object in stream
                var bObject = BencodeParser.Parse(reader);
                list.Add(bObject);
            }

            if (reader.ReadChar() != 'e')
                 throw InvalidBencodeException<BList>.MissingEndChar();

            return list;
        }
    }
}
