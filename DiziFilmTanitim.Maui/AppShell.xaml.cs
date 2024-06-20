using DiziFilmTanitim.MAUI.Views;

namespace DiziFilmTanitim.MAUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Routing kayıtları
		Routing.RegisterRoute("GirisSayfasi", typeof(GirisSayfasi));
		Routing.RegisterRoute("KayitSayfasi", typeof(KayitSayfasi));
		Routing.RegisterRoute("FilmDetay", typeof(FilmDetaySayfasi));
		Routing.RegisterRoute("DiziDetay", typeof(DiziDetaySayfasi));
		Routing.RegisterRoute("KullaniciListeDetay", typeof(KullaniciListeDetaySayfasi));
		Routing.RegisterRoute("SifreDegistirSayfasi", typeof(SifreDegistirSayfasi));
	}
}
