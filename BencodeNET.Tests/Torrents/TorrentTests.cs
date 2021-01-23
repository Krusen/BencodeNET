using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture.Xunit2;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Torrents;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Torrents
{
    public class TorrentTests
    {
        [Fact]
        public void FileMode_IsSingleFile_IfFilesIsEmptyAndFileIsNotNull()
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo(),
                Files = new MultiFileInfoList()
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Single);
        }

        [Fact]
        public void FileMode_IsMultiFile_IfFilesIsNotEmpty()
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList
                {
                    new MultiFileInfo()
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
        }

        [Fact]
        public void FileMode_IsUnknown_IfFileIsNullAndFilesIsEmpty()
        {
            var torrent = new Torrent
            {
                File = null,
                Files = new MultiFileInfoList()
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Unknown);
        }

        [Theory]
        [AutoMockedData]
        public void DisplayName_SingleFile_IsFileName(string fileName)
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileName = fileName
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Single);
            torrent.DisplayName.Should().Be(fileName);
        }

        [Theory]
        [AutoMockedData]
        public void DisplayName_MultiFile_IsDirectoryName(string directoryName)
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList(directoryName)
                {
                    new MultiFileInfo()
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
            torrent.DisplayName.Should().Be(directoryName);
        }

        [Fact]
        public void DisplayName_UnknownFileMode_ThrowsBencodeException()
        {
            var torrent = new Torrent();

            Func<string> act = () => torrent.DisplayName;

            torrent.FileMode.Should().Be(TorrentFileMode.Unknown);
            act.Should().Throw<BencodeException>();
        }

        [Theory]
        [AutoMockedData]
        public void DisplayNameUtf8_SingleFile_IsFileName(string fileName)
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileNameUtf8 = fileName
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Single);
            torrent.DisplayNameUtf8.Should().Be(fileName);
        }

        [Theory]
        [AutoMockedData]
        public void DisplayNameUtf8_MultiFile_IsDirectoryName(string directoryNameUtf8)
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList(default, directoryNameUtf8)
                {
                    new MultiFileInfo()
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
            torrent.DisplayNameUtf8.Should().Be(directoryNameUtf8);
        }

        [Fact]
        public void DisplayNameUtf8_UnknownFileMode_ThrowsBencodeException()
        {
            var torrent = new Torrent();

            Func<string> act = () => torrent.DisplayNameUtf8;

            torrent.FileMode.Should().Be(TorrentFileMode.Unknown);
            act.Should().Throw<BencodeException>();
        }

        [Theory]
        [AutoMockedData]
        public void Pieces_Setter_NullValue_ThrowsArgumentNullException([NoAutoProperties] Torrent torrent)
        {
            Action act = () => torrent.Pieces = null;

            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [AutoMockedData]
        public void Pieces_Setter_NotMultipleOf20_ThrowsArgumentException([NoAutoProperties] Torrent torrent)
        {
            var pieces = new byte[] { 66, 115, 135, 19, 149, 125, 229, 85, 68, 117, 252, 185, 243, 247, 139, 38, 11, 37, 60 };
            pieces.Should().NotHaveCount(20);

            Action act = () => torrent.Pieces = pieces;

            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void PiecesAsHexString_Get_ReturnsHexUppercaseWithoutDashes()
        {
            var pieces = new byte[] { 66, 115, 135, 19, 149, 125, 229, 85, 68, 117, 252, 185, 243, 247, 139, 38, 11, 37, 60, 112 };
            var torrent = new Torrent
            {
                Pieces = pieces
            };

            torrent.PiecesAsHexString.Should().Be("42738713957DE5554475FCB9F3F78B260B253C70");
        }

        [Fact]
        public void PiecesAsHexString_Set_ValidUppercaseHexSetsPiecesProperty()
        {
            var pieces = new byte[] { 66, 115, 135, 19, 149, 125, 229, 85, 68, 117, 252, 185, 243, 247, 139, 38, 11, 37, 60, 112 };
            var torrent = new Torrent();

            torrent.PiecesAsHexString = "42738713957DE5554475FCB9F3F78B260B253C70";

            torrent.Pieces.Should().Equal(pieces);
        }

        [Fact]
        public void PiecesAsHexString_Set_NullThrowsArgumentException()
        {
            var torrent = new Torrent();
            Action action = () => torrent.PiecesAsHexString = null;
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [AutoMockedData("qwer")]
        [AutoMockedData("123456789012345678901234567890123456789")]
        [AutoMockedData("12345678901234567890123456789012345678901")]
        public void PiecesAsHexString_Set_LengthNotMultipleOf40ShouldThrowArgumentException(string value)
        {
            var torrent = new Torrent();
            Action action = () => torrent.PiecesAsHexString = value;
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [AutoMockedData("a")]
        [AutoMockedData("b")]
        [AutoMockedData("c")]
        [AutoMockedData("G")]
        [AutoMockedData("H")]
        [AutoMockedData("T")]
        [AutoMockedData(".")]
        [AutoMockedData("*")]
        [AutoMockedData("[")]
        public void PiecesAsHexString_Set_ValueWithNonUppercaseHexCharacterShouldThrowArgumentException(string value)
        {
            var torrent = new Torrent();
            Action action = () => torrent.PiecesAsHexString = value;
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [AutoMockedData]
        public void TotalSize_SingleFile_ShouldBeFileSize(long fileSize)
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileSize = fileSize
                }
            };

            torrent.TotalSize.Should().Be(fileSize);
        }

        [Theory]
        [AutoMockedData]
        public void TotalSize_MultiFile_ShouldBeSumOfFileSizes(long fileSize1, long fileSize2)
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList
                {
                    new MultiFileInfo {FileSize = fileSize1},
                    new MultiFileInfo {FileSize = fileSize2},
                }
            };

            torrent.TotalSize.Should().Be(fileSize1 + fileSize2);
        }

        [Fact]
        public void TotalSize_UnknownFileMode_IsZero()
        {
            var torrent = new Torrent();

            torrent.TotalSize.Should().Be(0);
        }

        [Theory]
        [AutoMockedData]
        public void NumberOfPieces_ShouldBePiecesLengthDividedBy20(int piecesByteCount)
        {
            while (piecesByteCount % 20 != 0) piecesByteCount--;
            var expected = piecesByteCount / 20;

            var torrent = new Torrent {Pieces = new byte[piecesByteCount]};

            torrent.Pieces.Length.Should().Be(piecesByteCount);
            torrent.NumberOfPieces.Should().Be(expected);
        }

        [Fact]
        public void ToBDictionary_Encoding_UsesWebNameToUpper()
        {
            var torrent = new Torrent { Encoding = Encoding.UTF8 };

            var result = torrent.ToBDictionary();

            result.Get<BString>(TorrentFields.Encoding).Should().Be(Encoding.UTF8.WebName.ToUpper());
        }

        [Fact]
        public void ToBDictionary_Encoding_AddsToCorrectField()
        {
            var torrent = new Torrent {Encoding = Encoding.UTF8};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.Encoding, (BString)Encoding.UTF8.WebName.ToUpper());
        }

        [Fact]
        public void ToBDictionary_Encoding_DoesNotAddNull()
        {
            var torrent = new Torrent {Encoding = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Encoding);
        }

        [Fact]
        public void ToBDictionary_Announce_NoTrackers_DoesNotAddAnnounce()
        {
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>()
                }
            };

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Announce);
        }

        [Fact]
        public void ToBDictionary_Announce_SingleTracker_AddsAnnounceButNotAddAnnounceList()
        {
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>
                    {
                        "http://sometracker.com"
                    }
                }
            };

            var result = torrent.ToBDictionary();

            result.Should().ContainKey(TorrentFields.Announce);
            result.Should().NotContainKey(TorrentFields.AnnounceList);
        }

        [Fact]
        public void ToBDictionary_Announce_SingleTracker_AnnounceMustBeAString()
        {
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>
                    {
                        "http://sometracker.com"
                    }
                }
            };

            var result = torrent.ToBDictionary();
            result[TorrentFields.Announce].Should().BeAssignableTo<BString>();
        }

        [Fact]
        public void ToBDictionary_Announce_MultipleTrackers_AddAnnounceAndAnnounceList()
        {
            const string firstTracker = "http://sometracker.com";
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>
                    {
                        firstTracker,
                        "http://someothertracker.com"
                    }
                }
            };

            var result = torrent.ToBDictionary();
            result.Should().ContainKey(TorrentFields.Announce);
            result.Should().ContainKey(TorrentFields.AnnounceList);
            result[TorrentFields.Announce].ToString().Should().Be(firstTracker);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Comment_AddsToCorrectField(string comment)
        {
            var torrent = new Torrent {Comment = comment
            };

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.Comment, (BString)comment);
        }

        [Fact]
        public void ToBDictionary_Comment_DoesNotAddNull()
        {
            var torrent = new Torrent {Comment = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Comment);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_CreatedBy_AddsToCorrectField(string createdBy)
        {
            var torrent = new Torrent {CreatedBy = createdBy};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.CreatedBy, (BString)createdBy);
        }

        [Fact]
        public void ToBDictionary_CreatedBy_DoesNotAddNull()
        {
            var torrent = new Torrent {CreatedBy = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.CreatedBy);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_CreationDate_AddsToCorrectField(DateTime creationDate)
        {
            var torrent = new Torrent {CreationDate = creationDate};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.CreationDate, (BNumber)creationDate);
        }

        [Fact]
        public void ToBDictionary_CreationDate_DoesNotAddNull()
        {
            var torrent = new Torrent {CreationDate = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.CreationDate);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Info_SingleFile_AddsNameAndLength(long fileSize, string fileName, string fileNameUtf8)
        {
            // Arrange
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName,
                    FileNameUtf8 = fileNameUtf8
                }
            };

            // Act
            var info = torrent.ToBDictionary().Get<BDictionary>("info");

            // Assert
            info.Should().Contain(TorrentInfoFields.Length, (BNumber)fileSize);
            info.Should().Contain(TorrentInfoFields.Name, (BString)fileName);
            info.Should().Contain(TorrentInfoFields.NameUtf8, (BString)fileNameUtf8);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Info_MultiFile_AddsFiles(string directoryName, long fileSize, IList<string> path, IList<string> pathUtf8)
        {
            // Arrange
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList(directoryName)
                {
                    new MultiFileInfo
                    {
                        FileSize = fileSize,
                        Path = path,
                        PathUtf8 = pathUtf8
                    }
                }
            };

            // Act
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            var files = info.Get<BList>(TorrentInfoFields.Files).AsType<BDictionary>();

            // Assert
            info.Should().Contain(TorrentInfoFields.Name, (BString) directoryName);
            info.Should().ContainKey(TorrentInfoFields.Files);
            info[TorrentInfoFields.Files].Should().BeOfType<BList<BDictionary>>();

            var file = files[0];
            file.Should().BeOfType<BDictionary>();
            file.Should().Contain(TorrentFilesFields.Length, (BNumber) fileSize);
            file.Should().ContainKey(TorrentFilesFields.Path);
            file.Should().ContainKey(TorrentFilesFields.PathUtf8);

            var list = file.Get<BList>(TorrentFilesFields.Path);
            var listUtf8 = file.Get<BList>(TorrentFilesFields.PathUtf8);
            list.Should().HaveCount(path.Count);
            listUtf8.Should().HaveCount(pathUtf8.Count);

            for (var i = 0; i < list.Count; i++)
            {
                list[i].ToString().Should().Be(path[i]);
            }

            for (var i = 0; i < listUtf8.Count; i++)
            {
                listUtf8[i].ToString().Should().Be(pathUtf8[i]);
            }
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_ExtraFields_AreAdded(BString key, BString value)
        {
            // Arrange
            var torrent = new Torrent
            {
                ExtraFields = new BDictionary
                {
                    [key] = value
                }
            };

            // Act
            var result = torrent.ToBDictionary();

            // Assert
            result.Should().Contain(key, value);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_ExtraFields_OverwritesExistingData(string comment, BString extraValue)
        {
            // Arrange
            var torrent = new Torrent
            {
                Comment = comment,
                ExtraFields = new BDictionary
                {
                    {TorrentFields.Comment, extraValue}
                }
            };

            // Act
            var result = torrent.ToBDictionary();

            // Assert
            result.Should().Contain(TorrentFields.Comment, extraValue);
        }

        [Fact]
        public void ToBDictionary_EqualsEmptyBDictionaryForEmptyTorrent()
        {
            var torrent = new Torrent();
            var result = torrent.ToBDictionary();
            result.Should().HaveCount(0);
            result.Should().BeEquivalentTo(new BDictionary());
        }

        [Theory]
        [AutoMockedData]
        public void Equals_ComparesContent(long fileSize, string fileName, string comment, string createdBy, DateTime creationDate, long pieceSize, IList<IList<string>> trackers)
        {
            var t1 = new Torrent
            {
                Encoding = Encoding.UTF8,
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName
                },
                Comment = comment,
                CreatedBy = createdBy,
                CreationDate = creationDate,
                IsPrivate = true,
                PieceSize = pieceSize,
                Trackers = trackers
            };

            var t2 = new Torrent
            {
                Encoding = Encoding.UTF8,
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName
                },
                Comment = comment,
                CreatedBy = createdBy,
                CreationDate = creationDate,
                IsPrivate = true,
                PieceSize = pieceSize,
                Trackers = trackers
            };

            t1.Equals(t2).Should().BeTrue();
            t2.Equals(t1).Should().BeTrue();
            (t1 == t2).Should().BeTrue();
        }
    }
}
