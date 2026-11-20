using System.Windows.Markup;

namespace System.Windows.Controls;

public abstract partial class Panel : IAddChild
{
	void IAddChild.AddChild(object value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (value is not UIElement child)
			throw new ArgumentException($"Cannot add '{value.GetType().FullName}' to a Panel.", nameof(value));
		Children.Add(child);
	}

	void IAddChild.AddText(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		if (!string.IsNullOrWhiteSpace(text))
			throw new ArgumentException("A Panel does not accept text.", nameof(text));
	}
}
