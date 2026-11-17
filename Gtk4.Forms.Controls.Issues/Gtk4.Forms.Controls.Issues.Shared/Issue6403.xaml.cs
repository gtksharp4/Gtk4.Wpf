using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6403, "Gtk4.Forms UWP Picker collapses on opening Dropdown menu [Bug]", PlatformAffected.UWP)]
	public partial class Issue6403 : TestContentPage
	{

		public Issue6403()
		{
#if APP
			InitializeComponent();
#endif
		}


		protected override void Init()
		{

		}
	}
}