using System;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class ListParser : BObjectParser<BList>
    {
        protected const int MinimumLength = 2;

        public ListParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        protected IBencodeParser BencodeParser { get; set; }

        public override BList Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BList>($"Minimum valid length is {MinimumLength} (an empty list: 'le')", stream.Position);

            // Lists must start with 'l'
            if (stream.ReadChar() != 'l')
                throw new BencodeParsingException<BList>(
                    $"Must begin with 'l' but began with '{stream.ReadPreviousChar()}'.", stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // Decode next object in stream
                var bObject = BencodeParser.Parse(stream);
                if (bObject == null)
                    throw new BencodeParsingException<BList>($"Invalid object beginning with '{stream.PeekChar()}'", stream.Position);

                list.Add(bObject);
            }

            if (stream.ReadChar() != 'e')
                throw new BencodeParsingException<BList>("Missing end character 'e'.", stream.Position);

            return list;
        }

        public override async Task<BList> ParseAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BList>($"Minimum valid length is {MinimumLength} (an empty list: 'le')", stream.Position);

            // Lists must start with 'l'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'l')
                throw new BencodeParsingException<BList>(
                    $"Must begin with 'l' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'.", stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (await stream.PeekAsync().ConfigureAwait(false) != 'e' && await stream.PeekAsync().ConfigureAwait(false) != -1)
            {
                // Decode next object in stream
                var bObject = await BencodeParser.ParseAsync(stream).ConfigureAwait(false);
                if (bObject == null)
                    throw new BencodeParsingException<BList>($"Invalid object beginning with '{await stream.PeekCharAsync().ConfigureAwait(false)}'", stream.Position);

                list.Add(bObject);
            }

            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'e')
                throw new BencodeParsingException<BList>("Missing end character 'e'.", stream.Position);

            return list;
        }

        public BList<T> Parse<T>(BencodeStream stream) where T : IBObject
        {
            return Parse(stream).As<T>();
        }

        public async Task<BList<T>> ParseAsync<T>(BencodeStream stream) where T : IBObject
        {
            return (await ParseAsync(stream)).As<T>();
        }
    }
}
