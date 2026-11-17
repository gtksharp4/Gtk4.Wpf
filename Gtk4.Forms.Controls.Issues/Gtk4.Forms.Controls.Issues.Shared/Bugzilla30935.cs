using System;

using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 30935, "NullReferenceException in ViewRenderer<TView, TNativeView> (Gtk4.Forms.Platform.Android)")]
	public class Bugzilla30935 : TestContentPage
	{
		Entry _entry;
		protected override void Init()
		{
			_entry = new Entry { AutomationId = "entry" };
			// Initialize ui here instead of ctor
			Content = new StackLayout
			{
				Children = { new Label {
						AutomationId = "IssuePageLabel",
						Text = "See if I'm here"
					},_entry
				}
			};
		}

		protected override void OnAppearing()
		{
			_entry.Focus();
			Content = null;
			base.OnAppearing();
		}

#if UITEST
		[Test]
		public void Bugzilla30935DoesntThrowException()
		{
			RunningApp.WaitForNoElement(q => q.Marked("IssuePageLabel"));
			RunningApp.WaitForNoElement(q => q.Marked("entry"));
		}
#endif
	}
}
