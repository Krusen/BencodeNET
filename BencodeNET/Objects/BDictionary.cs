using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded dictionary of <see cref="BString"/> keys and <see cref="IBObject"/> values.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IDictionary{BString,IBObject}"/>.
    /// </remarks>
    public sealed class BDictionary : BObject<IDictionary<BString, IBObject>>, IDictionary<BString, IBObject>
    {
        public override IDictionary<BString, IBObject> Value { get; }

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

        public T Get<T>(BString key) where T : class, IBObject
        {
            return this[key] as T;
        }

        /// <summary>
        /// Existing keys on this instance will be overwritten with the values from the passed <see cref="BDictionary"/>.
        /// In the case the existing and new value are both <see cref="BList"/> the new list will be appended to the existing list.
        /// In the case the existing and new value are both <see cref="BDictionary"/> they will be merged recursively.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="existingKeyAction"></param>
        public void MergeWith(BDictionary dictionary, ExistingKeyAction existingKeyAction = ExistingKeyAction.Merge)
        {
            foreach (var field in dictionary)
            {
                if (!ContainsKey(field.Key))
                {
                    Add(field);
                    continue;
                }

                if (existingKeyAction == ExistingKeyAction.Skip)
                    continue;

                // Replace strings and numbers
                if (field.Value is BString || field.Value is BNumber)
                {
                    this[field.Key] = field.Value;
                    continue;
                }

                // Append list to existing list or replace other types
                var newList = field.Value as BList;
                if (newList != null)
                {
                    var existingList = Get<BList>(field.Key);
                    if (existingList == null || existingKeyAction == ExistingKeyAction.Replace)
                    {
                        this[field.Key] = field.Value;
                        continue;
                    }
                    existingList.AddRange(newList);
                    continue;
                }

                // Merge dictionary with existing or replace other types
                var newDictionary = field.Value as BDictionary;
                if (newDictionary != null)
                {
                    var existingDictionary = Get<BDictionary>(field.Key);
                    if (existingDictionary == null || existingKeyAction == ExistingKeyAction.Replace)
                    {
                        this[field.Key] = field.Value;
                        continue;
                    }
                    existingDictionary.MergeWith(newDictionary);
                }
            }
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

        public override async Task<TStream> EncodeToStreamAsync<TStream>(TStream stream)
        {
            await stream.WriteAsync('d').ConfigureAwait(false);
            foreach (var kvPair in this)
            {
                await kvPair.Key.EncodeToStreamAsync(stream).ConfigureAwait(false);
                await kvPair.Value.EncodeToStreamAsync(stream).ConfigureAwait(false);
            }
            await stream.WriteAsync('e').ConfigureAwait(false);
            return stream;
        }

        #region IDictionary<BString, IBObject> Members

        public ICollection<BString> Keys => Value.Keys;

        public ICollection<IBObject> Values => Value.Values;

        public int Count => Value.Count;

        public bool IsReadOnly => Value.IsReadOnly;

        /// <summary>
        /// Returns the value associated with the key or null if the key doesn't exist.
        /// </summary>
        public IBObject this[BString key]
        {
            get
            {
                if (!ContainsKey(key))
                    return null;
                return Value[key];
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Value[key] = value;
            }
        }

        public void Add(KeyValuePair<BString, IBObject> item)
        {
            if (item.Value == null) throw new ArgumentException("Must not contain a null value", nameof(item));
            Value.Add(item);
        }

        public void Add(BString key, IBObject value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
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

    public enum ExistingKeyAction
    {
        Merge,
        Replace,
        Skip
    }
}
