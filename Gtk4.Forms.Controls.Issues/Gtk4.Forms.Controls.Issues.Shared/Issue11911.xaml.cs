using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11911, "[Bug] Brushes can't use DynamicResource", PlatformAffected.All)]
	public partial class Issue11911 : ContentPage
	{
		public Issue11911()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}