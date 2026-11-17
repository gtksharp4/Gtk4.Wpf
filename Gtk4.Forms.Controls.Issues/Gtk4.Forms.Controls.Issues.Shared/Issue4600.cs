using System.Collections.Generic;
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
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4600, "[iOS] CollectionView crash with empty ObservableCollection", PlatformAffected.iOS)]
	public class Issue4600 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCodeCollectionViewGallery(initialItems: 0));
#endif
		}

#if UITEST
		[Test]
		public void InitiallyEmptySourceDisplaysAddedItem()
		{
			RunningApp.WaitForElement("Insert");
			RunningApp.Tap("Insert");
			RunningApp.WaitForElement("Inserted");
		}
#endif
	}
}