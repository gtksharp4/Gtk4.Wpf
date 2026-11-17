using System;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// <see cref="RenderTransform"/> and <see cref="RenderTransformOrigin"/> dependency
/// properties on <see cref="UIElement"/>.
/// <para>
/// <b>GTK application:</b> when the backing <see cref="Gtk.Widget"/> is realised, the
/// transform is applied via a per-widget <c>Gtk.CssProvider</c> using a unique CSS name
/// so only this widget is affected. CSS <c>transform</c> is honoured from GTK 4.14+;
/// on earlier GTK versions the CSS is silently ignored. The object model (DP value and
/// <see cref="Transform.Value"/> matrix) is always correct regardless of GTK availability.
/// </para>
/// </summary>
public abstract partial class UIElement
{
	// ── RenderTransform ──────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="RenderTransform"/> dependency property.
	/// Default: <see cref="Transform.Identity"/> (no transform).
	/// </summary>
	public static readonly DependencyProperty RenderTransformProperty =
		DependencyProperty.Register(
			nameof(RenderTransform),
			typeof(Transform),
			typeof(UIElement),
			new PropertyMetadata(Transform.Identity, OnRenderTransformChanged));

	static void OnRenderTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyRenderTransform(el._widget!);
	}

	/// <summary>
	/// Gets or sets the transform applied before rendering.
	/// Setting <c>null</c> resets to the identity transform.
	/// </summary>
	public Transform? RenderTransform
	{
		get => GetValue(RenderTransformProperty) as Transform ?? Transform.Identity;
		set => SetValue(RenderTransformProperty, value ?? Transform.Identity);
	}

	// ── RenderTransformOrigin ────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="RenderTransformOrigin"/> dependency property.
	/// Default: (0, 0) — top-left corner. Values are normalised (0..1 per axis).
	/// </summary>
	public static readonly DependencyProperty RenderTransformOriginProperty =
		DependencyProperty.Register(
			nameof(RenderTransformOrigin),
			typeof(Point),
			typeof(UIElement),
			new PropertyMetadata(new Point(0, 0), OnRenderTransformOriginChanged));

	static void OnRenderTransformOriginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyRenderTransform(el._widget!);
	}

	/// <summary>
	/// Gets or sets the origin point (0..1 per axis) around which the
	/// <see cref="RenderTransform"/> is applied.
	/// </summary>
	public Point RenderTransformOrigin
	{
		get => (Point)(GetValue(RenderTransformOriginProperty) ?? new Point(0, 0));
		set => SetValue(RenderTransformOriginProperty, value);
	}

	// ── GTK application ──────────────────────────────────────────────────────

	/// CSS priority used for render-transform providers (just above application level).
	const uint _cssPriority = 600;

	/// Per-widget CssProvider; null until the first transform is applied.
	Gtk.CssProvider? _transformCssProvider;

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any transform that was
	/// set before widget creation.
	/// </summary>
	partial void ApplyRenderTransformOnCreated(Gtk.Widget widget) =>
		ApplyRenderTransform(widget);

	/// <summary>
	/// Pushes the current <see cref="RenderTransform"/> and
	/// <see cref="RenderTransformOrigin"/> values into the widget's style context.
	/// </summary>
	void ApplyRenderTransform(Gtk.Widget widget)
	{
		Transform? t = GetValue(RenderTransformProperty) as Transform;
		Point origin = (Point)(GetValue(RenderTransformOriginProperty) ?? new Point(0, 0));

		// Assign a unique widget name so the CSS selector targets only this widget.
		if (string.IsNullOrEmpty(widget.Name))
			widget.Name = $"rt-{GetHashCode():x8}";

		string selector = $"#{widget.Name}";
		string css = RenderTransformCss.BuildCssSnippet(selector, t, origin);

		if (_transformCssProvider == null)
		{
			_transformCssProvider = new Gtk.CssProvider();
			widget.StyleContext.AddProvider(_transformCssProvider, _cssPriority);
		}

		_transformCssProvider.LoadFromData(css);
	}
}
