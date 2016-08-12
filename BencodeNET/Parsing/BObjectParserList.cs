using System;
using System.Collections;
using System.Collections.Generic;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class BObjectParserList : IEnumerable<KeyValuePair<Type, IBObjectParser>>
    {
        private IDictionary<Type, IBObjectParser> Parsers { get; } = new Dictionary<Type, IBObjectParser>();

        // TODO: Add type check (IBObject) here, for easier understanding of exception?
        public void Add(Type type, IBObjectParser parser)
        {
            AddOrReplace(type, parser);
        }

        public void Add(IEnumerable<Type> types, IBObjectParser parser)
        {
            AddOrReplace(types, parser);
        }

        public void Add<T>(IBObjectParser<T> parser) where T : IBObject
        {
            AddOrReplace(typeof(T), parser);
        }

        public void AddOrReplace(Type type, IBObjectParser parser)
        {
            if (!typeof(IBObject).IsAssignableFrom(type))
                throw new ArgumentException($"The '{nameof(type)}' parameter must be assignable to '{typeof(IBObject).FullName}'");

            if (Parsers.ContainsKey(type))
                Parsers.Remove(type);
            Parsers.Add(type, parser);
        }

        public void AddOrReplace(IEnumerable<Type> types, IBObjectParser parser)
        {
            foreach (var type in types)
            {
                AddOrReplace(type, parser);
            }
        }

        public void AddOrReplace<T>(IBObjectParser<T> parser) where T : IBObject
        {
            AddOrReplace(typeof(T), parser);
        }

        public IBObjectParser Get(Type type)
        {
            return Parsers.GetValueOrDefault(type);
        }

        public IBObjectParser this[Type type]
        {
            get { return Get(type); }
            set { AddOrReplace(type, value); }
        }

        public IBObjectParser<T> Get<T>() where T : IBObject
        {
            return Get(typeof(T)) as IBObjectParser<T>;
        }

        public bool Remove(Type type) => Parsers.Remove(type);

        public bool Remove<T>() => Remove(typeof (T));

        public void Clear() => Parsers.Clear();

        public IEnumerator<KeyValuePair<Type, IBObjectParser>> GetEnumerator()
        {
            return Parsers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
