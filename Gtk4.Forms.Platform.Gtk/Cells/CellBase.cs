using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gtk;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Cells;

public abstract class CellBase : EventBox
{
	// Which widget currently shows a given Forms Cell - i.e. which one owes it a Disappearing.
	// It has to live on the Cell, not on the widget: a ListView refresh throws away every cell
	// widget and builds a fresh one (CellRenderer.GetCell is always called with reusableView:
	// null - see ListViewRenderer.UpdateItems and Controls/TableView), while the Forms Cell
	// survives. Held on the widget, "is this cell on screen?" was false again on every refresh,
	// so the setter below re-sent Appearing for all rows and never sent Disappearing: an
	// ItemAppearing-driven paging handler re-triggered itself forever, and anything pairing the
	// two events leaked one "appeared" per row per collection change. Recording the owner - not
	// just a bool - is what lets a discarded widget stay quiet instead of sending Disappearing
	// for the cell its own replacement is already showing.
	static readonly BindableProperty AppearedInProperty =
		BindableProperty.CreateAttached("AppearedIn", typeof(CellBase), typeof(Cell), null);

	Cell _cell;
	int _desiredHeight;
	IList<MenuItem> _contextActions;

	public Action<object, PropertyChangedEventArgs> PropertyChanged;

	protected CellBase()
	{
		ButtonReleaseEvent += OnClick;

		// GTK4 removed the parent-set vfunc; the notify::parent GObject property notification
		// fires on every parent change and is the idiomatic replacement.
		// (GLib.Object.AddNotification — GLibSharp/Object.cs:877)
		AddNotification("parent", OnParentNotify);
	}

	public Cell Cell
	{
		get { return _cell; }
		set
		{
			if (_cell == value)
				return;

			if (_cell != null)
				ReleaseAppearance(_cell);

			_cell = value;
			UpdateCell();
			_contextActions = Cell.ContextActions;

			if (_cell != null)
				ClaimAppearance(_cell);
		}
	}

	public object Item => Cell?.BindingContext;

	protected bool ParentHasUnevenRows
	{
		get
		{
			var table = Cell.RealParent as TableView;
			if (table != null)
				return table.HasUnevenRows;

			var list = Cell.RealParent as ListView;
			if (list != null)
				return list.HasUnevenRows;

			return false;
		}
	}

	public int DesiredHeight
	{
		get
		{
			return _desiredHeight;
		}

		set
		{
			_desiredHeight = value;
		}
	}

	public void SetDesiredHeight(int height)
	{
		DesiredHeight = height;

		if (IsRealized)
		{
			HeightRequest = DesiredHeight;
		}
	}

	public void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}

	protected override void OnRealized()
	{
		base.OnRealized();

		HeightRequest = DesiredHeight;
	}

	// Replaces the removed OnParentSet(Widget) vfunc (GTK4 removed ::parent-set).
	// Being unparented is the only notice this widget gets that the list threw it away:
	// Controls/ListView.ClearList and Controls/TableView.RefreshSource simply Remove() their
	// children, nobody clears Cell first. Without this a row dropped from the ItemsSource
	// would never disappear at all. Re-parenting is the symmetric case, and claiming is a
	// no-op unless the slot is free, so the usual pack-after-create order stays silent.
	void OnParentNotify(object o, GLib.NotifyArgs args)
	{
		if (_cell == null)
			return;

		if (Parent == null)
			ReleaseAppearance(_cell);
		else
			ClaimAppearance(_cell);
	}

	// GTK4 removed the ::destroy signal and the OnDestroyed() vfunc; Dispose(bool) is the
	// correct teardown point. (Widget.Compat.cs:445-452 documents the deliberate omission.)
	// Covers a widget destroyed while unparented, which OnParentNotify never hears about.
	// The two are idempotent between them: whichever runs first releases, the other sees that
	// this widget no longer owns the slot and does nothing.
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			RemoveNotification("parent", OnParentNotify);
			ButtonReleaseEvent -= OnClick;

			if (_cell != null)
				ReleaseAppearance(_cell);
		}

		base.Dispose(disposing);
	}

	protected virtual void UpdateCell()
	{
	}

	// Takes over the on-screen slot of `cell` for this widget. If a (by now discarded) widget
	// still held it, the row never left the screen, so the slot changes hands silently - raising
	// a second Appearing there is exactly what made incremental paging loop.
	void ClaimAppearance(Cell cell)
	{
		var owner = (CellBase)cell.GetValue(AppearedInProperty);

		if (ReferenceEquals(owner, this))
			return;

		cell.SetValue(AppearedInProperty, this);

		if (owner == null)
			Device.BeginInvokeOnMainThread(cell.SendAppearing);
	}

	// The mirror of ClaimAppearance: only the widget holding the slot may give it up, so a widget
	// being torn down after its cell was handed to a newer widget stays quiet.
	void ReleaseAppearance(Cell cell)
	{
		var owner = (CellBase)cell.GetValue(AppearedInProperty);

		if (!ReferenceEquals(owner, this))
			return;

		cell.SetValue(AppearedInProperty, null);

		Device.BeginInvokeOnMainThread(cell.SendDisappearing);
	}

	void OnClick(object o, ButtonReleaseEventArgs args)
	{
		if (args.Event.Button != 3)  // Right button
			return;

		if (_contextActions.Any())
			OpenContextMenu();
	}

	// GTK4 removed Gtk.Menu / Gtk.MenuItem. Replace with a Gtk.PopoverMenu backed by a
	// GLib.Menu model (GioSharp/Generated/GLib/Menu.cs) and a GLib.SimpleActionGroup
	// (GioSharp/Generated/GLib/SimpleActionGroup.cs) inserted on this widget so the popover
	// can resolve its "ctx.<n>" detailed-action strings.
	// (Gtk.PopoverMenu ctor: GtkSharp/Generated/Gtk/PopoverMenu.cs:25
	//  Gtk.Widget.InsertActionGroup: GtkSharp/Generated/Gtk/Widget.cs:3114
	//  Gtk.Widget.Parent setter: GtkSharp/Generated/Gtk/Widget.cs:504)
	void OpenContextMenu()
	{
		var gMenu = new GLib.Menu();
		var actionGroup = new GLib.SimpleActionGroup();
		var actions = Cell.ContextActions;

		for (var i = 0; i < actions.Count; i++)
		{
			var item = actions[i];
			var actionName = "item" + i;
			var action = new GLib.SimpleAction(actionName, null);
			var capturedItem = item;
			action.Activated += (s, e) => ((IMenuItemController)capturedItem).Activate();
			actionGroup.AddAction(action);
			gMenu.Append(item.Text ?? string.Empty, "ctx." + actionName);
		}

		InsertActionGroup("ctx", actionGroup);

		var popover = new Gtk.PopoverMenu(gMenu);
		popover.Parent = this;
		popover.HasArrow = false;
		popover.Autohide = true;
		popover.Popup();
	}
}