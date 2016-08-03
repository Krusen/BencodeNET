using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Torrents;

namespace BencodeNET.Parsing
{
    public class TorrentParser : BObjectParser<Torrent>
    {
        public TorrentParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        protected IBencodeParser BencodeParser { get; set; }

        public override Torrent Parse(BencodeStream stream)
        {
            var data = BencodeParser.ParseDictionary(stream);
            return CreateTorrent(data);
        }

        public override async Task<Torrent> ParseAsync(BencodeStream stream)
        {
            var data = await BencodeParser.ParseDictionaryAsync(stream);
            return CreateTorrent(data);
        }

        protected Torrent CreateTorrent(BDictionary data)
        {
            var info = data.Get<BDictionary>(TorrentFields.Info);

            var torrent = new Torrent
            {
                IsPrivate = info.Get<BNumber>(TorrentFields.Private) == 1,
                PieceSize = info.Get<BNumber>(TorrentFields.PieceLength),
                Pieces = info.Get<BString>(TorrentFields.Pieces),

                Comment = data.Get<BString>(TorrentFields.Comment)?.ToString(),
                CreatedBy = data.Get<BString>(TorrentFields.CreatedBy)?.ToString(),
                Encoding = data.Get<BString>(TorrentFields.Encoding)?.ToString(),
                CreationDate = data.Get<BNumber>(TorrentFields.CreationDate),

                Trackers = LoadTrackers(data)
            };

            if (torrent.FileMode == TorrentFileMode.Single)
                torrent.File = LoadSingleFileInfo(info);

            if (torrent.FileMode == TorrentFileMode.Multi)
                torrent.Files = LoadMultiFileInfoList(info);

            return torrent;
        }

        protected virtual TorrentSingleFileInfo LoadSingleFileInfo(BDictionary info)
        {
            return new TorrentSingleFileInfo
            {
                FileName = info.Get<BString>(TorrentFields.Name)?.ToString(),
                FileSize = info.Get<BNumber>(TorrentFields.Length),
                Md5Sum = info.Get<BString>(TorrentFields.Md5Sum)?.ToString()
            };
        }

        protected virtual TorrentMultiFileInfoList LoadMultiFileInfoList(BDictionary info)
        {
            var list = new TorrentMultiFileInfoList
            {
                DirectoryName = info.Get<BString>(TorrentFields.Name).ToString(),
            };

            var fileInfos = info.Get<BList>(TorrentFields.Files).Cast<BDictionary>()
                .Select(x => new TorrentMultiFileInfo
                {
                    FileSize = x.Get<BNumber>(TorrentFields.Length),
                    Path = x.Get<BList>(TorrentFields.Path)?.AsStrings().ToList(),
                    Md5Sum = x.Get<BString>(TorrentFields.Md5Sum)?.ToString()
                });

            list.AddRange(fileInfos);

            return list;
        }

        // TODO: Make trackers a list of lists (as the actual torrent structure)
        protected virtual IList<string> LoadTrackers(BDictionary data)
        {
            var trackers = new List<string>();
            var announce = data.Get<BString>(TorrentFields.Announce)?.ToString();
            if (!string.IsNullOrEmpty(announce))
            {
                trackers.Add(announce);
            }

            var announceLists = data.Get<BList>(TorrentFields.AnnounceList)?.As<BList>();
            if (announceLists != null)
            {
                trackers.AddRange(announceLists.SelectMany(list => list.AsStrings()));

                trackers = trackers.Distinct().ToList();
            }

            return trackers;
        }
    }
}
