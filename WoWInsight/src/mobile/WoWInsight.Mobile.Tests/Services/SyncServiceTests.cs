using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using WoWInsight.Mobile.DTOs;
using WoWInsight.Mobile.Models;
using WoWInsight.Mobile.Services;
using Xunit;

namespace WoWInsight.Mobile.Tests.Services;

public class SyncServiceTests
{
    private readonly Mock<IBackendApiClient> _apiClientMock;
    private readonly Mock<ILocalDbService> _localDbMock;
    private readonly SyncService _syncService;

    public SyncServiceTests()
    {
        _apiClientMock = new Mock<IBackendApiClient>();
        _localDbMock = new Mock<ILocalDbService>();
        _syncService = new SyncService(_apiClientMock.Object, _localDbMock.Object);
    }

    [Fact]
    public async Task SyncCharactersAsync_SyncsAllCharacters()
    {
        // Arrange
        var characters = new List<CharacterDto>
        {
            new CharacterDto { CharacterKey = "c1", Name = "Char1" },
            new CharacterDto { CharacterKey = "c2", Name = "Char2" }
        };
        _apiClientMock.Setup(client => client.GetCharactersAsync()).ReturnsAsync(characters);
        _apiClientMock.Setup(client => client.GetMythicPlusSummaryAsync(It.IsAny<string>()))
            .ReturnsAsync(new MythicPlusSummaryDto());

        // Act
        var result = await _syncService.SyncCharactersAsync();

        // Assert
        Assert.True(result);
        _localDbMock.Verify(db => db.SaveCharactersAsync(It.Is<List<Character>>(list => list.Count == 2)), Times.Once);
        _apiClientMock.Verify(client => client.GetMythicPlusSummaryAsync("c1"), Times.Once);
        _apiClientMock.Verify(client => client.GetMythicPlusSummaryAsync("c2"), Times.Once);
    }

    [Fact]
    public async Task SyncCharactersAsync_HandlesPartialFailure()
    {
        // Arrange
        var characters = new List<CharacterDto>
        {
            new CharacterDto { CharacterKey = "c1" },
            new CharacterDto { CharacterKey = "c2" }
        };
        _apiClientMock.Setup(client => client.GetCharactersAsync()).ReturnsAsync(characters);

        // Mock c1 fails
        _apiClientMock.Setup(client => client.GetMythicPlusSummaryAsync("c1")).ThrowsAsync(new System.Exception("API Error"));
        // Mock c2 success
        _apiClientMock.Setup(client => client.GetMythicPlusSummaryAsync("c2")).ReturnsAsync(new MythicPlusSummaryDto());

        // Act
        var result = await _syncService.SyncCharactersAsync();

        // Assert
        Assert.True(result); // Overall sync is success (as defined in SyncService implementation)
        _apiClientMock.Verify(client => client.GetMythicPlusSummaryAsync("c1"), Times.Once);
        _apiClientMock.Verify(client => client.GetMythicPlusSummaryAsync("c2"), Times.Once);
        // Ensure db save was called for M+ summary only for c2 (c1 failed before saving M+)
        // Wait, SyncMythicPlusAsync saves to DB. If GetMythicPlusSummaryAsync throws, DB save is skipped.
        _localDbMock.Verify(db => db.SaveMythicPlusSummaryAsync(It.Is<MythicPlusSummary>(s => s.CharacterKey == "c2")), Times.Once);
        _localDbMock.Verify(db => db.SaveMythicPlusSummaryAsync(It.Is<MythicPlusSummary>(s => s.CharacterKey == "c1")), Times.Never);
    }
}
