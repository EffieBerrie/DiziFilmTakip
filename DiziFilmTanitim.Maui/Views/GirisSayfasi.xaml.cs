using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class GirisSayfasi : ContentPage
{
    public GirisSayfasi(GirisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}