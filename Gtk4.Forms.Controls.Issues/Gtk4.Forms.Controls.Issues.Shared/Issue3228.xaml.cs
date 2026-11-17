using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using static Gtk4.Forms.PlatformConfiguration.WindowsSpecific.Application;
using WindowsOS = Gtk4.Forms.PlatformConfiguration.Windows;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3228, "[UWP] Default Search Directory for UWP Icons (Platform Specific)")]
	public partial class Issue3228 : TestContentPage
	{
		public Issue3228()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			Application.Current.On<WindowsOS>().SetImageDirectory("Assets");
		}
	}
}