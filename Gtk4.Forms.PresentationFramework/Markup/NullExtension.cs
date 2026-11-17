using System;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{x:Null}</c>.
/// <see cref="ProvideValue"/> always returns <c>null</c>.
/// </summary>
public class NullExtension : MarkupExtension
{
	/// <inheritdoc/>
	public override object? ProvideValue(IServiceProvider? serviceProvider) => null;
}
