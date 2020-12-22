using System;
using System.Collections.Generic;
using System.Text;

namespace BencodeNET.Torrents.Validation
{
    /// <summary>
    /// Options for torrent file(s) validation
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        /// What percentage validated is considered as valid torrent existing data
        /// </summary>
        /// <remarks>>=1 = 100%, 0.95 = 95%. Only valid with torrent in MultiFile mode.</remarks>
        public double Tolerance { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        public static readonly ValidationOptions DefaultOptions = new ValidationOptions();
    }
}
