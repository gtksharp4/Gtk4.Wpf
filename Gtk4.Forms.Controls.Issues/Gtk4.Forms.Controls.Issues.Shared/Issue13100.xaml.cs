using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Brush)]
#endif
	[Issue(IssueTracker.Github, 13100,
		"[Bug] LinearGradientBrush GradientStop offset (Android)",
		PlatformAffected.Android)]
	public partial class Issue13100 : TestContentPage
	{
		public Issue13100()
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