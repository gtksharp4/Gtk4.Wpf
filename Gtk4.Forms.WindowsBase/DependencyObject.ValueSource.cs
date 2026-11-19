using System;

namespace System.Windows;

public partial class DependencyObject
{
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
}
