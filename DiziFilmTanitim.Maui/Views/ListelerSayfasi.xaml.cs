using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class ListelerSayfasi : ContentPage
{
    private readonly ListelerViewModel _viewModel;
    public ListelerSayfasi(ListelerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.SayfaGorunurOldugundaAsync();
        }
    }
}