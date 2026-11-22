using System.Windows.Media;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Override to draw the element's content. Painted by the render layer (a drawing area under the element's
	/// children) when the type overrides this method; see <c>UIElement.RenderLayer.cs</c>.
	/// </summary>
	protected virtual void OnRender(DrawingContext drawingContext) { }

	/// <summary>Discards the recorded <see cref="OnRender"/> output so it runs again on the next paint.</summary>
	public void InvalidateVisual() => InvalidateRenderLayer();
}
