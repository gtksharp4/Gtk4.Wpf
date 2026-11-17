namespace System.Windows.Input;

/// <summary>
/// Standard WPF component-level navigation/focus commands.
/// </summary>
public static class ComponentCommands
{
	static readonly Type Owner = typeof(ComponentCommands);

	static InputGestureCollection Gestures(params InputGesture[] g)
	{
		var c = new InputGestureCollection();
		foreach (var item in g) c.Add(item);
		return c;
	}

	static KeyGesture K(Key k, ModifierKeys m = ModifierKeys.None) =>
		new KeyGesture(k, m);

	/// <summary>Move focus / caret left.</summary>
	public static RoutedUICommand MoveLeft { get; } =
		new RoutedUICommand("Move Left", nameof(MoveLeft), Owner,
			Gestures(K(Key.Left)));

	/// <summary>Move focus / caret right.</summary>
	public static RoutedUICommand MoveRight { get; } =
		new RoutedUICommand("Move Right", nameof(MoveRight), Owner,
			Gestures(K(Key.Right)));

	/// <summary>Move focus / caret up.</summary>
	public static RoutedUICommand MoveUp { get; } =
		new RoutedUICommand("Move Up", nameof(MoveUp), Owner,
			Gestures(K(Key.Up)));

	/// <summary>Move focus / caret down.</summary>
	public static RoutedUICommand MoveDown { get; } =
		new RoutedUICommand("Move Down", nameof(MoveDown), Owner,
			Gestures(K(Key.Down)));

	/// <summary>Move to line/document start (Home).</summary>
	public static RoutedUICommand MoveToHome { get; } =
		new RoutedUICommand("Move To Home", nameof(MoveToHome), Owner,
			Gestures(K(Key.Home)));

	/// <summary>Move to line/document end (End).</summary>
	public static RoutedUICommand MoveToEnd { get; } =
		new RoutedUICommand("Move To End", nameof(MoveToEnd), Owner,
			Gestures(K(Key.End)));

	/// <summary>Move up by page (PageUp).</summary>
	public static RoutedUICommand MoveToPageUp { get; } =
		new RoutedUICommand("Move To Page Up", nameof(MoveToPageUp), Owner,
			Gestures(K(Key.PageUp)));

	/// <summary>Move down by page (PageDown).</summary>
	public static RoutedUICommand MoveToPageDown { get; } =
		new RoutedUICommand("Move To Page Down", nameof(MoveToPageDown), Owner,
			Gestures(K(Key.PageDown)));

	/// <summary>Scroll view by one line (Ctrl+Down).</summary>
	public static RoutedUICommand ScrollByLine { get; } =
		new RoutedUICommand("Scroll By Line", nameof(ScrollByLine), Owner);

	/// <summary>Scroll up by page.</summary>
	public static RoutedUICommand ScrollPageUp { get; } =
		new RoutedUICommand("Scroll Page Up", nameof(ScrollPageUp), Owner);

	/// <summary>Scroll down by page.</summary>
	public static RoutedUICommand ScrollPageDown { get; } =
		new RoutedUICommand("Scroll Page Down", nameof(ScrollPageDown), Owner);

	/// <summary>Scroll left.</summary>
	public static RoutedUICommand ScrollPageLeft { get; } =
		new RoutedUICommand("Scroll Page Left", nameof(ScrollPageLeft), Owner);

	/// <summary>Scroll right.</summary>
	public static RoutedUICommand ScrollPageRight { get; } =
		new RoutedUICommand("Scroll Page Right", nameof(ScrollPageRight), Owner);

	/// <summary>Extend selection left (Shift+Left).</summary>
	public static RoutedUICommand SelectToHome { get; } =
		new RoutedUICommand("Select To Home", nameof(SelectToHome), Owner,
			Gestures(K(Key.Home, ModifierKeys.Shift)));

	/// <summary>Extend selection to end (Shift+End).</summary>
	public static RoutedUICommand SelectToEnd { get; } =
		new RoutedUICommand("Select To End", nameof(SelectToEnd), Owner,
			Gestures(K(Key.End, ModifierKeys.Shift)));

	/// <summary>Move focus backward (Shift+Tab).</summary>
	public static RoutedUICommand MoveFocusBack { get; } =
		new RoutedUICommand("Move Focus Back", nameof(MoveFocusBack), Owner,
			Gestures(K(Key.Tab, ModifierKeys.Shift)));

	/// <summary>Move focus forward (Tab).</summary>
	public static RoutedUICommand MoveFocusForward { get; } =
		new RoutedUICommand("Move Focus Forward", nameof(MoveFocusForward), Owner,
			Gestures(K(Key.Tab)));

	/// <summary>Move focus up (Ctrl+Up).</summary>
	public static RoutedUICommand MoveFocusUp { get; } =
		new RoutedUICommand("Move Focus Up", nameof(MoveFocusUp), Owner,
			Gestures(K(Key.Up, ModifierKeys.Control)));

	/// <summary>Move focus down (Ctrl+Down).</summary>
	public static RoutedUICommand MoveFocusDown { get; } =
		new RoutedUICommand("Move Focus Down", nameof(MoveFocusDown), Owner,
			Gestures(K(Key.Down, ModifierKeys.Control)));

	/// <summary>Move focus page up.</summary>
	public static RoutedUICommand MoveFocusPageUp { get; } =
		new RoutedUICommand("Move Focus Page Up", nameof(MoveFocusPageUp), Owner,
			Gestures(K(Key.PageUp, ModifierKeys.Control)));

	/// <summary>Move focus page down.</summary>
	public static RoutedUICommand MoveFocusPageDown { get; } =
		new RoutedUICommand("Move Focus Page Down", nameof(MoveFocusPageDown), Owner,
			Gestures(K(Key.PageDown, ModifierKeys.Control)));
}
