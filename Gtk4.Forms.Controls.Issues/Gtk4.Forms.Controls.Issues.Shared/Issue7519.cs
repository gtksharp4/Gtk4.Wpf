using System;
using System.Collections.Generic;
using System.Text;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7519, "ItemSpacing not working on LinearLayout",
		PlatformAffected.All)]
	public class Issue7519 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new Issue7519Xaml());
#endif
		}
	}
}
