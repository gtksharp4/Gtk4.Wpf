using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 13495,
		"[Bug] Regression in XF5: SwipeView glitches when pulled out",
		PlatformAffected.iOS)]
	public partial class Issue13495 : TestContentPage
	{
		public Issue13495()
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