using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoWInsight.Application.DTOs;
using WoWInsight.Application.Services;

namespace WoWInsight.Api.Controllers;

[ApiController]
[Route("auth/blizzard")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("start")]
    public async Task<IActionResult> StartLogin([FromQuery] string region = "eu")
    {
        try
        {
            var authUrl = await _authService.StartLoginAsync(region);
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return BadRequest("Invalid callback parameters.");
        }

        try
        {
            // Handle callback and get backend JWT + Refresh
            var (backendToken, refreshToken) = await _authService.HandleCallbackAsync(code, state);

            // Redirect to Deep Link
            var deepLink = $"wowinsight://auth/callback?token={backendToken}&refreshToken={refreshToken}";

            return Redirect(deepLink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken)) return BadRequest("Refresh token required.");

        try
        {
            var (access, refresh) = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(new TokenResponse { AccessToken = access, RefreshToken = refresh });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
