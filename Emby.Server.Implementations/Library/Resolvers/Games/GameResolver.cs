#nullable disable

#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Entities.Games;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;

namespace Emby.Server.Implementations.Library.Resolvers.Games
{
    /// <summary>
    /// Resolves game files (ROMs) from the filesystem.
    /// </summary>
    public class GameResolver : ItemResolver<Game>
    {
        /// <summary>
        /// Mapping of file extensions to platform names.
        /// </summary>
        private static readonly Dictionary<string, string> ExtensionToPlatform = new(StringComparer.OrdinalIgnoreCase)
        {
            // Nintendo
            { ".nes", "Nintendo Entertainment System" },
            { ".sfc", "Super Nintendo" },
            { ".smc", "Super Nintendo" },
            { ".n64", "Nintendo 64" },
            { ".z64", "Nintendo 64" },
            { ".v64", "Nintendo 64" },
            { ".nds", "Nintendo DS" },
            { ".3ds", "Nintendo 3DS" },
            { ".gb", "Game Boy" },
            { ".gbc", "Game Boy Color" },
            { ".gba", "Game Boy Advance" },
            { ".gcm", "GameCube" },
            { ".gcz", "GameCube" },
            { ".wbfs", "Wii" },
            { ".wad", "Wii" },

            // Sega
            { ".sms", "Sega Master System" },
            { ".gg", "Sega Game Gear" },
            { ".md", "Sega Genesis" },
            { ".gen", "Sega Genesis" },
            { ".32x", "Sega 32X" },
            { ".cdi", "Sega Dreamcast" },
            { ".gdi", "Sega Dreamcast" },

            // Sony
            { ".pbp", "PlayStation Portable" },
            { ".pkg", "PlayStation" },

            // Atari
            { ".a26", "Atari 2600" },
            { ".a78", "Atari 7800" },
            { ".lnx", "Atari Lynx" },
            { ".jag", "Atari Jaguar" },

            // Other
            { ".pce", "TurboGrafx-16" },
            { ".ngp", "Neo Geo Pocket" },
            { ".ngc", "Neo Geo Pocket Color" },
            { ".ws", "WonderSwan" },
            { ".wsc", "WonderSwan Color" },

            // Disc images (multi-platform)
            { ".iso", "Disc Image" },
            { ".bin", "Disc Image" },
            { ".cue", "Disc Image" },
            { ".chd", "Compressed Disc Image" },

            // Archives (ROMs inside)
            { ".zip", "Archive" },
            { ".7z", "Archive" }
        };

        /// <inheritdoc />
        protected override Game Resolve(ItemResolveArgs args)
        {
            var collectionType = args.GetCollectionType();

            // Only process items that are in a games collection
            if (collectionType != CollectionType.games)
            {
                return null;
            }

            // Skip directories for now - individual files only
            if (args.IsDirectory)
            {
                return null;
            }

            var extension = Path.GetExtension(args.Path.AsSpan());

            if (extension.IsEmpty)
            {
                return null;
            }

            var extensionString = extension.ToString();
            if (!ExtensionToPlatform.TryGetValue(extensionString, out var platform))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(args.Path);
            var gameName = CleanGameName(fileName);

            return new Game
            {
                Path = args.Path,
                RomPath = args.Path,
                Name = gameName,
                Platform = platform,
                IsInMixedFolder = true
            };
        }

        /// <summary>
        /// Cleans up the game name by removing common ROM naming conventions.
        /// Handles No-Intro and GoodTools naming patterns.
        /// </summary>
        /// <param name="fileName">The filename without extension.</param>
        /// <returns>A cleaned game name.</returns>
        private static string CleanGameName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            var name = fileName;

            // Remove content in parentheses (region codes, versions, etc.)
            // e.g., "Super Mario 64 (USA)" -> "Super Mario 64"
            var parenIndex = name.IndexOf('(', StringComparison.Ordinal);
            if (parenIndex > 0)
            {
                name = name[..parenIndex];
            }

            // Remove content in square brackets (GoodTools codes)
            // e.g., "Super Mario Bros [!]" -> "Super Mario Bros"
            var bracketIndex = name.IndexOf('[', StringComparison.Ordinal);
            if (bracketIndex > 0)
            {
                name = name[..bracketIndex];
            }

            return name.Trim();
        }
    }
}
