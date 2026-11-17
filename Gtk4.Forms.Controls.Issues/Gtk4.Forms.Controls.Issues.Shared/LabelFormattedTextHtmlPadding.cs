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
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Label with FormattedText, HTML, and Padding shouldn't cause crash", PlatformAffected.iOS)]
	public class LabelFormattedTextHtmlPadding : TestContentPage
	{
		protected override void Init()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "This test passes if app doesn't crash. Label is not expected to actually work" });

			Content = new StackLayout()
			{
				Margin = 20,

				Children =
				{
					new Label()
					{
						AutomationId = "LabelFormattedTextHtmlPaddingTest",
						Text = "If you can see this text, this test has passed"
					},
					new Label()
					{
						AutomationId = "LabelFormattedTextHtmlPadding",
						FormattedText = formattedString,
						TextType = TextType.Html,
						Padding = 5
					}
				}
			};
		}
	}
}