namespace System.Windows.Data;

/// <summary>
/// Abstract base for all binding expressions (<see cref="Binding"/>, <see cref="MultiBinding"/>).
/// Holds properties shared by all binding kinds.
/// </summary>
public abstract partial class BindingBase
{
	/// <summary>
	/// Format string applied after conversion.
	/// A format without <c>{0}</c> is treated as <c>"{StringFormat}{0}"</c> (WPF shortcut).
	/// </summary>
	public string? StringFormat { get; set; }

	/// <summary>
	/// Value used when the binding cannot resolve the source or path.
	/// Defaults to <see cref="DependencyProperty.UnsetValue"/>.
	/// </summary>
	public object? FallbackValue { get; set; } = DependencyProperty.UnsetValue;

	/// <summary>
	/// Value substituted when the resolved (and converted) value is <c>null</c>.
	/// </summary>
	public object? TargetNullValue { get; set; }
}