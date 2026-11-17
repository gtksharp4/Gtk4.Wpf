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
	[Issue(IssueTracker.None, 8675312, "CollectionView Header/Footer View", PlatformAffected.All)]
	public class CollectionViewHeaderFooterView : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.HeaderFooterGalleries.HeaderFooterView());
#endif
		}

#if UITEST
		[Test]
		public void CollectionViewHeaderAndFooterUsingViews()
		{
			RunningApp.WaitForElement("This Is A Header");
			RunningApp.WaitForElement("This Is A Footer");
		}
#endif
	}
}
