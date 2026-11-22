using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Plays audio and video. The decoder is GTK's <see cref="Gtk.MediaFile"/> (whatever media backend the GTK
/// runtime ships); its frames are drawn by a <see cref="Gtk.Picture"/>, so no player controls are shown.
/// <see cref="Balance"/>, <see cref="ScrubbingEnabled"/>, <see cref="SpeedRatio"/>, <see cref="StretchDirection"/>
/// and buffering are stored only: GTK media streams expose no equivalent.
/// </summary>
public partial class MediaElement : FrameworkElement, IUriContext
{
	/// <summary>Identifies <see cref="Source"/>.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(Uri), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnSourceChanged));

	/// <summary>Identifies <see cref="Volume"/>. Default 0.5.</summary>
	public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
		nameof(Volume), typeof(double), typeof(MediaElement),
		new FrameworkPropertyMetadata(0.5, OnAudioChanged));

	/// <summary>Identifies <see cref="Balance"/>. Default 0; stored only.</summary>
	public static readonly DependencyProperty BalanceProperty = DependencyProperty.Register(
		nameof(Balance), typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(0.0));

	/// <summary>Identifies <see cref="IsMuted"/>. Default false.</summary>
	public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register(
		nameof(IsMuted), typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false, OnAudioChanged));

	/// <summary>Identifies <see cref="ScrubbingEnabled"/>. Default false; stored only.</summary>
	public static readonly DependencyProperty ScrubbingEnabledProperty = DependencyProperty.Register(
		nameof(ScrubbingEnabled), typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="UnloadedBehavior"/>. Default <see cref="MediaState.Close"/>.</summary>
	public static readonly DependencyProperty UnloadedBehaviorProperty = DependencyProperty.Register(
		nameof(UnloadedBehavior), typeof(MediaState), typeof(MediaElement),
		new FrameworkPropertyMetadata(MediaState.Close), IsValidState);

	/// <summary>Identifies <see cref="LoadedBehavior"/>. Default <see cref="MediaState.Play"/>.</summary>
	public static readonly DependencyProperty LoadedBehaviorProperty = DependencyProperty.Register(
		nameof(LoadedBehavior), typeof(MediaState), typeof(MediaElement),
		new FrameworkPropertyMetadata(MediaState.Play, OnLoadedBehaviorChanged), IsValidState);

	/// <summary>Identifies <see cref="Stretch"/>. Default <see cref="Media.Stretch.Uniform"/>.</summary>
	public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
		nameof(Stretch), typeof(Stretch), typeof(MediaElement),
		new FrameworkPropertyMetadata(
			Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStretchChanged));

	/// <summary>
	/// Identifies <see cref="StretchDirection"/>. Default
	/// <see cref="Controls.StretchDirection.Both"/>.
	/// </summary>
	public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register(
		nameof(StretchDirection), typeof(StretchDirection), typeof(MediaElement),
		new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Gets or sets the media file.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>Gets or sets the volume, 0 to 1.</summary>
	public double Volume
	{
		get => (double)GetValue(VolumeProperty)!;
		set => SetValue(VolumeProperty, value);
	}

	/// <summary>Gets or sets the left/right balance, -1 to 1. Stored only.</summary>
	public double Balance
	{
		get => (double)GetValue(BalanceProperty)!;
		set => SetValue(BalanceProperty, value);
	}

	/// <summary>Gets or sets whether the audio is muted.</summary>
	public bool IsMuted
	{
		get => (bool)GetValue(IsMutedProperty)!;
		set => SetValue(IsMutedProperty, value);
	}

	/// <summary>Gets or sets whether frames are shown while scrubbing. Stored only.</summary>
	public bool ScrubbingEnabled
	{
		get => (bool)GetValue(ScrubbingEnabledProperty)!;
		set => SetValue(ScrubbingEnabledProperty, value);
	}

	/// <summary>Gets or sets what happens to the media when the element is unloaded.</summary>
	public MediaState UnloadedBehavior
	{
		get => (MediaState)GetValue(UnloadedBehaviorProperty)!;
		set => SetValue(UnloadedBehaviorProperty, value);
	}

	/// <summary>Gets or sets what happens to the media when the element is loaded.</summary>
	public MediaState LoadedBehavior
	{
		get => (MediaState)GetValue(LoadedBehaviorProperty)!;
		set => SetValue(LoadedBehaviorProperty, value);
	}

	/// <summary>Gets or sets how the video is stretched to fill the element.</summary>
	public Stretch Stretch
	{
		get => (Stretch)GetValue(StretchProperty)!;
		set => SetValue(StretchProperty, value);
	}

	/// <summary>Gets or sets the direction in which the video may be scaled. Stored only.</summary>
	public StretchDirection StretchDirection
	{
		get => (StretchDirection)GetValue(StretchDirectionProperty)!;
		set => SetValue(StretchDirectionProperty, value);
	}

	Uri? _baseUri;

	/// <summary>The base URI that a relative <see cref="Source"/> is resolved against.</summary>
	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	static bool IsValidState(object? value) => value is MediaState s && s >= MediaState.Manual && s <= MediaState.Stop;

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((MediaElement)d).OnSourceChanged();

	static void OnAudioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((MediaElement)d).ApplyAudio();

	static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((MediaElement)d).ApplyStretch();

	static void OnLoadedBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var element = (MediaElement)d;
		if (element.IsLoaded) element.ApplyBehavior(element.LoadedBehavior);
	}
}
