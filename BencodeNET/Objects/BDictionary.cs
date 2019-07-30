using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.IO;

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
        /// <summary>
        /// The underlying dictionary.
        /// </summary>
        public override IDictionary<BString, IBObject> Value { get; } = new SortedDictionary<BString, IBObject>();

        /// <summary>
        /// Creates an empty dictionary.
        /// </summary>
        public BDictionary()
        { }

        /// <summary>
        /// Creates a dictionary from key-value pairs.
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public BDictionary(IEnumerable<KeyValuePair<BString, IBObject>> keyValuePairs)
        {
            Value = new SortedDictionary<BString, IBObject>(keyValuePairs.ToDictionary(x => x.Key, x => x.Value));
        }

        /// <summary>
        /// Creates a dictionary with an initial value of the supplied dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public BDictionary(IDictionary<BString, IBObject> dictionary)
        {
            Value = dictionary;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary as <see cref="BString"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value) => Add(new BString(key), new BString(value));

        /// <summary>
        /// Adds the specified key and value to the dictionary as <see cref="BNumber"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, long value) => Add(new BString(key), new BNumber(value));

        /// <summary>
        /// Gets the value associated with the specified key and casts it as <typeparamref name="T"/>.
        /// If the key does not exist or the value is not of the specified type null is returned.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="key">The key to get the associated value of.</param>
        /// <returns>The associated value of the specified key or null if the key does not exist.
        /// If the value is not of the specified type null is returned as well.</returns>
        public T Get<T>(BString key) where T : class, IBObject
        {
            return this[key] as T;
        }

        /// <summary>
        /// Merges this instance with another <see cref="BDictionary"/>.
        /// </summary>
        /// <remarks>
        /// By default existing keys are either overwritten (<see cref="BString"/> and <see cref="BNumber"/>) or merged if possible (<see cref="BList"/> and <see cref="BDictionary"/>).
        /// This behavior can be changed with the <paramref name="existingKeyAction"/> parameter.
        /// </remarks>
        /// <param name="dictionary">The dictionary to merge into this instance.</param>
        /// <param name="existingKeyAction">Decides how to handle the values of existing keys.</param>
        public void MergeWith(BDictionary dictionary, ExistingKeyAction existingKeyAction = ExistingKeyAction.Merge)
        {
            foreach (var field in dictionary)
            {
                // Add non-existing key
                if (!ContainsKey(field.Key))
                {
                    Add(field);
                    continue;
                }

                if (existingKeyAction == ExistingKeyAction.Skip)
                    continue;

                switch (field.Value)
                {
                    // Replace strings and numbers
                    case BString _:
                    case BNumber _:
                        this[field.Key] = field.Value;
                        continue;

                    // Append list to existing list or replace other types
                    case BList newList:
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
                    case BDictionary newDictionary:
                    {
                        var existingDictionary = Get<BDictionary>(field.Key);
                        if (existingDictionary == null || existingKeyAction == ExistingKeyAction.Replace)
                        {
                            this[field.Key] = field.Value;
                            continue;
                        }
                        existingDictionary.MergeWith(newDictionary);
                        break;
                    }
                }
            }
        }

#pragma warning disable 1591
        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('d');
            foreach (var entry in this)
            {
                entry.Key.EncodeTo(stream);
                entry.Value.EncodeTo(stream);
            }
            stream.Write('e');
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
            get => ContainsKey(key) ? Value[key] : null;
            set => Value[key] = value ?? throw new ArgumentNullException(nameof(value), "A null value cannot be added to a BDictionary");
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

        public void Clear() => Value.Clear();

        public bool Contains(KeyValuePair<BString, IBObject> item) => Value.Contains(item);

        public bool ContainsKey(BString key) => Value.ContainsKey(key);

        public void CopyTo(KeyValuePair<BString, IBObject>[] array, int arrayIndex) => Value.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<BString, IBObject>> GetEnumerator() => Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(KeyValuePair<BString, IBObject> item) => Value.Remove(item);

        public bool Remove(BString key) => Value.Remove(key);

        public bool TryGetValue(BString key, out IBObject value) => Value.TryGetValue(key, out value);

        #endregion
#pragma warning restore 1591
    }

    /// <summary>
    /// Specifies the action to take when encountering an already existing key when merging two <see cref="BDictionary"/>.
    /// </summary>
    public enum ExistingKeyAction
    {
        /// <summary>
        /// Merges the values of existing keys for <see cref="BList"/> and <see cref="BDictionary"/>.
        /// Overwrites existing keys for <see cref="BString"/> and <see cref="BNumber"/>.
        /// </summary>
        Merge,

        /// <summary>
        /// Replaces the values of all existing keys.
        /// </summary>
        Replace,

        /// <summary>
        /// Leaves all existing keys as they were.
        /// </summary>
        Skip
    }
}
