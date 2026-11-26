using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

/// <summary>AdornerLayer.Update re-lays out the adorners of one element, or of the whole layer.</summary>
public class AdornerLayerUpdateTests
{
	sealed class CountingAdorner : Adorner
	{
		public int Measures;
		public CountingAdorner(UIElement el) : base(el) { }
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException();
		protected override Size MeasureOverride(Size constraint)
		{
			Measures++;
			return base.MeasureOverride(constraint);
		}
	}

	static CountingAdorner Settled(AdornerLayer layer, UIElement el)
	{
		var a = new CountingAdorner(el);
		layer.Add(a);
		a.Measure(new Size(100, 100));
		a.Arrange(new Rect(0, 0, 100, 100));
		return a;
	}

	[Fact]
	public void Update_of_an_element_re_measures_only_its_adorners()
	{
		var layer = new AdornerLayer();
		var a = Settled(layer, new TextBlock());
		var b = Settled(layer, new TextBlock());
		layer.Update(a.AdornedElement);
		a.UpdateLayout();
		b.UpdateLayout();
		Assert.Equal(2, a.Measures);
		Assert.Equal(1, b.Measures);
	}

	[Fact]
	public void Update_of_the_layer_re_measures_every_adorner()
	{
		var layer = new AdornerLayer();
		var a = Settled(layer, new TextBlock());
		var b = Settled(layer, new TextBlock());
		layer.Update();
		a.UpdateLayout();
		b.UpdateLayout();
		Assert.Equal(2, a.Measures);
		Assert.Equal(2, b.Measures);
	}

	[Fact]
	public void Update_ignores_an_element_without_adorners_and_rejects_null()
	{
		var layer = new AdornerLayer();
		layer.Update(new TextBlock());
		Assert.Throws<ArgumentNullException>(() => layer.Update(null!));
	}
}
