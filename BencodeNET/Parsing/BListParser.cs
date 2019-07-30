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
        protected override Encoding Encoding => BencodeParser.Encoding;

        /// <summary>
        /// Parses the next <see cref="BList"/> from the stream.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BList"/>.</returns>
        /// <exception cref="InvalidBencodeException{BList}">Invalid bencode</exception>
        public override BList Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw InvalidBencodeException<BList>.BelowMinimumLength(MinimumLength, stream.Length, stream.Position);

            // Lists must start with 'l'
            if (stream.ReadChar() != 'l')
                throw InvalidBencodeException<BList>.UnexpectedChar('l', stream.ReadPreviousChar(), stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // Decode next object in stream
                var bObject = BencodeParser.Parse(stream);
                list.Add(bObject);
            }

            if (stream.ReadChar() != 'e')
            {
                if (stream.EndOfStream) throw InvalidBencodeException<BList>.MissingEndChar();
                throw InvalidBencodeException<BList>.InvalidEndChar(stream.ReadPreviousChar(), stream.Position);
            }

            return list;
        }
    }
}
