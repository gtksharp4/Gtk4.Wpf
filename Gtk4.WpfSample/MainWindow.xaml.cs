namespace WpfSample
{
	// Unmodified WPF code-behind: partial class over System.Windows.Window, InitializeComponent is
	// generated at build time by the Gtk4.Wpf markup compiler from the untouched MainWindow.xaml.
	public partial class MainWindow : System.Windows.Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
		}

		// Standard WPF code-behind lifecycle handler, wired from Loaded="Window_Loaded" in XAML.
		void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Title = Title + " (loaded)";
		}
	}
}
