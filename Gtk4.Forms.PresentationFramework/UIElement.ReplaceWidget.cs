namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Swaps the backing root widget for <paramref name="replacement"/> in place: the old root's slot in its GTK
	/// parent is handed to the replacement. Returns false (nothing changed) when the parent kind is not supported.
	/// </summary>
	internal bool ReplaceRootWidget(Gtk.Widget replacement)
	{
		var old = _widget;
		if (old == null) return false;
		var parent = old.Parent;
		var prev = old.PrevSibling;
		if (parent == null) { Adopt(replacement); return true; }
		if (LogicalParent is Controls.Panel panel && panel.IsWidgetCreated)
		{
			panel.RemoveChildWidgetForSwap(this);
			Adopt(replacement);
			panel.AddChildWidgetForSwap(this);
			if (parent is Gtk.Box panelBox && ReferenceEquals(replacement.Parent, panelBox))
				panelBox.ReorderChildAfter(replacement, prev);
		}
		else if (parent is Gtk.Box box)
		{
			box.Remove(old);
			box.InsertChildAfter(replacement, prev);
			Adopt(replacement);
		}
		else
		{
			var child = parent.GetType().GetProperty("Child");
			if (child == null || !child.CanWrite || !typeof(Gtk.Widget).IsAssignableFrom(child.PropertyType))
				return false;
			child.SetValue(parent, replacement);
			Adopt(replacement);
		}
		return true;
	}

	void Adopt(Gtk.Widget replacement)
	{
		_owners.Remove(replacement);
		_owners.Add(replacement, this);
		_widget = replacement;
	}
}
