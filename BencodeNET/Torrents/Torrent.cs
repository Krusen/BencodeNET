using BencodeNET.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;

namespace BencodeNET.Torrents
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    // TODO: Equals comparison
    // TODO: Support for adding extra fields (List of BObject)
    /// <summary>
    ///
    /// </summary>
    public class Torrent : BObject
    {
        /// <summary>
        /// Add any custom fields to this <see cref="BDictionary"/> and they will
        /// be merged with the torrent data when encoded.
        /// </summary>
        /// <remarks>
        /// Existing keys will be overwritten with the values from this property.
        /// In the case the existing and new value are both <see cref="BList"/> the new list will be appended to the existing list.
        /// In the case the existing and new value are both <see cref="BDictionary"/> they will be merged recursively.
        /// </remarks>
        public virtual BDictionary ExtraFields { get; set; }

        /// <summary>
        /// A list of list of trackers (announce URLs).
        /// Lists are processed in order of first to last. Trackers in a list are processed randomly.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// The lists will be processed by clients in order of first to last.
        /// For each list the trackers will be processed in random order until one is successful.
        /// If no trackers in the first list responds, the next list is attempted etc.
        /// </para>
        ///
        /// <para>
        /// See more here: http://bittorrent.org/beps/bep_0012.html
        /// </para>
        /// </remarks>
        public virtual IList<IList<string>> Trackers { get; set; } = new List<IList<string>>();

        /// <summary>
        /// File info  for the file in the torrent. Will be <c>null</c> for multi-file torrents.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'info' field in a single-file torrent.
        /// </remarks>
        public virtual TorrentSingleFileInfo File { get; set; }

        /// <summary>
        /// A list of file info for the files in the torrent. Will be <c>null</c> for single-file torrents.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'info' field in a multi-file torrent.
        /// </remarks>
        public virtual TorrentMultiFileInfoList Files { get; set; }

        /// <summary>
        /// The file mode of the torrent.
        /// Torrents can be either single-file or multi-file and the content of the 'info' differs depending on this.
        /// <para>
        /// If <c>Single</c> then the <see cref="Torrent.File"/> property is populated.
        /// if <c>Multi</c> then the <see cref="Torrent.Files"/> property is populated.
        /// </para>
        /// </summary>
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
        /// [optional] The creation date of the torrent.
        /// </summary>
        public virtual DateTime? CreationDate { get; set; }

        /// <summary>
        /// [optional] Torrent comment.
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// [optional] The name and version of the program used to create this torrent.
        /// </summary>
        public virtual string CreatedBy { get; set; }

        /// <summary>
        /// [optional] Indicates the encoding used to store the strings in this torrents.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// The size in bytes of each file piece (piece length).
        /// </summary>
        public virtual long PieceSize { get; set; }

        /// <summary>
        /// A concatenation of all 20-byte SHA1 hash values (one for each piece).
        /// </summary>
        public virtual BString Pieces { get; internal set; }

        /// <summary>
        /// [optional] If set to true clients must only publish it's presence to the defined trackers.
        /// Mainly used for private trackers which don't allow PEX, DHT etc.
        /// </summary>
        public virtual bool IsPrivate { get; set; }

        /// <summary>
        /// The total size in bytes of the included files.
        /// </summary>
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

        /// <summary>
        /// The total number of file pieces.
        /// </summary>
        public virtual int NumberOfPieces => (int)Math.Ceiling((double)TotalSize / PieceSize);

        // TODO: Validation that torrent is valid?
        /// <summary>
        /// Converts the torrent to a <see cref="BDictionary"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual BDictionary ToBDictionary()
        {
            var torrent = new BDictionary();

            if (Trackers?.Count > 0)
            {
                torrent[TorrentFields.Announce] = new BList(Trackers.First().Select(x => new BString(x, Encoding)));
            }

            if (Trackers?.Count > 1)
            {
                torrent[TorrentFields.AnnounceList] = new BList(Trackers.Select(x => new BList(x, Encoding)));
            }

            if (Encoding != null)
                torrent[TorrentFields.Encoding] = new BString(Encoding.WebName.ToUpper(), Encoding);

            if (Comment != null)
                torrent[TorrentFields.Comment] = new BString(Comment, Encoding);

            if (CreatedBy != null)
                torrent[TorrentFields.CreatedBy] = new BString(CreatedBy, Encoding);

            if (CreationDate != null)
                torrent[TorrentFields.CreationDate] = (BNumber)CreationDate;

            torrent[TorrentFields.Info] = CreateInfo(Encoding);

            if (ExtraFields != null)
                torrent.MergeWith(ExtraFields, ExistingKeyAction.Merge);

            return torrent;
        }

        // TODO: Split into smaller parts - maybe a TorrentFactory
        // TODO: Some sort of error handling?
        /// <summary>
        /// Encodes the torrent and writes it to the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override void EncodeObject(BencodeStream stream)
        {
            var torrent = ToBDictionary();
            torrent.EncodeToStream(stream);
        }

        /// <summary>
        /// Encodes the torrent and writes it asynchronously to the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns>The stream that was passed to the method</returns>
        protected override Task EncodeObjectAsync(BencodeStream stream)
        {
            var torrent = ToBDictionary();
            return torrent.EncodeToStreamAsync(stream);
        }

        /// <summary>
        /// Creates the 'info' part of the torrent.
        /// </summary>
        /// <param name="encoding">The encoding used for writing strings</param>
        /// <returns>A <see cref="BDictionary"/> of the 'info' part of the torrent</returns>
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

        /// <summary>
        /// Calculates the info hash of the torrent. This is used when communicating with trackers.
        /// The info hash is a 20-byte SHA1 hash of the value of the 'info' <see cref="BDictionary"/> of the torrent.
        /// </summary>
        /// <returns>A string representation of a 20-byte SHA1 hash of the value of the 'info' part</returns>
        public virtual string CalculateInfoHash()
        {
            var info = CreateInfo(Encoding);
            return TorrentUtil.CalculateInfoHash(info);
        }

        /// <summary>
        /// Calculates the info hash of the torrent. This is used when communicating with trackers.
        /// The info hash is a 20-byte SHA1 hash of the value of the 'info' <see cref="BDictionary"/> of the torrent.
        /// </summary>
        /// <returns>A 20-byte SHA1 hash of the value of the 'info' part</returns>
        public virtual byte[] CalculateInfoHashBytes()
        {
            var info = CreateInfo(Encoding);
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
