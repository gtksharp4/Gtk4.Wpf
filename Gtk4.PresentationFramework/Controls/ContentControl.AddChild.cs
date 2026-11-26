using System.Windows.Markup;

namespace System.Windows.Controls;

public partial class ContentControl : IAddChild
{
	static readonly DependencyPropertyKey _hasContentKey = DependencyProperty.RegisterReadOnly(
		nameof(HasContent), typeof(bool), typeof(ContentControl), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="HasContent"/> dependency property.</summary>
	public static readonly DependencyProperty HasContentProperty = _hasContentKey.DependencyProperty;

	/// <summary>Gets whether <see cref="Content"/> is non-null.</summary>
	public bool HasContent
	{
		get => (bool)GetValue(HasContentProperty)!;
	}

	void IAddChild.AddChild(object value) => AddChild(value);

	void IAddChild.AddText(string text) => AddText(text);

	/// <summary>Sets <paramref name="value"/> as the content; a second non-null child is rejected.</summary>
	/// <exception cref="InvalidOperationException">The control already has content.</exception>
	protected virtual void AddChild(object value)
	{
		if (Content == null || value == null)
			Content = value;
		else
			throw new InvalidOperationException("Content of a ContentControl must be a single element.");
	}

	/// <summary>Adds text as the content.</summary>
	protected virtual void AddText(string text) => AddChild(text);
}
