using System;

namespace BencodeNET.Torrents
{
    /// <summary>
    /// Possible options for controlling magnet link generation.
    /// </summary>
    [Flags]
    public enum MagnetLinkOptions
    {
        /// <summary>
        /// Results in the bare minimum magnet link containing only info hash and display name.
        /// </summary>
        None = 0,

        /// <summary>
        /// Includes trackers in the magnet link.
        /// </summary>
        IncludeTrackers = 1
    }
}