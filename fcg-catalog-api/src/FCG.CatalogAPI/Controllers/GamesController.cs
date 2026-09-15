using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly GameService _gameService;

    public GamesController(GameService gameService) => _gameService = gameService;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var games = await _gameService.GetAllAsync(ct);
        return Ok(games);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameService.GetByIdAsync(id, ct);
        return Ok(game);
    }

    [HttpGet("genre/{genre}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGenre(string genre, CancellationToken ct)
    {
        var games = await _gameService.GetByGenreAsync(genre, ct);
        return Ok(games);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var result = await _gameService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        var result = await _gameService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/promotion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyPromotion(Guid id, [FromBody] ApplyPromotionRequest request, CancellationToken ct)
    {
        var result = await _gameService.ApplyPromotionAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/promotion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePromotion(Guid id, CancellationToken ct)
    {
        await _gameService.RemovePromotionAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _gameService.DeactivateAsync(id, ct);
        return NoContent();
    }
}
