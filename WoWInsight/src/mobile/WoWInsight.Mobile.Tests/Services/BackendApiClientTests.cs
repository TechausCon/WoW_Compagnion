using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WoWInsight.Mobile.DTOs;
using WoWInsight.Mobile.Services;
using Xunit;

namespace WoWInsight.Mobile.Tests.Services;

public class BackendApiClientTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IAppConfig> _appConfigMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly BackendApiClient _apiClient;

    public BackendApiClientTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _appConfigMock = new Mock<IAppConfig>();
        _appConfigMock.Setup(c => c.ApiBaseUrl).Returns("http://test.com");

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com")
        };

        _apiClient = new BackendApiClient(httpClient, _authServiceMock.Object, _appConfigMock.Object);
    }

    [Fact]
    public async Task GetCharactersAsync_AddsAuthHeader()
    {
        // Arrange
        _authServiceMock.Setup(a => a.GetTokenAsync()).ReturnsAsync("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        // Act
        await _apiClient.GetCharactersAsync();

        // Assert
        _authServiceMock.Verify(x => x.GetTokenAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUnauthorized_AndRefreshes()
    {
        // Arrange
        _authServiceMock.Setup(a => a.GetTokenAsync())
            .ReturnsAsync("expired-token");

        _authServiceMock.Setup(a => a.GetRefreshTokenAsync()).ReturnsAsync("refresh-token");

        // Mock Sequence:
        // 1. Original Request -> 401
        // 2. Refresh Request -> 200 OK
        // 3. Retry Original Request -> 200 OK
        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new TokenResponse { AccessToken = "new-token", RefreshToken = "new-refresh" })
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        // Act
        await _apiClient.GetCharactersAsync();

        // Assert
        _authServiceMock.Verify(a => a.SaveTokensAsync("new-token", "new-refresh"), Times.Once);
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUnauthorized_AndFailsRefresh()
    {
        // Arrange
        _authServiceMock.Setup(a => a.GetTokenAsync()).ReturnsAsync("expired-token");
        _authServiceMock.Setup(a => a.GetRefreshTokenAsync()).ReturnsAsync("refresh-token");

        // Mock Sequence:
        // 1. Original -> 401
        // 2. Refresh -> 401 (Failed)
        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _apiClient.GetCharactersAsync());

        _authServiceMock.Verify(a => a.DeleteTokensAsync(), Times.Once);
    }
}
