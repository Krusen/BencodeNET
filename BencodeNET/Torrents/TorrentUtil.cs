using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BencodeNET.Objects;

namespace BencodeNET.Torrents
{
    /// <summary>
    /// Utility class for doing torrent-related work like calculating info hash and creating magnet links.
    /// </summary>
    public static class TorrentUtil
    {
        /// <summary>
        /// Calculates the info hash of the torrent.
        /// The info hash is a 20-byte SHA1 hash of the 'info'-dictionary of the torrent
        /// used to uniquely identify it and it's contents.
        ///
        /// <para>Example: 6D60711ECF005C1147D8973A67F31A11454AB3F5</para>
        /// </summary>
        /// <param name="torrent">The torrent to calculate the info hash for.</param>
        /// <returns>A string representation of the 20-byte SHA1 hash without dashes.</returns>
        public static string CalculateInfoHash(Torrent torrent)
        {
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            return CalculateInfoHash(info);
        }

        /// <summary>
        /// Calculates the info hash of the torrent.
        /// The info hash is a 20-byte SHA1 hash of the 'info'-dictionary of the torrent
        /// used to uniquely identify it and it's contents.
        ///
        /// <para>Example: 6D60711ECF005C1147D8973A67F31A11454AB3F5</para>
        /// </summary>
        /// <param name="torrent">The torrent to calculate the info hash for.</param>
        /// <returns>A byte-array of the 20-byte SHA1 hash.</returns>
        public static byte[] CalculateInfoHashBytes(Torrent torrent)
        {
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            return CalculateInfoHashBytes(info);
        }

        /// <summary>
        /// Calculates the hash of the 'info'-dictionary.
        /// The info hash is a 20-byte SHA1 hash of the 'info'-dictionary of the torrent
        /// used to uniquely identify it and it's contents.
        ///
        /// <para>Example: 6D60711ECF005C1147D8973A67F31A11454AB3F5</para>
        /// </summary>
        /// <param name="info">The 'info'-dictionary of a torrent.</param>
        /// <returns>A string representation of the 20-byte SHA1 hash without dashes.</returns>
        public static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);
            return BytesToHexString(hashBytes);
        }

        /// <summary>
        /// Calculates the hash of the 'info'-dictionary.
        /// The info hash is a 20-byte SHA1 hash of the 'info'-dictionary of the torrent
        /// used to uniquely identify it and it's contents.
        ///
        /// <para>Example: 6D60711ECF005C1147D8973A67F31A11454AB3F5</para>
        /// </summary>
        /// <param name="info">The 'info'-dictionary of a torrent.</param>
        /// <returns>A byte-array of the 20-byte SHA1 hash.</returns>
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
        /// Converts the byte array to a hexadecimal string representation without hyphens.
        /// </summary>
        /// <param name="bytes"></param>
        public static string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
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
