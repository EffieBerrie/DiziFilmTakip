using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class DizilerSayfasi : ContentPage
{
    public DizilerSayfasi(DizilerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}