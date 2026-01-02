#pragma warning disable CS1591

using System;

namespace MediaBrowser.Model.Configuration
{
    /// <summary>
    /// Configuration options for the gaming/emulation features.
    /// </summary>
    public class GamingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamingOptions"/> class.
        /// </summary>
        public GamingOptions()
        {
            EmulatorProfiles = Array.Empty<EmulatorProfile>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether gaming features are enabled.
        /// </summary>
        public bool EnableGaming { get; set; } = true;

        /// <summary>
        /// Gets or sets the configured emulator profiles.
        /// </summary>
        public EmulatorProfile[] EmulatorProfiles { get; set; }

        /// <summary>
        /// Gets or sets the Sunshine server host for game streaming.
        /// </summary>
        /// <example>192.168.1.100</example>
        public string SunshineHost { get; set; }

        /// <summary>
        /// Gets or sets the Sunshine server port.
        /// </summary>
        public int SunshinePort { get; set; } = 47989;

        /// <summary>
        /// Gets or sets a value indicating whether to use Sunshine for streaming.
        /// When enabled, games will be launched through Sunshine instead of directly.
        /// </summary>
        public bool UseSunshineStreaming { get; set; }

        /// <summary>
        /// Gets or sets the default emulator profile ID to use when no specific match is found.
        /// </summary>
        public string DefaultEmulatorProfileId { get; set; }
    }
}
