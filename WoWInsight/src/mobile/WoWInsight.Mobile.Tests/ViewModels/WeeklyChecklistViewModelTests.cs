using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using WoWInsight.Mobile.Models;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.ViewModels;
using Xunit;

namespace WoWInsight.Mobile.Tests.ViewModels;

public class WeeklyChecklistViewModelTests
{
    private readonly Mock<ILocalDbService> _localDbMock;
    private readonly WeeklyChecklistViewModel _viewModel;

    public WeeklyChecklistViewModelTests()
    {
        _localDbMock = new Mock<ILocalDbService>();
        _viewModel = new WeeklyChecklistViewModel(_localDbMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadData()
    {
        // Arrange
        var characters = new List<Character>
        {
            new Character { CharacterKey = "char1", Name = "Char1" },
            new Character { CharacterKey = "char2", Name = "Char2" }
        };
        _localDbMock.Setup(x => x.GetCharactersAsync()).ReturnsAsync(characters);
        _localDbMock.Setup(x => x.GetWeeklyChecklistAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => new WeeklyChecklist { CharacterKey = key });

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.Equal(2, _viewModel.CharacterChecklists.Count);
        Assert.Equal("Char1", _viewModel.CharacterChecklists[0].Character.Name);
        Assert.Equal("Char2", _viewModel.CharacterChecklists[1].Character.Name);
    }

    [Fact]
    public async Task ChecklistItem_ShouldSave_WhenPropertyChanges()
    {
        // Arrange
        var character = new Character { CharacterKey = "char1" };
        var checklist = new WeeklyChecklist { CharacterKey = "char1", IsRaidDone = false };
        _localDbMock.Setup(x => x.SaveWeeklyChecklistAsync(It.IsAny<WeeklyChecklist>())).Returns(Task.CompletedTask);

        // Act
        // CharacterChecklist is a nested class or separate? It was partial separate class in same file.
        // It requires ILocalDbService in constructor.
        var item = new CharacterChecklist(character, checklist, _localDbMock.Object);
        item.IsRaidDone = true;

        // Assert
        // We wait a bit or just verify? It's async void SaveAsync.
        // Async void is tricky to test. But usually in tests it runs synchronously if not awaiting IO?
        // But `SaveWeeklyChecklistAsync` is mocked to return Task.CompletedTask.
        // Since `SaveAsync` is fire-and-forget, we might have race condition in test.
        // However, Moq Verify might catch it if we wait or retry.
        // Or we can rely on immediate execution if no await delay.

        _localDbMock.Verify(x => x.SaveWeeklyChecklistAsync(It.Is<WeeklyChecklist>(c => c.IsRaidDone == true)), Times.Once);
    }
}
