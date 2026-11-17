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
	[Issue(IssueTracker.Github, 6620, "[iOS] Crash when creating a CollectionView inside a CollectionView",
		PlatformAffected.iOS | PlatformAffected.UWP)]
	public class NestedCollectionViews : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.NestedGalleries.NestedCollectionViewGallery());
#endif
		}

#if UITEST
		[Test]
		public void NestedCollectionViewsShouldNotCrash()
		{
			// If this page loaded and didn't crash, we're good.
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
