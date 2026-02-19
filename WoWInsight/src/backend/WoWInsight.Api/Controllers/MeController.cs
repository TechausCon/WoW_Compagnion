using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoWInsight.Application.Services;

namespace WoWInsight.Api.Controllers;

[ApiController]
[Route("me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICharacterService _characterService;

    public MeController(IAuthService authService, ICharacterService characterService)
    {
        _authService = authService;
        _characterService = characterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var user = await _authService.GetUserAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("characters")]
    public async Task<IActionResult> GetCharacters()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var characters = await _characterService.GetCharactersAsync(userId);
            return Ok(characters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        // JWT Sub is User Id (Guid)
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var guid))
        {
            return Guid.Empty;
        }
        return guid;
    }
}
