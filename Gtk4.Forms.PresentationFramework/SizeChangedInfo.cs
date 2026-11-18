namespace System.Windows;

/// <summary>
/// Reports a render-size change to <see cref="UIElement.OnRenderSizeChanged"/>.
/// </summary>
public class SizeChangedInfo
{
	/// <summary>Creates the info for a size change of <paramref name="element"/>.</summary>
	/// <param name="element">The element whose size changed.</param>
	/// <param name="previousSize">The size before the change.</param>
	/// <param name="widthChanged">Whether the width changed.</param>
	/// <param name="heightChanged">Whether the height changed.</param>
	public SizeChangedInfo(UIElement element, Size previousSize, bool widthChanged, bool heightChanged)
	{
		Element = element;
		PreviousSize = previousSize;
		WidthChanged = widthChanged;
		HeightChanged = heightChanged;
	}

	/// <summary>The element whose size changed.</summary>
	internal UIElement Element { get; }

	/// <summary>The size before the change.</summary>
	public Size PreviousSize { get; }

	/// <summary>The new size — the element's current <see cref="UIElement.RenderSize"/>.</summary>
	public Size NewSize => Element.RenderSize;

	/// <summary>Whether the width changed.</summary>
	public bool WidthChanged { get; }

	/// <summary>Whether the height changed.</summary>
	public bool HeightChanged { get; }
}
