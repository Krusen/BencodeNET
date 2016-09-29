using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected override Encoding Encoding => BencodeParser.Encoding;

        public override Torrent Parse(BencodeStream stream)
        {
            var data = BencodeParser.Parse<BDictionary>(stream);
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
                Encoding = ParseEncoding(data.Get<BString>(TorrentFields.Encoding)),
                CreationDate = data.Get<BNumber>(TorrentFields.CreationDate),

                Trackers = LoadTrackers(data)
            };

            // TODO: Better check of single/multi file?
            if (info.ContainsKey(TorrentFields.Files))
            {
                torrent.Files = LoadMultiFileInfoList(info);
            }
            else
            {
                torrent.File = LoadSingleFileInfo(info);
            }

            return torrent;
        }

        protected virtual SingleFileInfo LoadSingleFileInfo(BDictionary info)
        {
            return new SingleFileInfo
            {
                FileName = info.Get<BString>(TorrentFields.Name)?.ToString(),
                FileSize = info.Get<BNumber>(TorrentFields.Length),
                Md5Sum = info.Get<BString>(TorrentFields.Md5Sum)?.ToString()
            };
        }

        protected virtual MultiFileInfoList LoadMultiFileInfoList(BDictionary info)
        {
            var list = new MultiFileInfoList
            {
                DirectoryName = info.Get<BString>(TorrentFields.Name).ToString(),
            };

            var fileInfos = info.Get<BList>(TorrentFields.Files).Cast<BDictionary>()
                .Select(x => new MultiFileInfo
                {
                    FileSize = x.Get<BNumber>(TorrentFields.Length),
                    Path = x.Get<BList>(TorrentFields.Path)?.AsStrings().ToList(),
                    Md5Sum = x.Get<BString>(TorrentFields.Md5Sum)?.ToString()
                });

            list.AddRange(fileInfos);

            return list;
        }

        protected virtual IList<IList<string>> LoadTrackers(BDictionary data)
        {
            var trackerList = new List<IList<string>>();
            var primary = new List<string>();
            trackerList.Add(primary);

            var announce = data.Get<BString>(TorrentFields.Announce)?.ToString();
            if (!string.IsNullOrEmpty(announce))
            {
                primary.Add(announce);
            }

            var announceLists = data.Get<BList>(TorrentFields.AnnounceList)?.As<BList>();
            if (announceLists?.Any() == true)
            {
                primary.AddRange(announceLists.First().AsStrings());
                trackerList[0] = primary.Distinct().ToList();

                trackerList.AddRange(
                    announceLists.Skip(1)
                        .Select(x => x.AsStrings().ToList()));
            }

            return trackerList;
        }

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
