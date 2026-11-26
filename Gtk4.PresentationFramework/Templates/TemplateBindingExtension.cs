using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Markup extension that creates a one-way <see cref="Binding"/> from a template child
/// to the <see cref="Controls.Control"/> that applied the template
/// (<see cref="RelativeSourceMode.TemplatedParent"/>).
/// <para>
/// Usage (runtime): <c>element.SetBinding(dp, new TemplateBindingExtension("Background"));</c><br/>
/// Usage (XAML, via compiler): <c>Attr="{TemplateBinding Background}"</c> inside a
/// <c>&lt;ControlTemplate&gt;</c> emits a <see cref="Binding"/> with
/// <c>RelativeSource = RelativeSource.TemplatedParent</c> and
/// <c>Mode = OneWay</c>.
/// </para>
/// </summary>
public sealed partial class TemplateBindingExtension
{
	/// <summary>The property path on the templated parent to bind to.</summary>
	public string? Property { get; }

	/// <summary>Creates a template binding for the given property path.</summary>
	public TemplateBindingExtension(string? property)
	{
		Property = property;
	}

	/// <summary>
	/// Converts this extension into a <see cref="Binding"/> that resolves against
	/// <see cref="RelativeSource.TemplatedParent"/> in one-way mode.
	/// </summary>
	public Binding ToBinding()
	{
		var b = new Binding { Mode = BindingMode.OneWay, RelativeSource = RelativeSource.TemplatedParent };
		if (Property != null) b.Path = new PropertyPath(Property, (object[])null!);
		return b;
	}
}