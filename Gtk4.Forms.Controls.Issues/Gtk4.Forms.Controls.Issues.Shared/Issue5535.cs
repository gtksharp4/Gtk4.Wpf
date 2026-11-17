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
	[Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5535, "CollectionView: Swapping EmptyViews has no effect",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue5535 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.EmptyViewGalleries.EmptyViewSwapGallery());
#endif
		}

#if UITEST
		[Test]
		public void SwappingEmptyViews()
		{
			RunningApp.WaitForElement("FilterItems");
			RunningApp.Tap("FilterItems");	
			RunningApp.EnterText("abcdef");
		
			// Default empty view
			RunningApp.WaitForElement("Nothing to see here.");

			RunningApp.Tap("ToggleEmptyView");	

			// Other empty view
			RunningApp.WaitForElement("No results matched your filter.");
		}
#endif
	}
}