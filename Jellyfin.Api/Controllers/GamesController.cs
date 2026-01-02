using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Jellyfin.Api.Extensions;
using Jellyfin.Api.Helpers;
using Jellyfin.Api.ModelBinders;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Games;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Api.Controllers;

/// <summary>
/// Games controller for managing game libraries and launching games.
/// </summary>
[Route("[controller]")]
[Authorize]
public class GamesController : BaseJellyfinApiController
{
    private readonly IUserManager _userManager;
    private readonly ILibraryManager _libraryManager;
    private readonly IDtoService _dtoService;
    private readonly IServerConfigurationManager _configurationManager;
    private readonly ILogger<GamesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GamesController"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="dtoService">Instance of the <see cref="IDtoService"/> interface.</param>
    /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{GamesController}"/> interface.</param>
    public GamesController(
        IUserManager userManager,
        ILibraryManager libraryManager,
        IDtoService dtoService,
        IServerConfigurationManager configurationManager,
        ILogger<GamesController> logger)
    {
        _userManager = userManager;
        _libraryManager = libraryManager;
        _dtoService = dtoService;
        _configurationManager = configurationManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets all games.
    /// </summary>
    /// <param name="userId">Optional. Filter by user id.</param>
    /// <param name="parentId">Optional. Specify a parent folder to search within.</param>
    /// <param name="startIndex">Optional. The record index to start at.</param>
    /// <param name="limit">Optional. The maximum number of records to return.</param>
    /// <param name="searchTerm">Optional. Filter based on a search term.</param>
    /// <param name="sortBy">Optional. Specify one or more sort orders.</param>
    /// <param name="sortOrder">Sort Order - Ascending or Descending.</param>
    /// <param name="fields">Optional. Specify additional fields of information to return.</param>
    /// <param name="enableImages">Optional. Include image information in output.</param>
    /// <param name="enableTotalRecordCount">Optional. Include total record count.</param>
    /// <response code="200">Games returned.</response>
    /// <returns>An <see cref="QueryResult{BaseItemDto}"/> with the games.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<QueryResult<BaseItemDto>> GetGames(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? parentId,
        [FromQuery] int? startIndex,
        [FromQuery] int? limit,
        [FromQuery] string? searchTerm,
        [FromQuery, ModelBinder(typeof(CommaDelimitedCollectionModelBinder))] ItemSortBy[] sortBy,
        [FromQuery, ModelBinder(typeof(CommaDelimitedCollectionModelBinder))] SortOrder[] sortOrder,
        [FromQuery, ModelBinder(typeof(CommaDelimitedCollectionModelBinder))] ItemFields[] fields,
        [FromQuery] bool? enableImages,
        [FromQuery] bool enableTotalRecordCount = true)
    {
        var user = userId.HasValue && !userId.Value.IsEmpty()
            ? _userManager.GetUserById(userId.Value)
            : null;

        var dtoOptions = new DtoOptions { Fields = fields }
            .AddClientFields(User);

        if (enableImages.HasValue)
        {
            dtoOptions.EnableImages = enableImages.Value;
        }

        var query = new InternalItemsQuery(user)
        {
            MediaTypes = new[] { MediaType.Game },
            StartIndex = startIndex,
            Limit = limit,
            SearchTerm = searchTerm,
            OrderBy = RequestHelpers.GetOrderBy(sortBy, sortOrder),
            DtoOptions = dtoOptions,
            Recursive = true,
            EnableTotalRecordCount = enableTotalRecordCount
        };

        if (parentId.HasValue && !parentId.Value.IsEmpty())
        {
            var parentItem = _libraryManager.GetItemById(parentId.Value);
            if (parentItem is not null)
            {
                query.AncestorIds = new[] { parentId.Value };
            }
        }

        var result = _libraryManager.GetItemsResult(query);

        var dtos = result.Items.Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user));

        return new QueryResult<BaseItemDto>(
            startIndex,
            result.TotalRecordCount,
            dtos.ToArray());
    }

    /// <summary>
    /// Gets a game by id.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="userId">Optional. Filter by user id.</param>
    /// <response code="200">Game returned.</response>
    /// <response code="404">Game not found.</response>
    /// <returns>An <see cref="BaseItemDto"/> with the game.</returns>
    [HttpGet("{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BaseItemDto> GetGame(
        [FromRoute, Required] Guid itemId,
        [FromQuery] Guid? userId)
    {
        var user = userId.HasValue && !userId.Value.IsEmpty()
            ? _userManager.GetUserById(userId.Value)
            : null;

        var item = _libraryManager.GetItemById(itemId);

        if (item is not Game game)
        {
            return NotFound();
        }

        var dtoOptions = new DtoOptions().AddClientFields(User);

        return _dtoService.GetBaseItemDto(game, dtoOptions, user);
    }

    /// <summary>
    /// Launches a game. Returns launch information for the game streaming client.
    /// </summary>
    /// <remarks>
    /// Returns emulator configuration based on the game's platform and file extension.
    /// If Sunshine streaming is configured, includes Sunshine app information.
    /// </remarks>
    /// <param name="itemId">The item id of the game to launch.</param>
    /// <response code="200">Game launch info returned.</response>
    /// <response code="404">Game not found.</response>
    /// <returns>Launch information for the game.</returns>
    [HttpPost("{itemId}/Launch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameLaunchInfo> LaunchGame([FromRoute, Required] Guid itemId)
    {
        var item = _libraryManager.GetItemById(itemId);

        if (item is not Game game)
        {
            return NotFound();
        }

        _logger.LogInformation("Game launch requested: {GameName} ({GameId})", game.Name, game.Id);

        var gamingOptions = _configurationManager.Configuration.GamingOptions;
        var romPath = game.RomPath ?? game.Path;
        var fileExtension = Path.GetExtension(romPath);

        // Find matching emulator profile
        EmulatorProfile? emulatorProfile = null;
        if (gamingOptions?.EmulatorProfiles is not null)
        {
            // First try to match by platform
            emulatorProfile = gamingOptions.EmulatorProfiles
                .FirstOrDefault(p => p.IsEnabled && p.Platforms.Contains(game.Platform, StringComparer.OrdinalIgnoreCase));

            // If no match, try by file extension
            emulatorProfile ??= gamingOptions.EmulatorProfiles
                .FirstOrDefault(p => p.IsEnabled && p.FileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase));

            // Fall back to default profile
            if (emulatorProfile is null && !string.IsNullOrEmpty(gamingOptions.DefaultEmulatorProfileId))
            {
                emulatorProfile = gamingOptions.EmulatorProfiles
                    .FirstOrDefault(p => p.Id == gamingOptions.DefaultEmulatorProfileId);
            }
        }

        var launchInfo = new GameLaunchInfo
        {
            GameId = game.Id,
            GameName = game.Name,
            Platform = game.Platform,
            RomPath = romPath,
            EmulatorId = game.EmulatorId,
            Status = "ready"
        };

        if (emulatorProfile is not null)
        {
            launchInfo.EmulatorName = emulatorProfile.Name;
            launchInfo.EmulatorPath = emulatorProfile.ExecutablePath;
            launchInfo.CommandLineArgs = emulatorProfile.CommandLineArgs?.Replace("{rom}", romPath);
            launchInfo.SunshineAppName = emulatorProfile.SunshineAppName;
            launchInfo.Message = $"Launch with {emulatorProfile.Name}";
        }
        else
        {
            launchInfo.Message = "No emulator configured for this platform. Configure emulators in server settings.";
        }

        // Include Sunshine streaming info if configured
        if (gamingOptions?.UseSunshineStreaming == true && !string.IsNullOrEmpty(gamingOptions.SunshineHost))
        {
            launchInfo.SunshineHost = gamingOptions.SunshineHost;
            launchInfo.SunshinePort = gamingOptions.SunshinePort;
            launchInfo.UseSunshineStreaming = true;
        }

        return launchInfo;
    }
}

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
