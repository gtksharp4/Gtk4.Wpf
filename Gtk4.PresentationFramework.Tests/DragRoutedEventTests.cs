using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for DragEnter and DragOver routed events (tunnel+bubble pairs, Handled flag).
/// No GTK widget or display needed.
/// </summary>
public sealed class DragRoutedEventTests
{
	sealed class TestElement : UIElement
	{
		public new UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new System.NotSupportedException("No GTK in pure tests.");
	}

	// ── DragEnter ─────────────────────────────────────────────────────────────

	[Fact]
	public void DragEnter_bubbles_to_parent()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool fired = false;
		parent.DragEnter += (s, e) => fired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DragEnterEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.True(fired);
	}

	[Fact]
	public void PreviewDragEnter_tunnels_before_DragEnter()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		var order  = new List<string>();

		parent.AddHandler(UIElement.PreviewDragEnterEvent,
			new DragEventHandler((s, e) => order.Add("preview-parent")));
		child.AddHandler(UIElement.PreviewDragEnterEvent,
			new DragEventHandler((s, e) => order.Add("preview-child")));
		child.DragEnter  += (s, e) => order.Add("enter-child");
		parent.DragEnter += (s, e) => order.Add("enter-parent");

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDragEnterEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DragEnterEvent, data,
				DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.Equal(new[] { "preview-parent", "preview-child", "enter-child", "enter-parent" }, order);
	}

	[Fact]
	public void DragEnter_handled_stops_bubble()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool parentFired = false;

		child.DragEnter  += (s, e) => e.Handled = true;
		parent.DragEnter += (s, e) => parentFired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DragEnterEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.False(parentFired);
	}

	[Fact]
	public void PreviewDragEnter_handled_suppresses_DragEnter()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool bubbleFired = false;

		parent.AddHandler(UIElement.PreviewDragEnterEvent,
			new DragEventHandler((s, e) => e.Handled = true));
		child.DragEnter += (s, e) => bubbleFired = true;

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDragEnterEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DragEnterEvent, data,
				DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.False(bubbleFired);
	}

	// ── DragOver ──────────────────────────────────────────────────────────────

	[Fact]
	public void DragOver_bubbles()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool fired = false;
		parent.DragOver += (s, e) => fired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DragOverEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.True(fired);
	}

	[Fact]
	public void PreviewDragOver_tunnels_before_DragOver()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		var order  = new List<string>();

		parent.AddHandler(UIElement.PreviewDragOverEvent,
			new DragEventHandler((s, e) => order.Add("pv-parent")));
		child.DragOver  += (s, e) => order.Add("over-child");
		parent.DragOver += (s, e) => order.Add("over-parent");

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDragOverEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DragOverEvent, data,
				DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.Equal(new[] { "pv-parent", "over-child", "over-parent" }, order);
	}
}
