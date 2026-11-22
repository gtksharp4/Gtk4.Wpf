using System.Reflection;

namespace System.Windows.Controls;

public partial class TreeView
{
	/// <summary>Identifies <see cref="SelectedValuePath"/>. Default empty.</summary>
	public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
		nameof(SelectedValuePath), typeof(string), typeof(TreeView), new FrameworkPropertyMetadata(string.Empty));

	/// <summary>Gets or sets the path, on the selected item, of the value <see cref="SelectedValue"/> returns.</summary>
	public string SelectedValuePath
	{
		get => (string?)GetValue(SelectedValuePathProperty) ?? string.Empty;
		set => SetValue(SelectedValuePathProperty, value);
	}

	/// <summary>
	/// The value of <see cref="SelectedValuePath"/> on the selected item (the item itself when the path is empty;
	/// null when nothing is selected or a segment is missing). The path is a dotted list of public properties.
	/// </summary>
	public object? SelectedValue => ValueAt(SelectedItem, SelectedValuePath);

	static object? ValueAt(object? item, string path)
	{
		if (item is TreeViewItem container) item = container.Header;
		if (path.Length == 0) return item;
		foreach (var segment in path.Split('.'))
		{
			if (item == null) return null;
			item = item.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance)?.GetValue(item);
		}
		return item;
	}
}
