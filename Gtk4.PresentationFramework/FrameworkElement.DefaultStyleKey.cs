namespace System.Windows;

public abstract partial class FrameworkElement
{
	// ── DefaultStyleKey ──────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="DefaultStyleKey"/> dependency property.
	/// Controls set this in their static constructors to <c>typeof(ThatControl)</c>
	/// so the theme dictionary can supply their default <see cref="Style"/>.
	/// </summary>
	public static readonly DependencyProperty DefaultStyleKeyProperty =
		DependencyProperty.Register(
			nameof(DefaultStyleKey),
			typeof(Type),
			typeof(FrameworkElement),
			new PropertyMetadata(null));

	/// <summary>
	/// The key used to look up the default style in the theme dictionary.
	/// Set by each control's static constructor to its own <see cref="Type"/>.
	/// A subclass that wants to inherit the base control's default style simply
	/// does not override this — WPF's standard pattern.
	/// </summary>
	protected Type? DefaultStyleKey
	{
		get => (Type?)GetValue(DefaultStyleKeyProperty);
		set => SetValue(DefaultStyleKeyProperty, value);
	}

	// ── OverridesDefaultStyle ───────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c> the element bypasses the theme-dictionary lookup and
	/// uses only explicit/implicit styles.  Mirrors WPF's
	/// <c>FrameworkElement.OverridesDefaultStyle</c>.
	/// </summary>
	public bool OverridesDefaultStyle { get; set; }

	// ── Default-style application ────────────────────────────────────────────

	/// <summary>
	/// Applies the default (theme-dictionary) style when no explicit or implicit
	/// style is present.  Called from <see cref="ApplyImplicitStyle"/> after the
	/// implicit-style pass and from <see cref="Themes.ThemeManager"/> helpers.
	/// Order: explicit Style > implicit TargetType style > theme/default style.
	/// <para>
	/// When <see cref="DefaultStyleKey"/> is <c>null</c>, <see cref="GetType"/> is
	/// used as the lookup key so controls that do not set a key still benefit from
	/// theme registration.  A subclass that did not register its own default style
	/// automatically inherits the base's via <see cref="Themes.ThemeManager.FindDefaultStyle"/>
	/// (which walks the type hierarchy).
	/// </para>
	/// </summary>
	internal void ApplyDefaultStyle()
	{
		if (OverridesDefaultStyle) return;

		// Already has an explicit local Style — do not touch.
		if (!ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
			return;

		// Already got a style from the implicit pass.
		if (Style != null) return;

		// Use the registered DefaultStyleKey; fall back to this element's own type.
		var key = DefaultStyleKey ?? GetType();

		var defaultStyle = Themes.ThemeManager.FindDefaultStyle(key);
		if (defaultStyle != null)
		{
			// Apply directly into the style layer (not via Style property assignment
			// so we don't trigger an un-apply/re-apply cycle on later explicit set).
			defaultStyle.Apply(this);
			defaultStyle.AttachTriggers(this);
		}
	}
}