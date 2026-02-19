using WoWInsight.Mobile.ViewModels;

namespace WoWInsight.Mobile.Views;

public partial class CharacterDetailPage : ContentPage
{
    private readonly CharacterDetailViewModel _viewModel;

    public CharacterDetailPage(CharacterDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
