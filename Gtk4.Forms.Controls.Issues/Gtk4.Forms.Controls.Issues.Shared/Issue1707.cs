using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1707, "[Enhancement] Drop shadow support for iOS", PlatformAffected.iOS)]
	public class Issue1707 : ContentPage
	{
		public Issue1707()
		{
			Title = "Shadow Test Page";
			Padding = 10;

			var layout = new StackLayout();
			var box = new BoxView();
			box.Color = Color.Aqua;

			box.On<iOS>().SetIsShadowEnabled(true);
			box.On<iOS>().SetShadowOffset(new Size(10, 10));
			box.On<iOS>().SetShadowColor(Color.Purple);

			box.WidthRequest = box.HeightRequest = 100;

			layout.Children.Add(box);

			Content = layout;
		}
	}
}
