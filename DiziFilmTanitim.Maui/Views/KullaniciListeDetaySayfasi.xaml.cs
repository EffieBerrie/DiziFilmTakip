using DiziFilmTanitim.MAUI.ViewModels;

namespace DiziFilmTanitim.MAUI.Views;

[QueryProperty(nameof(ListeIdQuery), "listeId")]
public partial class KullaniciListeDetaySayfasi : ContentPage
{
    private readonly KullaniciListeDetayViewModel _viewModel;

    public KullaniciListeDetaySayfasi(KullaniciListeDetayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public string ListeIdQuery
    {
        set
        {
            if (int.TryParse(value, out int listeId))
            {
                _viewModel.ListeId = listeId;
            }
            else
            {
                // Hata yönetimi veya loglama eklenebilir
                Console.WriteLine($"Geçersiz listeId query parametresi: {value}");
            }
        }
    }

    // Sayfa göründüğünde verilerin yeniden yüklenmesi
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null && _viewModel.ListeId > 0)
        {
            await _viewModel.RefreshAsync(); // ViewModel'da bu metot eklenmeli
        }
    }
}