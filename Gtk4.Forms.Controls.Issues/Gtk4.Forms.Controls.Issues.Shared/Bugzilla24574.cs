using System;
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
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 24574, "Tap Double Tap")]
	public class Issue24574 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				AutomationId = "TapLabel",
				Text = "123",
				FontSize = 50
			};

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
			rec.Tapped += (s, e) => { label.Text = "Single"; };
			label.GestureRecognizers.Add(rec);

			rec = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
			rec.Tapped += (s, e) => { label.Text = "Double"; };
			label.GestureRecognizers.Add(rec);

			Content = label;
		}

#if UITEST
		[Test]
		public void TapThenDoubleTap()
		{
			RunningApp.Screenshot("I am at Issue 24574");

			RunningApp.WaitForElement(q => q.Marked("TapLabel"));

			RunningApp.Tap(q => q.Marked("TapLabel"));
			RunningApp.WaitForElement(q => q.Marked("Single"));

			RunningApp.DoubleTap(q => q.Marked("TapLabel"));
			RunningApp.WaitForElement(q => q.Marked("Double"));
		}
#endif
	}
}
