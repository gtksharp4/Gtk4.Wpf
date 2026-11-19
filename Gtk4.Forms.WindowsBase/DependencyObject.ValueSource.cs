using System;

namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>
	/// Raised after any layer (local, style, trigger, dynamic resource, animation) of a property is
	/// set or cleared, even when the effective value did not change. Lets PresentationFramework
	/// refresh state that depends on the value source (a static delegate avoids a cross-assembly
	/// internal virtual).
	/// </summary>
	internal static Action<DependencyObject, DependencyProperty>? ValueSourceChanged;

	void NotifyValueSource(DependencyProperty dp) => ValueSourceChanged?.Invoke(this, dp);

	/// <summary>
	/// True when <paramref name="dp"/> has a value from any layer above inheritance and default
	/// (animation, local, trigger, dynamic resource, style).
	/// </summary>
	internal bool HasNonInheritedValue(DependencyProperty dp) =>
		_animationValues.ContainsKey(dp)
		|| _localValues.ContainsKey(dp)
		|| !ReferenceEquals(GetTopTriggerValue(dp), DependencyProperty.UnsetValue)
		|| _dynResValues.ContainsKey(dp)
		|| _styleValues.ContainsKey(dp);

	/// <summary>True when the style layer is the only non-inherited source of <paramref name="dp"/>.</summary>
	internal bool HasOnlyStyleValue(DependencyProperty dp) =>
		_styleValues.ContainsKey(dp)
		&& !_animationValues.ContainsKey(dp)
		&& !_localValues.ContainsKey(dp)
		&& ReferenceEquals(GetTopTriggerValue(dp), DependencyProperty.UnsetValue)
		&& !_dynResValues.ContainsKey(dp);

	/// <summary>
	/// The value <paramref name="dp"/> would inherit: the inheritance parent's effective value, or the
	/// metadata default when there is no parent.
	/// </summary>
	internal object? GetInheritedValue(DependencyProperty dp) =>
		_inheritanceParent != null
			? _inheritanceParent.ResolveEffectiveValue(dp)
			: dp.GetMetadata(GetType()).DefaultValue;
}
