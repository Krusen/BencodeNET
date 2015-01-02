using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BencodeNET.Exceptions;

namespace BencodeNET.Objects
{
    public class BDictionary : BObject<IDictionary<BString, IBObject>>, IDictionary<BString, IBObject>
    {
        public BDictionary()
        {
            Value = new SortedDictionary<BString, IBObject>();
        }

        public void Add(string key, string value)
        {
            Add(new BString(key), new BString(value));
        }

        public void Add(string key, long value)
        {
            Add(new BString(key), new BNumber(value));
        }

        protected void Add(IBObject key, IBObject value)
        {
            if (!(key is BString))
                throw new ArgumentException("Only types of BString are allowed as key", "key");
            Add((BString) key, value);
        }

        public override T EncodeToStream<T>(T stream, Encoding encoding)
        {
            stream.Write('d');
            foreach (var kvPair in this)
            {
                kvPair.Key.EncodeToStream(stream, encoding);
                kvPair.Value.EncodeToStream(stream, encoding);
            }
            stream.Write('e');
            return stream;
        }

        public static BDictionary Decode(string bencodedString)
        {
            return Decode(bencodedString, Bencode.DefaultEncoding);
        }

        public static BDictionary Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");
            if (encoding == null) throw new ArgumentNullException("encoding");

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        public static BDictionary Decode(Stream stream)
        {
            return Decode(stream, Bencode.DefaultEncoding);
        }

        public static BDictionary Decode(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (encoding == null) throw new ArgumentNullException("encoding");

            var startPosition = stream.Position;

            if (stream.Length < 2)
                throw InvalidException("Minimum valid length is 2 (an empty dictionary: 'de')", startPosition);

            using (var reader = new BinaryReader(stream, encoding, leaveOpen: true))
            {
                // Dictionaries must start with 'd'
                var firstChar = reader.ReadCharOrDefault();
                if (firstChar != 'd')
                    throw InvalidException(string.Format("Must begin with 'd' but began with '{0}'", firstChar), startPosition);

                var dictionary = new BDictionary();
                // Loop until next character is the end character 'e' or the end of stream
                while (reader.PeekChar() != 'e' && reader.PeekChar() != -1)
                {
                    // Decode next string in stream as the key
                    BString key;
                    try
                    {
                        key = BString.Decode(stream, encoding);
                    }
                    catch (InvalidBencodeException ex)
                    {
                        throw InvalidException("Dictionary keys must be strings.", stream.Position);
                    }

                    // Decode next object in stream as the value
                    var value = Bencode.Decode(stream, encoding);
                    if (value == null)
                        throw InvalidException("All keys must have a corresponding value.", stream.Position);

                    dictionary.Add(key, value);
                }

                if (stream.EndOfStream())
                    throw InvalidException("Reached end of stream/string and did not find the required end character 'e'.", startPosition);

                // Advance past end character 'e'
                stream.Position += 1;

                return dictionary;
            }
        }

        private static InvalidBencodeException InvalidException(string message, long streamPosition)
        {
            return InvalidBencodeException.New("Invalid bencode dictionary. " + message, streamPosition);
        }

        #region IDictionary<BString, IBObject> Members

        public IEnumerator<KeyValuePair<BString, IBObject>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<BString, IBObject> item)
        {
            if (item.Value == null) throw new ArgumentException("Must not contain a null value", "item");
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(KeyValuePair<BString, IBObject> item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(KeyValuePair<BString, IBObject>[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<BString, IBObject> item)
        {
            return Value.Remove(item);
        }

        public int Count { get { return Value.Count; }}

        public bool IsReadOnly { get { return Value.IsReadOnly; }}

        public bool ContainsKey(BString key)
        {
            return Value.ContainsKey(key);
        }

        public void Add(BString key, IBObject value)
        {
            if (value == null) throw new ArgumentNullException("value");
            Value.Add(key, value);
        }

        public bool Remove(BString key)
        {
            return Value.Remove(key);
        }

        public bool TryGetValue(BString key, out IBObject value)
        {
            return Value.TryGetValue(key, out value);
        }

        public IBObject this[BString key]
        {
            get { return Value[key]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                Value[key] = value;
            }
        }

        public ICollection<BString> Keys { get { return Value.Keys; } }
        public ICollection<IBObject> Values { get { return Value.Values; } }

        #endregion
    }
}
