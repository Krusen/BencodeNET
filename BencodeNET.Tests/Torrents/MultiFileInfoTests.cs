using System;
using BencodeNET.Torrents;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Torrents
{
    public class MultiFileInfoTests
    {
        [Theory]
        [AutoMockedData]
        public void FullPath_PathIsNull_ShouldNotThrowException(MultiFileInfo multiFileInfo)
        {
            // Arrange
            multiFileInfo.Path = null;

            // Act
            Action act = () => { var _ = multiFileInfo.FullPath; };

            // Assert
            act.Should().NotThrow();
            multiFileInfo.FullPath.Should().BeNull();
        }

        [Theory]
        [AutoMockedData]
        public void FullPathUtf8_PathUtf8IsNull_ShouldNotThrowException(MultiFileInfo multiFileInfo)
        {
            // Arrange
            multiFileInfo.PathUtf8 = null;

            // Act
            Action act = () => { var _ = multiFileInfo.FullPathUtf8; };

            // Assert
            act.Should().NotThrow();
            multiFileInfo.FullPathUtf8.Should().BeNull();
        }
    }
}
