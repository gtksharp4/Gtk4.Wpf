using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows.Automation;

/// <summary>Identifies the kind of control an automation element represents.</summary>
public sealed class ControlType : AutomationIdentifier
{
	static readonly Dictionary<int, ControlType> _table = new Dictionary<int, ControlType>();

	ControlType(int id, string name, string localized) : base(id, "ControlType." + name)
	{
		LocalizedControlType = localized;
	}

	static ControlType C(int id, string name, string localized) =>
		Register(_table, new ControlType(id, name, localized));

	/// <summary>The localized, human-readable control type name.</summary>
	public string LocalizedControlType { get; }

	/// <summary>Returns the control type with the given id, or <c>null</c>.</summary>
	public static ControlType? LookupById(int id)
	{
		RuntimeHelpers.RunClassConstructor(typeof(ControlType).TypeHandle);
		lock (_table) return _table.TryGetValue(id, out var found) ? found : null;
	}

	public static readonly ControlType Button = C(50000, "Button", "button");
	public static readonly ControlType Calendar = C(50001, "Calendar", "calendar");
	public static readonly ControlType CheckBox = C(50002, "CheckBox", "check box");
	public static readonly ControlType ComboBox = C(50003, "ComboBox", "combo box");
	public static readonly ControlType Edit = C(50004, "Edit", "edit");
	public static readonly ControlType Hyperlink = C(50005, "Hyperlink", "hyperlink");
	public static readonly ControlType Image = C(50006, "Image", "image");
	public static readonly ControlType ListItem = C(50007, "ListItem", "list item");
	public static readonly ControlType List = C(50008, "List", "list");
	public static readonly ControlType Menu = C(50009, "Menu", "menu");
	public static readonly ControlType MenuBar = C(50010, "MenuBar", "menu bar");
	public static readonly ControlType MenuItem = C(50011, "MenuItem", "menu item");
	public static readonly ControlType ProgressBar = C(50012, "ProgressBar", "progress bar");
	public static readonly ControlType RadioButton = C(50013, "RadioButton", "radio button");
	public static readonly ControlType ScrollBar = C(50014, "ScrollBar", "scroll bar");
	public static readonly ControlType Slider = C(50015, "Slider", "slider");
	public static readonly ControlType Spinner = C(50016, "Spinner", "spinner");
	public static readonly ControlType StatusBar = C(50017, "StatusBar", "status bar");
	public static readonly ControlType Tab = C(50018, "Tab", "tab");
	public static readonly ControlType TabItem = C(50019, "TabItem", "tab item");
	public static readonly ControlType Text = C(50020, "Text", "text");
	public static readonly ControlType ToolBar = C(50021, "ToolBar", "tool bar");
	public static readonly ControlType ToolTip = C(50022, "ToolTip", "tool tip");
	public static readonly ControlType Tree = C(50023, "Tree", "tree");
	public static readonly ControlType TreeItem = C(50024, "TreeItem", "tree item");
	public static readonly ControlType Custom = C(50025, "Custom", "custom");
	public static readonly ControlType Group = C(50026, "Group", "group");
	public static readonly ControlType Thumb = C(50027, "Thumb", "thumb");
	public static readonly ControlType DataGrid = C(50028, "DataGrid", "data grid");
	public static readonly ControlType DataItem = C(50029, "DataItem", "data item");
	public static readonly ControlType Document = C(50030, "Document", "document");
	public static readonly ControlType SplitButton = C(50031, "SplitButton", "split button");
	public static readonly ControlType Window = C(50032, "Window", "window");
	public static readonly ControlType Pane = C(50033, "Pane", "pane");
	public static readonly ControlType Header = C(50034, "Header", "header");
	public static readonly ControlType HeaderItem = C(50035, "HeaderItem", "header item");
	public static readonly ControlType Table = C(50036, "Table", "table");
	public static readonly ControlType TitleBar = C(50037, "TitleBar", "title bar");
	public static readonly ControlType Separator = C(50038, "Separator", "separator");
}
