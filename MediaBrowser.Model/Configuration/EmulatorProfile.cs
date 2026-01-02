#pragma warning disable CS1591

using System;

namespace MediaBrowser.Model.Configuration
{
    /// <summary>
    /// Configuration for a specific emulator used to launch games.
    /// </summary>
    public class EmulatorProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatorProfile"/> class.
        /// </summary>
        public EmulatorProfile()
        {
            Id = Guid.NewGuid().ToString("N");
            Platforms = Array.Empty<string>();
            FileExtensions = Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the unique identifier for this profile.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display name for this emulator (e.g., "RetroArch", "Dolphin").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the platforms/consoles this emulator supports.
        /// </summary>
        /// <example>["Nintendo 64", "PlayStation"]</example>
        public string[] Platforms { get; set; }

        /// <summary>
        /// Gets or sets the file extensions this emulator handles.
        /// </summary>
        /// <example>[".n64", ".z64", ".iso"]</example>
        public string[] FileExtensions { get; set; }

        /// <summary>
        /// Gets or sets the path to the emulator executable.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets the command line arguments template.
        /// Use {rom} as placeholder for the ROM path.
        /// </summary>
        /// <example>-L /path/to/core.so "{rom}"</example>
        public string CommandLineArgs { get; set; }

        /// <summary>
        /// Gets or sets the Sunshine application name for streaming.
        /// If set, games will be launched through Sunshine using this app name.
        /// </summary>
        public string SunshineAppName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this emulator is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
