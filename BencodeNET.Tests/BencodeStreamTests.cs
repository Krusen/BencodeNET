using System;
using System.IO;
using System.Text;
using BencodeNET.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeStreamTests
    {
        [TestMethod]
        public void ReadBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(12);
                Assert.AreEqual(12, bytes.Length);
                Assert.AreEqual("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [TestMethod]
        public void ReadZeroBytes()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(0);
                Assert.AreEqual(0, bytes.Length);
                Assert.AreEqual("", Encoding.UTF8.GetString(bytes));
            }
        }

        [TestMethod]
        public void ReadMoreBytesThanInStream()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")))
            using (var bs = new BencodeStream(ms))
            {
                var bytes = bs.Read(20);
                Assert.AreEqual(12, bytes.Length);
                Assert.AreEqual("Hello World!", Encoding.UTF8.GetString(bytes));
            }
        }

        [TestMethod]
        public void ReadBytesChangesStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual(0, bs.Position);
                
                var bytes = bs.Read(str.Length);
                Assert.AreEqual(12, bytes.Length);
                Assert.AreEqual(str, Encoding.UTF8.GetString(bytes));
                
                Assert.AreEqual(12, bs.Position);
            }
        }

        [TestMethod]
        public void Read()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('H', bs.Read());
                Assert.AreEqual('e', bs.Read());
                Assert.AreEqual('l', bs.Read());
                Assert.AreEqual('l', bs.Read());
                Assert.AreEqual('o', bs.Read());
                Assert.AreEqual(' ', bs.Read());
                Assert.AreEqual('W', bs.Read());
                Assert.AreEqual('o', bs.Read());
                Assert.AreEqual('r', bs.Read());
                Assert.AreEqual('l', bs.Read());
                Assert.AreEqual('d', bs.Read());
                Assert.AreEqual('!', bs.Read());
                Assert.AreEqual(-1, bs.Read());
            }
        }

        [TestMethod]
        public void ReadChangeStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('H', bs.Read());
                Assert.AreEqual('e', bs.Read());
                bs.Position -= 1;
                Assert.AreEqual('e', bs.Read());
            }
        }

        [TestMethod]
        public void ReadPrevious()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual(-1, bs.ReadPrevious());
                Assert.AreEqual('H', bs.Read());
                Assert.AreEqual('H', bs.ReadPrevious());
                Assert.AreEqual('e', bs.Read());
                Assert.AreEqual('e', bs.ReadPrevious());

                bs.Position = 20;

                Assert.AreEqual(-1, bs.ReadPrevious());
            }
        }

        [TestMethod]
        public void ReadPreviousAtStartOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual(-1, bs.ReadPrevious());
            }
        }

        [TestMethod]
        public void ReadPreviousUnaffectedByPeek()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(1);
                Assert.AreEqual('H', bs.ReadPrevious());
                Assert.AreEqual('e', bs.Peek());
                Assert.AreEqual('H', bs.ReadPrevious());
            }
        }

        [TestMethod]
        public void ReadPreviousChar()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual(default(char), bs.ReadPreviousChar());
                bs.Read(1);
                Assert.AreEqual('H', bs.ReadPreviousChar());
            }
        }

        [TestMethod]
        public void PeekUnnaffectedByReadPrevious()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(0);
                Assert.AreEqual('a', bs.Peek());
                bs.ReadPrevious();
                Assert.AreEqual('a', bs.Peek());

                bs.Read(1);
                Assert.AreEqual('b', bs.Peek());
                bs.ReadPrevious();
                Assert.AreEqual('b', bs.Peek());
            }
        }

        [TestMethod]
        public void ReadUnnaffectedByReadPrevious()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('a', bs.Read());
                bs.ReadPrevious();
                Assert.AreEqual('b', bs.Read());
            }
        }
        
        [TestMethod]
        public void Peek()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('H', bs.Peek());
            }
        }
        
        [TestMethod]
        public void PeekDoesNotAdvanceStreamPosition()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual(0, bs.Position);
                Assert.AreEqual('H', bs.Peek());
                Assert.AreEqual(0, bs.Position);
                Assert.AreEqual('H', bs.Peek());
                Assert.AreEqual(0, bs.Position);
            }
        }
        
        [TestMethod]
        public void PeekAndReadAreEqual()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('H', bs.Peek());
                Assert.AreEqual('H', bs.Read());
            }
        }
        
        [TestMethod]
        public void PeekAreChangedAfterRead()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                Assert.AreEqual('a', bs.Peek());
                Assert.AreEqual('a', bs.Read());

                Assert.AreEqual('b', bs.Peek());
                Assert.AreEqual('b', bs.Read());

                Assert.AreEqual('c', bs.Peek());
                Assert.AreEqual('c', bs.Read());

                Assert.AreEqual('d', bs.Peek());
                Assert.AreEqual('d', bs.Read());

                Assert.AreEqual('e', bs.Peek());
                Assert.AreEqual('e', bs.Read());
            }
        }
        
        [TestMethod]
        public void PeekAreChangedAfterReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;
                
                Assert.AreEqual('a', bs.Peek());

                bytes = bs.Read(1);
                Assert.AreEqual('a', (char)bytes[0]);
                Assert.AreEqual('b', bs.Peek());

                bytes = bs.Read(1);
                Assert.AreEqual('b', (char)bytes[0]);
                Assert.AreEqual('c', bs.Peek());
            }
        }
        
        [TestMethod]
        public void PeekAreChangedAfterReadMutipleBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                byte[] bytes;

                Assert.AreEqual('a', bs.Peek());

                bytes = bs.Read(2);
                Assert.AreEqual('a', (char)bytes[0]);
                Assert.AreEqual('b', (char)bytes[1]);
                Assert.AreEqual('c', bs.Peek());

                bytes = bs.Read(2);
                Assert.AreEqual('c', (char)bytes[0]);
                Assert.AreEqual('d', (char)bytes[1]);
                Assert.AreEqual('e', bs.Peek());
            }
        }
        
        [TestMethod]
        public void PeekAtEndOfStreamThenReadSingleByte()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.AreEqual(-1, bs.Peek());
                Assert.AreEqual(-1, bs.Read());
            }
        }
        
        [TestMethod]
        public void PeekAtEndOfStreamThenReadBytes()
        {
            var str = "abcdefghijkl";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.AreEqual(-1, bs.Peek());
                Assert.AreEqual(0, bs.Read(4).Length);
            }
        }

        [TestMethod]
        public void EndOfStream()
        {
            var str = "Hello World!";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var bs = new BencodeStream(ms))
            {
                bs.Read(12);
                Assert.IsTrue(bs.EndOfStream);
                Assert.AreEqual(-1, bs.Read());
            }
        }
    }
}
