namespace System.Windows.Input;

/// <summary>
/// Standard WPF media playback commands.
/// </summary>
public static class MediaCommands
{
	static readonly Type Owner = typeof(MediaCommands);

	static InputGestureCollection Gestures(params InputGesture[] g)
	{
		var c = new InputGestureCollection();
		foreach (var item in g) c.Add(item);
		return c;
	}

	static KeyGesture K(Key k, ModifierKeys m = ModifierKeys.None) =>
		new KeyGesture(k, m);

	/// <summary>Play media.</summary>
	public static RoutedUICommand Play { get; } =
		new RoutedUICommand("Play", nameof(Play), Owner);

	/// <summary>Pause media.</summary>
	public static RoutedUICommand Pause { get; } =
		new RoutedUICommand("Pause", nameof(Pause), Owner);

	/// <summary>Stop media.</summary>
	public static RoutedUICommand Stop { get; } =
		new RoutedUICommand("Stop", nameof(Stop), Owner);

	/// <summary>Toggle play/pause.</summary>
	public static RoutedUICommand TogglePlayPause { get; } =
		new RoutedUICommand("Toggle Play/Pause", nameof(TogglePlayPause), Owner);

	/// <summary>Skip to next track.</summary>
	public static RoutedUICommand NextTrack { get; } =
		new RoutedUICommand("Next Track", nameof(NextTrack), Owner);

	/// <summary>Skip to previous track.</summary>
	public static RoutedUICommand PreviousTrack { get; } =
		new RoutedUICommand("Previous Track", nameof(PreviousTrack), Owner);

	/// <summary>Fast forward.</summary>
	public static RoutedUICommand FastForward { get; } =
		new RoutedUICommand("Fast Forward", nameof(FastForward), Owner);

	/// <summary>Rewind.</summary>
	public static RoutedUICommand Rewind { get; } =
		new RoutedUICommand("Rewind", nameof(Rewind), Owner);

	/// <summary>Increase volume.</summary>
	public static RoutedUICommand IncreaseVolume { get; } =
		new RoutedUICommand("Increase Volume", nameof(IncreaseVolume), Owner);

	/// <summary>Decrease volume.</summary>
	public static RoutedUICommand DecreaseVolume { get; } =
		new RoutedUICommand("Decrease Volume", nameof(DecreaseVolume), Owner);

	/// <summary>Mute audio.</summary>
	public static RoutedUICommand MuteVolume { get; } =
		new RoutedUICommand("Mute Volume", nameof(MuteVolume), Owner);

	/// <summary>Increase bass.</summary>
	public static RoutedUICommand IncreaseBass { get; } =
		new RoutedUICommand("Increase Bass", nameof(IncreaseBass), Owner);

	/// <summary>Decrease bass.</summary>
	public static RoutedUICommand DecreaseBass { get; } =
		new RoutedUICommand("Decrease Bass", nameof(DecreaseBass), Owner);

	/// <summary>Increase treble.</summary>
	public static RoutedUICommand IncreaseTreble { get; } =
		new RoutedUICommand("Increase Treble", nameof(IncreaseTreble), Owner);

	/// <summary>Decrease treble.</summary>
	public static RoutedUICommand DecreaseTreble { get; } =
		new RoutedUICommand("Decrease Treble", nameof(DecreaseTreble), Owner);

	/// <summary>Increase playback speed.</summary>
	public static RoutedUICommand IncreasePlaybackRate { get; } =
		new RoutedUICommand("Increase Playback Rate", nameof(IncreasePlaybackRate), Owner);

	/// <summary>Decrease playback speed.</summary>
	public static RoutedUICommand DecreasePlaybackRate { get; } =
		new RoutedUICommand("Decrease Playback Rate", nameof(DecreasePlaybackRate), Owner);

	/// <summary>Toggle fullscreen mode.</summary>
	public static RoutedUICommand ToggleFullScreen { get; } =
		new RoutedUICommand("Toggle Full Screen", nameof(ToggleFullScreen), Owner,
			Gestures(K(Key.F11)));

	/// <summary>Record media.</summary>
	public static RoutedUICommand Record { get; } =
		new RoutedUICommand("Record", nameof(Record), Owner);

	/// <summary>Select caption language.</summary>
	public static RoutedUICommand Select { get; } =
		new RoutedUICommand("Select", nameof(Select), Owner);

	/// <summary>Toggle subtitles on/off.</summary>
	public static RoutedUICommand BoostBase { get; } =
		new RoutedUICommand("Boost Base", nameof(BoostBase), Owner);

	/// <summary>Scan backward through media.</summary>
	public static RoutedUICommand ScanBackward { get; } =
		new RoutedUICommand("Scan Backward", nameof(ScanBackward), Owner);

	/// <summary>Scan forward through media.</summary>
	public static RoutedUICommand ScanForward { get; } =
		new RoutedUICommand("Scan Forward", nameof(ScanForward), Owner);

	/// <summary>Skip backward a chapter or segment.</summary>
	public static RoutedUICommand SkipBackward { get; } =
		new RoutedUICommand("Skip Backward", nameof(SkipBackward), Owner);

	/// <summary>Skip forward a chapter or segment.</summary>
	public static RoutedUICommand SkipForward { get; } =
		new RoutedUICommand("Skip Forward", nameof(SkipForward), Owner);
}
