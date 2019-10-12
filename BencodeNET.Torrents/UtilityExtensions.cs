using System.Collections.Generic;
using System.Linq;

namespace BencodeNET.Torrents
{
    internal static class UtilityExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(x => x);
        }
    }
}
