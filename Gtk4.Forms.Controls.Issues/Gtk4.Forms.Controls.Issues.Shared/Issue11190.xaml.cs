using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST && __ANDROID__
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
using System.Linq;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST && __ANDROID__
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11190, "[Bug] Shapes: ArcSegment throwing on iOS, doing nothing on Android", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue11190 : TestContentPage
	{

		public Issue11190()
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