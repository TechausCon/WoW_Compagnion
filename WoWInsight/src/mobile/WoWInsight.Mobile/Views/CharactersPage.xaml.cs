using WoWInsight.Mobile.ViewModels;

namespace WoWInsight.Mobile.Views;

public partial class CharactersPage : ContentPage
{
    private readonly CharactersViewModel _viewModel;

    public CharactersPage(CharactersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
