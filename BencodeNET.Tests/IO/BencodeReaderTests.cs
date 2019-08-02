using System.IO;
using System.Text;
using BencodeNET.IO;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.IO
{
    public class BencodeReaderTests
    {
        [Fact]
        public void ReadBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeReader(ms))
            {
                var bytes = new byte[12];
                var read = bs.Read(bytes);

                read.Should().Be(12);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void ReadZeroBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeReader(ms))
            {
                var bytes = new byte[0];
                var read = bs.Read(bytes);

                read.Should().Be(0);
                Assert.Equal("", Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void ReadMoreBytesThanInStream()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeReader(ms))
            {
                var bytes = new byte[20];
                var read = bs.Read(bytes);

                read.Should().Be(12);
                Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes, 0, read));
            }
        }

        [Fact]
        public void ReadCharChangesStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.Position.Should().Be(0);
                bs.ReadChar();
                bs.Position.Should().Be(1);
                bs.ReadChar();
                bs.Position.Should().Be(2);
            }
        }

        [Fact]
        public void ReadBytesChangesStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                Assert.Equal(0, bs.Position);

                var bytes = new byte[str.Length];
                var read = bs.Read(bytes);

                read.Should().Be(12);
                bs.Position.Should().Be(12);
                Assert.Equal(str, Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void PeekCharDoesNotChangeStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.Position.Should().Be(0);
                bs.PeekChar();
                bs.Position.Should().Be(0);
                bs.PeekChar();
                bs.Position.Should().Be(0);
            }
        }


        [Fact]
        public void ReadCharAfterPeekCharChangesStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.Position.Should().Be(0);
                bs.PeekChar();
                bs.Position.Should().Be(0);
                bs.ReadChar();
                bs.Position.Should().Be(1);
            }
        }

        [Fact]
        public void ReadChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.ReadChar().Should().Be('H');
                bs.ReadChar().Should().Be('e');
                bs.ReadChar().Should().Be('l');
                bs.ReadChar().Should().Be('l');
                bs.ReadChar().Should().Be('o');
                bs.ReadChar().Should().Be(' ');
                bs.ReadChar().Should().Be('W');
                bs.ReadChar().Should().Be('o');
                bs.ReadChar().Should().Be('r');
                bs.ReadChar().Should().Be('l');
                bs.ReadChar().Should().Be('d');
                bs.ReadChar().Should().Be('!');
                bs.ReadChar().Should().Be(default);
            }
        }

        [Fact]
        public void PreviousChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.PreviousChar.Should().Be(default);

                Assert.Equal('H', bs.ReadChar());
                Assert.Equal('H', bs.PreviousChar);
                Assert.Equal('e', bs.ReadChar());
                Assert.Equal('e', bs.PreviousChar);

                bs.Read(new byte[20]);

                Assert.Equal('!', bs.PreviousChar);
            }
        }

        [Fact]
        public void PreviousCharAtStartOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.PreviousChar.Should().Be(default);
            }
        }

        [Fact]
        public void PreviousCharUnaffectedByPeekChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.SkipBytes(1);
                Assert.Equal('H', bs.PreviousChar);
                Assert.Equal('e', bs.PeekChar());
                Assert.Equal('H', bs.PreviousChar);
            }
        }

        [Fact]
        public void PeekChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                Assert.Equal('H', bs.PeekChar());
            }
        }

        [Fact]
        public void PeekDoesNotAdvanceStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.Position.Should().Be(0);
                bs.PeekChar().Should().Be('H');
                bs.Position.Should().Be(0);
                bs.PeekChar().Should().Be('H');
                bs.Position.Should().Be(0);
            }
        }

        [Fact]
        public void PeekAndReadEqual()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.PeekChar().Should().Be('H');
                bs.ReadChar().Should().Be('H');
            }
        }

        [Fact]
        public void PeekAreChangedAfterReadChar()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                Assert.Equal('a', bs.PeekChar());
                Assert.Equal('a', bs.ReadChar());

                Assert.Equal('b', bs.PeekChar());
                Assert.Equal('b', bs.ReadChar());

                Assert.Equal('c', bs.PeekChar());
                Assert.Equal('c', bs.ReadChar());

                Assert.Equal('d', bs.PeekChar());
                Assert.Equal('d', bs.ReadChar());

                Assert.Equal('e', bs.PeekChar());
                Assert.Equal('e', bs.ReadChar());
            }
        }

        [Fact]
        public void PeekAreChangedAfterReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                var buffer = new byte[1];
                int read;

                Assert.Equal('a', bs.PeekChar());

                read = bs.Read(buffer);
                Assert.Equal(1, read);
                Assert.Equal('a', (char)buffer[0]);
                Assert.Equal('b', bs.PeekChar());

                read = bs.Read(buffer);
                Assert.Equal(1, read);
                Assert.Equal('b', (char)buffer[0]);
                Assert.Equal('c', bs.PeekChar());
            }
        }

        [Fact]
        public void PeekAreChangedAfterReadMultipleBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                var buffer = new byte[2];

                Assert.Equal('a', bs.PeekChar());

                var read = bs.Read(buffer);
                read.Should().Be(2);
                Assert.Equal('a', (char)buffer[0]);
                Assert.Equal('b', (char)buffer[1]);
                Assert.Equal('c', bs.PeekChar());

                read = bs.Read(buffer);
                read.Should().Be(2);
                Assert.Equal('c', (char)buffer[0]);
                Assert.Equal('d', (char)buffer[1]);
                Assert.Equal('e', bs.PeekChar());
            }
        }

        [Fact]
        public void PeekAtEndOfStreamThenReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.SkipBytes(12);
                bs.PeekChar().Should().Be(default);
                bs.ReadChar().Should().Be(default);
            }
        }

        [Fact]
        public void PeekAtEndOfStreamThenReadBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.SkipBytes(12);
                bs.PeekChar().Should().Be(default);
                bs.Read(new byte[4]).Should().Be(0);
            }
        }

        [Fact]
        public void EndOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeReader(ms))
            {
                bs.SkipBytes(12);
                bs.EndOfStream.Should().BeTrue();
                bs.ReadChar().Should().Be(default);
            }
        }
    }
}
