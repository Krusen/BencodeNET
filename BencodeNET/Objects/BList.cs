using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BencodeNET.Exceptions;

namespace BencodeNET.Objects
{
    public class BList : BObject<IList<IBObject>>, IList<IBObject>
    {
        public BList()
        {
            Value = new List<IBObject>();
        }

        public void Add(string value)
        {
            Add(new BString(value));
        }

        public void Add(int value)
        {
            Add((IBObject) new BNumber(value));
        }

        public void Add(long value)
        {
            Add((IBObject) new BNumber(value));
        }

        public override T EncodeToStream<T>(T stream, Encoding encoding)
        {
            stream.Write('l');
            foreach (var item in this)
                item.EncodeToStream(stream, encoding);
            stream.Write('e');
            return stream;
        }

        public static BList Decode(string bencodedString)
        {
            return Decode(bencodedString, Bencode.DefaultEncoding);
        }

        public static BList Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");
            if (encoding == null) throw new ArgumentNullException("encoding");

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        public static BList Decode(Stream stream)
        {
            return Decode(stream, Bencode.DefaultEncoding);
        }

        public static BList Decode(Stream stream, Encoding encoding)
        {
            return Decode(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static BList Decode(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (encoding == null) throw new ArgumentNullException("encoding");

            if (stream.Length < 2)
                throw InvalidException("Minimum valid length is 2 (an empty list: 'le')", stream.Position);

            // Lists must start with 'l'
            if (stream.ReadChar() != 'l')
                throw InvalidException(string.Format("Must begin with 'l' but began with '{0}'.", stream.ReadPreviousChar()), stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && !stream.EndOfStream)
            {
                // Decode next object in stream
                var bObject = Bencode.Decode(stream, encoding);
                if (bObject == null)
                    throw InvalidException(string.Format("Invalid object beginning with '{0}'", stream.PeekChar()), stream.Position);

                list.Add(bObject);
            }

            if (stream.ReadChar() != 'e')
                throw InvalidException("Missing end character 'e'.", stream.Position);

            return list;
        }

        private static InvalidBencodeException InvalidException(string message, long streamPosition)
        {
            return InvalidBencodeException.New("Invalid bencode list. " + message, streamPosition);
        }

        #region IList<IBObject> Members

        public IEnumerator<IBObject> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IBObject item)
        {
            if (item == null) throw new ArgumentNullException("item");
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(IBObject item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(IBObject[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public bool Remove(IBObject item)
        {
            return Value.Remove(item);
        }

        public int Count { get { return Value.Count; } }
        public bool IsReadOnly { get { return Value.IsReadOnly; } }
        
        public int IndexOf(IBObject item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, IBObject item)
        {
            Value.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

        public IBObject this[int index]
        {
            get { return Value[index]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                Value[index] = value;
            }
        }

        #endregion
    }
}
