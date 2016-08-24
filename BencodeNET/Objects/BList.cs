using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded list of <see cref="IBObject"/>.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IList{IBObject}"/>.
    /// </remarks>
    public class BList : BList<IBObject>
    {
        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value { get; } = new List<IBObject>();

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        { }

        /// <summary>
        /// Creates a list from strings using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="strings"></param>
        public BList(IEnumerable<string> strings)
            : this(strings, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates a list from strings using the specified encoding.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="encoding"></param>
        public BList(IEnumerable<string> strings, Encoding encoding)
        {
            foreach (var str in strings)
            {
                Add(str, encoding);
            }
        }

        /// <summary>
        /// Creates a list from en <see cref="IEnumerable{T}"/> of <see cref="IBObject"/>.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<IBObject> objects)
            : base(objects)
        { }

        /// <summary>
        /// Adds a string to the list using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="value"></param>
        public void Add(string value)
        {
            Add(new BString(value));
        }

        /// <summary>
        /// Adds a string to the list using the specified encoding.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void Add(string value, Encoding encoding)
        {
            Add(new BString(value, encoding));
        }

        /// <summary>
        /// Adds an integer to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            Add((IBObject) new BNumber(value));
        }

        /// <summary>
        /// Adds a long to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(long value)
        {
            Add((IBObject) new BNumber(value));
        }

        /// <summary>
        /// Appends a list to the end of this instance.
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(BList list)
        {
            foreach (var obj in list)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/> and returns
        /// an enumerable of their string representation.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AsStrings()
        {
            return AsStrings(Encoding.UTF8);
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/> and returns
        /// an enumerable of their string representation using the specified encoding.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AsStrings(Encoding encoding)
        {
            return As<BString>().Select(x => x.ToString(encoding));
        }

        /// <summary>
        /// Attempts to cast all elements to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// An element is not of type <typeparamref name="T"/>.
        /// </exception>
        public BList<T> As<T>() where T : IBObject
        {
            try
            {
                return new BList<T>(this.Cast<T>());
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Not all elements are of type '{typeof(T).FullName}'.", ex);
            }
        }
    }

    /// <summary>
    /// Represents a bencoded list of <see cref="IBObject"/> of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IList{T}"/>.
    /// </remarks>
    public class BList<T> : BObject<IList<T>>, IList<T> where T : IBObject
    {
        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<T> Value { get; }

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        {
            Value = new List<T>();
        }

        /// <summary>
        /// Creates a list from the specified objects.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<T> objects)
        {
            Value = objects.ToList();
        }

#pragma warning disable 1591
        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('l');
            foreach (var item in this)
            {
                item.EncodeTo(stream);
            }
            stream.Write('e');
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
#pragma warning restore 1591
    }
}
