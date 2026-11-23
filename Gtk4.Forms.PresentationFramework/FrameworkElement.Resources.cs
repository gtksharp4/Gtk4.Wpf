using System;

namespace System.Windows;

public partial class FrameworkElement
{
	// ── Style DP ────────────────────────────────────────────────────────────

	/// <summary>Identifies the <see cref="Style"/> dependency property.</summary>
	public static readonly DependencyProperty StyleProperty =
		DependencyProperty.Register(
			nameof(Style),
			typeof(Style),
			typeof(FrameworkElement),
			new PropertyMetadata(null, OnStyleChanged));

	static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement fe)
		{
			// D6+D8: un-apply old style setters + detach its triggers before applying new one.
			if (e.OldValue is Style oldStyle)
			{
				oldStyle.DetachTriggers(fe);
				oldStyle.Unapply(fe);
				var snapshot = fe.SnapshotStyleValues();
				fe.ClearStyleValues(snapshot);
			}
			if (e.NewValue is Style newStyle)
			{
				newStyle.Apply(fe);
				newStyle.AttachTriggers(fe);
			}
		}
	}

	/// <summary>Gets or sets the style applied to this element.</summary>
	public Style? Style
	{
		get => (Style?)GetValue(StyleProperty);
		set => SetValue(StyleProperty, value);
	}

	// ── Resource lookup ─────────────────────────────────────────────────────

	/// <summary>
	/// Walks up the logical parent chain checking each element's
	/// <see cref="FrameworkElement.Resources"/>, then
	/// <see cref="Application.Current"/>?.Resources.
	/// Returns the resource or throws <see cref="InvalidOperationException"/>.
	/// </summary>
	public object? FindResource(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		// Walk the full chain; use _TryFindResourceCore to distinguish "found null" from "absent".
		if (TryFindResourceCore(key, out var v))
			return v;
		throw new InvalidOperationException(
			$"Resource with key '{key}' not found.");
	}

	/// <summary>
	/// Same walk as <see cref="FindResource"/>; returns <c>true</c> even when the
	/// stored value is <c>null</c> (D2: found-null ≠ absent).
	/// </summary>
	bool TryFindResourceCore(object key, out object? value)
	{
		DependencyObject? current = this;
		int steps = 0;
		while (current != null && steps < 1000)
		{
			steps++;
			if (current is FrameworkElement fe && fe.Resources.TryGetValue(key, out value))
				return true;
			current = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
		}
		if (Application.Current?.Resources.TryGetValue(key, out value) == true)
			return true;
		return SystemResources.TryFind(key, out value);
	}

	/// <summary>
	/// Same walk as <see cref="FindResource"/>, returns <c>null</c> when absent or when the
	/// stored value is <c>null</c>.  Use <see cref="FindResource"/> to distinguish the two cases.
	/// </summary>
	public object? TryFindResource(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		TryFindResourceCore(key, out var v);
		return v;
	}

	// ── Implicit style application ───────────────────────────────────────────

	/// <summary>
	/// Looks up an implicit style keyed by <c>GetType()</c> in the resource tree
	/// and applies it when no explicit <see cref="Style"/> is set.  When no
	/// implicit style is found, falls back to the theme-dictionary default style
	/// via <see cref="ApplyDefaultStyle"/>.
	/// Call from <c>InitializeComponent</c> or after the element is added to the tree.
	/// Order: explicit Style > implicit TargetType style > theme/default style.
	/// </summary>
	internal void ApplyImplicitStyle()
	{
		// D7: check for an EXPLICIT (local) Style, not just any effective value != null.
		if (!ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
			return;
		if (TryFindResourceCore(GetType(), out var res) && res is Style s)
		{
			Style = s;
			return;
		}
		// No implicit style — fall back to theme default style (lowest precedence).
		ApplyDefaultStyle();
	}

	ResourceDictionary? _resources;

	ResourceDictionary NewResources() => new ResourceDictionary { Owner = this };
}
