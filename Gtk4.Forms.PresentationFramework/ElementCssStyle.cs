using System.Threading;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// Per-widget <c>Gtk.CssProvider</c> for appearance DPs (same mechanism as <c>UIElement.Effect</c>).
/// GTK 4 has no per-widget background/foreground/font setters, so the values become CSS on a unique
/// widget name. The provider is only created once there is something to say, which keeps untouched
/// controls on the pure theme.
/// <para>
/// Only values set locally, by style or by trigger are emitted; inherited and default values are left
/// to GTK CSS inheritance of <c>color</c>/<c>font-*</c>, mirroring WPF's TextElement inheritance.
/// </para>
/// </summary>
internal sealed class ElementCssStyle
{
	const uint Priority = 600;
	static int _counter;

	readonly Gtk.Widget _widget;
	Gtk.CssProvider? _provider;

	internal ElementCssStyle(Gtk.Widget widget) { _widget = widget; }

	/// <summary>The declarations last loaded (empty until a provider exists).</summary>
	internal string Declarations { get; private set; } = string.Empty;

	/// <summary>True once a provider has been attached to the widget.</summary>
	internal bool HasProvider => _provider != null;

	internal void Update(string declarations)
	{
		if (_provider == null && declarations.Length == 0) return;
		if (string.IsNullOrEmpty(_widget.Name))
			_widget.Name = $"ap-{Interlocked.Increment(ref _counter):x8}";
		if (_provider == null)
		{
			_provider = new Gtk.CssProvider();
			_widget.StyleContext.AddProvider(_provider, Priority);
		}
		Declarations = declarations;
		_provider.LoadFromData(declarations.Length == 0 ? string.Empty : $"#{_widget.Name} {{ {declarations} }}");
	}

	// ── Declaration builders shared by Control / TextBlock / Panel / Border ───────

	static object? Explicit(DependencyObject d, DependencyProperty dp) =>
		d.HasNonInheritedValue(dp) ? d.GetValue(dp) : null;

	internal static string Join(params string[] parts) =>
		string.Join(" ", System.Linq.Enumerable.Where(parts, p => p.Length > 0));

	internal static string Background(DependencyObject d) =>
		BrushCss.Background(Explicit(d, Controls.Panel.BackgroundProperty) as Brush);

	internal static string Text(DependencyObject d) => Join(
		BrushCss.Foreground(Explicit(d, TextElement.ForegroundProperty) as Brush),
		FontCss.Build(
			Explicit(d, TextElement.FontFamilyProperty) as FontFamily,
			(double?)Explicit(d, TextElement.FontSizeProperty),
			(FontWeight?)Explicit(d, TextElement.FontWeightProperty),
			(FontStyle?)Explicit(d, TextElement.FontStyleProperty),
			(FontStretch?)Explicit(d, TextElement.FontStretchProperty)));

	/// <summary>Border brush + thickness (shared by Control and Border via AddOwner'd DPs).</summary>
	internal static string Border(DependencyObject d)
	{
		var brush = Explicit(d, Controls.Border.BorderBrushProperty) as Brush;
		bool thickness = d.HasNonInheritedValue(Controls.Border.BorderThicknessProperty);
		if (brush == null && !thickness) return string.Empty;
		string width = BrushCss.BorderWidth((Thickness)d.GetValue(Controls.Border.BorderThicknessProperty)!);
		string color = BrushCss.BorderColor(brush);
		return Join(width, color.Length > 0 ? "border-style: solid; " + color : string.Empty);
	}
}
