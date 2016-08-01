using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;
using Xunit;
// ReSharper disable ConsiderUsingConfigureAwait

namespace BencodeNET.Tests
{
    public class BencodeStreamTests
    {
        [Fact]
        public void ReadBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(12);
                Assert.Equal(12, bytes.Length);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public async Task ReadBytesAsync()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = await bs.ReadAsync(12);
                Assert.Equal(12, bytes.Length);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void ReadZeroBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(0);
                Assert.Equal(0, bytes.Length);
                Assert.Equal("", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public async Task ReadZeroBytesAsync()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = await bs.ReadAsync(0);
                Assert.Equal(0, bytes.Length);
                Assert.Equal("", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void ReadMoreBytesThanInStream()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(20);
                Assert.Equal(12, bytes.Length);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public async Task ReadMoreBytesThanInStreamAsync()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = await bs.ReadAsync(20);
                Assert.Equal(12, bytes.Length);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void ReadBytesChangesStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);

                var bytes = bs.Read(str.Length);
                Assert.Equal(12, bytes.Length);
                Assert.Equal(str, Encoding.UTF8.GetString(bytes));

                Assert.Equal(12, bs.Position);
            }
        }

        [Fact]
        public void Read()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', bs.Read());
                Assert.Equal('e', bs.Read());
                Assert.Equal('l', bs.Read());
                Assert.Equal('l', bs.Read());
                Assert.Equal('o', bs.Read());
                Assert.Equal(' ', bs.Read());
                Assert.Equal('W', bs.Read());
                Assert.Equal('o', bs.Read());
                Assert.Equal('r', bs.Read());
                Assert.Equal('l', bs.Read());
                Assert.Equal('d', bs.Read());
                Assert.Equal('!', bs.Read());
                Assert.Equal(-1, bs.Read());
            }
        }

        [Fact]
        public async Task ReadAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', await bs.ReadAsync());
                Assert.Equal('e', await bs.ReadAsync());
                Assert.Equal('l', await bs.ReadAsync());
                Assert.Equal('l', await bs.ReadAsync());
                Assert.Equal('o', await bs.ReadAsync());
                Assert.Equal(' ', await bs.ReadAsync());
                Assert.Equal('W', await bs.ReadAsync());
                Assert.Equal('o', await bs.ReadAsync());
                Assert.Equal('r', await bs.ReadAsync());
                Assert.Equal('l', await bs.ReadAsync());
                Assert.Equal('d', await bs.ReadAsync());
                Assert.Equal('!', await bs.ReadAsync());
                Assert.Equal(-1, await bs.ReadAsync());
            }
        }

        [Fact]
        public void ReadChangeStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', bs.Read());
                Assert.Equal('e', bs.Read());
                bs.Position -= 1;
                Assert.Equal('e', bs.Read());
            }
        }

        [Fact]
        public async Task ReadChangeStreamPositionAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', await bs.ReadAsync());
                Assert.Equal('e', await bs.ReadAsync());
                bs.Position -= 1;
                Assert.Equal('e', await bs.ReadAsync());
            }
        }

        [Fact]
        public void ReadPrevious()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(-1, bs.ReadPrevious());
                Assert.Equal('H', bs.Read());
                Assert.Equal('H', bs.ReadPrevious());
                Assert.Equal('e', bs.Read());
                Assert.Equal('e', bs.ReadPrevious());

                bs.Position = 20;

                Assert.Equal(-1, bs.ReadPrevious());
            }
        }

        [Fact]
        public async Task ReadPreviousAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(-1, await bs.ReadPreviousAsync());
                Assert.Equal('H', await bs.ReadAsync());
                Assert.Equal('H', await bs.ReadPreviousAsync());
                Assert.Equal('e', await bs.ReadAsync());
                Assert.Equal('e', await bs.ReadPreviousAsync());

                bs.Position = 20;

                Assert.Equal(-1, await bs.ReadPreviousAsync());
            }
        }

        [Fact]
        public void ReadPreviousAtStartOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(-1, bs.ReadPrevious());
            }
        }

        [Fact]
        public async Task ReadPreviousAtStartOfStreamAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(-1, await bs.ReadPreviousAsync());
            }
        }

        [Fact]
        public void ReadPreviousUnaffectedByPeek()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(1);
                Assert.Equal('H', bs.ReadPrevious());
                Assert.Equal('e', bs.Peek());
                Assert.Equal('H', bs.ReadPrevious());
            }
        }

        [Fact]
        public async Task ReadPreviousUnaffectedByPeekAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(1);
                Assert.Equal('H', await bs.ReadPreviousAsync());
                Assert.Equal('e', await bs.PeekAsync());
                Assert.Equal('H', await bs.ReadPreviousAsync());
            }
        }

        [Fact]
        public void ReadPreviousChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(default(char), bs.ReadPreviousChar());
                bs.Read(1);
                Assert.Equal('H', bs.ReadPreviousChar());
            }
        }

        [Fact]
        public async Task ReadPreviousCharAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(default(char), await bs.ReadPreviousCharAsync());
                await bs.ReadAsync(1);
                Assert.Equal('H', await bs.ReadPreviousCharAsync());
            }
        }

        [Fact]
        public void PeekUnnaffectedByReadPrevious()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(0);
                Assert.Equal('a', bs.Peek());
                bs.ReadPrevious();
                Assert.Equal('a', bs.Peek());

                bs.Read(1);
                Assert.Equal('b', bs.Peek());
                bs.ReadPrevious();
                Assert.Equal('b', bs.Peek());
            }
        }

        [Fact]
        public async Task PeekUnnaffectedByReadPreviousAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                await bs.ReadAsync(0);
                Assert.Equal('a', await bs.PeekAsync());
                await bs.ReadPreviousAsync();
                Assert.Equal('a', await bs.PeekAsync());

                await bs.ReadAsync(1);
                Assert.Equal('b', await bs.PeekAsync());
                await bs.ReadPreviousAsync();
                Assert.Equal('b', await bs.PeekAsync());
            }
        }

        [Fact]
        public void ReadUnnaffectedByReadPrevious()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('a', bs.Read());
                bs.ReadPrevious();
                Assert.Equal('b', bs.Read());
            }
        }

        [Fact]
        public async Task ReadUnnaffectedByReadPreviousAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('a', await bs.ReadAsync());
                await bs.ReadPreviousAsync();
                Assert.Equal('b', await bs.ReadAsync());
            }
        }

        [Fact]
        public void Peek()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', bs.Peek());
            }
        }

        [Fact]
        public async Task PeekAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', await bs.PeekAsync());
            }
        }

        [Fact]
        public void PeekDoesNotAdvanceStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);
                Assert.Equal('H', bs.Peek());
                Assert.Equal(0, bs.Position);
                Assert.Equal('H', bs.Peek());
                Assert.Equal(0, bs.Position);
            }
        }

        [Fact]
        public async Task PeekDoesNotAdvanceStreamPositionAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);
                Assert.Equal('H', await bs.PeekAsync());
                Assert.Equal(0, bs.Position);
                Assert.Equal('H', await bs.PeekAsync());
                Assert.Equal(0, bs.Position);
            }
        }

        [Fact]
        public void PeekAndReadEqual()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', bs.Peek());
                Assert.Equal('H', bs.Read());
            }
        }

        [Fact]
        public async Task PeekAndReadEqualAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('H', await bs.PeekAsync());
                Assert.Equal('H', await bs.ReadAsync());
            }
        }

        [Fact]
        public void PeekAreChangedAfterRead()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('a', bs.Peek());
                Assert.Equal('a', bs.Read());

                Assert.Equal('b', bs.Peek());
                Assert.Equal('b', bs.Read());

                Assert.Equal('c', bs.Peek());
                Assert.Equal('c', bs.Read());

                Assert.Equal('d', bs.Peek());
                Assert.Equal('d', bs.Read());

                Assert.Equal('e', bs.Peek());
                Assert.Equal('e', bs.Read());
            }
        }

        [Fact]
        public async Task PeekAreChangedAfterReadAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal('a', await bs.PeekAsync());
                Assert.Equal('a', await bs.ReadAsync());

                Assert.Equal('b', await bs.PeekAsync());
                Assert.Equal('b', await bs.ReadAsync());

                Assert.Equal('c', await bs.PeekAsync());
                Assert.Equal('c', await bs.ReadAsync());

                Assert.Equal('d', await bs.PeekAsync());
                Assert.Equal('d', await bs.ReadAsync());

                Assert.Equal('e', await bs.PeekAsync());
                Assert.Equal('e', await bs.ReadAsync());
            }
        }

        [Fact]
        public void PeekAreChangedAfterReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;

                Assert.Equal('a', bs.Peek());

                bytes = bs.Read(1);
                Assert.Equal('a', (char)bytes[0]);
                Assert.Equal('b', bs.Peek());

                bytes = bs.Read(1);
                Assert.Equal('b', (char)bytes[0]);
                Assert.Equal('c', bs.Peek());
            }
        }

        [Fact]
        public async Task PeekAreChangedAfterReadSingleByteAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;

                Assert.Equal('a', await bs.PeekAsync());

                bytes = bs.Read(1);
                Assert.Equal('a', (char)bytes[0]);
                Assert.Equal('b', await bs.PeekAsync());

                bytes = bs.Read(1);
                Assert.Equal('b', (char)bytes[0]);
                Assert.Equal('c', await bs.PeekAsync());
            }
        }

        [Fact]
        public void PeekAreChangedAfterReadMutipleBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;

                Assert.Equal('a', bs.Peek());

                bytes = bs.Read(2);
                Assert.Equal('a', (char)bytes[0]);
                Assert.Equal('b', (char)bytes[1]);
                Assert.Equal('c', bs.Peek());

                bytes = bs.Read(2);
                Assert.Equal('c', (char)bytes[0]);
                Assert.Equal('d', (char)bytes[1]);
                Assert.Equal('e', bs.Peek());
            }
        }

        [Fact]
        public async Task PeekAreChangedAfterReadMutipleBytesAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;

                Assert.Equal('a', await bs.PeekAsync());

                bytes = bs.Read(2);
                Assert.Equal('a', (char)bytes[0]);
                Assert.Equal('b', (char)bytes[1]);
                Assert.Equal('c', await bs.PeekAsync());

                bytes = bs.Read(2);
                Assert.Equal('c', (char)bytes[0]);
                Assert.Equal('d', (char)bytes[1]);
                Assert.Equal('e', await bs.PeekAsync());
            }
        }

        [Fact]
        public void PeekAtEndOfStreamThenReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.Equal(-1, bs.Peek());
                Assert.Equal(-1, bs.Read());
            }
        }

        [Fact]
        public async Task PeekAtEndOfStreamThenReadSingleByteAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.Equal(-1, await bs.PeekAsync());
                Assert.Equal(-1, await bs.ReadAsync());
            }
        }

        [Fact]
        public void PeekAtEndOfStreamThenReadBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.Equal(-1, bs.Peek());
                Assert.Equal(0, bs.Read(4).Length);
            }
        }

        [Fact]
        public async Task PeekAtEndOfStreamThenReadBytesAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                await bs.ReadAsync(12);
                Assert.Equal(-1, await bs.PeekAsync());
                Assert.Equal(0, (await bs.ReadAsync(4)).Length);
            }
        }

        [Fact]
        public void PeekAfterPositionChange()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);
                Assert.Equal('a', bs.PeekChar());
                bs.Position = 1;
                Assert.Equal(1, bs.Position);
                Assert.Equal('b', bs.PeekChar());
            }
        }

        [Fact]
        public async Task PeekAfterPositionChangeAsync()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);
                Assert.Equal('a', await bs.PeekCharAsync());
                bs.Position = 1;
                Assert.Equal(1, bs.Position);
                Assert.Equal('b', await bs.PeekCharAsync());
            }
        }

        [Fact]
        public async Task PeekAfterSeek()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.Equal(0, bs.Position);
                Assert.Equal('a', await bs.PeekCharAsync());
                bs.Seek(1, SeekOrigin.Current);
                Assert.Equal(1, bs.Position);
                Assert.Equal('b', await bs.PeekCharAsync());
            }
        }

        [Fact]
        public void EndOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.True(bs.EndOfStream);
                Assert.Equal(-1, bs.Read());
            }
        }

        [Fact]
        public async Task EndOfStreamAsync()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.True(bs.EndOfStream);
                Assert.Equal(-1, await bs.ReadAsync());
            }
        }
    }
}
