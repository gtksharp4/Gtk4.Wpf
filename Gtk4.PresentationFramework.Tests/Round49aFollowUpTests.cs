using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 49a follow-up: SetResourceReference vs local references, hooked-type emission.</summary>
public class Round49aFollowUpTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	sealed class Pane : FrameworkElement
	{
		public static readonly DependencyProperty AProperty = DependencyProperty.Register(
			"A49f", typeof(object), typeof(Pane), new PropertyMetadata("a0", OnAChanged));

		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register("B49f", typeof(object), typeof(Pane), new PropertyMetadata("b0"));

		static void OnAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is string s && s.StartsWith("fire")) ((Pane)d).SetValue(BProperty, "explicit");
		}

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");

		protected override Size MeasureOverride(Size availableSize) => new Size(10, 10);
	}

	static SolidColorBrush Brush(Color c) => new SolidColorBrush(c);

	[Fact]
	public void SetResourceReference_replaces_a_xaml_attribute_reference_on_the_same_property() => Run(() =>
	{
		var window = new Window();
		var b = Brush(Colors.Blue);
		window.Resources["A"] = Brush(Colors.Red);
		window.Resources["B"] = b;
		XamlValueConverter.SetLocalResourceReference(window, Control.BackgroundProperty, "A");

		window.SetResourceReference(Control.BackgroundProperty, "B");
		window.Resources["A"] = Brush(Colors.Black);

		Assert.Same(b, window.Background);
	});


	[Fact]
	public void A_callback_setting_another_local_reference_property_detaches_that_reference()
	{
		var pane = new Pane();
		pane.Resources["ka"] = "plain";
		pane.Resources["kb"] = "linked";
		XamlValueConverter.SetLocalResourceReference(pane, Pane.AProperty, "ka");
		XamlValueConverter.SetLocalResourceReference(pane, Pane.BProperty, "kb");
		Assert.Equal("linked", pane.GetValue(Pane.BProperty));

		pane.Resources["ka"] = "fire";
		Assert.Equal("explicit", pane.GetValue(Pane.BProperty));
		pane.Resources["kb"] = "linked2";

		Assert.Equal("explicit", pane.GetValue(Pane.BProperty));
	}
}
