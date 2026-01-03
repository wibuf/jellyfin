using System;

namespace Jellyfin.Api.Models.GameDtos;

/// <summary>
/// Information returned when launching a game.
/// </summary>
public class GameLaunchInfo
{
    /// <summary>
    /// Gets or sets the game's unique identifier.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the game's name.
    /// </summary>
    public string? GameName { get; set; }

    /// <summary>
    /// Gets or sets the platform/console for the game.
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// Gets or sets the path to the ROM or game file.
    /// </summary>
    public string? RomPath { get; set; }

    /// <summary>
    /// Gets or sets the emulator configuration ID from the game.
    /// </summary>
    public string? EmulatorId { get; set; }

    /// <summary>
    /// Gets or sets the name of the matched emulator.
    /// </summary>
    public string? EmulatorName { get; set; }

    /// <summary>
    /// Gets or sets the path to the emulator executable.
    /// </summary>
    public string? EmulatorPath { get; set; }

    /// <summary>
    /// Gets or sets the command line arguments for launching.
    /// </summary>
    public string? CommandLineArgs { get; set; }

    /// <summary>
    /// Gets or sets the Sunshine app name for streaming.
    /// </summary>
    public string? SunshineAppName { get; set; }

    /// <summary>
    /// Gets or sets the Sunshine server host.
    /// </summary>
    public string? SunshineHost { get; set; }

    /// <summary>
    /// Gets or sets the Sunshine server port.
    /// </summary>
    public int? SunshinePort { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use Sunshine for streaming.
    /// </summary>
    public bool UseSunshineStreaming { get; set; }

    /// <summary>
    /// Gets or sets the launch status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets any message about the launch.
    /// </summary>
    public string? Message { get; set; }
}
