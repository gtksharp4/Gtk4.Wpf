using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9536, "PopModalAsync hides navigation bar on macOS", PlatformAffected.macOS)]
	public partial class Github9536 : ContentPage
	{
		public Github9536()
		{
#if APP
			Title = "PopModalAsync";
			InitializeComponent();
#endif
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new Issue2964.ModalPage()));
		}
	}
}