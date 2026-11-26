namespace System.Windows.Automation.Peers;

/// <summary>Maps a UI Automation control type to the GTK 4 accessible role that represents it.</summary>
static class AutomationRoleMap
{
	internal static Gtk.AccessibleRole? For(AutomationControlType type)
	{
		switch (type)
		{
			case AutomationControlType.Button:
			case AutomationControlType.SplitButton: return Gtk.AccessibleRole.Button;
			case AutomationControlType.Calendar:
			case AutomationControlType.DataGrid: return Gtk.AccessibleRole.Grid;
			case AutomationControlType.CheckBox: return Gtk.AccessibleRole.Checkbox;
			case AutomationControlType.ComboBox: return Gtk.AccessibleRole.ComboBox;
			case AutomationControlType.Edit: return Gtk.AccessibleRole.TextBox;
			case AutomationControlType.Hyperlink: return Gtk.AccessibleRole.Link;
			case AutomationControlType.Image: return Gtk.AccessibleRole.Img;
			case AutomationControlType.ListItem: return Gtk.AccessibleRole.ListItem;
			case AutomationControlType.List: return Gtk.AccessibleRole.List;
			case AutomationControlType.Menu: return Gtk.AccessibleRole.Menu;
			case AutomationControlType.MenuBar: return Gtk.AccessibleRole.MenuBar;
			case AutomationControlType.MenuItem: return Gtk.AccessibleRole.MenuItem;
			case AutomationControlType.ProgressBar: return Gtk.AccessibleRole.ProgressBar;
			case AutomationControlType.RadioButton: return Gtk.AccessibleRole.Radio;
			case AutomationControlType.ScrollBar: return Gtk.AccessibleRole.Scrollbar;
			case AutomationControlType.Slider: return Gtk.AccessibleRole.Slider;
			case AutomationControlType.Spinner: return Gtk.AccessibleRole.SpinButton;
			case AutomationControlType.StatusBar: return Gtk.AccessibleRole.Status;
			case AutomationControlType.Tab: return Gtk.AccessibleRole.TabList;
			case AutomationControlType.TabItem: return Gtk.AccessibleRole.Tab;
			case AutomationControlType.Text: return Gtk.AccessibleRole.Label;
			case AutomationControlType.ToolBar: return Gtk.AccessibleRole.Toolbar;
			case AutomationControlType.ToolTip: return Gtk.AccessibleRole.Tooltip;
			case AutomationControlType.Tree: return Gtk.AccessibleRole.Tree;
			case AutomationControlType.TreeItem: return Gtk.AccessibleRole.TreeItem;
			case AutomationControlType.Group: return Gtk.AccessibleRole.Group;
			case AutomationControlType.DataItem: return Gtk.AccessibleRole.Row;
			case AutomationControlType.Document: return Gtk.AccessibleRole.Document;
			case AutomationControlType.Window: return Gtk.AccessibleRole.Window;
			case AutomationControlType.Header:
			case AutomationControlType.HeaderItem: return Gtk.AccessibleRole.ColumnHeader;
			case AutomationControlType.Table: return Gtk.AccessibleRole.Table;
			case AutomationControlType.Separator: return Gtk.AccessibleRole.Separator;
			default: return null; // Custom, Pane, Thumb, TitleBar: keep the widget's own role.
		}
	}
}
