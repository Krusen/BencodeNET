using System;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.Torrents;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BencodeNET.Tests.Torrents
{
    public class TorrentUtilTests
    {
        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData((string)null)]
        public void CreateMagnetLink_NullOrEmptyInfoHash_ThrowsArgumentException(string infoHash)
        {
            Action action = () => TorrentUtil.CreateMagnetLink(infoHash, null, null, MagnetLinkOptions.None);

            action.ShouldThrow<ArgumentException>("because a Magnet link is invalid without an info hash.");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_NonEmptyInfoHash_IsIncluded(string infoHash)
        {
            var magnet = TorrentUtil.CreateMagnetLink(infoHash, null, null, MagnetLinkOptions.None);

            magnet.Should().Be($"magnet:?xt=urn:btih:{infoHash}");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_NonEmptyDisplayName_IsIncluded(string infoHash, string displayName)
        {
            var magnet = TorrentUtil.CreateMagnetLink(infoHash, displayName, null, MagnetLinkOptions.None);

            magnet.Should().Be($"magnet:?xt=urn:btih:{infoHash}&dn={displayName}");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_NonEmptyTracker_WithoutOptionIncludeTrackers_IsNotIncluded(string infoHash, string displayName, string tracker1)
        {
            var trackers = new List<string> {tracker1};

            var magnet = TorrentUtil.CreateMagnetLink(infoHash, displayName, trackers, MagnetLinkOptions.None);

            magnet.Should().Be($"magnet:?xt=urn:btih:{infoHash}&dn={displayName}");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_NonEmptyTracker_WithOptionIncludeTrackers_IsIncluded(string infoHash, string displayName, string tracker1)
        {
            var trackers = new List<string> {tracker1};

            var magnet = TorrentUtil.CreateMagnetLink(infoHash, displayName, trackers, MagnetLinkOptions.IncludeTrackers);

            magnet.Should().Be($"magnet:?xt=urn:btih:{infoHash}&dn={displayName}&tr={tracker1}");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_NonEmptyTrackers_WithOptionIncludeTrackers_AreIncluded(string infoHash, string displayName, string tracker1, string tracker2)
        {
            var trackers = new List<string> {tracker1, tracker2};

            var magnet = TorrentUtil.CreateMagnetLink(infoHash, displayName, trackers, MagnetLinkOptions.IncludeTrackers);

            magnet.Should().Be($"magnet:?xt=urn:btih:{infoHash}&dn={displayName}&tr={tracker1}&tr={tracker2}");
        }

        [Theory]
        [AutoMockedData]
        public void CreateMagnetLink_Torrent_UsesInfoHashDisplayNameAndTrackersFromTorrent(string infoHash, string displayName, IList<IList<string>> trackers)
        {
            // Arrange
            var torrent = Substitute.For<Torrent>();
            torrent.GetInfoHash().Returns(infoHash);
            torrent.DisplayName.Returns(displayName);
            torrent.Trackers.Returns(trackers);

            // Act
            var expected = TorrentUtil.CreateMagnetLink(infoHash.ToLower(), displayName, trackers.SelectMany(x => x), MagnetLinkOptions.IncludeTrackers);
            var magnet = TorrentUtil.CreateMagnetLink(torrent);

            // Assert
            magnet.Should().Be(expected);
        }
    }
}
