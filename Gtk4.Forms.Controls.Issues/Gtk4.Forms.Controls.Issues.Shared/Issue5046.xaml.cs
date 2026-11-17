using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5046, "CornerRadius doesn't work in explicit style when implicit style exists",
		   PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue5046 : ContentPage
	{
		public Issue5046()
		{
#if APP
			InitializeComponent();
#endif
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
#if APP
			this.DisplayAlert("CornerRadius", "CornerRadius = " + ((Button)sender).CornerRadius, "OK");
#endif
		}
	}
}