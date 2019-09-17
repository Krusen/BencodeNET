using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BencodeNET.Tests.IO
{
    public class PipeBencodeReaderTests
    {
        private Pipe Pipe { get; } = new Pipe();
        private PipeReader PipeReader => Pipe.Reader;
        private PipeWriter PipeWriter => Pipe.Writer;

        private PipeBencodeReader PipeBencodeReader { get; }

        public PipeBencodeReaderTests()
        {
            PipeBencodeReader = new PipeBencodeReader(PipeReader);
        }

        [Fact]
        public async Task CanReadAsyncBeforeWrite()
        {
            var bytes = Encoding.UTF8.GetBytes("abc").AsMemory();

            var (reader, writer) = new Pipe();
            var bencodeReader = new PipeBencodeReader(reader);

            var readTask = bencodeReader.ReadCharAsync();

            await writer.WriteAsync(bytes.Slice(0, 2));

            writer.Complete();

            var c = await readTask;

            c.Should().Be('a');
        }

        [Fact]
        public async Task CanReadLessThanRequested()
        {
            var bytes = Encoding.UTF8.GetBytes("abc").AsMemory();

            var (reader, writer) = new Pipe();
            var bencodeReader = new PipeBencodeReader(reader);

            await writer.WriteAsync(bytes.Slice(0, 1));

            var buffer = new byte[bytes.Length];
            var readTask = bencodeReader.ReadAsync(buffer);

            await writer.WriteAsync(bytes.Slice(1, 1));
            writer.Complete();

            var bytesRead = await readTask;

            bytesRead.Should().Be(2);
            buffer[0].Should().Be((byte)'a');
            buffer[1].Should().Be((byte)'b');
            buffer[2].Should().Be(default);
        }

        [Theory]
        [AutoMockedData]
        public async Task CanReadIncompleteFirstTry(PipeReader reader)
        {
            var bytes = Encoding.UTF8.GetBytes("abcdef").AsMemory();

            reader.TryRead(out _).Returns(false);
            reader.ReadAsync().Returns(
                new ReadResult(new ReadOnlySequence<byte>(bytes.Slice(0, 5)), false, false),
                new ReadResult(new ReadOnlySequence<byte>(bytes), false, true)
            );

            var bencodeReader = new PipeBencodeReader(reader);

            var buffer = new byte[bytes.Length];
            var bytesRead = await bencodeReader.ReadAsync(buffer);

            bytesRead.Should().Be(bytes.Length);
            buffer.Should().Equal(bytes.ToArray());
        }

        [Theory]
        [AutoMockedData]
        public async Task CanReadLessThanReceivedAsync(PipeReader reader)
        {
            var bytes = Encoding.UTF8.GetBytes("abcdef").AsMemory();

            reader.TryRead(out _).Returns(false);
            reader.ReadAsync().Returns(new ReadResult(new ReadOnlySequence<byte>(bytes), false, true));

            var bencodeReader = new PipeBencodeReader(reader);

            var buffer = new byte[bytes.Length-3];
            var readTask = bencodeReader.ReadAsync(buffer);

            var bytesRead = await readTask;

            bytesRead.Should().Be(bytes.Length-3);
            buffer[0].Should().Be((byte)'a');
            buffer[1].Should().Be((byte)'b');
            buffer[2].Should().Be((byte)'c');
        }

        [Fact]
        public async Task CanPeekAsyncBeforeWrite()
        {
            var bytes = Encoding.UTF8.GetBytes("abc").AsMemory();

            var pipe = new Pipe();
            var reader = pipe.Reader;
            var bencodeReader = new PipeBencodeReader(reader);

            var peekTask = bencodeReader.PeekCharAsync();

            await pipe.Writer.WriteAsync(bytes.Slice(0, 2));

            var c = await peekTask;

            c.Should().Be('a');

            (await bencodeReader.ReadCharAsync()).Should().Be('a');
        }

        #region BencodeReader tests

        [Fact]
        public async Task ReadBytesAsync()
        {
            Write("Hello World!");

            var output = new byte[12];
            var read = await PipeBencodeReader.ReadAsync(output);

            read.Should().Be(output.Length);
            Assert.Equal("Hello World!", Encoding.UTF8.GetString(output));
        }

        [Fact]
        public async Task ReadZeroBytesAsync()
        {
            Write("Hello World!");

            var output = new byte[0];
            var read = await PipeBencodeReader.ReadAsync(output);

            read.Should().Be(output.Length);
            Assert.Equal("", Encoding.UTF8.GetString(output));
        }

        [Fact]
        public async Task ReadMoreBytesThanInStream()
        {
            Write("Hello World!");

            var bytes = new byte[20];
            var read = await PipeBencodeReader.ReadAsync(bytes);

            read.Should().Be(12);
            Assert.Equal("Hello World!", Encoding.UTF8.GetString(bytes, 0, (int) read));
        }

        [Fact]
        public async Task ReadCharChangesStreamPosition()
        {
            Write("Hello World!");

            PipeBencodeReader.Position.Should().Be(0);
            await PipeBencodeReader.ReadCharAsync();
            PipeBencodeReader.Position.Should().Be(1);
            await PipeBencodeReader.ReadCharAsync();
            PipeBencodeReader.Position.Should().Be(2);
        }

        [Fact]
        public async Task ReadBytesChangesStreamPosition()
        {
            var str = "Hello World!";
            Write(str);

            Assert.Equal(0, PipeBencodeReader.Position);

            var bytes = new byte[str.Length];
            var read = await PipeBencodeReader.ReadAsync(bytes);

            read.Should().Be(12);
            PipeBencodeReader.Position.Should().Be(12);
            Assert.Equal(str, Encoding.UTF8.GetString(bytes));
        }

        [Fact]
        public async Task PeekCharDoesNotChangeStreamPosition()
        {
            Write("Hello World!");

            PipeBencodeReader.Position.Should().Be(0);
            await PipeBencodeReader.PeekCharAsync();
            PipeBencodeReader.Position.Should().Be(0);
            await PipeBencodeReader.PeekCharAsync();
            PipeBencodeReader.Position.Should().Be(0);
        }


        [Fact]
        public async Task ReadCharAfterPeekCharChangesStreamPosition()
        {
            Write("Hello World!");

            PipeBencodeReader.Position.Should().Be(0);
            await PipeBencodeReader.PeekCharAsync();
            PipeBencodeReader.Position.Should().Be(0);
            await PipeBencodeReader.ReadCharAsync();
            PipeBencodeReader.Position.Should().Be(1);
        }

        [Fact]
        public async Task ReadChar()
        {
            Write("Hello World!");

            Assert.Equal('H', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('e', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('l', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('l', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('o', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal(' ', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('W', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('o', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('r', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('l', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('d', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('!', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal(default, await PipeBencodeReader.ReadCharAsync());
        }

        [Fact]
        public async Task PreviousChar()
        {
            Write("Hello World!");

            PipeBencodeReader.PreviousChar.Should().Be(default);

            Assert.Equal('H', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('H', PipeBencodeReader.PreviousChar);
            Assert.Equal('e', await PipeBencodeReader.ReadCharAsync());
            Assert.Equal('e', PipeBencodeReader.PreviousChar);

            await PipeBencodeReader.ReadAsync(new byte[20]);

            Assert.Equal('!', PipeBencodeReader.PreviousChar);
        }

        [Fact]
        public void PreviousCharAtStartOfStream()
        {
            Write("Hello World!");

            PipeBencodeReader.PreviousChar.Should().Be(default);
        }

        [Fact]
        public async Task PreviousCharUnaffectedByPeekCharAsync()
        {
            Write("Hello World!");

            await PipeBencodeReader.SkipBytesAsync(1);
            Assert.Equal('H', PipeBencodeReader.PreviousChar);
            Assert.Equal('e', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('H', PipeBencodeReader.PreviousChar);
        }

        [Fact]
        public async Task PeekCharAsync()
        {
            Write("Hello World!");

            Assert.Equal('H', await PipeBencodeReader.PeekCharAsync());
        }

        [Fact]
        public async Task PeekDoesNotAdvanceStreamPosition()
        {
            Write("Hello World!");

            PipeBencodeReader.Position.Should().Be(0);
            Assert.Equal('H', await PipeBencodeReader.PeekCharAsync());
            PipeBencodeReader.Position.Should().Be(0);
            Assert.Equal('H', await PipeBencodeReader.PeekCharAsync());
            PipeBencodeReader.Position.Should().Be(0);
        }

        [Fact]
        public async Task PeekAndReadEqual()
        {
            Write("Hello World!");

            Assert.Equal('H', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('H', await PipeBencodeReader.ReadCharAsync());
        }

        [Fact]
        public async Task PeekAreChangedAfterReadChar()
        {
            Write("abcdefghijkl");

            Assert.Equal('a', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('a', await PipeBencodeReader.ReadCharAsync());

            Assert.Equal('b', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('b', await PipeBencodeReader.ReadCharAsync());

            Assert.Equal('c', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('c', await PipeBencodeReader.ReadCharAsync());

            Assert.Equal('d', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('d', await PipeBencodeReader.ReadCharAsync());

            Assert.Equal('e', await PipeBencodeReader.PeekCharAsync());
            Assert.Equal('e', await PipeBencodeReader.ReadCharAsync());
        }

        [Fact]
        public async Task PeekAreChangedAfterReadSingleByte()
        {
            Write("abcdefghijkl");

            var buffer = new byte[1];
            long read;

            Assert.Equal('a', await PipeBencodeReader.PeekCharAsync());

            read = await PipeBencodeReader.ReadAsync(buffer);
            Assert.Equal(1, read);
            Assert.Equal('a', (char)buffer[0]);
            Assert.Equal('b', await PipeBencodeReader.PeekCharAsync());

            read = await PipeBencodeReader.ReadAsync(buffer);
            Assert.Equal(1, read);
            Assert.Equal('b', (char)buffer[0]);
            Assert.Equal('c', await PipeBencodeReader.PeekCharAsync());
        }

        [Fact]
        public async Task PeekAreChangedAfterReadMultipleBytes()
        {
            Write("abcdefghijkl");

            var buffer = new byte[2];

            Assert.Equal('a', await PipeBencodeReader.PeekCharAsync());

            var read = await PipeBencodeReader.ReadAsync(buffer);
            read.Should().Be(2);
            Assert.Equal('a', (char)buffer[0]);
            Assert.Equal('b', (char)buffer[1]);
            Assert.Equal('c', await PipeBencodeReader.PeekCharAsync());

            read = await PipeBencodeReader.ReadAsync(buffer);
            read.Should().Be(2);
            Assert.Equal('c', (char)buffer[0]);
            Assert.Equal('d', (char)buffer[1]);
            Assert.Equal('e', await PipeBencodeReader.PeekCharAsync());
        }

        [Fact]
        public async Task PeekAtEndOfStreamThenReadSingleByte()
        {
            Write("abcdefghijkl");

            await PipeBencodeReader.SkipBytesAsync(12);
            Assert.Equal(default, await PipeBencodeReader.PeekCharAsync());
            Assert.Equal(default, await PipeBencodeReader.ReadCharAsync());
        }

        [Fact]
        public async Task PeekAtEndOfStreamThenReadBytes()
        {
            Write("abcdefghijkl");

            await PipeBencodeReader.SkipBytesAsync(12);
            Assert.Equal(default, await PipeBencodeReader.PeekCharAsync());
            Assert.Equal(0, await PipeBencodeReader.ReadAsync(new byte[4]));
        }

        #endregion

        private void Write(string str)
        {
            PipeWriter.Write(Encoding.UTF8.GetBytes(str));
            PipeWriter.Complete();
        }
    }
}
