using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BencodeNET.Objects
{
    public sealed class BDictionary : BObject<IDictionary<BString, IBObject>>, IDictionary<BString, IBObject>
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

        public override T EncodeToStream<T>(T stream)
        {
            stream.Write('d');
            foreach (var kvPair in this)
            {
                kvPair.Key.EncodeToStream(stream);
                kvPair.Value.EncodeToStream(stream);
            }
            stream.Write('e');
            return stream;
        }

        #region IDictionary<BString, IBObject> Members

        public ICollection<BString> Keys { get { return Value.Keys; } }

        public ICollection<IBObject> Values { get { return Value.Values; } }

        public int Count { get { return Value.Count; } }

        public bool IsReadOnly { get { return Value.IsReadOnly; } }

        public IBObject this[BString key]
        {
            get { return Value[key]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                Value[key] = value;
            }
        }

        public void Add(KeyValuePair<BString, IBObject> item)
        {
            if (item.Value == null) throw new ArgumentException("Must not contain a null value", "item");
            Value.Add(item);
        }

        public void Add(BString key, IBObject value)
        {
            if (value == null) throw new ArgumentNullException("value");
            Value.Add(key, value);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(KeyValuePair<BString, IBObject> item)
        {
            return Value.Contains(item);
        }

        public bool ContainsKey(BString key)
        {
            return Value.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<BString, IBObject>[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<BString, IBObject>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(KeyValuePair<BString, IBObject> item)
        {
            return Value.Remove(item);
        }

        public bool Remove(BString key)
        {
            return Value.Remove(key);
        }

        public bool TryGetValue(BString key, out IBObject value)
        {
            return Value.TryGetValue(key, out value);
        }

        #endregion
    }
}
