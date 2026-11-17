using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4459, "[UWP] BoxView CornerRadius doesn't work", PlatformAffected.UWP)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue4459 : ContentPage
	{
		public Issue4459()
		{
#if APP
			InitializeComponent();
#endif
		}

		void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
		{
#if APP
			BoxView.CornerRadius = new CornerRadius(double.Parse(TopLeft.Text), double.Parse(TopRight.Text),
			double.Parse(BottomLeft.Text), double.Parse(BottomRight.Text));
#endif
		}
	}
}