using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 49a wrap-up: local-precedence resource references, reparenting and dictionary swaps.</summary>
public class Round49aWrapResourceTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	sealed class El : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("V49w", typeof(object), typeof(El), new PropertyMetadata("default"));

		public object? Value => GetValue(ValueProperty);

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");

		protected override Size MeasureOverride(Size availableSize) => new Size(10, 10);
	}

	static SolidColorBrush Brush(Color c) => new SolidColorBrush(c);

	[Fact]
	public void Local_resource_reference_beats_a_style_trigger_on_the_same_property() => Run(() =>
	{
		var b = Brush(Colors.Blue);
		var button = new Button();
		button.Resources["B"] = b;
		var style = new Style(typeof(Button));
		var trigger = new Trigger { Property = Button.IsDefaultProperty, Value = true };
		trigger.Setters.Add(new Setter(Control.BackgroundProperty, Brush(Colors.Red)));
		style.Triggers.Add(trigger);
		button.Style = style;
		XamlValueConverter.SetLocalResourceReference(button, Control.BackgroundProperty, "B");

		button.IsDefault = true;

		Assert.Same(b, button.Background);
		Assert.Same(b, button.ReadLocalValue(Control.BackgroundProperty));
	});

	[Fact]
	public void Local_resource_reference_stays_live_and_an_explicit_SetValue_detaches_it() => Run(() =>
	{
		var button = new Button();
		button.Resources["B"] = Brush(Colors.Blue);
		XamlValueConverter.SetLocalResourceReference(button, Control.BackgroundProperty, "B");
		var yellow = Brush(Colors.Yellow);
		button.Resources["B"] = yellow;
		Assert.Same(yellow, button.Background);

		var green = Brush(Colors.Green);
		button.Background = green;
		button.Resources["B"] = Brush(Colors.Black);

		Assert.Same(green, button.Background);
	});

	[Fact]
	public void ClearValue_detaches_a_local_resource_reference() => Run(() =>
	{
		var button = new Button();
		button.Resources["B"] = Brush(Colors.Blue);
		XamlValueConverter.SetLocalResourceReference(button, Control.BackgroundProperty, "B");

		button.ClearValue(Control.BackgroundProperty);
		button.Resources["B"] = Brush(Colors.Black);

		Assert.Equal(DependencyProperty.UnsetValue, button.ReadLocalValue(Control.BackgroundProperty));
	});

	[Fact]
	public void Resource_reference_reverts_when_the_element_leaves_its_parent_and_follows_a_new_one()
	{
		var p = new El();
		p.Resources["k"] = "p";
		var q = new El();
		q.Resources["k"] = "q";
		var child = new El();
		child.SetResourceReference(El.ValueProperty, "k");
		Assert.Equal("default", child.Value);

		child.LogicalParent = p;
		Assert.Equal("p", child.Value);

		child.LogicalParent = null;
		Assert.Equal("default", child.Value);
		p.Resources["k"] = "p2";
		Assert.Equal("default", child.Value);

		child.LogicalParent = q;
		Assert.Equal("q", child.Value);
		p.Resources["k"] = "p3";
		Assert.Equal("q", child.Value);
	}

	[Fact]
	public void Resource_reference_tracks_application_resources_swapped_A_B_A_B() => Run(() =>
	{
		var app = new RecordingApp();
		var a = new ResourceDictionary { ["K"] = Brush(Colors.Red) };
		var b = new ResourceDictionary { ["K"] = Brush(Colors.Blue) };
		app.Resources = a;
		var border = new Border();
		border.SetResourceReference(Border.BackgroundProperty, "K");

		app.Resources = b;
		Assert.Equal(Colors.Blue, ((SolidColorBrush)border.Background!).Color);
		app.Resources = a;
		Assert.Equal(Colors.Red, ((SolidColorBrush)border.Background!).Color);
		app.Resources = b;
		Assert.Equal(Colors.Blue, ((SolidColorBrush)border.Background!).Color);

		b["K"] = Brush(Colors.Yellow);
		Assert.Equal(Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00), ((SolidColorBrush)border.Background!).Color);
	});
}
