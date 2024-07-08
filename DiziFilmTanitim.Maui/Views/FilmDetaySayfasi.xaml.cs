using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

[QueryProperty(nameof(FilmId), "filmId")]
public partial class FilmDetaySayfasi : ContentPage
{
    private readonly FilmDetayViewModel _viewModel;

    public FilmDetaySayfasi(FilmDetayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public string FilmId
    {
        set
        {
            if (int.TryParse(value, out int filmId))
            {
                _viewModel.FilmId = filmId;
            }
        }
    }
}