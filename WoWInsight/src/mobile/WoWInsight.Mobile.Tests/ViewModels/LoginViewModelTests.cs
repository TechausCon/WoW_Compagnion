using System;
using System.Threading.Tasks;
using Moq;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.ViewModels;
using Xunit;

namespace WoWInsight.Mobile.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly Mock<IBackendApiClient> _apiClientMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<IBrowserService> _browserServiceMock;
    private readonly LoginViewModel _viewModel;

    public LoginViewModelTests()
    {
        _apiClientMock = new Mock<IBackendApiClient>();
        _authServiceMock = new Mock<IAuthService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _browserServiceMock = new Mock<IBrowserService>();

        _viewModel = new LoginViewModel(
            _apiClientMock.Object,
            _authServiceMock.Object,
            _navigationServiceMock.Object,
            _dialogServiceMock.Object,
            _browserServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldOpenBrowser_WhenCalled()
    {
        // Arrange
        var authUrl = "https://example.com/auth";
        _apiClientMock.Setup(x => x.GetAuthUrl()).Returns(authUrl);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _browserServiceMock.Verify(x => x.OpenAsync(authUrl), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldShowError_WhenBrowserFails()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetAuthUrl()).Throws(new Exception("Fail"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CheckLoginStatusAsync_ShouldNavigate_WhenAuthenticated()
    {
        // Arrange
        _authServiceMock.Setup(x => x.GetTokenAsync()).ReturnsAsync("token");

        // Act
        await _viewModel.CheckLoginStatusAsync();

        // Assert
        _navigationServiceMock.Verify(x => x.GoToAsync("//main/characters"), Times.Once);
    }

    [Fact]
    public async Task CheckLoginStatusAsync_ShouldNotNavigate_WhenNotAuthenticated()
    {
        // Arrange
        _authServiceMock.Setup(x => x.GetTokenAsync()).ReturnsAsync((string?)null);

        // Act
        await _viewModel.CheckLoginStatusAsync();

        // Assert
        _navigationServiceMock.Verify(x => x.GoToAsync(It.IsAny<string>()), Times.Never);
    }
}
