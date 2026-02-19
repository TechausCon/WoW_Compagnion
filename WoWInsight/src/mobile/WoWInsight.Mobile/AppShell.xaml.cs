using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(CharacterDetailPage), typeof(CharacterDetailPage));
    }
}
