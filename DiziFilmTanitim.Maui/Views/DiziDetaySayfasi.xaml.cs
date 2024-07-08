using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

[QueryProperty(nameof(DiziId), "diziId")]
public partial class DiziDetaySayfasi : ContentPage
{
    private readonly DiziDetayViewModel _viewModel;

    public DiziDetaySayfasi(DiziDetayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public string DiziId
    {
        set
        {
            if (int.TryParse(value, out int diziId))
            {
                _viewModel.DiziId = diziId;
            }
        }
    }
}