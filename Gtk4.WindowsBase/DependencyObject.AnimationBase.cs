namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>Resolves the effective value of <paramref name="dp"/> as if it had no animation.</summary>
	internal object? ResolveBaseValue(DependencyProperty dp)
	{
		if (!_animationValues.TryGetValue(dp, out var animated))
			return ResolveEffectiveValue(dp);

		_animationValues.Remove(dp);
		try { return ResolveEffectiveValue(dp); }
		finally { _animationValues[dp] = animated; }
	}
}
