using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.Objects;

namespace BencodeNET.Torrents
{
    /// <summary>
    ///
    /// </summary>
    public class Torrent : BObject
    {
        private struct FileValidation
        {
            public bool isValid;
            public int remainder;
            public byte[] buffer;
            public bool validateRemainder;

            public FileValidation(long bufferSize, bool validateReminder)
            {
                isValid = false;
                remainder = 0;
                buffer = new byte[bufferSize];
                this.validateRemainder = validateReminder;
            }
        }

        private const int SHA1_NUMBER_OF_BYTES = 20;
        private HashAlgorithm sha1 = SHA1.Create();

        /// <summary>
        ///
        /// </summary>
        public Torrent()
        {
        }

        /// <summary>
        /// Creates a torrent and populates the <see cref="OriginalInfoHash"/> and <see cref="OriginalInfoHashBytes"/>
        /// properties from the provided <see cref="BDictionary"/>.
        /// </summary>
        /// <param name="originalInfoDictionary"></param>
        internal Torrent(BDictionary originalInfoDictionary)
        {
            OriginalInfoHashBytes = TorrentUtil.CalculateInfoHashBytes(originalInfoDictionary);
            OriginalInfoHash = TorrentUtil.BytesToHexString(OriginalInfoHashBytes);
        }

        /// <summary>
        /// The original info hash value from when the torrent was parsed.
        /// This will be null if the instance was created manually and not by the parser.
        /// </summary>
        public string OriginalInfoHash { get; protected set; }

        /// <summary>
        /// The original info hash bytes from when the torrent was parsed.
        /// This will be null if the instance was created manually and not by the parser.
        /// </summary>
        public byte[] OriginalInfoHashBytes { get; protected set; }

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
        public virtual SingleFileInfo File { get; set; }

        /// <summary>
        /// A list of file info for the files in the torrent. Will be <c>null</c> for single-file torrents.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'info' field in a multi-file torrent.
        /// </remarks>
        public virtual MultiFileInfoList Files { get; set; }

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
        /// Returns the "display name" of the torrent.
        /// For single-file torrents this is the file name of that file.
        /// For multi-file torrents this is the directory name.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                if (FileMode == TorrentFileMode.Single)
                    return File.FileName;

                if (FileMode == TorrentFileMode.Multi)
                    return Files.DirectoryName;

