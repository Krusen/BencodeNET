using System;
using System.IO;
using System.Security.Cryptography;
using BencodeNET.Objects;

namespace BencodeNET.Torrents
{
    public static class TorrentUtil
    {
        public static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static byte[] CalculateInfoHashBytes(BDictionary info)
        {
            using (var sha1 = new SHA1Managed())
            using (var stream = new MemoryStream())
            {
                info.EncodeToStream(stream);
                stream.Position = 0;

                return sha1.ComputeHash(stream);
            }
        }
    }
}
