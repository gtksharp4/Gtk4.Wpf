using System;
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
internal sealed partial class ElementCssStyle
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

	/// <summary>CSS parse errors GTK reported for the loaded declarations (GTK ignores bad CSS silently).</summary>
	internal int ParseErrors { get; private set; }

	internal void Update(string declarations, string rules = "")
	{
		if (_provider != null && declarations == Declarations && rules == Rules) return;
		if (_provider == null && declarations.Length == 0 && rules.Length == 0) return;
		if (IsUnnamed(_widget.Name))
			_widget.Name = $"ap-{Interlocked.Increment(ref _counter):x8}";
		if (_provider == null)
		{
			_provider = new Gtk.CssProvider();
			_provider.ParsingError += (_, __) => ParseErrors++;
			_widget.StyleContext.AddProvider(_provider, Priority);
		}
		Declarations = declarations;
		Rules = rules;
		ParseErrors = 0;
		string main = declarations.Length == 0 ? string.Empty : $"#{_widget.Name} {{ {declarations} }}";
		_provider.LoadFromData(main + ExpandRules(rules, _widget.Name));
	}

	/// <summary>
	/// GTK 4 reports the type name (<c>GtkLabel</c>) for a widget that was never named, so a plain
	/// empty check would keep it and the <c>#name</c> selector would match nothing.
	/// </summary>
	internal static bool IsUnnamed(string? name) =>
		string.IsNullOrEmpty(name) || name!.StartsWith("Gtk", StringComparison.Ordinal);

	// ── Declaration builders shared by Control / TextBlock / Panel / Border ───────

	static object? Explicit(DependencyObject d, DependencyProperty dp) =>
		d.HasNonInheritedValue(dp) ? d.GetValue(dp) : null;

	internal static string Join(params string[] parts) =>
		string.Join(" ", System.Linq.Enumerable.Where(parts, p => p.Length > 0));

	internal static string Background(DependencyObject d) =>
		BrushCss.Background(Explicit(d, Controls.Panel.BackgroundProperty) as Brush);

	internal static string Text(DependencyObject d) => Join(
		BrushCss.Foreground(ExplicitForeground(d)),
		FontCss.Build(
			Explicit(d, TextElement.FontFamilyProperty) as FontFamily,
			Explicit(d, TextElement.FontSizeProperty) is double s ? s : (double?)null,
			Explicit(d, TextElement.FontWeightProperty) is FontWeight w ? w : (FontWeight?)null,
			Explicit(d, TextElement.FontStyleProperty) is FontStyle st ? st : (FontStyle?)null,
			Explicit(d, TextElement.FontStretchProperty) is FontStretch fs ? fs : (FontStretch?)null));

	// Theme text brushes the style layer merely repeats are left to GTK inheritance, so untouched
	// controls stay provider-free and follow dark themes.
	static Brush? ExplicitForeground(DependencyObject d)
	{
		var brush = Explicit(d, TextElement.ForegroundProperty) as Brush;
		if (!(brush is SolidColorBrush solid) || !d.HasOnlyStyleValue(TextElement.ForegroundProperty)) return brush;
		if (!ReferenceEquals(brush, SystemColors.ControlTextBrush)
			&& !ReferenceEquals(brush, SystemColors.WindowTextBrush))
			return brush;
		var inherited = d.GetInheritedValue(TextElement.ForegroundProperty) as Brush;
		bool same = ReferenceEquals(inherited, brush)
			|| (inherited is SolidColorBrush other
				&& other.Color.Equals(solid.Color) && other.Opacity == solid.Opacity);
		return same ? null : brush;
	}

	/// <summary>Border brush + thickness (shared by Control and Border via AddOwner'd DPs).</summary>
	internal static string Border(DependencyObject d)
	{
		var brush = Explicit(d, Controls.Border.BorderBrushProperty) as Brush;
		bool thickness = d.HasNonInheritedValue(Controls.Border.BorderThicknessProperty);
		if (brush == null && !thickness) return string.Empty;
		var thicknessValue = d.GetValue(Controls.Border.BorderThicknessProperty);
		string width = BrushCss.BorderWidth(thicknessValue is Thickness t ? t : default);
		string color = BrushCss.BorderColor(brush);
		// No brush: keep the width but draw nothing (the theme frame rule would otherwise paint).
		return Join(width, "border-style: solid; " + (color.Length > 0 ? color : "border-color: transparent;"));
	}
}
