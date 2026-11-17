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
	[Issue(IssueTracker.None, 4539134, "CollectionView: Single Selection Binding", PlatformAffected.All)]
	public class CollectionViewBoundSingleSelection : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.SingleBoundSelection());
#endif
		}

#if UITEST
		[Test]
		public void SelectionShouldUpdateBinding()
		{
			// Initially Item 2 should be selected (from the view model)
			RunningApp.WaitForElement("Selected: Item: 2");

			// Tapping Item 3 should select it and updating the binding
			RunningApp.Tap("Item 1");	
			RunningApp.WaitForElement("Selected: Item: 1");
		}
#endif
	}
}
