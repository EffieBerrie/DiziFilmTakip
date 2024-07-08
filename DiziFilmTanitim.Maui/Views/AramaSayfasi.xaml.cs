using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class AramaSayfasi : ContentPage
{
    public AramaSayfasi(AramaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}