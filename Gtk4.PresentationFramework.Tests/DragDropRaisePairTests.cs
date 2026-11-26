using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Exercises the production tunnel-then-bubble raisers in <c>UIElement.GtkDragDrop.cs</c>
/// (<c>RaiseDragEnterPair</c>/<c>RaiseDragOverPair</c>/<c>RaiseDragLeavePair</c>/<c>RaiseDropPair</c>)
/// directly, so the real preview→bubble sequence and Handled-suppression are covered —
/// not an inline re-implementation. No GTK widget or display needed.
/// </summary>
public sealed class DragDropRaisePairTests
{
	sealed class TestElement : UIElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new System.NotSupportedException("No GTK in pure tests.");
	}

	[Fact]
	public void RaiseDropPair_fires_preview_then_bubble()
	{
		var el = new TestElement();
		var order = new List<string>();
		el.PreviewDrop += (s, e) => order.Add("preview");
		el.Drop += (s, e) => order.Add("drop");

		el.RaiseDropPair(new Point(3, 4));

		Assert.Equal(new[] { "preview", "drop" }, order);
	}

	[Fact]
	public void RaiseDropPair_handled_preview_suppresses_drop()
	{
		var el = new TestElement();
		bool dropFired = false;
		el.PreviewDrop += (s, e) => e.Handled = true;
		el.Drop += (s, e) => dropFired = true;

		el.RaiseDropPair(new Point(1, 1));

		Assert.False(dropFired);
	}

	[Fact]
	public void RaiseDragEnterPair_fires_preview_then_bubble()
	{
		var el = new TestElement();
		var order = new List<string>();
		el.PreviewDragEnter += (s, e) => order.Add("preview");
		el.DragEnter += (s, e) => order.Add("enter");

		el.RaiseDragEnterPair(new Point(5, 6));

		Assert.Equal(new[] { "preview", "enter" }, order);
	}

	[Fact]
	public void RaiseDragEnterPair_handled_preview_suppresses_enter()
	{
		var el = new TestElement();
		bool enterFired = false;
		el.PreviewDragEnter += (s, e) => e.Handled = true;
		el.DragEnter += (s, e) => enterFired = true;

		el.RaiseDragEnterPair(new Point(0, 0));

		Assert.False(enterFired);
	}

	[Fact]
	public void RaiseDragOverPair_handled_preview_suppresses_over()
	{
		var el = new TestElement();
		bool overFired = false;
		el.PreviewDragOver += (s, e) => e.Handled = true;
		el.DragOver += (s, e) => overFired = true;

		el.RaiseDragOverPair(new Point(2, 2));

		Assert.False(overFired);
	}

	[Fact]
	public void RaiseDragLeavePair_fires_preview_then_bubble()
	{
		var el = new TestElement();
		var order = new List<string>();
		el.PreviewDragLeave += (s, e) => order.Add("preview");
		el.DragLeave += (s, e) => order.Add("leave");

		el.RaiseDragLeavePair(new Point(9, 9));

		Assert.Equal(new[] { "preview", "leave" }, order);
	}
}
