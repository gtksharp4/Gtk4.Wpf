using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11963, "Time and Date Picker is broken in Gtk4.Forms as of iOS 14 Public Beta 6", PlatformAffected.iOS)]
	public class Issue11963 : TestContentPage
	{
		protected override void Init()
		{
			var timePicker = new TimePicker();
			var datePicker = new DatePicker();

			Content = new StackLayout
			{
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children = { timePicker, datePicker }
			};
		}
	}
}