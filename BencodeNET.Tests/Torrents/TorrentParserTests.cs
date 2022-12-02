using System;
using System.Collections.Generic;
using System.Text;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Tests.AutoFixture;
using BencodeNET.Torrents;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BencodeNET.Tests.Torrents
{
    public class TorrentParserTests
    {
        private BDictionary ValidSingleFileTorrentData { get; }

        private BDictionary ValidMultiFileTorrentData { get; }

        private IBencodeParser BencodeParser { get; }

        private BDictionary ParsedData { get; set; }

        public TorrentParserTests()
        {
            BencodeParser = Substitute.For<IBencodeParser>();
            BencodeParser.Parse<BDictionary>(null).ReturnsForAnyArgs(x => ParsedData);

            ValidSingleFileTorrentData = new BDictionary
            {
                [TorrentFields.Info] = new BDictionary
                {
                    [TorrentInfoFields.Name] = (BString)"",
                    [TorrentInfoFields.NameUtf8] = (BString)"",
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
                    [TorrentInfoFields.NameUtf8] = (BString)"",
                    [TorrentInfoFields.Pieces] = (BString)"",
                    [TorrentInfoFields.PieceLength] = (BNumber)0,
                    [TorrentInfoFields.Files] = new BList()
                },
            };
        }

        [Theory]
        [AutoMockedData]
        public void Comment_IsParsed(string comment)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Comment] = (BString)comment;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Comment.Should().Be(comment);
        }

        [Theory]
        [AutoMockedData]
        public void CreatedBy_IsParsed(string createdBy)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreatedBy] = (BString)createdBy;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.CreatedBy.Should().Be(createdBy);
        }

        [Fact]
        public void CreationDate_IsParsed()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreationDate] = (BNumber)1451606400;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.CreationDate.Should().Be(new DateTime(2016, 1, 1));
        }

        [Fact]
        public void CreationDate_UnixTimeInMilliseconds_IsParsed()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreationDate] = (BNumber)1451606400000;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.CreationDate.Should().Be(new DateTime(2016, 1, 1));
        }

        [Fact]
        public void CreationDate_InvalidValue_ReturnsEpoch()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.CreationDate] = (BNumber)long.MaxValue;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.CreationDate.Should().Be(new DateTime(1970, 1, 1));
        }

        [Theory]
        [AutoMockedData("utf8")]
        [AutoMockedData("UTF8")]
        [AutoMockedData("utf-8")]
        [AutoMockedData("UTF-8")]
        public void Encoding_UTF8_CanBeParsed(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString)encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Encoding.Should().Be(Encoding.UTF8);
        }

        [Theory]
        [AutoMockedData("ascii")]
        [AutoMockedData("ASCII")]
        public void Encoding_ASCII_CanBeParsed(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString)encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Encoding.Should().Be(Encoding.ASCII);
        }

        [Theory]
        [AutoMockedData("")]
        [AutoMockedData("asdf")]
        [AutoMockedData("1")]
        [AutoMockedData("UTF 8")]
        public void Encoding_InvalidValidAsNull(string encoding)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString)encoding;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Encoding.Should().Be(null);
        }

        [Theory]
        [AutoMockedData]
        public void Info_PieceLength_IsParsed(long pieceSize)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.PieceLength] = (BNumber)pieceSize;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.PieceSize.Should().Be(pieceSize);
        }

        [Theory]
        [AutoMockedData]
        public void Info_Pieces_IsParsed([RepeatCount(20)] byte[] pieces)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Pieces] = new BString(pieces);
            pieces.Should().HaveCount(20);

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Pieces.Should().Equal(pieces);
        }

        [Theory]
        [AutoMockedData(-1, false)]
        [AutoMockedData(0, false)]
        [AutoMockedData(1, true)]
        [AutoMockedData(42, false)]
        [AutoMockedData(12345, false)]
        public void Info_Private_ShouldBeTrueOnlyIfValueIsOne(int value, bool expectedResult)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Private] = (BNumber)value;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.IsPrivate.Should().Be(expectedResult);
        }

        [Theory]
        [AutoMockedData]
        public void ExtraFields_IsParsed(string extraKey, string extraValue, string extraInfoKey, string extraInfoValue)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[extraKey] = (BString)extraValue;
            ParsedData.Get<BDictionary>(TorrentFields.Info)[extraInfoKey] = (BString)extraInfoValue;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.ExtraFields.Should().Contain(extraKey, (BString)extraValue);
            torrent.ExtraFields.Get<BDictionary>(TorrentFields.Info).Should().Contain(extraInfoKey, (BString)extraInfoValue);
        }

        [Theory]
        [AutoMockedData]
        public void Announce_IsParsed(string announceUrl)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString)announceUrl;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(1);
            torrent.Trackers[0][0].Should().Be(announceUrl);
        }

        [Theory]
        [AutoMockedData]
        public void AnnounceList_Single_IsParsed(IList<string> announceList)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList(announceList)
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(announceList.Count);
            torrent.Trackers[0].Should().BeEquivalentTo(announceList);
        }

        [Theory]
        [AutoMockedData]
        public void AnnounceList_Multiple_IsParsed(IList<string> announceList1, IList<string> announceList2)
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
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Trackers.Should().HaveCount(2);
            torrent.Trackers[0].Should().HaveCount(announceList1.Count);
            torrent.Trackers[0].Should().BeEquivalentTo(announceList1);
            torrent.Trackers[1].Should().HaveCount(announceList2.Count);
            torrent.Trackers[1].Should().BeEquivalentTo(announceList2);
        }

        [Theory]
        [AutoMockedData]
        public void AnnounceList_Multiple_StringsInsteadOfLists_IsParsed(string announce1, string announce2, string announce3)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BString(announce1),
                new BString(announce2),
                new BString(announce3),
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(3);
            torrent.Trackers[0][0].Should().Be(announce1);
            torrent.Trackers[0][1].Should().Be(announce2);
            torrent.Trackers[0][2].Should().Be(announce3);
        }

        [Theory]
        [AutoMockedData]
        public void AnnounceAndAnnounceList_IsParsed(string announceUrl, IList<string> announceList1, IList<string> announceList2)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString)announceUrl;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList(announceList1),
                new BList(announceList2)
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);


            // Assert
            var primary = new List<string> { announceUrl };
            primary.AddRange(announceList1);
            torrent.Trackers.Should().HaveCount(2);
            torrent.Trackers[0].Should().HaveCount(primary.Count);
            torrent.Trackers[0].Should().BeEquivalentTo(primary);
            torrent.Trackers[1].Should().HaveCount(announceList2.Count);
            torrent.Trackers[1].Should().BeEquivalentTo(announceList2);
        }

        [Theory]
        [AutoMockedData]
        public void AnnounceAndAnnounceList_DoesNotContainDuplicatesInPrimaryList(string announceUrl1, string announceUrl2)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Announce] = (BString)announceUrl1;
            ParsedData[TorrentFields.AnnounceList] = new BList
            {
                new BList { announceUrl1, announceUrl2}
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Trackers.Should().HaveCount(1);
            torrent.Trackers[0].Should().HaveCount(2);
            torrent.Trackers[0].Should().ContainInOrder(announceUrl1, announceUrl2);
        }

        [Theory]
        [AutoMockedData]
        public void SingleFileInfo_IsParsed(long length, string fileName, string fileNameUtf8, string md5Sum)
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Length] = (BNumber)length;
            info[TorrentInfoFields.Name] = (BString)fileName;
            info[TorrentInfoFields.NameUtf8] = (BString)fileNameUtf8;
            info[TorrentInfoFields.Md5Sum] = (BString)md5Sum;

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.Files.Should().BeNull();
            torrent.FileMode.Should().Be(TorrentFileMode.Single);
            torrent.TotalSize.Should().Be(length);
            torrent.File.Should().NotBeNull();
            torrent.File.FileSize.Should().Be(length);
            torrent.File.FileName.Should().Be(fileName);
            torrent.File.FileNameUtf8.Should().Be(fileNameUtf8);
            torrent.File.Md5Sum.Should().Be(md5Sum);
        }

        [Fact]
        public void SingleFileInfo_NameUtf8_IsParsedAsUtf8EncodingIndependentlyOfActualEncoding()
        {
            var encoding = "ISO-8859-1";
            var fileName = "øæå"; // Use characters with different byte values for UTF8 and ISO-8859-1

            ParsedData = ValidSingleFileTorrentData;
            ParsedData[TorrentFields.Encoding] = (BString)encoding;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Name] = new BString(fileName, Encoding.GetEncoding(encoding));
            info[TorrentInfoFields.NameUtf8] = new BString(fileName, Encoding.UTF8);

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.File.FileName.Should().Be(fileName);
            torrent.File.FileNameUtf8.Should().Be(fileName);
        }

        [Theory]
        [AutoMockedData]
        public void MultiFileInfo_IsParsed(string directoryName, long length1, IList<string> paths1, IList<string> paths1Utf8, string md5Sum1, long length2, IList<string> paths2, IList<string> paths2Utf8, string md5Sum2)
        {
            // Arrange
            ParsedData = ValidMultiFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Name] = (BString)directoryName;
            info[TorrentInfoFields.Files] = new BList<BDictionary>
            {
                new BDictionary
                {
                    [TorrentFilesFields.Length] = (BNumber) length1,
                    [TorrentFilesFields.Path] = new BList(paths1),
                    [TorrentFilesFields.PathUtf8] = new BList(paths1Utf8),
                    [TorrentFilesFields.Md5Sum] = (BString) md5Sum1
                },
                new BDictionary
                {
                    [TorrentFilesFields.Length] = (BNumber) length2,
                    [TorrentFilesFields.Path] = new BList(paths2),
                    [TorrentFilesFields.PathUtf8] = new BList(paths2Utf8),
                    [TorrentFilesFields.Md5Sum] = (BString) md5Sum2
                }
            };

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.File.Should().BeNull();
            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
            torrent.TotalSize.Should().Be(length1 + length2);
            torrent.Files.DirectoryName.Should().Be(directoryName);
            torrent.Files.Should().HaveCount(2);
            torrent.Files[0].FileSize.Should().Be(length1);
            torrent.Files[0].Path.Should().BeEquivalentTo(paths1);
            torrent.Files[0].PathUtf8.Should().BeEquivalentTo(paths1Utf8);
            torrent.Files[0].Md5Sum.Should().Be(md5Sum1);
            torrent.Files[1].FileSize.Should().Be(length2);
            torrent.Files[1].Path.Should().BeEquivalentTo(paths2);
            torrent.Files[1].PathUtf8.Should().BeEquivalentTo(paths2Utf8);
            torrent.Files[1].Md5Sum.Should().Be(md5Sum2);
        }

        [Fact]
        public void Root_MissingInfoField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            ParsedData.Remove(TorrentFields.Info);

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentFields.Info);
        }

        [Fact]
        public void Info_ContainingBothLengthAndFilesField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Length] = (BNumber)1;
            info[TorrentInfoFields.Files] = new BList();

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .WithMessage($"*{TorrentInfoFields.Length}*")
                .WithMessage($"*{TorrentInfoFields.Files}*");
        }

        [Fact]
        public void Info_MissingPieceLengthField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info.Remove(TorrentInfoFields.PieceLength);

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentInfoFields.PieceLength);
        }

        [Fact]
        public void Info_MissingPiecesField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info.Remove(TorrentInfoFields.Pieces);

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentInfoFields.Pieces);
        }

        [Fact]
        public void Info_MissingNameField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info.Remove(TorrentInfoFields.Name);

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentInfoFields.Name);
        }

        [Fact]
        public void MultiFileInfo_MissingFilesField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidMultiFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info.Remove(TorrentInfoFields.Files);

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentInfoFields.Files);
        }

        [Fact]
        public void MultiFile_Files_MissingLengthField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidMultiFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Files] = new BList<BDictionary>
            {
                new BDictionary
                {
                    [TorrentFilesFields.Path] = new BList()
                }
            };

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentFilesFields.Length);
        }

        [Fact]
        public void MultiFile_Files_MissingPathField_Strict_ThrowsInvalidTorrentException()
        {
            // Arrange
            ParsedData = ValidMultiFileTorrentData;
            var info = ParsedData.Get<BDictionary>(TorrentFields.Info);
            info[TorrentInfoFields.Files] = new BList<BDictionary>
            {
                new BDictionary
                {
                    [TorrentFilesFields.Length] = (BNumber) 1
                }
            };

            // Act
            var parser = new TorrentParser(BencodeParser, TorrentParserMode.Strict);
            Action action = () => parser.Parse((BencodeReader)null);

            // Assert
            action.Should().Throw<InvalidTorrentException>()
                .Where(ex => ex.InvalidField == TorrentFilesFields.Path);
        }

        [Fact]
        public void OriginalInfoHash_IsSet()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var expectedInfoHash = TorrentUtil.CalculateInfoHash(ParsedData.Get<BDictionary>("info"));

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.OriginalInfoHash.Should().Be(expectedInfoHash);
        }

        [Fact]
        public void OriginalInfoHashBytes_IsSet()
        {
            // Arrange
            ParsedData = ValidSingleFileTorrentData;
            var expectedInfoHashBytes = TorrentUtil.CalculateInfoHashBytes(ParsedData.Get<BDictionary>("info"));

            // Act
            var parser = new TorrentParser(BencodeParser);
            var torrent = parser.Parse((BencodeReader)null);

            // Assert
            torrent.OriginalInfoHashBytes.Should().Equal(expectedInfoHashBytes);
        }
    }
}
