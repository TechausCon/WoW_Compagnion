using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using Moq.Protected;
using WoWInsight.Mobile.Services;
using Xunit;
using System;
using System.Net;

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
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

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
            })
            .Verifiable();

        // Act
        await _apiClient.GetCharactersAsync();

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Authorization != null &&
                req.Headers.Authorization.Parameter == "test-token"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetCharactersAsync_HandlesUnauthorized()
    {
        // Arrange
        _authServiceMock.Setup(a => a.GetTokenAsync()).ReturnsAsync("expired-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _apiClient.GetCharactersAsync());

        _authServiceMock.Verify(a => a.DeleteTokenAsync(), Times.Once);
    }
}
