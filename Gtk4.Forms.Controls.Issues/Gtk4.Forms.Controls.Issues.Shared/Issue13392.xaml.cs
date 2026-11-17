using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 13392, "[Bug] Brush dissappear after scroll in ListView on Android", PlatformAffected.Android)]
	public partial class Issue13392 : TestContentPage
	{
		public Issue13392()
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
