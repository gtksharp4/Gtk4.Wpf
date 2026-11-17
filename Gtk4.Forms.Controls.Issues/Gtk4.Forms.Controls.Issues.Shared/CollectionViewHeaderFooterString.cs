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
	[Issue(IssueTracker.None, 8675310, "CollectionView Header/Footer Strings", PlatformAffected.All)]
	public class CollectionViewHeaderFooterString : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.HeaderFooterGalleries.HeaderFooterString());
#endif
		}

#if UITEST
		[Test]
		public void CollectionViewHeaderAndFooterUsingStrings()
		{
			RunningApp.WaitForElement("Just a string as a header");
			RunningApp.WaitForElement("This footer is also a string");
		}
#endif
	}

}
