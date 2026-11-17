using System.Collections.Generic;

namespace System.Windows.Themes;

/// <summary>
/// Holds the theme-dictionary of default <see cref="Style"/>s keyed by
/// <see cref="Type"/> (the <c>DefaultStyleKey</c> of a control).
/// <para>
/// This is the lowest-precedence style source — below both an explicit
/// <see cref="FrameworkElement.Style"/> and an implicit TargetType style found
/// in element/application resources.  It is populated by <see cref="Generic"/>
/// at application start and consulted only when a control has no explicit or
/// implicit style.
/// </para>
/// <para>
/// Note: loading from <c>generic.xaml</c> (WPF's standard mechanism) requires
/// compiler support that is planned for a later round.  Default styles are
/// registered in code here.
/// </para>
/// </summary>
public static class ThemeManager
{
	// Keyed by the control's DefaultStyleKey (a Type).
	static readonly Dictionary<Type, Style> _styles = new Dictionary<Type, Style>();

	/// <summary>
	/// Registers a default <see cref="Style"/> for the given <paramref name="controlType"/>.
	/// Replaces any previously registered style for that type.
	/// </summary>
	public static void RegisterDefaultStyle(Type controlType, Style style)
	{
		if (controlType == null) throw new ArgumentNullException(nameof(controlType));
		if (style == null) throw new ArgumentNullException(nameof(style));
		_styles[controlType] = style;
	}

	/// <summary>
	/// Returns the default <see cref="Style"/> for <paramref name="key"/>, walking up the
	/// type hierarchy when no exact match is found — mirrors WPF's
	/// <c>DefaultStyleKey</c> resolution (a subclass that did not set its own
	/// <c>DefaultStyleKey</c> inherits its base's default style).
	/// Returns <c>null</c> when nothing is registered.
	/// </summary>
	public static Style? FindDefaultStyle(Type key)
	{
		if (key == null) return null;
		var t = key;
		while (t != null && t != typeof(object))
		{
			if (_styles.TryGetValue(t, out var s)) return s;
			t = t.BaseType;
		}
		return null;
	}

	/// <summary>
	/// Removes the registration for <paramref name="controlType"/> (test helper).
	/// </summary>
	public static void UnregisterDefaultStyle(Type controlType)
	{
		if (controlType != null) _styles.Remove(controlType);
	}

	/// <summary>Clears all registrations (test helper / reset).</summary>
	public static void Clear() => _styles.Clear();
}