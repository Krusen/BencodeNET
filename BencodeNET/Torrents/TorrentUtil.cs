using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BencodeNET.Objects;

namespace BencodeNET.Torrents
{
    public static class TorrentUtil
    {
        public static string CalculateInfoHash(Torrent torrent)
        {
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            return CalculateInfoHash(info);
        }

        public static byte[] CalculateInfoHashBytes(Torrent torrent)
        {
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            return CalculateInfoHashBytes(info);
        }

        public static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static byte[] CalculateInfoHashBytes(BDictionary info)
        {
            using (var sha1 = SHA1.Create())
            using (var stream = new MemoryStream())
            {
                info.EncodeTo(stream);
                stream.Position = 0;

                return sha1.ComputeHash(stream);
            }
        }

        /// <summary>
        /// Creates a Magnet link in the BTIH (BitTorrent Info Hash) format: xt=urn:btih:{info hash}
        /// </summary>
        /// <param name="torrent">Torrent to create Magnet link for.</param>
        /// <param name="options">Controls how the Magnet link is constructed.</param>
        /// <returns></returns>
        public static string CreateMagnetLink(Torrent torrent, MagnetLinkOptions options = MagnetLinkOptions.IncludeTrackers)
        {
            var infoHash = torrent.GetInfoHash().ToLower();
            var displayName = torrent.DisplayName;
            var trackers = torrent.Trackers.Flatten();

            return CreateMagnetLink(infoHash, displayName, trackers, options);
        }

        // TODO: Unit test
        /// <summary>
        /// Creates a Magnet link in the BTIH (BitTorrent Info Hash) format: xt=urn:btih:{info hash}
        /// </summary>
        /// <param name="infoHash">The info has of the torrent.</param>
        /// <param name="displayName">The display name of the torrent. Usually the file name or directory name for multi-file torrents</param>
        /// <param name="trackers">A list of trackers if any.</param>
        /// <param name="options">Controls how the Magnet link is constructed.</param>
        /// <returns></returns>
        public static string CreateMagnetLink(string infoHash, string displayName, IEnumerable<string> trackers, MagnetLinkOptions options)
        {
            if (string.IsNullOrEmpty(infoHash))
                throw new ArgumentException("Info hash cannot be null or empty.", nameof(infoHash));

            var magnet = $"magnet:?xt=urn:btih:{infoHash}";

            if (!string.IsNullOrWhiteSpace(displayName))
                magnet += $"&dn={displayName}";

            var validTrackers = trackers?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();

            if (options.HasFlag(MagnetLinkOptions.IncludeTrackers) && validTrackers.Any())
            {
                var trackersString = string.Join("&", validTrackers.Select(x => $"tr={x}"));
                magnet += $"&{trackersString}";
            }

            return magnet;
        }
    }
}
