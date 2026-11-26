using System;

namespace System.Windows.Navigation;

public partial class NavigationWindow
{
	/// <summary>Identifies <see cref="Source"/>; shares the owner chain of <c>Frame.SourceProperty</c>.</summary>
	public static readonly DependencyProperty SourceProperty = Controls.Frame.SourceProperty.AddOwner(
		typeof(NavigationWindow), new SourceMetadata(OnSourceChanged));

	// Frame's callback casts its target to Frame, so the window's metadata keeps only its own callback
	// instead of chaining the base one as Merge does.
	sealed class SourceMetadata : FrameworkPropertyMetadata
	{
		readonly PropertyChangedCallback _own;

		public SourceMetadata(PropertyChangedCallback callback) : base((Uri?)null, callback) => _own = callback;

		protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
		{
			base.Merge(baseMetadata, dp);
			PropertyChangedCallback = _own;
		}
	}

	bool _syncingSource;

	/// <summary>The URI of the current content; setting it navigates.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var window = (NavigationWindow)d;
		if (window._syncingSource || e.NewValue is not Uri uri) return;
		window.NavigationService.Navigate(uri);
	}

	// After navigation Source follows the content's URI (null for content navigated to as an object).
	void SyncSource()
	{
		Uri? current = NavigationService.CurrentSource;
		if (Equals(Source, current)) return;
		_syncingSource = true;
		// SetValue stands in for WPF SetCurrentValueInternal until the layer has SetCurrentValue (a binding on
		// Source is replaced after navigation).
		try { SetValue(SourceProperty, current); }
		finally { _syncingSource = false; }
	}
}
