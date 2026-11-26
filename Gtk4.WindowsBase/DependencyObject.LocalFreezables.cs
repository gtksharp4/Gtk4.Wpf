namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>Whether any local value is a freezable that satisfies <paramref name="predicate"/>.</summary>
	internal bool HasLocalFreezable(Func<Freezable, bool> predicate)
	{
		foreach (var value in _localValues.Values)
			if (value is Freezable freezable && predicate(freezable))
				return true;
		return false;
	}
}
