namespace System.Windows.Input;

/// <summary>
/// Standard WPF application-level commands.
/// Each command is a <see cref="RoutedUICommand"/> with its standard keyboard gestures.
/// </summary>
public static class ApplicationCommands
{
	static readonly Type Owner = typeof(ApplicationCommands);

	static InputGestureCollection Gestures(params InputGesture[] g)
	{
		var c = new InputGestureCollection();
		foreach (var item in g) c.Add(item);
		return c;
	}

	static KeyGesture Ctrl(Key k) => new KeyGesture(k, ModifierKeys.Control);
	static KeyGesture CtrlShift(Key k) => new KeyGesture(k, ModifierKeys.Control | ModifierKeys.Shift);

	/// <summary>New document.</summary>
	public static RoutedUICommand New { get; } =
		new RoutedUICommand("New", nameof(New), Owner, Gestures(Ctrl(Key.N)));

	/// <summary>Open file.</summary>
	public static RoutedUICommand Open { get; } =
		new RoutedUICommand("Open", nameof(Open), Owner, Gestures(Ctrl(Key.O)));

	/// <summary>Save file.</summary>
	public static RoutedUICommand Save { get; } =
		new RoutedUICommand("Save", nameof(Save), Owner, Gestures(Ctrl(Key.S)));

	/// <summary>Save file as.</summary>
	public static RoutedUICommand SaveAs { get; } =
		new RoutedUICommand("Save As", nameof(SaveAs), Owner);

	/// <summary>Print.</summary>
	public static RoutedUICommand Print { get; } =
		new RoutedUICommand("Print", nameof(Print), Owner, Gestures(Ctrl(Key.P)));

	/// <summary>Print preview.</summary>
	public static RoutedUICommand PrintPreview { get; } =
		new RoutedUICommand("Print Preview", nameof(PrintPreview), Owner,
			Gestures(CtrlShift(Key.F12)));

	/// <summary>Close window/document.</summary>
	public static RoutedUICommand Close { get; } =
		new RoutedUICommand("Close", nameof(Close), Owner);

	/// <summary>Cut selection to clipboard.</summary>
	public static RoutedUICommand Cut { get; } =
		new RoutedUICommand("Cut", nameof(Cut), Owner, Gestures(Ctrl(Key.X)));

	/// <summary>Copy selection to clipboard.</summary>
	public static RoutedUICommand Copy { get; } =
		new RoutedUICommand("Copy", nameof(Copy), Owner, Gestures(Ctrl(Key.C)));

	/// <summary>Paste from clipboard.</summary>
	public static RoutedUICommand Paste { get; } =
		new RoutedUICommand("Paste", nameof(Paste), Owner, Gestures(Ctrl(Key.V)));

	/// <summary>Delete selection.</summary>
	public static RoutedUICommand Delete { get; } =
		new RoutedUICommand("Delete", nameof(Delete), Owner, Gestures(new KeyGesture(Key.Delete)));

	/// <summary>Undo last action.</summary>
	public static RoutedUICommand Undo { get; } =
		new RoutedUICommand("Undo", nameof(Undo), Owner, Gestures(Ctrl(Key.Z)));

	/// <summary>Redo last undone action.</summary>
	public static RoutedUICommand Redo { get; } =
		new RoutedUICommand("Redo", nameof(Redo), Owner, Gestures(Ctrl(Key.Y)));

	/// <summary>Find text.</summary>
	public static RoutedUICommand Find { get; } =
		new RoutedUICommand("Find", nameof(Find), Owner, Gestures(Ctrl(Key.F)));

	/// <summary>Find and replace text.</summary>
	public static RoutedUICommand Replace { get; } =
		new RoutedUICommand("Replace", nameof(Replace), Owner, Gestures(Ctrl(Key.H)));

	/// <summary>Select all content.</summary>
	public static RoutedUICommand SelectAll { get; } =
		new RoutedUICommand("Select All", nameof(SelectAll), Owner, Gestures(Ctrl(Key.A)));

	/// <summary>Show help.</summary>
	public static RoutedUICommand Help { get; } =
		new RoutedUICommand("Help", nameof(Help), Owner, Gestures(new KeyGesture(Key.F1)));

	/// <summary>Show properties.</summary>
	public static RoutedUICommand Properties { get; } =
		new RoutedUICommand("Properties", nameof(Properties), Owner,
			Gestures(new KeyGesture(Key.F4)));

	/// <summary>Open context menu.</summary>
	public static RoutedUICommand ContextMenu { get; } =
		new RoutedUICommand("Context Menu", nameof(ContextMenu), Owner,
			Gestures(new KeyGesture(Key.Apps)));

	/// <summary>Correct spelling.</summary>
	public static RoutedUICommand CorrectionList { get; } =
		new RoutedUICommand("Correction List", nameof(CorrectionList), Owner);

	/// <summary>Not a command — a sentinel value.</summary>
	public static RoutedUICommand NotACommand { get; } =
		new RoutedUICommand("Not A Command", nameof(NotACommand), Owner);

	/// <summary>Stop the current operation.</summary>
	public static RoutedUICommand Stop { get; } =
		new RoutedUICommand("Stop", nameof(Stop), Owner, Gestures(new KeyGesture(Key.Escape)));
}
