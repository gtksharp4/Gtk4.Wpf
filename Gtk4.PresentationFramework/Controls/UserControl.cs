using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Hosts content and is the usual base of an <c>x:Class</c> markup root. Unlike other controls it is not
/// focusable and not a tab stop, and its content stretches to fill it.
/// </summary>
[ContentProperty("Content")]
public class UserControl : ContentControl
{
	static UserControl()
	{
		FocusableProperty.OverrideMetadata(typeof(UserControl), new PropertyMetadata(false));
		IsTabStopProperty.OverrideMetadata(typeof(UserControl), new PropertyMetadata(false));
		HorizontalContentAlignmentProperty.OverrideMetadata(
			typeof(UserControl), new PropertyMetadata(HorizontalAlignment.Stretch));
		VerticalContentAlignmentProperty.OverrideMetadata(
			typeof(UserControl), new PropertyMetadata(VerticalAlignment.Stretch));
	}

	/// <summary>Creates a user control.</summary>
	public UserControl()
	{
	}
}
