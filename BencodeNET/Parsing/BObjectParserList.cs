using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A special collection for <see cref="IBObjectParser"/> that has some extra methods
    /// for efficiently adding and accessing parsers by the type they can parse.
    /// </summary>
    public class BObjectParserList : IEnumerable<KeyValuePair<Type, IBObjectParser>>
    {
        private IDictionary<Type, IBObjectParser> Parsers { get; } = new Dictionary<Type, IBObjectParser>();

        /// <summary>
        /// Adds a parser for the specified type.
        /// Existing parsers for this type will be replaced.
        /// </summary>
        /// <param name="type">The type this parser can parse.</param>
        /// <param name="parser">The parser to add.</param>
        public void Add(Type type, IBObjectParser parser)
        {
            AddOrReplace(type, parser);
        }

        /// <summary>
        /// Adds a parser for the specified type.
        /// Existing parsers for this type will be replaced.
        /// </summary>
        /// <param name="types">The types this parser can parse.</param>
        /// <param name="parser">The parser to add.</param>
        public void Add(IEnumerable<Type> types, IBObjectParser parser)
        {
            AddOrReplace(types, parser);
        }

        /// <summary>
        /// Adds a specific parser.
        /// Existing parsers for the type will be replaced.
        /// </summary>
        /// <typeparam name="T">The type this parser can parse.</typeparam>
        /// <param name="parser">The parser to add.</param>
        public void Add<T>(IBObjectParser<T> parser) where T : IBObject
        {
            AddOrReplace(typeof(T), parser);
        }

        /// <summary>
        /// Adds a parser for the specified type.
        /// Existing parsers for this type will be replaced.
        /// </summary>
        /// <param name="type">The type this parser can parse.</param>
        /// <param name="parser">The parser to add.</param>
        public void AddOrReplace(Type type, IBObjectParser parser)
        {
            if (!typeof(IBObject).IsAssignableFrom(type))
                throw new ArgumentException($"The '{nameof(type)}' parameter must be assignable to '{typeof(IBObject).FullName}'");

            if (Parsers.ContainsKey(type))
                Parsers.Remove(type);

            Parsers.Add(type, parser);
        }

        /// <summary>
        /// Adds a parser for the specified type.
        /// Existing parsers for this type will be replaced.
        /// </summary>
        /// <param name="types">The types this parser can parse.</param>
        /// <param name="parser">The parser to add.</param>
        public void AddOrReplace(IEnumerable<Type> types, IBObjectParser parser)
        {
            foreach (var type in types)
            {
                AddOrReplace(type, parser);
            }
        }

        /// <summary>
        /// Adds a specific parser.
        /// Existing parsers for the type will be replaced.
        /// </summary>
        /// <typeparam name="T">The type this parser can parse.</typeparam>
        /// <param name="parser">The parser to add.</param>
        public void AddOrReplace<T>(IBObjectParser<T> parser) where T : IBObject
        {
            AddOrReplace(typeof(T), parser);
        }

        /// <summary>
        /// Gets the parser, if any, for the specified type.
        /// </summary>
        /// <param name="type">The type to get a parser for.</param>
        /// <returns>The parser for the specified type or null if there isn't one.</returns>
        public IBObjectParser Get(Type type)
        {
            return Parsers.GetValueOrDefault(type);
        }

        /// <summary>
        /// Gets the parser, if any, for the specified type.
        /// </summary>
        /// <param name="type">The type to get a parser for.</param>
        /// <returns>The parser for the specified type or null if there isn't one.</returns>
        public IBObjectParser this[Type type]
        {
            get => Get(type);
            set => AddOrReplace(type, value);
        }

        /// <summary>
        /// Gets the parser, if any, for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get a parser for.</typeparam>
        /// <returns>The parser for the specified type or null if there isn't one.</returns>
        public IBObjectParser<T> Get<T>() where T : IBObject
        {
            return Get(typeof(T)) as IBObjectParser<T>;
        }

        /// <summary>
        /// Gets the specific parser of the type specified or null if not found.
        /// </summary>
        /// <typeparam name="T">The parser type to get.</typeparam>
        /// <returns>The parser of the specified type or null if there isn't one.</returns>
        public T GetSpecific<T>() where T : class, IBObjectParser
        {
            return Parsers.FirstOrDefault(x => x.Value is T).Value as T;
        }

        /// <summary>
        /// Removes the parser for the specified type.
        /// </summary>
        /// <param name="type">The type to remove the parser for.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool Remove(Type type) => Parsers.Remove(type);

        /// <summary>
        /// Removes the parser for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to remove the parser for.</typeparam>
        /// <returns>True if successful, false otherwise.</returns>
        public bool Remove<T>() => Remove(typeof(T));

        /// <summary>
        /// Empties the collection.
        /// </summary>
        public void Clear() => Parsers.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
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
