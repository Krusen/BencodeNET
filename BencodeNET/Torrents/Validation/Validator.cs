using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;

namespace BencodeNET.Torrents.Validation
{
    class Validator
    {
        private readonly Torrent torrent;
        private readonly ValidationOptions options;
        private readonly HashAlgorithm sha1 = SHA1.Create();

        // Shorthand helpers
        private TorrentFileMode FileMode => torrent.FileMode;
        private MultiFileInfoList Files => torrent.Files;
        private long PieceSize => torrent.PieceSize;
        private int NumberOfPieces => torrent.NumberOfPieces;
        private List<byte[]> Pieces => torrent.Pieces;

        public Validator(Torrent torrent, ValidationOptions options)
        {
            this.options = options ?? ValidationOptions.DefaultOptions;
            this.torrent = torrent;

            // Ensure options are appropriate toward the current torrent
            if (this.torrent.FileMode == TorrentFileMode.Single || this.options.Tolerance > 1)
            {
                this.options.Tolerance = 1;
            }
        }

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
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            if (isDirectory && FileMode != TorrentFileMode.Multi)
            {
                throw new ArgumentException("The path represents a directory but the torrent is not set as a multi mode");
            }
            else if (isFile && FileMode != TorrentFileMode.Single)
            {
                throw new ArgumentException("The path represents a file but the torrent is not set as a single mode");
            }
            else if (!isFile && !isDirectory)
            {
                return false;
            }

            var validation = new ValidationData(PieceSize, false);
            if (isFile)
            {
                validation = await ValidateExistingFileAsync(new System.IO.FileInfo(path));
            }
            else if (isDirectory)
            {
                validation.isValid = true;
                var piecesOffset = 0;
                for (int i = 0; i < Files.Count && piecesOffset < NumberOfPieces; i++)
                {
                    var previousRemainder = validation.remainder;
                    validation.validateRemainder = (i + 1) == Files.Count;
                    var file = new FileInfo(Path.Combine(path, Files.DirectoryName, Files[i].FullPath));
                    validation = await ValidateExistingFileAsync(file, piecesOffset, validation);
                    if (!validation.isValid && options.Tolerance == 1)
                    {
                        break;
                    }

                    validation.remainder = (Files[i].FileSize + previousRemainder) % PieceSize; // Set again the remainder in case the file was not existing or partially good
                    piecesOffset += (int)((Files[i].FileSize + previousRemainder) / PieceSize);
                }
            }

            return ((double)validation.piecesValidated / (double)NumberOfPieces) >= options.Tolerance;
        }

        /// <summary>
        /// Validate integrity of an existing file
        /// </summary>
        /// <param name="file">file to validate</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD111:Use ConfigureAwait(bool)", Justification = "<Pending>")]
        private async Task<ValidationData> ValidateExistingFileAsync(FileInfo file)
        {
            return await ValidateExistingFileAsync(file, 0, new ValidationData(PieceSize, true)); ;
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
        private async Task<ValidationData> ValidateExistingFileAsync(FileInfo file, int piecesOffset, ValidationData validation)
        {
            if (!file.Exists)
            {
                validation.isValid = false;
                return validation;
            }

            int piecesIndex = piecesOffset, bytesRead = (int)validation.remainder;
            using (var stream = file.OpenRead())
            {
                while ((bytesRead += await stream.ReadAsync(validation.buffer, (int)validation.remainder, (int)(PieceSize - validation.remainder))) == PieceSize)
                {
                    var isFileTooLarge = piecesIndex >= NumberOfPieces;
                    var isPieceNotMatching = !isFileTooLarge && !Pieces[piecesIndex].SequenceEqual(sha1.ComputeHash(validation.buffer)) && options.Tolerance == 1;
                    if (isFileTooLarge || isPieceNotMatching)
                    {
                        validation.isValid = false;
                        return validation;
                    }

                    validation.piecesValidated++;
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
            validation.piecesValidated += (validation.isValid ? 1 : 0);

            return validation;
        }
    }
}
