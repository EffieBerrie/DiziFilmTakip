using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

public partial class ProfilSayfasi : ContentPage
{
    private readonly ProfilViewModel _viewModel;
    public ProfilSayfasi(ProfilViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.KullaniciBilgileriniYukle(); // Sayfa göründüğünde kullanıcı bilgilerini yükle
    }
}