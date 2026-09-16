using System.Security.Claims;
using FCG.Users.Infrastructure.NoSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.UsersAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProfilesController : ControllerBase
{
    private readonly UserProfileRepository _repository;

    public ProfilesController(UserProfileRepository repository) => _repository = repository;

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid userId, CancellationToken ct)
    {
        var (profile, source) = await _repository.GetAsync(userId, ct);
        if (profile is null)
            return NotFound();

        Response.Headers["X-Data-Source"] = source;
        return Ok(new { source, data = profile });
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upsert(Guid userId, [FromBody] UpsertProfileRequest request, CancellationToken ct)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? Guid.Empty.ToString());

        if (!User.IsInRole("Admin") && currentUserId != userId)
            return Forbid();

        var saved = await _repository.UpsertAsync(new UserProfileDocument
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            FavoriteGenres = request.FavoriteGenres ?? new List<string>(),
            Preferences = request.Preferences ?? new Dictionary<string, string>()
        }, ct);

        return Ok(saved);
    }

    [HttpGet("by-genre/{genre}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchByGenre(string genre, CancellationToken ct)
        => Ok(await _repository.SearchByGenreAsync(genre, ct));
}

public record UpsertProfileRequest(
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    List<string>? FavoriteGenres,
    Dictionary<string, string>? Preferences);
