using WoWInsight.Mobile.ViewModels;

namespace WoWInsight.Mobile.Views;

public partial class WeeklyChecklistPage : ContentPage
{
    private readonly WeeklyChecklistViewModel _viewModel;

    public WeeklyChecklistPage(WeeklyChecklistViewModel viewModel)
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
