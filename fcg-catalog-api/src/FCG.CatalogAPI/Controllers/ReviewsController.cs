using System.Security.Claims;
using FCG.Catalog.Infrastructure.NoSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.Controllers;

[ApiController]
[Route("api/games/{gameId:guid}/reviews")]
[Authorize]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly GameReviewRepository _repository;

    public ReviewsController(GameReviewRepository repository) => _repository = repository;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid gameId, [FromQuery] int limit = 20, CancellationToken ct = default)
        => Ok(await _repository.ListByGameAsync(gameId, limit, ct));

    [HttpGet("summary")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(Guid gameId, CancellationToken ct)
    {
        var (summary, source) = await _repository.GetRatingSummaryAsync(gameId, ct);
        Response.Headers["X-Data-Source"] = source;
        return Ok(new { source, data = summary });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(Guid gameId, [FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5)
            return BadRequest(new { error = "Rating deve estar entre 1 e 5." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? Guid.Empty.ToString());

        var review = await _repository.AddAsync(new GameReviewDocument
        {
            GameId = gameId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            Tags = request.Tags ?? new List<string>()
        }, ct);

        return CreatedAtAction(nameof(List), new { gameId }, review);
    }
}

public record CreateReviewRequest(int Rating, string Title, string? Comment, List<string>? Tags);
