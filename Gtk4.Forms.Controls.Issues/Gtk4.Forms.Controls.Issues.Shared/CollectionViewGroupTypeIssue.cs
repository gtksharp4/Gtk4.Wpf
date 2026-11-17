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
	[Issue(IssueTracker.None, 5882300, "Crash if group type is not IList",
		PlatformAffected.iOS)]
	public class CollectionViewGroupTypeIssue : TestNavigationPage
	{
		const string Success = "Success";

		protected override void Init()
		{
#if APP
			PushAsync(TestPage());
#endif
		}

		ContentPage TestPage()
		{
			var page = new ContentPage();

			var success = new Label { Text = Success };

			var data = new List<string> { "eins", "zwei", "drei" };

			var cv = new CollectionView
			{
				ItemsSource = data,
				IsGrouped = true
			};

			var layout = new StackLayout { Children = { success, cv } };

			page.Content = layout;

			return page;
		}

#if UITEST
		[Test]
		public void NonIListGroupTypeShouldNotCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
