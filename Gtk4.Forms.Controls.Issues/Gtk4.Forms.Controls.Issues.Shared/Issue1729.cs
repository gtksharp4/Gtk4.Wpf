using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	[Issue(IssueTracker.Github, 1729, "iOS PlatformSpecific for UISlider", PlatformAffected.iOS)]
	public class Issue1729 : TestContentPage
	{
		Slider _slider1;

		protected override void Init()
		{

			_slider1 = new Slider();

			var label = new Label { Text = "Enable TapOnUpdate" };
			var toggle = new Switch { IsToggled = false };
			toggle.Toggled += Toggled;

			var stackLayout = new StackLayout
			{
				Children = { label, toggle, _slider1 },
				Padding = new Thickness(0, 20, 0, 0)
			};
			Content = stackLayout;
		}

		void Toggled(object sender, ToggledEventArgs e)
		{
			_slider1.On<PlatformConfiguration.iOS>().SetUpdateOnTap(((Switch)sender).IsToggled);
		}
	}
}
