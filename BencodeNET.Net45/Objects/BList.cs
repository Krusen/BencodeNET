using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

namespace BencodeNET.Objects
{
    public sealed class BList : BList<IBObject>
    {
        public override IList<IBObject> Value { get; }

        public BList()
        {
            Value = new List<IBObject>();
        }

        public BList(IEnumerable<string> strings)
            : this(strings, Bencode.DefaultEncoding)
        { }

        public BList(IEnumerable<string> strings, Encoding encoding)
            : this()
        {
            foreach (var str in strings)
            {
                Add(str, encoding);
            }
        }

        public void Add(string value)
        {
            Add(new BString(value));
        }

        public void Add(string value, Encoding encoding)
        {
            Add(new BString(value, encoding));
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

        public IEnumerable<string> AsStrings()
        {
            return AsStrings(Bencode.DefaultEncoding);
        }

        public IEnumerable<string> AsStrings(Encoding encoding)
        {
            return As<BString>().Select(x => x.ToString(encoding));
        }

        public BList<T> As<T>() where T : IBObject
        {
            return new BList<T>(this);
        }
    }

    public class BList<T> : BObject<IList<T>>, IList<T> where T : IBObject
    {
        public override IList<T> Value { get; }

        public BList()
        {
            Value = new List<T>();
        }

        public BList(IEnumerable<IBObject> list)
        {
            try
            {
                Value = list.Cast<T>().ToList();
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Not all elements are of type '{typeof(T).FullName}'.", ex);
            }
        }

        public override TStream EncodeToStream<TStream>(TStream stream)
        {
            stream.Write('l');
            foreach (var item in this)
            {
                item.EncodeToStream(stream);
            }
            stream.Write('e');
            return stream;
        }

#if !NET35
        public override async Task<TStream> EncodeToStreamAsync<TStream>(TStream stream)
        {
            await stream.WriteAsync('l').ConfigureAwait(false);
            foreach (var item in this)
            {
                await item.EncodeToStreamAsync(stream).ConfigureAwait(false);
            }
            await stream.WriteAsync('e').ConfigureAwait(false);
            return stream;
        }
#endif

        public override int GetHashCode()
        {
            long hashValue = 269;

            for (var i = 0; i < Value.Count; i++)
            {
                var bObject = Value[i];

                var factor = 1;

                if (bObject is BList)
                    factor = 2;

                if (bObject is BString)
                    factor = 3;

                if (bObject is BNumber)
                    factor = 4;

                if (bObject is BDictionary)
                    factor = 5;

                hashValue = (hashValue + 37 * factor * (i + 2)) % int.MaxValue;
            }

            return (int)hashValue;
        }

        #region IList<T> Members

        public int Count => Value.Count;

        public bool IsReadOnly => Value.IsReadOnly;

        public T this[int index]
        {
            get { return Value[index]; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Value[index] = value;
            }
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(T item)
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
