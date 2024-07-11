using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class KayitSayfasi : ContentPage
{
    public KayitSayfasi(KayitViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}