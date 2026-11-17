using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 0,
		"[Bug] Frame Background not working",
		PlatformAffected.Android)]
	public partial class FrameBackgroundIssue : TestContentPage
	{
		public FrameBackgroundIssue()
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