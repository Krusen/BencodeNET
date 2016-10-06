using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    // TODO: Rename CanParse tests to better explain expected behavior/outcome
    public class TorrentParserTests
    {
        private BDictionary ValidSingleFileTorrentData { get; }

        private BDictionary ValidMultiFileTorrentData { get; }

        private IBencodeParser BencodeParser { get; }

        private BDictionary ParsedData { get; set; }

        public TorrentParserTests()
        {
            BencodeParser = Substitute.For<IBencodeParser>();
            BencodeParser.Parse<BDictionary>((BencodeStream) null).ReturnsForAnyArgs(x => ParsedData);

            ValidSingleFileTorrentData = new BDictionary
            {
                [TorrentFields.Info] = new BDictionary
                {
                    [TorrentInfoFields.Name] = (BString)"",
                    [TorrentInfoFields.Pieces] = (BString)"",
                    [TorrentInfoFields.PieceLength] = (BNumber)0,
                    [TorrentInfoFields.Length] = (BNumber)0
                },
            };

            ValidMultiFileTorrentData = new BDictionary
            {
                [TorrentFields.Info] = new BDictionary
                {
                    [TorrentInfoFields.Name] = (BString)"",
                    [TorrentInfoFields.Pieces] = (BString)"",
                    [TorrentInfoFields.PieceLength] = (BNumber)0,
                    [TorrentInfoFields.Files] = new BList()
                },
            };
        }

        [Theory]
        [AutoMockedData]
        public void CanParseComment(string comment)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Comment] = (BString) comment;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.Comment.Should().Be(comment);
        }

        [Theory]
        [AutoMockedData]
        public void CanParseCreatedBy(string createdBy)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreatedBy] = (BString) createdBy;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.CreatedBy.Should().Be(createdBy);
        }

        [Theory]
        [AutoMockedData]
        public void CanParseCreationDate()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreationDate] = (BNumber) 1451606400;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.CreationDate.Should().Be(new DateTime(2016, 1, 1));
        }

        [Theory]
        [InlineAutoMockedData("utf8")]
        [InlineAutoMockedData("UTF8")]
        [InlineAutoMockedData("utf-8")]
        [InlineAutoMockedData("UTF-8")]
        public void CanParseEncoding_UTF8(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString) encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.Encoding.Should().Be(Encoding.UTF8);
        }

        [Theory]
        [InlineAutoMockedData("ascii")]
        [InlineAutoMockedData("ASCII")]
        public void CanParseEncoding_ASCII(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString) encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.Encoding.Should().Be(Encoding.ASCII);
        }

        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData("asdf")]
        [InlineAutoMockedData("1")]
        [InlineAutoMockedData("UTF 8")]
        public void CanParseEncoding_InvalidValueIsNull(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString) encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream) null);

            // Assert
            torrent.Encoding.Should().Be(null);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_Info_PieceLength(long pieceSize)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.PieceLength] = (BNumber) pieceSize;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.PieceSize.Should().Be(pieceSize);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_Info_Pieces(string pieces)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Pieces] = (BString) pieces;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.Pieces.Should().Be(pieces);
        }

        [Theory]
        [InlineAutoMockedData(-1, false)]
        [InlineAutoMockedData(0, false)]
        [InlineAutoMockedData(1, true)]
        [InlineAutoMockedData(42, false)]
        [InlineAutoMockedData(12345, false)]
        public void CanParse_Info_Private_ShouldBeTrueIfValueIsOne(int value, bool expectedResult)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Private] = (BNumber) value;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.IsPrivate.Should().Be(expectedResult);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_ExtraFields(string extraKey, string extraValue, string extraInfoKey, string extraInfoValue)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[extraKey] = (BString) extraValue;
            ParsedData.Get<BDictionary>(TorrentFields.Info)[extraInfoKey] = (BString) extraInfoValue;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.ExtraFields.Should().Contain(extraKey, (BString) extraValue);
            torrent.ExtraFields.Get<BDictionary>(TorrentFields.Info).Should().Contain(extraInfoKey, (BString) extraInfoValue);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_Announce(string announceUrl)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString) announceUrl;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(1);
            torrent.Trackers[0][0].Should().Be(announceUrl);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_AnnounceList_Single(IList<string> announceList)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList(announceList)
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(announceList.Count);
            torrent.Trackers[0].ShouldAllBeEquivalentTo(announceList);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_AnnounceList_Multiple(IList<string> announceList1, IList<string> announceList2)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList(announceList1),
                new BList(announceList2)
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.Trackers.Should().HaveCount(2);
            torrent.Trackers[0].Should().HaveCount(announceList1.Count);
            torrent.Trackers[0].ShouldAllBeEquivalentTo(announceList1);
            torrent.Trackers[1].Should().HaveCount(announceList2.Count);
            torrent.Trackers[1].ShouldAllBeEquivalentTo(announceList2);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_AnnounceAndAnnounceList(string announceUrl, IList<string> announceList1, IList<string> announceList2)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString) announceUrl;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList(announceList1),
                new BList(announceList2)
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);


            // Assert
            var primary = new List<string> {announceUrl};
            primary.AddRange(announceList1);
            torrent.Trackers.Should().HaveCount(2);
            torrent.Trackers[0].Should().HaveCount(primary.Count);
            torrent.Trackers[0].ShouldAllBeEquivalentTo(primary);
            torrent.Trackers[1].Should().HaveCount(announceList2.Count);
            torrent.Trackers[1].ShouldAllBeEquivalentTo(announceList2);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_AnnounceAndAnnounceList_DoesNotContainDuplicatesInPrimary(string announceUrl1, string announceUrl2)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString) announceUrl1;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList { announceUrl1, announceUrl2}
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(2);
            torrent.Trackers[0].Should().ContainInOrder(announceUrl1, announceUrl2);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_SingleFileInfo(long length, string fileName, string md5Sum)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Length] = (BNumber) length;
            info[TorrentInfoFields.Name] = (BString) fileName;
            info[TorrentInfoFields.Md5Sum] = (BString) md5Sum;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.FileMode.Should().Be(TorrentFileMode.Single);
            torrent.TotalSize.Should().Be(length);
            torrent.File.Should().NotBeNull();
            torrent.File.FileSize.Should().Be(length);
            torrent.File.FileName.Should().Be(fileName);
            torrent.File.Md5Sum.Should().Be(md5Sum);
        }

        [Theory]
        [AutoMockedData]
        public void CanParse_MultiFileInfo(string directoryName, long length1, IList<string> paths1, string md5Sum1, long length2, IList<string> paths2, string md5Sum2)
        {
            // Arrange
            ParsedData = ValidMultiFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Name] = (BString) directoryName;
            info[TorrentInfoFields.Files] = new BList<BDictionary>
            {
                new BDictionary
                {
                    [TorrentFilesFields.Length] = (BNumber) length1,
                    [TorrentFilesFields.Path] = new BList(paths1),
                    [TorrentFilesFields.Md5Sum] = (BString) md5Sum1
                },
                new BDictionary
                {
                    [TorrentFilesFields.Length] = (BNumber) length2,
                    [TorrentFilesFields.Path] = new BList(paths2),
                    [TorrentFilesFields.Md5Sum] = (BString) md5Sum2
                }
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeStream)null);

            // Assert
            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
            torrent.TotalSize.Should().Be(length1 + length2);
            torrent.Files.DirectoryName.Should().Be(directoryName);
            torrent.Files.Should().HaveCount(2);
            torrent.Files[0].FileSize.Should().Be(length1);
            torrent.Files[0].Path.ShouldAllBeEquivalentTo(paths1);
            torrent.Files[0].Md5Sum.Should().Be(md5Sum1);
            torrent.Files[1].FileSize.Should().Be(length2);
            torrent.Files[1].Path.ShouldAllBeEquivalentTo(paths2);
            torrent.Files[1].Md5Sum.Should().Be(md5Sum2);
        }
    }
}
