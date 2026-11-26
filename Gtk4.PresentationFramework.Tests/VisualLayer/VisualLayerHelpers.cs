using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

/// <summary>Host that exposes AddVisualChild/RemoveVisualChild and the logical-child API.</summary>
class VisualHost : FrameworkElement
{
	public void AddV(Visual v) => AddVisualChild(v);
	public void RemoveV(Visual v) => RemoveVisualChild(v);
	public void AddL(object o) => AddLogicalChild(o);
	public void RemoveL(object o) => RemoveLogicalChild(o);
	public readonly System.Collections.Generic.List<Visual> Visuals = new();
	protected internal override int VisualChildrenCount => Visuals.Count;
	protected internal override Visual GetVisualChild(int index) => Visuals[index];
}

class RectVisual : DrawingVisual
{
	public RectVisual(Rect rect, Vector offset = default)
	{
		using (var dc = RenderOpen()) dc.DrawRectangle(Brushes.Red, null, rect);
		Offset = offset;
	}
}

class RenderingElement : FrameworkElement
{
	public int Renders;
	protected override void OnRender(DrawingContext dc)
	{
		Renders++;
		dc.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 10, 10));
	}
}

class LogicalPanel : System.Windows.Controls.StackPanel
{
	public readonly System.Collections.Generic.List<object> Own = new();
	protected internal override IEnumerator LogicalChildren => Own.GetEnumerator();
}
