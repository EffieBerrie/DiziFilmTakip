using Microsoft.Extensions.Logging;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.ViewModels;
using DiziFilmTanitim.MAUI.Views;
using CommunityToolkit.Maui;

namespace DiziFilmTanitim.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// HttpClient yapılandırması
		builder.Services.AddSingleton<HttpClient>(provider =>
		{
			var handler = new HttpClientHandler();
#if DEBUG && WINDOWS
			// Windows'ta localhost bağlantılarını düzelt
			handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
			var client = new HttpClient(handler);
			client.BaseAddress = new Uri("http://localhost:5097/");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
			client.Timeout = TimeSpan.FromSeconds(30);
			return client;
		});

		// Services
		builder.Services.AddSingleton<ILoggingService, LoggingService>();
		builder.Services.AddSingleton<IApiService, ApiService>();
		builder.Services.AddSingleton<IUserService, UserService>();

		// ViewModels - Tüm ViewModels artık ILoggingService kullanıyor
		builder.Services.AddTransient<AnaSayfaViewModel>();
		builder.Services.AddTransient<GirisViewModel>();
		builder.Services.AddTransient<KayitViewModel>();
		builder.Services.AddTransient<FilmlerViewModel>();
		builder.Services.AddTransient<FilmDetayViewModel>();
		builder.Services.AddTransient<DizilerViewModel>();
		builder.Services.AddTransient<DiziDetayViewModel>();
		builder.Services.AddTransient<ProfilViewModel>();
		builder.Services.AddTransient<ListelerViewModel>();
		builder.Services.AddTransient<KullaniciListeDetayViewModel>();
		builder.Services.AddTransient<SifreDegistirViewModel>();
		builder.Services.AddTransient<AramaViewModel>();

		// Views
		builder.Services.AddTransient<AnaSayfa>();
		builder.Services.AddTransient<GirisSayfasi>();
		builder.Services.AddTransient<KayitSayfasi>();
		builder.Services.AddTransient<FilmlerSayfasi>();
		builder.Services.AddTransient<DizilerSayfasi>();
		builder.Services.AddTransient<AramaSayfasi>();
		builder.Services.AddTransient<ListelerSayfasi>();
		builder.Services.AddTransient<ProfilSayfasi>();
		builder.Services.AddTransient<FilmDetaySayfasi>();
		builder.Services.AddTransient<DiziDetaySayfasi>();
		builder.Services.AddTransient<KullaniciListeDetaySayfasi>();
		builder.Services.AddTransient<SifreDegistirSayfasi>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
