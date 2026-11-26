using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for DragLeave and Drop routed events (tunnel+bubble pairs, Handled flag).
/// No GTK widget or display needed.
/// </summary>
public sealed class DropRoutedEventTests
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

	// ── DragLeave ─────────────────────────────────────────────────────────────

	[Fact]
	public void DragLeave_bubbles()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool fired = false;
		parent.DragLeave += (s, e) => fired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DragLeaveEvent, new DataObject(),
			DragDropEffects.None, DragDropKeyStates.None, default));

		Assert.True(fired);
	}

	[Fact]
	public void PreviewDragLeave_tunnels_before_DragLeave()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		var order  = new List<string>();

		parent.AddHandler(UIElement.PreviewDragLeaveEvent,
			new DragEventHandler((s, e) => order.Add("pv-parent")));
		child.DragLeave  += (s, e) => order.Add("leave-child");
		parent.DragLeave += (s, e) => order.Add("leave-parent");

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDragLeaveEvent, data,
			DragDropEffects.None, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DragLeaveEvent, data,
				DragDropEffects.None, DragDropKeyStates.None, default));

		Assert.Equal(new[] { "pv-parent", "leave-child", "leave-parent" }, order);
	}

	// ── Drop ──────────────────────────────────────────────────────────────────

	[Fact]
	public void Drop_bubbles()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool fired = false;
		parent.Drop += (s, e) => fired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DropEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.True(fired);
	}

	[Fact]
	public void PreviewDrop_tunnels_before_Drop()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		var order  = new List<string>();

		parent.AddHandler(UIElement.PreviewDropEvent,
			new DragEventHandler((s, e) => order.Add("preview-parent")));
		child.AddHandler(UIElement.PreviewDropEvent,
			new DragEventHandler((s, e) => order.Add("preview-child")));
		child.Drop  += (s, e) => order.Add("drop-child");
		parent.Drop += (s, e) => order.Add("drop-parent");

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDropEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DropEvent, data,
				DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.Equal(new[] { "preview-parent", "preview-child", "drop-child", "drop-parent" }, order);
	}

	[Fact]
	public void Drop_handled_stops_bubble()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool parentFired = false;

		child.Drop  += (s, e) => e.Handled = true;
		parent.Drop += (s, e) => parentFired = true;

		child.RaiseEvent(new DragEventArgs(UIElement.DropEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.False(parentFired);
	}

	[Fact]
	public void PreviewDrop_handled_suppresses_Drop()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool bubbleFired = false;

		parent.AddHandler(UIElement.PreviewDropEvent,
			new DragEventHandler((s, e) => e.Handled = true));
		child.Drop += (s, e) => bubbleFired = true;

		var data = new DataObject();
		var preview = new DragEventArgs(UIElement.PreviewDropEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		child.RaiseEvent(preview);
		if (!preview.Handled)
			child.RaiseEvent(new DragEventArgs(UIElement.DropEvent, data,
				DragDropEffects.Copy, DragDropKeyStates.None, default));

		Assert.False(bubbleFired);
	}
}
