using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Input;

// WPF registers these class handlers in each control's static constructor. C# allows one static constructor
// per class and those controls' are not ours to edit, so they are all registered here, before the first use
// of AccessKeyPressedEvent.
public sealed partial class AccessKeyManager
{
	static void RegisterClassHandlers()
	{
		Add(typeof(ButtonBase), OnButtonBase);
		Add(typeof(Label), OnLabel);
		Add(typeof(GroupBox), OnSelf);
		Add(typeof(Expander), OnSelf);
		Add(typeof(TabItem), OnTabItem);
		Add(typeof(MenuItem), OnMenuItem);
	}

	static void Add(Type type, AccessKeyPressedEventHandler handler)
		=> EventManager.RegisterClassHandler(type, AccessKeyPressedEvent, handler);

	static bool Open(AccessKeyPressedEventArgs e) => !e.Handled && e.Scope == null && e.Target == null;

	// ButtonBase and GroupBox: the sender is the target.
	static void OnButtonBase(object sender, AccessKeyPressedEventArgs e) => OnSelf(sender, e);

	static void OnSelf(object sender, AccessKeyPressedEventArgs e)
	{
		if (Open(e)) e.Target = (UIElement)sender;
	}

	static void OnLabel(object sender, AccessKeyPressedEventArgs e)
	{
		if (Open(e)) e.Target = ((Label)sender).Target;
	}

	static void OnTabItem(object sender, AccessKeyPressedEventArgs e)
	{
		if (Open(e)) e.Target = (TabItem)sender;
	}

	// An open submenu is the scope for a key raised on it or bubbling up from an item inside it (WPF's
	// isScope); any other menu item is a target unless the mouse is captured by something that is not a menu.
	static void OnMenuItem(object sender, AccessKeyPressedEventArgs e)
	{
		if (e.Handled || e.Scope != null) return;
		var item = (MenuItem)sender;
		if (e.Target != null)
		{
			if (!ReferenceEquals(e.Target, item) && item.IsSubmenuOpen) e.Scope = item;
		}
		else if (item.IsSubmenuOpen && ReferenceEquals(e.OriginalSource, item))
			e.Scope = item;
		else if (Mouse.Captured == null || Mouse.Captured is Menu || Mouse.Captured is ContextMenu)
			e.Target = item;
	}
}
