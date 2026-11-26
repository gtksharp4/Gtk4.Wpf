using System.Windows;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

/// <summary>A same-assembly element the markup compiler cannot inspect: it takes the hooked emission path.</summary>
public class HookedPane : FrameworkElement
{
	public static readonly DependencyProperty AProperty = DependencyProperty.Register(
		"A", typeof(object), typeof(HookedPane), new PropertyMetadata("a0"));

	public static readonly DependencyProperty BProperty = DependencyProperty.Register(
		"B", typeof(object), typeof(HookedPane), new PropertyMetadata("b0"));

	public object? A
	{
		get => GetValue(AProperty);
		set => SetValue(AProperty, value);
	}

	public object? B
	{
		get => GetValue(BProperty);
		set => SetValue(BProperty, value);
	}

	protected override Gtk.Widget CreateWidget() => new Gtk.Label();
}
