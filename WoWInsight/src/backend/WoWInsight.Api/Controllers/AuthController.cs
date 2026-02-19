using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            // Handle callback and get backend JWT
            var backendToken = await _authService.HandleCallbackAsync(code, state);

            // Redirect to Deep Link
            // Scheme: wowinsight://
            // Route: wowinsight://auth/callback?token=...
            var deepLink = $"wowinsight://auth/callback?token={backendToken}";

            // We can return a redirect to the deep link directly.
            // But browsers might show a warning or not redirect if schemes are unknown.
            // Usually, we render a simple HTML page that does window.location = ...
            // Or just return Redirect().
            // Modern browsers handle custom scheme redirects well if app is installed.

            return Redirect(deepLink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
