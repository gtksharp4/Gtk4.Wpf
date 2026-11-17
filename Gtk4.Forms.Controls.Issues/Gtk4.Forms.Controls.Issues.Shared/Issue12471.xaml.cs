using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;


#if UITEST
using Gtk4.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12471, "[Bug] Label.Clip won't clip on Android", PlatformAffected.Android)]
	public partial class Issue12471 : TestContentPage
	{
		public Issue12471()
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