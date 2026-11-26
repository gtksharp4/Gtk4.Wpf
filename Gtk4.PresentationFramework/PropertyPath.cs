using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>
/// A path to a property, as used by <c>Storyboard.TargetProperty</c>. Mirrors WPF's
/// <c>PropertyPath</c>: steps such as <c>(Owner.Prop)</c>, dotted chains and <c>[0]</c> indexers,
/// with <c>(0)</c> / <c>{0}</c> placeholders standing for <see cref="PathParameters"/>.
/// </summary>
[System.ComponentModel.TypeConverter(typeof(PropertyPathConverter))]
public sealed partial class PropertyPath
{
	/// <summary>Creates a path from a string and the objects its placeholders refer to.</summary>
	public PropertyPath(string path, params object[] pathParameters)
	{
		Path = path ?? throw new ArgumentNullException(nameof(path));
		PathParameters = new Collection<object>();
		if (pathParameters != null)
			foreach (var p in pathParameters) PathParameters.Add(p);
	}

	/// <summary>Creates a path made of one parameter, usually a <see cref="DependencyProperty"/>.</summary>
	public PropertyPath(object parameter)
	{
		if (parameter == null) throw new ArgumentNullException(nameof(parameter));
		Path = "(0)";
		PathParameters = new Collection<object> { parameter };
	}

	/// <summary>The path string.</summary>
	public string Path { get; }

	/// <summary>The objects the placeholders in <see cref="Path"/> refer to.</summary>
	public Collection<object> PathParameters { get; }
}
