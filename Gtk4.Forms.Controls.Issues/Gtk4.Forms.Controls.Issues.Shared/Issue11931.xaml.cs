using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Shapes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ViewBaseTests)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(
		IssueTracker.Github, 11931,
		"[Bug] View translation is incorrectly calculated",
		PlatformAffected.All)]
	public partial class Issue11931 : TestContentPage
	{
		public Issue11931()
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
