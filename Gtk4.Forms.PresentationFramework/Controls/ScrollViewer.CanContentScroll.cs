namespace System.Windows.Controls;

public partial class ScrollViewer
{
	/// <summary>Identifies the <c>CanContentScroll</c> property. Default false.</summary>
	public static readonly DependencyProperty CanContentScrollProperty = DependencyProperty.RegisterAttached(
		"CanContentScroll", typeof(bool), typeof(ScrollViewer), new FrameworkPropertyMetadata(false));

	/// <summary>Gets or sets whether the content scrolls by logical unit rather than by pixel. Stored only.</summary>
	public bool CanContentScroll
	{
		get => GetCanContentScroll(this);
		set => SetCanContentScroll(this, value);
	}

	/// <summary>Gets <c>CanContentScroll</c> from <paramref name="element"/>.</summary>
	public static bool GetCanContentScroll(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)element.GetValue(CanContentScrollProperty)!;
	}

	/// <summary>Sets <c>CanContentScroll</c> on <paramref name="element"/>.</summary>
	public static void SetCanContentScroll(DependencyObject element, bool canContentScroll)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(CanContentScrollProperty, canContentScroll);
	}
}
