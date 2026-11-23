namespace System.Windows;

public abstract partial class FrameworkElement
{
	// ── Loaded / Unloaded ─────────────────────────────────────────────────────

	/// <summary>Identifies the <see cref="Loaded"/> routed event (Direct).</summary>
	public static readonly RoutedEvent LoadedEvent =
		EventManager.RegisterRoutedEvent(
			"Loaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkElement));

	/// <summary>Identifies the <see cref="Unloaded"/> routed event (Direct).</summary>
	public static readonly RoutedEvent UnloadedEvent =
		EventManager.RegisterRoutedEvent(
			"Unloaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkElement));

	/// <summary>
	/// Occurs when the element is laid out, rendered and ready for interaction: raised pre-order
	/// (root first) over the tree once the hosting <see cref="Window"/> is shown, and — deferred
	/// at <see cref="Threading.DispatcherPriority.Loaded"/> — when the element is added to an
	/// already-loaded tree. Not raised again while the element stays loaded.
	/// </summary>
	/// <remarks>
	/// GTK trigger: the toplevel <c>map</c> signal only (see <c>Window.Lifecycle.cs</c>). Hiding
	/// an element (GTK unmap) does not raise <see cref="Unloaded"/>, matching WPF.
	/// </remarks>
	public event RoutedEventHandler Loaded
	{
		add => AddHandler(LoadedEvent, value);
		remove => RemoveHandler(LoadedEvent, value);
	}

	/// <summary>
	/// Occurs when the element is removed from a loaded tree, or when its hosting
	/// <see cref="Window"/> closes (after <see cref="Window.Closed"/>).
	/// </summary>
	public event RoutedEventHandler Unloaded
	{
		add => AddHandler(UnloadedEvent, value);
		remove => RemoveHandler(UnloadedEvent, value);
	}

	bool _isLoaded;

	/// <summary>
	/// <see langword="true"/> once the element has been loaded (set before the
	/// <see cref="Loaded"/> handlers run; cleared before <see cref="Unloaded"/> handlers run).
	/// </summary>
	public bool IsLoaded => _isLoaded;

	/// <summary>Updates <see cref="IsLoaded"/> then raises <see cref="Loaded"/> or <see cref="Unloaded"/>.</summary>
	internal void SetLoadedAndRaise(bool loaded)
	{
		_isLoaded = loaded;
		Data.BindingEngine.TreeChanged(this);
		RaiseEvent(new RoutedEventArgs(loaded ? LoadedEvent : UnloadedEvent) { Source = this });
	}
}
