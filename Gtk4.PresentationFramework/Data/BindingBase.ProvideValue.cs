using System.Windows.Markup;

namespace System.Windows.Data;

public abstract partial class BindingBase
{
	/// <summary>
	/// Markup-extension hook: returns <c>this</c> so a <see cref="Binding"/> (or
	/// <see cref="MultiBinding"/>) can be used as a runtime markup-extension object.
	/// The build-time compiler emits bindings imperatively and never calls this method;
	/// it is provided for programmatic / runtime-XAML-loader use.
	/// </summary>
	public object? ProvideValue(IServiceProvider? serviceProvider) => this;
}
