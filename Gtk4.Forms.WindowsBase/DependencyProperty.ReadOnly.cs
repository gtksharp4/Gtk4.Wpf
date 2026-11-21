namespace System.Windows;

public sealed partial class DependencyProperty
{
	/// <summary>Gets whether the property is read-only (registered with <c>RegisterReadOnly</c>).</summary>
	public bool ReadOnly => IsReadOnly;
}
