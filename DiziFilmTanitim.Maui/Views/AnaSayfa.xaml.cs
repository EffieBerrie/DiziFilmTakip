using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class AnaSayfa : ContentPage
{
    public AnaSayfa(AnaSayfaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}