using System.ComponentModel;

namespace System.Windows.Controls;

public partial class ItemsPanelTemplate
{
	/// <summary>
	/// The single panel instance the markup compiler's generic element path assigns
	/// (<c>template.Content = element</c>). Not part of WPF: the compiler has no ItemsPanelTemplate case that
	/// emits <see cref="FrameworkTemplate.Factory"/>, so this keeps generated code compiling. The instance can
	/// back one control only: a second control falls back to the default stack panel. Prefer
	/// <see cref="FrameworkTemplate.Factory"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public UIElement? Content
	{
		get => _content;
		set
		{
			_content = value;
			Factory = value == null ? (Func<UIElement>?)null : () => value;
		}
	}

	UIElement? _content;
}
