using System.Windows.Media.Effects;

namespace System.Windows;

/// <summary>
/// <see cref="Effect"/> dependency property on <see cref="UIElement"/>.
/// <para>
/// <b>GTK application:</b> when the backing <see cref="Gtk.Widget"/> is realised, the
/// effect is applied via a per-widget <c>Gtk.CssProvider</c> using a unique CSS name so
/// only this widget is affected. CSS <c>filter</c> is honoured from GTK 4.14+; on earlier
/// GTK versions the CSS is silently ignored. The object model (DP value) is always correct
/// regardless of GTK availability.
/// </para>
/// </summary>
public abstract partial class UIElement
{
	// ── Effect ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="Effect"/> dependency property.
	/// Default: <c>null</c> (no effect).
	/// </summary>
	public static readonly DependencyProperty EffectProperty =
		DependencyProperty.Register(
			nameof(Effect),
			typeof(Effect),
			typeof(UIElement),
			new PropertyMetadata(null, OnEffectChanged));

	static void OnEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyEffect(el._widget!);
	}

	/// <summary>
	/// Gets or sets the bitmap effect applied to this element.
	/// Setting to <c>null</c> removes any active effect.
	/// </summary>
	public Effect? Effect
	{
		get => GetValue(EffectProperty) as Effect;
		set => SetValue(EffectProperty, value);
	}

	// ── GTK application ──────────────────────────────────────────────────────

	/// CSS priority used for effect providers (just above application level).
	const uint _effectCssPriority = 601;

	// Process-wide monotonic source of unique effect CSS widget names.
	static int _effectNameCounter;

	/// Per-widget CssProvider for effects; null until first effect is applied.
	Gtk.CssProvider? _effectCssProvider;

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any effect that was
	/// set before widget creation.
	/// </summary>
	partial void ApplyEffectOnCreated(Gtk.Widget widget) =>
		ApplyEffect(widget);

	/// <summary>
	/// Pushes the current <see cref="Effect"/> value into the widget's style context.
	/// </summary>
	void ApplyEffect(Gtk.Widget widget)
	{
		Effect? effect = GetValue(EffectProperty) as Effect;

		// Assign a process-unique widget name so the CSS selector targets only this widget.
		// A monotonic counter (not GetHashCode) avoids hash-collision cascade onto same-named descendants.
		if (ElementCssStyle.IsUnnamed(widget.Name))
			widget.Name = $"ef-{System.Threading.Interlocked.Increment(ref _effectNameCounter):x8}";

		string selector = $"#{widget.Name}";
		string css = EffectCss.BuildFilterSnippet(selector, effect);

		if (_effectCssProvider == null)
		{
			_effectCssProvider = new Gtk.CssProvider();
			widget.StyleContext.AddProvider(_effectCssProvider, _effectCssPriority);
		}

		_effectCssProvider.LoadFromData(css);
	}
}
