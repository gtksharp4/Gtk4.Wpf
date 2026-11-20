using System.Windows.Markup;

namespace System.Windows.Controls;

public abstract partial class Panel : IAddChild
{
	void IAddChild.AddChild(object value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (IsItemsHost)
			throw new InvalidOperationException(
				"Cannot add children to a Panel that hosts the items of an ItemsControl.");
		if (value is not UIElement child)
			throw new ArgumentException($"Cannot add '{value.GetType().FullName}' to a Panel.", nameof(value));
		Children.Add(child);
	}

	void IAddChild.AddText(string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return;
		throw new ArgumentException("A Panel does not accept text.", nameof(text));
	}
}
