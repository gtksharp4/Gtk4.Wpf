using System.ComponentModel;

namespace System.Windows;

public sealed partial class PropertyPath
{
	/// <summary>
	/// Layer-only bridge for generated code that still assigns a string (<c>binding.Path = "Age"</c>).
	/// WPF has no such operator. Remove it once the markup compiler emits <c>new PropertyPath(...)</c>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static implicit operator PropertyPath?(string? path) =>
		path == null ? null : new PropertyPath(path, (object[])null!);
}
