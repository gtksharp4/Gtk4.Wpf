using System.Collections.ObjectModel;
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
	[Issue(IssueTracker.Github, 10124, "[Bug] Shell SearchHandler - SearchHandler blocks touch to view", PlatformAffected.iOS)]
	public class Issue10124 : TestShell
	{
		protected override void Init()
		{
			var cp = CreateContentPage();
			cp.Content = new Label
			{
				Text = "Enter a search query and execute. This content should no longer be obscured by the search controller dimmed background"
			};
			Shell.SetSearchHandler(cp, new SearchHandler());

		}
	}
}