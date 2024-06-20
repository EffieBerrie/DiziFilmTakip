namespace DiziFilmTanitim.MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// AppShell varsayılan olarak giriş sayfasını açacak
		return new Window(new AppShell());
	}
}