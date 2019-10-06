using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace BencodeNET.Torrents
{
    /// <summary>
    /// A parser for torrent files.
    /// </summary>
    public class TorrentParser : BObjectParser<Torrent>
    {
        /// <summary>
        /// The parsing mode used when parsing torrents.
        /// You can use <see cref="TorrentParserMode.Tolerant"/> if your torrent files are not always following the torrent specification.
        /// </summary>
        public TorrentParserMode ParseMode { get; set; }


        /// <summary>
        /// Creates an instance using a default <see cref="IBencodeParser"/>.
        /// </summary>
        public TorrentParser()
            : this(new BencodeParser())
        {
        }

        /// <summary>
        /// Creates an instance with the specified <see cref="TorrentParserMode"/> using a default <see cref="IBencodeParser"/>.
        /// </summary>
        /// <param name="torrentParserMode">The parser used for parsing the torrent <see cref="BDictionary"/>.</param>
        public TorrentParser(TorrentParserMode torrentParserMode)
            : this(new BencodeParser(), torrentParserMode)
        {
        }

        /// <summary>
        /// Creates an instance using the specified <see cref="IBencodeParser"/> for parsing
        /// the torrent <see cref="BDictionary"/>.
        /// </summary>
        /// <param name="bencodeParser">The parser used for parsing the torrent <see cref="BDictionary"/>.</param>
        public TorrentParser(IBencodeParser bencodeParser)
            : this(bencodeParser, TorrentParserMode.Tolerant)
        {
        }

        /// <summary>
        /// Creates an instance using the specified <see cref="IBencodeParser"/> for parsing
        /// the torrent <see cref="BDictionary"/>.
        /// </summary>
        /// <param name="bencodeParser">The parser used for parsing the torrent <see cref="BDictionary"/>.</param>
        /// <param name="torrentParserMode">The parsing mode to use.</param>
        public TorrentParser(IBencodeParser bencodeParser, TorrentParserMode torrentParserMode)
        {
            BencodeParser = bencodeParser ?? throw new ArgumentNullException(nameof(bencodeParser));
            ParseMode = torrentParserMode;
        }

        /// <summary>
        /// The parser ued for parsing the torrent <see cref="BDictionary"/>.
        /// </summary>
        protected IBencodeParser BencodeParser { get; set; }

        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        public override Encoding Encoding => BencodeParser.Encoding;

        /// <summary>
        /// Parses the next <see cref="BDictionary"/> from the reader as a <see cref="Torrent"/>.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <returns>The parsed <see cref="Torrent"/>.</returns>
        public override Torrent Parse(BencodeReader reader)
        {
            var data = BencodeParser.Parse<BDictionary>(reader);
            return CreateTorrent(data);
        }

        /// <summary>
        /// Parses the next <see cref="BDictionary"/> from the reader as a <see cref="Torrent"/>.
        /// </summary>
        /// <param name="pipeReader">The reader to parse from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed <see cref="Torrent"/>.</returns>
        public override async ValueTask<Torrent> ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default)
        {
            var data = await BencodeParser.ParseAsync<BDictionary>(pipeReader, cancellationToken).ConfigureAwait(false);
            return CreateTorrent(data);
        }

        /// <summary>
        /// Creates a torrent by reading the relevant data from the <see cref="BDictionary"/>.
        /// </summary>
        /// <param name="data">The torrent bencode data.</param>
        /// <returns>A <see cref="Torrent"/> matching the input.</returns>
        protected Torrent CreateTorrent(BDictionary data)
        {
            if (ParseMode == TorrentParserMode.Strict)
                EnsureValidTorrentData(data);

            var info = data.Get<BDictionary>(TorrentFields.Info);

            var encoding = ParseEncoding(data.Get<BString>(TorrentFields.Encoding)) ?? BencodeParser.Encoding;

            var torrent = new Torrent(info)
            {
                IsPrivate = info.Get<BNumber>(TorrentInfoFields.Private) == 1,
                PieceSize = info.Get<BNumber>(TorrentInfoFields.PieceLength),
                Pieces = info.Get<BString>(TorrentInfoFields.Pieces)?.Value.ToArray() ?? new byte[0],

                Comment = data.Get<BString>(TorrentFields.Comment)?.ToString(encoding),
                CreatedBy = data.Get<BString>(TorrentFields.CreatedBy)?.ToString(encoding),
                Encoding = ParseEncoding(data.Get<BString>(TorrentFields.Encoding)),
                CreationDate = data.Get<BNumber>(TorrentFields.CreationDate),

                File = ParseSingleFileInfo(info, encoding),
                Files = ParseMultiFileInfo(info, encoding),

                Trackers = ParseTrackers(data, encoding),

                ExtraFields = ParseAnyExtraFields(data, encoding)
            };

            return torrent;
        }

        /// <summary>
        /// Checks the torrent data for required fields and throws an exception if any are missing or invalid.
        /// </summary>
        /// <param name="data">The torrent data.</param>
        /// <exception cref="InvalidTorrentException">The torrent data is missing required fields or otherwise invalid.</exception>
        protected void EnsureValidTorrentData(BDictionary data)
        {
            /*
             *  NOTE: The 'announce' field is technically required according to the specification,
             *        but is not really required for DHT and PEX.
             */

            if (!data.ContainsKey(TorrentFields.Info))
                throw new InvalidTorrentException("Torrent is missing 'info'-dictionary.", TorrentFields.Info);

            var info = data.Get<BDictionary>(TorrentFields.Info);

            var requiredFields = new List<string>
            {
                TorrentInfoFields.PieceLength,
                TorrentInfoFields.Pieces,
                TorrentInfoFields.Name
            };

            // Single-file torrents must have either the 'length' field or the 'files' field, but not both
            if (info.ContainsKey(TorrentInfoFields.Length) && info.ContainsKey(TorrentInfoFields.Files))
            {
                throw new InvalidTorrentException(
                    $"Torrent 'info'-dictionary cannot contain both '{TorrentInfoFields.Length}' and '{TorrentInfoFields.Files}'.");
            }

            if (!info.ContainsKey(TorrentInfoFields.Length))
                requiredFields.Add(TorrentInfoFields.Files);

            EnsureFields(requiredFields, info, "Torrent is missing required field in 'info'-dictionary.");

            if (info.ContainsKey(TorrentInfoFields.Files))
            {
                var filesData = info.Get<BList>(TorrentInfoFields.Files).AsType<BDictionary>();

                var requiredFileFields = new[]
                {
                    TorrentFilesFields.Length,
                    TorrentFilesFields.Path
                };

                EnsureFields(requiredFileFields, filesData, "Torrent is missing required field in 'info.files' dictionaries.");
            }
        }

        private static void EnsureFields(IEnumerable<string> requiredFields, BDictionary data, string message = null)
        {
            message = message ?? "Torrent is missing required field.";

            foreach (var field in requiredFields.Where(field => !data.ContainsKey(field)))
            {
                throw new InvalidTorrentException(message, field);
            }
        }

        private static void EnsureFields(IEnumerable<string> requiredFields, IEnumerable<BDictionary> list, string message = null)
        {
            message = message ?? "Torrent is missing required field.";

            foreach (var data in list)
            {
                foreach (var field in requiredFields.Where(field => !data.ContainsKey(field)))
                {
                    throw new InvalidTorrentException(message, field);
                }
            }
        }

        /// <summary>
        /// Parses file info for single-file torrents.
        /// </summary>
        /// <param name="info">The 'info'-dictionary of a torrent.</param>
        /// <param name="encoding"></param>
        /// <returns>The file info.</returns>
        protected virtual SingleFileInfo ParseSingleFileInfo(BDictionary info, Encoding encoding)
        {
            if (!info.ContainsKey(TorrentInfoFields.Length))
                return null;

            return new SingleFileInfo
            {
                FileName = info.Get<BString>(TorrentInfoFields.Name)?.ToString(encoding),
                FileSize = info.Get<BNumber>(TorrentInfoFields.Length),
                Md5Sum = info.Get<BString>(TorrentInfoFields.Md5Sum)?.ToString(encoding)
            };
        }

        /// <summary>
        /// Parses file info for multi-file torrents.
        /// </summary>
        /// <param name="info">The 'info'-dictionary of a torrent.</param>
        /// <param name="encoding"></param>
        /// <returns>The file info.</returns>
        protected virtual MultiFileInfoList ParseMultiFileInfo(BDictionary info, Encoding encoding)
        {
            if (!info.ContainsKey(TorrentInfoFields.Files))
                return null;

            var list = new MultiFileInfoList
            {
                DirectoryName = info.Get<BString>(TorrentInfoFields.Name).ToString(encoding),
            };

            var fileInfos = info.Get<BList>(TorrentInfoFields.Files).Cast<BDictionary>()
                .Select(x => new MultiFileInfo
                {
                    FileSize = x.Get<BNumber>(TorrentFilesFields.Length),
                    Path = x.Get<BList>(TorrentFilesFields.Path)?.AsStrings(encoding).ToList(),
                    Md5Sum = x.Get<BString>(TorrentFilesFields.Md5Sum)?.ToString(encoding)
                });

            list.AddRange(fileInfos);

            return list;
        }

        /// <summary>
        /// Parses any extra fields from the root or the 'info'-dictionary
        /// that are not otherwise represented in a <see cref="Torrent"/>.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        protected virtual BDictionary ParseAnyExtraFields(BDictionary root, Encoding encoding)
        {
            var extraFields = ParseExtraRootFields(root);

            if (!root.ContainsKey(TorrentFields.Info))
                return extraFields;

            var info = root.Get<BDictionary>(TorrentFields.Info);
            var extraInfoFields = ParseExtraInfoFields(info);

            if (extraInfoFields.Any())
            {
                extraFields.Add(TorrentFields.Info, extraInfoFields);
            }

            FixEncoding(extraFields, encoding);

            return extraFields;
        }

        private BDictionary ParseExtraRootFields(BDictionary data)
        {
            var extraFields = new BDictionary();

            var extraRootFieldKeys = data.Keys.Except(TorrentFields.Keys);
            foreach (var key in extraRootFieldKeys)
            {
                extraFields.Add(key, data[key]);
            }

            return extraFields;
        }

        private BDictionary ParseExtraInfoFields(BDictionary info)
        {
            var extraFields = new BDictionary();

            var extraInfoFieldKeys = info.Keys.Except(TorrentInfoFields.Keys);
            foreach (var key in extraInfoFieldKeys)
            {
                extraFields.Add(key, info[key]);
            }

            return extraFields;
        }

        // TODO: Unit tests
        private void FixEncoding(IBObject bobject, Encoding encoding)
        {
            switch (bobject)
            {
                case BDictionary dictionary:
                    FixEncodingInDictionary(dictionary, encoding); break;
                case BList list:
                    FixEncodingInList(list, encoding); break;
                case BString str:
                    str.Encoding = encoding; break;
            }
        }

        private void FixEncodingInList(BList list, Encoding encoding)
        {
            foreach (var item in list)
            {
                FixEncoding(item, encoding);
            }
        }

        private void FixEncodingInDictionary(BDictionary data, Encoding encoding)
        {
            foreach (var field in data)
            {
                // TODO: Add unit tests for this
                var key = field.Key.ToString(encoding);
                var isUtf8Key = key.EndsWith("utf-8", StringComparison.OrdinalIgnoreCase);
                var fieldEncoding = isUtf8Key ? Encoding.UTF8 : encoding;

                FixEncoding(field.Value, fieldEncoding);
            }
        }

        /// <summary>
        /// Parses trackers (announce URLs) from a torrent.
        /// </summary>
        /// <param name="data">The torrent data to parse trackers from.</param>
        /// <param name="encoding"></param>
        /// <returns>A list of list of trackers (announce URLs).</returns>
        protected virtual IList<IList<string>> ParseTrackers(BDictionary data, Encoding encoding)
        {
            var trackerList = new List<IList<string>>();
            var primary = new List<string>();
            trackerList.Add(primary);

            // Get single 'announce' url and add it to the primary list if there is any
            var announce = data.Get<BString>(TorrentFields.Announce)?.ToString(encoding);
            if (!string.IsNullOrEmpty(announce))
            {
                primary.Add(announce);
            }

            // Get the 'announce-list' list´s
            var announceLists = data.Get<BList>(TorrentFields.AnnounceList)?.AsType<BList>() as IList<BList>;
            if (announceLists?.Any() == true)
            {
                // Add the first list to the primary list and remove duplicates
                primary.AddRange(announceLists.First().AsStrings(encoding));
                trackerList[0] = primary.Distinct().ToList();

                // Add the other lists to the lists of lists of announce urls
                trackerList.AddRange(
                    announceLists.Skip(1)
                        .Select(x => x.AsStrings(encoding).ToList()));
            }

            return trackerList;
        }

        /// <summary>
        /// Parses the encoding string to an <see cref="Encoding"/>.
        /// Returns null if parsing fails.
        /// </summary>
        /// <param name="bstring">The <see cref="BString"/> value to parse.</param>
        /// <returns>The parsed encoding or null if parsing fails.</returns>
        protected virtual Encoding ParseEncoding(BString bstring)
        {
            if (bstring == null)
                return null;

            var str = bstring.ToString();
            try
            {
                return Encoding.GetEncoding(str);
            }
            catch (Exception)
            {
                if (string.Equals(str, "UTF8", StringComparison.OrdinalIgnoreCase))
                {
                    return Encoding.UTF8;
                }
            }

            return null;
        }
    }
}
