using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoWInsight.Application.Services;

namespace WoWInsight.Api.Controllers;

[ApiController]
[Route("characters")]
[Authorize]
public class CharactersController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharactersController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpGet("{characterKey}/mythicplus")]
    public async Task<IActionResult> GetMythicPlusSummary(string characterKey)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var summary = await _characterService.GetMythicPlusSummaryAsync(userId, characterKey);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            // Usually check exception type for 404/400 etc.
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var guid))
        {
            return Guid.Empty;
        }
        return guid;
    }
}
