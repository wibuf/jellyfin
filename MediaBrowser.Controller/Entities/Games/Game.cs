#nullable disable

#pragma warning disable CS1591

using System;
using System.Text.Json.Serialization;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Controller.Entities.Games
{
    /// <summary>
    /// Represents a game (ROM or game file) in the library.
    /// </summary>
    [Common.RequiresSourceSerialisation]
    public class Game : BaseItem, IHasLookupInfo<GameInfo>
    {
        /// <summary>
        /// Gets or sets the platform/console for this game (e.g., "Nintendo 64", "PlayStation", "GameBoy Advance").
        /// </summary>
        [JsonIgnore]
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the emulator configuration identifier to use for launching this game.
        /// </summary>
        [JsonIgnore]
        public string EmulatorId { get; set; }

        /// <summary>
        /// Gets or sets the path to the ROM or game file.
        /// </summary>
        [JsonIgnore]
        public string RomPath { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public override MediaType MediaType => MediaType.Game;

        /// <inheritdoc />
        public override bool SupportsPlayedStatus => true;

        /// <inheritdoc />
        [JsonIgnore]
        public override bool SupportsPeople => false;

        /// <inheritdoc />
        public override bool CanDownload()
        {
            return IsFileProtocol;
        }

        /// <inheritdoc />
        public override UnratedItem GetBlockUnratedType()
        {
            return UnratedItem.Game;
        }

        /// <summary>
        /// Gets the lookup info for metadata providers.
        /// </summary>
        /// <returns>The game lookup info.</returns>
        public GameInfo GetLookupInfo()
        {
            var info = GetItemLookupInfo<GameInfo>();
            info.Platform = Platform;
            return info;
        }
    }
}
