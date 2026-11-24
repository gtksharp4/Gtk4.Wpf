using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Appearance;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.ContentModel;

/// <summary>The DataTrigger sample's row colours and template text colour reach the screen.</summary>
public class ListBoxTriggerPixelTests : ContentModelTestBase
{
	[Fact]
	public void Triggered_rows_paint_the_cyan_background_and_the_red_text() => Run(() =>
	{
		var style = new Style(typeof(ListBoxItem));
		var wa = new DataTrigger { Binding = new System.Windows.Data.Binding("State"), Value = "WA" };
		wa.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Red });
		style.Triggers.Add(wa);
		var multi = new MultiDataTrigger();
		multi.Conditions.Add(new Condition { Binding = new System.Windows.Data.Binding("Name"), Value = "Portland" });
		multi.Conditions.Add(new Condition { Binding = new System.Windows.Data.Binding("State"), Value = "OR" });
		multi.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = Brushes.Cyan });
		style.Triggers.Add(multi);

		var window = new Window { Width = 300, Height = 200 };
		window.Resources[typeof(ListBoxItem)] = style;
		window.Resources[new DataTemplateKey(typeof(Place))] = new DataTemplate
		{
			ItemFactory = item => new TextBlock { Text = "WWWWWWWW " + ((Place)item!).Name, FontSize = 20 },
		};
		var lb = new ListBox
		{
			ItemsSource = new[]
			{
				new Place { Name = "Seattle", State = "WA" },
				new Place { Name = "Portland", State = "OR" },
			},
		};
		window.Content = lb;
		window.Show();
		var seattle = (ListBoxItem)lb.ContainerAt(0)!;
		var portland = (ListBoxItem)lb.ContainerAt(1)!;
		GtkPump.Until(() => portland.Widget.IsMapped && portland.Widget.Width > 0);
		GtkPump.For(300);

		var cyanRow = WidgetPixels.Capture(portland.Widget);
		var (r, g, b) = cyanRow.At(cyanRow.Width - 4, cyanRow.Height / 2);
		Assert.True(r < 40 && g > 215 && b > 215, (r, g, b).ToString());

		var redRow = WidgetPixels.Capture(seattle.Widget);
		bool anyRed = false;
		for (int y = 0; y < redRow.Height && !anyRed; y++)
			for (int x = 0; x < redRow.Width && !anyRed; x++)
			{
				var p = redRow.At(x, y);
				anyRed = p.R > 200 && p.G < 60 && p.B < 60;
			}
		Assert.True(anyRed);
		window.Close();
	});
}
