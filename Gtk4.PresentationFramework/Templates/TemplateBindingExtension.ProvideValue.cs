using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

public sealed partial class TemplateBindingExtension
{
	/// <summary>
	/// Markup-extension hook: converts this descriptor into a <see cref="Binding"/>
	/// (via <see cref="ToBinding"/>) and returns it.
	/// The build-time compiler emits <see cref="Binding"/> directly and never calls this;
	/// it is provided for programmatic use.
	/// </summary>
	public object? ProvideValue(IServiceProvider? serviceProvider) => ToBinding();
}
