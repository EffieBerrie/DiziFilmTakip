using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class FilmlerSayfasi : ContentPage
{
    public FilmlerSayfasi(FilmlerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}