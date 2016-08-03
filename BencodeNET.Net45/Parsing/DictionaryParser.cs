using System;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class DictionaryParser : BObjectParser<BDictionary>
    {
        protected const int MinimumLength = 2;

        public DictionaryParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        protected IBencodeParser BencodeParser { get; set; }

        public override BDictionary Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var startPosition = stream.Position;

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BDictionary>($"Minimum valid length is {MinimumLength} (an empty dictionary: 'de')", startPosition);

            // Dictionaries must start with 'd'
            if (stream.ReadChar() != 'd')
                throw new BencodeParsingException<BDictionary>($"Must begin with 'd' but began with '{stream.ReadPreviousChar()}'", startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1)
            {
                // Decode next string in stream as the key
                BString key;
                try
                {
                    key = BencodeParser.ParseString(stream);
                }
                catch (BencodeParsingException<BString>)
                {
                    throw new BencodeParsingException<BDictionary>("Dictionary keys must be strings.", stream.Position);
                }

                // Decode next object in stream as the value
                var value = BencodeParser.Parse(stream);
                if (value == null)
                    throw new BencodeParsingException<BDictionary>("All keys must have a corresponding value.", stream.Position);

                dictionary.Add(key, value);
            }

            if (stream.ReadChar() != 'e')
                throw new BencodeParsingException<BDictionary>("Missing end character 'e'.", stream.Position);

            return dictionary;
        }

        public override async Task<BDictionary> ParseAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var startPosition = stream.Position;

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BDictionary>($"Minimum valid length is {MinimumLength} (an empty dictionary: 'de')", startPosition);

            // Dictionaries must start with 'd'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'd')
                throw new BencodeParsingException<BDictionary>($"Must begin with 'd' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'", startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (await stream.PeekAsync().ConfigureAwait(false) != 'e' && await stream.PeekAsync().ConfigureAwait(false) != -1)
            {
                // Decode next string in stream as the key
                BString key;
                try
                {
                    key = await BencodeParser.ParseStringAsync(stream).ConfigureAwait(false);
                }
                catch (BencodeParsingException<BString>)
                {
                    throw new BencodeParsingException<BDictionary>("Dictionary keys must be strings.", stream.Position);
                }

                // Decode next object in stream as the value
                var value = await BencodeParser.ParseAsync(stream).ConfigureAwait(false);
                if (value == null)
                    throw new BencodeParsingException<BDictionary>("All keys must have a corresponding value.", stream.Position);

                dictionary.Add(key, value);
            }

            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'e')
                throw new BencodeParsingException<BDictionary>("Missing end character 'e'.", stream.Position);

            return dictionary;
        }
    }
}
