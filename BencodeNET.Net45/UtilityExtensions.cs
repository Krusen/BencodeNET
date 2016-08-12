using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET
{
    public static class UtilityExtensions
    {
        public static bool IsDigit(this char c)
        {
            return (c >= '0' && c <= '9');
        }

        public static MemoryStream AsStream(this string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return default(TValue);
        }

        public static void Write(this Stream stream, char c)
        {
            stream.WriteByte((byte)c);
        }

        public static async Task<int> ReadByteAsync(this Stream stream)
        {
            var data = new byte[1];
            var bytesRead = await stream.ReadAsync(data, 0, 1).ConfigureAwait(false);
            if (bytesRead == 0)
                return -1;
            return data[0];
        }

        public static Task WriteAsync(this Stream stream, char c)
        {
            return stream.WriteAsync(new [] {(byte) c}, 0, 1);
        }

        public static Task<TBase> FromDerived<TBase, TDerived>(this Task<TDerived> task) where TDerived : TBase
        {
            var tcs = new TaskCompletionSource<TBase>();

            task.ContinueWith(t => tcs.SetResult(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith(t => tcs.SetException(t.Exception.InnerExceptions), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);

            return tcs.Task;
        }
    }
}
