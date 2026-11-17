using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12809, "[Bug] Entry text is invisible on iOS", PlatformAffected.iOS)]
	public partial class Issue12809 : TestContentPage
	{
		protected override void Init()
		{

		}

#if APP
		public Issue12809()
		{
			InitializeComponent();
		}
#endif
	}
}