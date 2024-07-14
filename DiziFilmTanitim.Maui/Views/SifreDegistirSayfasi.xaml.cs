using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class SifreDegistirSayfasi : ContentPage
{
    public SifreDegistirSayfasi(SifreDegistirViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}