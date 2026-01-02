#nullable disable

#pragma warning disable CS1591

namespace MediaBrowser.Controller.Providers
{
    /// <summary>
    /// Represents lookup information for a game, used by metadata providers.
    /// </summary>
    public class GameInfo : ItemLookupInfo
    {
        /// <summary>
        /// Gets or sets the platform/console for this game (e.g., "Nintendo 64", "PlayStation").
        /// </summary>
        public string Platform { get; set; }
    }
}
