using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// GTK 4 <c>Gtk.DropTarget</c> controller wiring for the drag routed events.
/// Event declarations and <c>AllowDrop</c> DP live in <c>UIElement.DragDrop.cs</c>.
/// </summary>
/// <remarks>
/// <b>Headless degrade:</b> when <c>Gdk.Display.Default</c> is <see langword="null"/>
/// <see cref="ApplyAllowDrop"/> returns immediately without attaching any controller.
/// The routed events are still raisable via <see cref="UIElement.RaiseEvent"/>.
/// </remarks>
public abstract partial class UIElement
{
	Gtk.DropTarget? _gtkDropTarget;

	/// <inheritdoc cref="UIElement.AttachDragDropOnCreated"/>
	internal partial void AttachDragDropOnCreated(Gtk.Widget widget)
	{
		if (AllowDrop)
			ApplyAllowDrop(widget, allow: true);
	}

	/// <inheritdoc cref="UIElement.ApplyAllowDrop"/>
	partial void ApplyAllowDrop(Gtk.Widget widget, bool allow)
	{
		if (!HasGtkDisplay())
			return;

		if (allow)
		{
			if (_gtkDropTarget != null)
				return;

			var allActions = Gdk.DragAction.Copy | Gdk.DragAction.Move | Gdk.DragAction.Link;
			_gtkDropTarget = new Gtk.DropTarget(GLib.GType.Object, allActions);
			_gtkDropTarget.Enter   += (s, e) => RaiseDragEnterPair(new Point(e.X, e.Y));
			_gtkDropTarget.Motion  += (s, e) => RaiseDragOverPair(new Point(e.X, e.Y));
			_gtkDropTarget.Leave   += (s, e) => RaiseDragLeavePair(Mouse._lastPosition);
			_gtkDropTarget.Dropped += (s, e) => RaiseDropPair(new Point(e.X, e.Y));
			widget.AddController(_gtkDropTarget);
		}
		else
		{
			if (_gtkDropTarget == null)
				return;

			widget.RemoveController(_gtkDropTarget);
			_gtkDropTarget = null;
		}
	}

	/// <summary>Raises PreviewDragEnter (tunnel) then DragEnter (bubble) unless the preview is handled.</summary>
	internal void RaiseDragEnterPair(Point pos)
	{
		var data   = new DataObject();
		var tunnel = new DragEventArgs(PreviewDragEnterEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos);
		RaiseEvent(tunnel);
		if (tunnel.Handled) return;
		RaiseEvent(new DragEventArgs(DragEnterEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos));
	}

	/// <summary>Raises PreviewDragOver (tunnel) then DragOver (bubble) unless the preview is handled.</summary>
	internal void RaiseDragOverPair(Point pos)
	{
		var data   = new DataObject();
		var tunnel = new DragEventArgs(PreviewDragOverEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos);
		RaiseEvent(tunnel);
		if (tunnel.Handled) return;
		RaiseEvent(new DragEventArgs(DragOverEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos));
	}

	/// <summary>Raises PreviewDragLeave (tunnel) then DragLeave (bubble) unless the preview is handled.</summary>
	internal void RaiseDragLeavePair(Point pos)
	{
		var data   = new DataObject();
		var tunnel = new DragEventArgs(PreviewDragLeaveEvent, data,
			DragDropEffects.None, DragDropKeyStates.None, pos);
		RaiseEvent(tunnel);
		if (tunnel.Handled) return;
		RaiseEvent(new DragEventArgs(DragLeaveEvent, data,
			DragDropEffects.None, DragDropKeyStates.None, pos));
	}

	/// <summary>Raises PreviewDrop (tunnel) then Drop (bubble) unless the preview is handled.</summary>
	internal void RaiseDropPair(Point pos)
	{
		var data   = new DataObject();
		var tunnel = new DragEventArgs(PreviewDropEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos);
		RaiseEvent(tunnel);
		if (tunnel.Handled) return;
		RaiseEvent(new DragEventArgs(DropEvent, data,
			DragDropEffects.All, DragDropKeyStates.None, pos));
	}

	static bool HasGtkDisplay()
	{
		try { return Gdk.Display.Default != null; }
		catch { return false; }
	}
}
