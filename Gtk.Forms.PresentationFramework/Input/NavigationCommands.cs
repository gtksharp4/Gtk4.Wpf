namespace System.Windows.Input;

/// <summary>
/// Standard WPF navigation commands.
/// </summary>
public static class NavigationCommands
{
	static readonly Type Owner = typeof(NavigationCommands);

	static InputGestureCollection Gestures(params InputGesture[] g)
	{
		var c = new InputGestureCollection();
		foreach (var item in g) c.Add(item);
		return c;
	}

	static KeyGesture Key(System.Windows.Input.Key k) => new KeyGesture(k);
	static KeyGesture Alt(System.Windows.Input.Key k) => new KeyGesture(k, ModifierKeys.Alt);
	static KeyGesture Ctrl(System.Windows.Input.Key k) => new KeyGesture(k, ModifierKeys.Control);

	/// <summary>Browse back (Alt+Left).</summary>
	public static RoutedUICommand BrowseBack { get; } =
		new RoutedUICommand("Browse Back", nameof(BrowseBack), Owner,
			Gestures(Alt(System.Windows.Input.Key.Left),
				new KeyGesture(System.Windows.Input.Key.Back)));

	/// <summary>Browse forward (Alt+Right).</summary>
	public static RoutedUICommand BrowseForward { get; } =
		new RoutedUICommand("Browse Forward", nameof(BrowseForward), Owner,
			Gestures(Alt(System.Windows.Input.Key.Right)));

	/// <summary>Browse to home.</summary>
	public static RoutedUICommand BrowseHome { get; } =
		new RoutedUICommand("Browse Home", nameof(BrowseHome), Owner,
			Gestures(Alt(System.Windows.Input.Key.Home)));

	/// <summary>Stop loading.</summary>
	public static RoutedUICommand BrowseStop { get; } =
		new RoutedUICommand("Browse Stop", nameof(BrowseStop), Owner,
			Gestures(new KeyGesture(System.Windows.Input.Key.Escape)));

	/// <summary>Refresh page (F5).</summary>
	public static RoutedUICommand Refresh { get; } =
		new RoutedUICommand("Refresh", nameof(Refresh), Owner,
			Gestures(Key(System.Windows.Input.Key.F5)));

	/// <summary>Go to a specific page.</summary>
	public static RoutedUICommand GoToPage { get; } =
		new RoutedUICommand("Go To Page", nameof(GoToPage), Owner);

	/// <summary>Zoom.</summary>
	public static RoutedUICommand Zoom { get; } =
		new RoutedUICommand("Zoom", nameof(Zoom), Owner);

	/// <summary>Search.</summary>
	public static RoutedUICommand Search { get; } =
		new RoutedUICommand("Search", nameof(Search), Owner,
			Gestures(Key(System.Windows.Input.Key.F3)));

	/// <summary>Go to next page.</summary>
	public static RoutedUICommand NextPage { get; } =
		new RoutedUICommand("Next Page", nameof(NextPage), Owner,
			Gestures(Key(System.Windows.Input.Key.PageDown)));

	/// <summary>Go to previous page.</summary>
	public static RoutedUICommand PreviousPage { get; } =
		new RoutedUICommand("Previous Page", nameof(PreviousPage), Owner,
			Gestures(Key(System.Windows.Input.Key.PageUp)));

	/// <summary>Go to first page (Ctrl+Home).</summary>
	public static RoutedUICommand FirstPage { get; } =
		new RoutedUICommand("First Page", nameof(FirstPage), Owner,
			Gestures(Ctrl(System.Windows.Input.Key.Home)));

	/// <summary>Go to last page (Ctrl+End).</summary>
	public static RoutedUICommand LastPage { get; } =
		new RoutedUICommand("Last Page", nameof(LastPage), Owner,
			Gestures(Ctrl(System.Windows.Input.Key.End)));

	/// <summary>Toggle favorites (Ctrl+I).</summary>
	public static RoutedUICommand Favorites { get; } =
		new RoutedUICommand("Favorites", nameof(Favorites), Owner,
			Gestures(Ctrl(System.Windows.Input.Key.I)));
}
