namespace System.Windows.Data
{
	/// <summary>
	/// Abstract base for binding expression objects (<see cref="BindingExpression"/>,
	/// MultiBindingExpression, …). Phase-2 will provide a live implementation;
	/// the type is declared here so <see cref="FrameworkElement.SetBinding"/> can
	/// return it without a compiler error.
	/// </summary>
	public abstract class BindingExpressionBase
	{
	}
}
