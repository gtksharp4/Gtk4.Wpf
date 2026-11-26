namespace System.Windows;

/// <summary>
/// Exposes the <see cref="DependencyObjectType"/> property on
/// <see cref="DependencyObject"/>.
/// </summary>
public partial class DependencyObject
{
	/// <summary>
	/// Returns the <see cref="DependencyObjectType"/> that represents this instance's
	/// runtime type in the dependency-property type system.
	/// </summary>
	public DependencyObjectType DependencyObjectType =>
		DependencyObjectType.FromSystemType(GetType());
}
