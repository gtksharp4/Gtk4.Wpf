namespace WpfSample
{
	// Plain MVVM view model — never references any WPF or GTK type. It binds unchanged.
	public class MainViewModel
	{
		public string Greeting => "Hello from unmodified WPF XAML, rendered on GTK 4";
	}
}
