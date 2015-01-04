using System;
using System.Collections;
using System.Collections.Generic;

namespace BencodeNET.Objects
{
    public sealed class BList : BObject<IList<IBObject>>, IList<IBObject>
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

        public override T EncodeToStream<T>(T stream)
        {
            stream.Write('l');
            foreach (var item in this)
                item.EncodeToStream(stream);
            stream.Write('e');
            return stream;
        }

        #region IList<IBObject> Members

        public int Count { get { return Value.Count; } }

        public bool IsReadOnly { get { return Value.IsReadOnly; } }

        public IBObject this[int index]
        {
            get { return Value[index]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                Value[index] = value;
            }
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

        public IEnumerator<IBObject> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IBObject item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, IBObject item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(IBObject item)
        {
            return Value.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

        #endregion
    }
}
