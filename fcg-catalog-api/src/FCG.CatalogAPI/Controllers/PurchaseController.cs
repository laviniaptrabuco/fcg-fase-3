using System.Security.Claims;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.Controllers;

[ApiController]
[Route("api/games/{gameId:guid}/purchase")]
[Authorize]
[Produces("application/json")]
public class PurchaseController : ControllerBase
{
    private readonly PurchaseService _purchaseService;

    public PurchaseController(PurchaseService purchaseService) => _purchaseService = purchaseService;

    /// <summary>Inicia a compra de um jogo para o usuário autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Purchase(Guid gameId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? Guid.Empty.ToString());

        var result = await _purchaseService.PurchaseAsync(gameId, new PurchaseRequest(userId), ct);
        return Accepted(result);
    }

    /// <summary>Retorna a biblioteca de jogos do usuário autenticado.</summary>
    [HttpGet("/api/users/{userId:guid}/library")]
    [ProducesResponseType(typeof(IEnumerable<UserLibraryItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibrary(Guid userId, CancellationToken ct)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? Guid.Empty.ToString());

        if (!User.IsInRole("Admin") && currentUserId != userId)
            return Forbid();

        var library = await _purchaseService.GetUserLibraryAsync(userId, ct);
        return Ok(library);
    }
}
