using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8148, "WPF Entry initial TextColor ignored when typing", PlatformAffected.WPF)]
	public class Issue8148 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label() { Text = "Start typing - text should be red immediately as you typing" },
					new Entry { TextColor = Color.Red },
				}
			};
		}
	}
}