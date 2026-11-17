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
	[Category(UITestCategories.Button)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14192,
		"[Bug] iOS NullReferenceException on ListViewRenderer.UnevenListViewDataSource cause application crash",
		PlatformAffected.iOS)]
	public partial class Issue14192 : TestContentPage
	{
		public Issue14192()
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