using System;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1914, "Android rotation ignores anchor", PlatformAffected.Android)]
	public class Issue1914 : ContentPage
	{
		public Issue1914()
		{
			Content = new Rotator();
		}

		class Rotator : AbsoluteLayout
		{
			public Rotator()
			{
				var image = new Image { Aspect = Aspect.AspectFit, Source = "bank.png" };
				Children.Add(image, new Rectangle(.5, .5, .5, .5), AbsoluteLayoutFlags.All);
				VerticalOptions = HorizontalOptions = LayoutOptions.Center;
				image.RotateTo(3600, 10000);
			}
		}
	}
}

