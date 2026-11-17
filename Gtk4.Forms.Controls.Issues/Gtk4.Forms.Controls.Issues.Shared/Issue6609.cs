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
	[Issue(IssueTracker.Github, 6609, "[Bug, CollectionView] SelectionChangedCommand invoked before SelectedItem is set",
		PlatformAffected.All)]
	public class Issue6609 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.SelectionChangedCommandParameter());
#endif
		}

#if UITEST
		[Test]
		public void SelectionChangedCommandParameterBoundToSelectedItemShouldMatchSelectedItem()
		{
			RunningApp.WaitForElement("Item 2");
			RunningApp.Tap("Item 2");

			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
