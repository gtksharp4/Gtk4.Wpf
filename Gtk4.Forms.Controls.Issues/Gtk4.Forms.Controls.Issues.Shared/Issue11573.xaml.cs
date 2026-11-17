using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11573, "[Bug][Brushes] RadialGradient size on iOS",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Brush)]
#endif
	public partial class Issue11573 : TestContentPage
	{
		public Issue11573()
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