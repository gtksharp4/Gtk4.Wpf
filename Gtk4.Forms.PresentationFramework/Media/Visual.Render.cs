namespace System.Windows.Media;

public abstract partial class Visual
{
	/// <summary>Asks the hosting element (nearest UIElement ancestor) to repaint its render layer.</summary>
	internal void InvalidateRender()
	{
		for (Visual? v = this; v != null; v = v.VisualParent)
		{
			if (v is UIElement host)
			{
				host.InvalidateRenderLayer();
				return;
			}
		}
	}
}
