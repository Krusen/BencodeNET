using BencodeNET.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET.Torrents
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    // TODO: Equals comparison
    // TODO: Support for extra fields
    public class Torrent : BObject
    {
        // TODO: This controls the encoding used during encoding. Maybe abstract away the "string Encoding" property and reuse this for both
        public Encoding OutputEncoding { get; set; } = System.Text.Encoding.UTF8;

        // Announce + Announce-List
        // The announce list is actually a list of lists of trackers, but we don't support that for now.
        public virtual IList<string> Trackers { get; set; } = new List<string>();

        // Info - Single file
        public virtual TorrentSingleFileInfo File { get; set; }

        // Info - Multi file
        public virtual TorrentMultiFileInfoList Files { get; set; }

        public virtual TorrentFileMode FileMode
        {
            get
            {
                if (Files?.Any() == true)
                    return TorrentFileMode.Multi;

                if (File != null)
                    return TorrentFileMode.Single;

                return TorrentFileMode.Unknown;
            }
        }

        /// <summary>
        /// The creation date of the .torrent file [optional]
        /// </summary>
        public virtual DateTime? CreationDate { get; set; }

        /// <summary>
        /// The comment of the .torrent file [optional]
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// The name and version of the program used to create the .torrent [optional]
        /// </summary>
        public virtual string CreatedBy { get; set; }

        /// <summary>
        /// The encoding used by the client that created the .torrent file [optional]
        /// </summary>
        public virtual string Encoding { get; set; }

        public virtual long PieceSize { get; set; }
        public virtual BString Pieces { get; internal set; }
        public virtual bool IsPrivate { get; set; }

        public virtual long TotalSize
        {
            get
            {
                if (FileMode == TorrentFileMode.Single)
                    return File?.FileSize ?? 0;

                if (FileMode == TorrentFileMode.Multi)
                    return Files?.Sum(x => x.FileSize) ?? 0;

                return 0;
            }
        }

        public virtual int NumberOfPieces => (int)Math.Ceiling((double)TotalSize / PieceSize);

        protected virtual BDictionary ToBDictionary()
        {
            var torrent = new BDictionary();

            if (Trackers?.Count > 0)
            {
                torrent[TorrentFields.Announce] = new BString(Trackers.First(), OutputEncoding);
            }

            if (Trackers?.Count > 1)
            {
                torrent[TorrentFields.AnnounceList] = new BList
                {
                    new BList<BString>(Trackers.Select(x => new BString(x, OutputEncoding)))
                };
            }

            if (Encoding != null)
                torrent[TorrentFields.Encoding] = new BString(Encoding, OutputEncoding);

            if (Comment != null)
                torrent[TorrentFields.Comment] = new BString(Comment, OutputEncoding);

            if (CreatedBy != null)
                torrent[TorrentFields.CreatedBy] = new BString(CreatedBy, OutputEncoding);

            if (CreationDate != null)
            {
                torrent[TorrentFields.CreationDate] = (BNumber)CreationDate;
            }

            torrent[TorrentFields.Info] = CreateInfo(OutputEncoding);

            return torrent;
        }

        // TODO: Split into smaller parts - maybe a TorrentFactory
        // TODO: Some sort of error handling?
        public override T EncodeToStream<T>(T stream)
        {
            var torrent = ToBDictionary();
            return torrent.EncodeToStream(stream);
        }

        public override Task<T> EncodeToStreamAsync<T>(T stream)
        {
            var torrent = ToBDictionary();
            return torrent.EncodeToStreamAsync(stream);
        }

        protected virtual BDictionary CreateInfo(Encoding encoding)
        {
            var info = new BDictionary
            {
                [TorrentFields.PieceLength] = (BNumber)PieceSize,
                [TorrentFields.Pieces] = Pieces,
            };

            if (IsPrivate)
                info[TorrentFields.Private] = (BNumber)1;

            if (FileMode == TorrentFileMode.Single)
            {
                info[TorrentFields.Name] = new BString(File.FileName, encoding);
                info[TorrentFields.Length] = (BNumber)File.FileSize;

                if (File.Md5Sum != null)
                    info[TorrentFields.Md5Sum] = new BString(File.Md5Sum, encoding);

            }
            else if (FileMode == TorrentFileMode.Multi)
            {
                info[TorrentFields.Name] = new BString(Files.DirectoryName, encoding);

                var files = new BList<BDictionary>();
                foreach (var file in Files)
                {
                    var fileDictionary = new BDictionary
                    {
                        [TorrentFields.Length] = (BNumber)file.FileSize,
                        [TorrentFields.Path] = new BList(file.Path)
                    };

                    if (file.Md5Sum != null)
                        fileDictionary[TorrentFields.Md5Sum] = new BString(file.Md5Sum, encoding);

                    files.Add(fileDictionary);
                }

                info[TorrentFields.Files] = files;
            }

            return info;
        }

        public virtual string CalculateInfoHash()
        {
            var info = CreateInfo(OutputEncoding);
            return TorrentUtil.CalculateInfoHash(info);
        }

        public virtual byte[] CalculateInfoHashBytes()
        {
            var info = CreateInfo(OutputEncoding);
            return TorrentUtil.CalculateInfoHashBytes(info);
        }

        public static bool operator ==(Torrent first, Torrent second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(Torrent first, Torrent second)
        {
            return !(first == second);
        }

        public override bool Equals(object other)
        {
            var obj = other as Torrent;
            if (obj == null)
                return false;

            using (var ms1 = EncodeToStream(new MemoryStream()))
            using (var ms2 = obj.EncodeToStream(new MemoryStream()))
            {
                var bytes1 = ms1.ToArray();
                var bytes2 = ms2.ToArray();

                return bytes1.SequenceEqual(bytes2);
            }
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException();
        }
    }
}