                throw new BencodeException("Cannot get torrent display name. Unknown torrent file mode.");
            }
        }

        public virtual string DisplayNameUtf8
        {
            get
            {
                if (FileMode == TorrentFileMode.Single)
                    return File.FileNameUtf8;

                if (FileMode == TorrentFileMode.Multi)
                    return Files.DirectoryNameUtf8;

                throw new BencodeException("Cannot get torrent display name. Unknown torrent file mode.");
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

        // TODO: Split into list of 20-byte hashes and rename to something appropriate?
        /// <summary>
        /// A list of all 20-byte SHA1 hash values (one for each piece).
        /// </summary>
        public List<byte[]> Pieces
        {
            get
            {
                var pieces = new List<byte[]>();
                for (int i = 0; i < PiecesConcatenated.Length; i += SHA1_NUMBER_OF_BYTES)
                {
                    var piece = new byte[SHA1_NUMBER_OF_BYTES];
                    Array.Copy(PiecesConcatenated, i, piece, 0, SHA1_NUMBER_OF_BYTES);
                    pieces.Add(piece);
                }
                return pieces;
            }
        }

        /// <summary>
        /// A concatenation of all 20-byte SHA1 hash values (one for each piece).
        /// Use <see cref="PiecesConcatenatedAsHexString"/> to get/set this value as a hex string instead.
        /// </summary>
        public virtual byte[] PiecesConcatenated { get; set; } = new byte[0];

        /// <summary>
        /// Gets or sets <see cref="PiecesConcatenated"/> from/to a hex string (without dashes), e.g. 1C115D26444AEF2A5E936133DCF8789A552BBE9F[...].
        /// The length of the string must be a multiple of 40.
        /// </summary>
        public virtual string PiecesConcatenatedAsHexString
        {
            get => BitConverter.ToString(PiecesConcatenated).Replace("-", "");
            set
            {
                if (value?.Length % 40 != 0)
                    throw new ArgumentException("Value length must be a multiple of 40 (20 bytes as hex).");

                if (Regex.IsMatch(value, "[^0-9A-F]"))
                    throw new ArgumentException("Value must only contain hex characters (0-9 and A-F) and only uppercase.");

                var bytes = new byte[value.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    var str = $"{value[i * 2]}{value[i * 2 + 1]}";
                    bytes[i] = Convert.ToByte(str, 16);
                }

                PiecesConcatenated = bytes;
            }
        }

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
        public virtual int NumberOfPieces => PiecesConcatenated != null
            ? (int)Math.Ceiling((double)PiecesConcatenated.Length / 20)
            : 0;

        /// <summary>
        /// Verify integrity of the torrent content versus existing data
        /// </summary>
        /// <param name="path">either a folder path in multi mode or a file path in single mode</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD111:Use ConfigureAwait(bool)", Justification = "<Pending>")]
        public async virtual Task<bool> ValidateExistingDataAsync(string path)
        {
            var isDirectory = Directory.Exists(path);
            var isFile = System.IO.File.Exists(path);
            if (isDirectory && FileMode != TorrentFileMode.Multi)
            {
                throw new BencodeException("The path represents a directory but the torrent is not set as a multi mode");
            }
            else if (isFile && FileMode != TorrentFileMode.Single)
            {
                throw new BencodeException("The path represents a file but the torrent is not set as a single mode");
            }
            else if (!isFile && !isDirectory)
            {
                throw new BencodeException("The path does not exist");
            }

            var validation = new FileValidation(PieceSize, false);
            if (isFile)
            {
                validation = await ValidateExistingFileAsync(new System.IO.FileInfo(path));
            }
            else if (isDirectory)
            {
                validation.isValid = true;
                var piecesOffset = 0;
                for (int i = 0; i < Files.Count && validation.isValid; i++)
                {
                    validation.validateRemainder = (i + 1) == Files.Count;
                    var file = new FileInfo(Path.Combine(path, Files.DirectoryName, Files[i].FullPath));
                    validation = await ValidateExistingFileAsync(file, piecesOffset, validation);
                    if (!validation.isValid)
                    {
                        break;
                    }

                    piecesOffset += (file.Exists ? (int)(file.Length / PieceSize) : 0);
                }
            }

            return validation.isValid;
        }

        /// <summary>
        /// Validate integrity of an existing file
        /// </summary>
        /// <param name="file">file to validate</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD111:Use ConfigureAwait(bool)", Justification = "<Pending>")]
        private async Task<FileValidation> ValidateExistingFileAsync(FileInfo file)
        {
            return await ValidateExistingFileAsync(file, 0, new FileValidation(PieceSize, true));
        }

        /// <summary>
        /// Validate integrity of an existing file
        /// </summary>
        /// <param name="file">file to validate</param>
        /// <param name="piecesOffset">next piece index to validate</param>
        /// <param name="validation">current validation data</param>
        /// <remarks>Based on https://raw.githubusercontent.com/eclipse/ecf/master/protocols/bundles/org.eclipse.ecf.protocol.bittorrent/src/org/eclipse/ecf/protocol/bittorrent/TorrentFile.java</remarks>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD111:Use ConfigureAwait(bool)", Justification = "<Pending>")]
        private async Task<FileValidation> ValidateExistingFileAsync(FileInfo file, int piecesOffset, FileValidation validation)
        {
            if (!file.Exists)
            {
                return validation;
            }

            int piecesIndex = piecesOffset, bytesRead = validation.remainder;
            using (var stream = file.OpenRead())
            {
                while ((bytesRead += await stream.ReadAsync(validation.buffer, validation.remainder, (int)PieceSize - validation.remainder)) == PieceSize)
                {
                    if (!Pieces[piecesIndex].SequenceEqual(sha1.ComputeHash(validation.buffer)))
                    {
                        return validation;
                    }
                    piecesIndex++;
                    bytesRead = 0;
                    validation.remainder = 0;
                }
            }

            validation.remainder = bytesRead;
            if (!validation.validateRemainder || validation.remainder == 0)
            {
                validation.isValid = true;
                return validation;
            }

            byte[] lastBuffer = new byte[validation.remainder];
            Array.Copy(validation.buffer, lastBuffer, bytesRead);

            validation.isValid = Pieces[piecesIndex].SequenceEqual(sha1.ComputeHash(lastBuffer));

            return validation;
        }

        /// <summary>
        /// Converts the torrent to a <see cref="BDictionary"/>.
        /// </summary>
        /// <returns></returns>
        public virtual BDictionary ToBDictionary()
        {
            var torrent = new BDictionary();

            var trackerCount = Trackers.Flatten().Count();

            if (trackerCount > 0)
                torrent[TorrentFields.Announce] = new BString(Trackers.Flatten().First(), Encoding);

            if (trackerCount > 1)
                torrent[TorrentFields.AnnounceList] = new BList(Trackers.Select(x => new BList(x, Encoding)));

            if (Encoding != null)
                torrent[TorrentFields.Encoding] = new BString(Encoding.WebName.ToUpper(), Encoding);

            if (Comment != null)
                torrent[TorrentFields.Comment] = new BString(Comment, Encoding);

            if (CreatedBy != null)
                torrent[TorrentFields.CreatedBy] = new BString(CreatedBy, Encoding);

            if (CreationDate != null)
                torrent[TorrentFields.CreationDate] = (BNumber)CreationDate;

            var info = CreateInfoDictionary(Encoding);
            if (info.Any())
                torrent[TorrentFields.Info] = info;

            if (ExtraFields != null)
                torrent.MergeWith(ExtraFields, ExistingKeyAction.Merge);

            return torrent;
        }

        /// <summary>
        /// Creates the 'info' part of the torrent.
        /// </summary>
        /// <param name="encoding">The encoding used for writing strings</param>
        /// <returns>A <see cref="BDictionary"/> of the 'info' part of the torrent</returns>
        protected virtual BDictionary CreateInfoDictionary(Encoding encoding)
        {
            var info = new BDictionary();

            if (PieceSize > 0)
                info[TorrentInfoFields.PieceLength] = (BNumber)PieceSize;

            if (PiecesConcatenated?.Length > 0)
                info[TorrentInfoFields.Pieces] = new BString(PiecesConcatenated, encoding);

            if (IsPrivate)
                info[TorrentInfoFields.Private] = (BNumber)1;

            if (FileMode == TorrentFileMode.Single)
            {
                info[TorrentInfoFields.Name] = new BString(File.FileName, encoding);

                if (File.FileNameUtf8 != null)
                    info[TorrentInfoFields.NameUtf8] = new BString(File.FileNameUtf8, Encoding.UTF8);

                info[TorrentInfoFields.Length] = (BNumber)File.FileSize;

                if (File.Md5Sum != null)
                    info[TorrentInfoFields.Md5Sum] = new BString(File.Md5Sum, encoding);

            }
            else if (FileMode == TorrentFileMode.Multi)
            {
                info[TorrentInfoFields.Name] = new BString(Files.DirectoryName, encoding);

                if (Files.DirectoryNameUtf8 != null)
                    info[TorrentInfoFields.NameUtf8] = new BString(Files.DirectoryNameUtf8, Encoding.UTF8);

                var files = new BList<BDictionary>();
                foreach (var file in Files)
                {
                    var fileDictionary = new BDictionary
                    {
                        [TorrentFilesFields.Length] = (BNumber)file.FileSize,
                        [TorrentFilesFields.Path] = new BList(file.Path)
                    };

                    if (file.PathUtf8 != null && file.PathUtf8.Any())
                        fileDictionary[TorrentFilesFields.PathUtf8] = new BList(file.PathUtf8, Encoding.UTF8);

                    if (file.Md5Sum != null)
                        fileDictionary[TorrentFilesFields.Md5Sum] = new BString(file.Md5Sum, encoding);

                    files.Add(fileDictionary);
                }

                info[TorrentInfoFields.Files] = files;
            }

            return info;
        }

        /// <summary>
        /// Calculates the info hash of the torrent. This is used when communicating with trackers.
        /// The info hash is a 20-byte SHA1 hash of the value of the 'info' <see cref="BDictionary"/> of the torrent.
        /// </summary>
        /// <returns>A string representation of a 20-byte SHA1 hash of the value of the 'info' part</returns>
        public virtual string GetInfoHash() => TorrentUtil.CalculateInfoHash(this);

        /// <summary>
        /// Calculates the info hash of the torrent. This is used when communicating with trackers.
        /// The info hash is a 20-byte SHA1 hash of the value of the 'info' <see cref="BDictionary"/> of the torrent.
        /// </summary>
        /// <returns>A 20-byte SHA1 hash of the value of the 'info' part</returns>
        public virtual byte[] GetInfoHashBytes() => TorrentUtil.CalculateInfoHashBytes(this);

        /// <summary>
        /// Creates a Magnet link in the BTIH (BitTorrent Info Hash) format: xt=urn:btih:{info hash}
        /// </summary>
        public virtual string GetMagnetLink(MagnetLinkOptions options = MagnetLinkOptions.IncludeTrackers)
        {
            return TorrentUtil.CreateMagnetLink(this, options);
        }

        /// <inheritdoc/>
        public override int GetSizeInBytes() => ToBDictionary().GetSizeInBytes();

        /// <summary>
        /// Encodes the torrent and writes it to the stream.
        /// </summary>
        /// <param name="stream"></param>
        protected override void EncodeObject(Stream stream)
        {
            var torrent = ToBDictionary();
            torrent.EncodeTo(stream);
        }

        /// <summary>
        /// Encodes the torrent and writes it to the <see cref="PipeWriter"/>.
        /// </summary>
        protected override void EncodeObject(PipeWriter writer)
        {
            var torrent = ToBDictionary();
            torrent.EncodeTo(writer);
        }

        /// <inheritdoc/>
        protected override ValueTask<FlushResult> EncodeObjectAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            var torrent = ToBDictionary();
            return torrent.EncodeToAsync(writer, cancellationToken);
        }

#pragma warning disable 1591
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

            using (var ms1 = EncodeTo(new MemoryStream()))
            using (var ms2 = obj.EncodeTo(new MemoryStream()))
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
#pragma warning restore 1591
    }
}
