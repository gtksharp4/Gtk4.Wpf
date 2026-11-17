using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11709, "[Bug] [WPF] ScrollView - ScrollBarVisibility not respected ", PlatformAffected.WPF)]
	public partial class Issue11709 : ContentPage
	{
		public Issue11709()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}