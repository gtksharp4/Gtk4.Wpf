namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>The template for <paramref name="item"/>: the selector choice, else ItemTemplate.</summary>
	internal DataTemplate? ResolveItemTemplate(object? item) =>
		ItemTemplateSelector?.SelectTemplate(item, this) ?? ItemTemplate;

	/// <summary>The text <see cref="DisplayMemberPath"/> selects from <paramref name="item"/>, or null.</summary>
	internal string? ResolveDisplayText(object? item)
	{
		var path = DisplayMemberPath;
		if (string.IsNullOrEmpty(path) || item == null) return null;
		var prop = item.GetType().GetProperty(path!);
		return prop != null ? prop.GetValue(item)?.ToString() ?? string.Empty : string.Empty;
	}
}
