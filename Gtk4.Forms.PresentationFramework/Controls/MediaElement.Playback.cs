using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Controls;

public partial class MediaElement
{
	TimeSpan _position = TimeSpan.Zero;
	double _speedRatio = 1.0;
	MediaClock? _clock;

	/// <summary>Gets or sets the playback position. Applied to the stream once it has opened.</summary>
	public TimeSpan Position
	{
		get => PositionCore();
		set => SetPosition(value);
	}

	/// <summary>Gets or sets the playback speed. Stored only: GTK media streams cannot change speed.</summary>
	public double SpeedRatio
	{
		get => _speedRatio;
		set => _speedRatio = value;
	}

	/// <summary>Gets or sets the clock that drives the media; its timeline must be a <see cref="MediaTimeline"/>.</summary>
	public MediaClock? Clock
	{
		get => _clock;
		set => SetClock(value);
	}

	/// <summary>The length of the opened media; Automatic until it opens.</summary>
	public Duration NaturalDuration => NaturalDurationCore();

	/// <summary>The width of the video in pixels, or 0.</summary>
	public int NaturalVideoWidth => NaturalVideoWidthCore();

	/// <summary>The height of the video in pixels, or 0.</summary>
	public int NaturalVideoHeight => NaturalVideoHeightCore();

	/// <summary>Whether the opened media has an audio track.</summary>
	public bool HasAudio => HasAudioCore();

	/// <summary>Whether the opened media has a video track.</summary>
	public bool HasVideo => HasVideoCore();

	/// <summary>Whether the media is buffering. Always false: local GTK streams do not report buffering.</summary>
	public bool IsBuffering => false;

	/// <summary>Buffering progress, 0 to 1. Always 0.</summary>
	public double BufferingProgress => 0.0;

	/// <summary>Download progress, 0 to 1: 1 once the media has opened.</summary>
	public double DownloadProgress => IsOpened ? 1.0 : 0.0;

	/// <summary>Whether the opened media can be paused.</summary>
	public bool CanPause => IsOpened;

	/// <summary>Starts playback. Only valid when <see cref="LoadedBehavior"/> is Manual.</summary>
	public void Play()
	{
		RequireManual();
		ApplyBehavior(MediaState.Play);
	}

	/// <summary>Pauses playback. Only valid when <see cref="LoadedBehavior"/> is Manual.</summary>
	public void Pause()
	{
		RequireManual();
		ApplyBehavior(MediaState.Pause);
	}

	/// <summary>Stops playback and returns to the start. Only valid when <see cref="LoadedBehavior"/> is Manual.</summary>
	public void Stop()
	{
		RequireManual();
		ApplyBehavior(MediaState.Stop);
	}

	/// <summary>Releases the media. Only valid when <see cref="LoadedBehavior"/> is Manual.</summary>
	public void Close()
	{
		RequireManual();
		ApplyBehavior(MediaState.Close);
	}

	void RequireManual()
	{
		if (LoadedBehavior != MediaState.Manual)
			throw new InvalidOperationException("The media can only be controlled when LoadedBehavior is Manual.");
		if (_clock != null)
			throw new InvalidOperationException("The media cannot be controlled while a Clock is set.");
	}

	void SetPosition(TimeSpan value)
	{
		if (_clock != null)
			throw new InvalidOperationException("The position cannot be set while a Clock is set.");
		_position = value;
		SeekStream(value);
	}

	void SetClock(MediaClock? value)
	{
		if (_clock != null)
		{
			_clock.CurrentStateInvalidated -= OnClockChanged;
			_clock.CurrentGlobalSpeedInvalidated -= OnClockChanged;
		}
		_clock = value;
		if (value != null)
		{
			value.CurrentStateInvalidated += OnClockChanged;
			value.CurrentGlobalSpeedInvalidated += OnClockChanged;
		}
		OnSourceChanged();
	}

	void OnClockChanged(object? sender, EventArgs e)
	{
		if (_clock == null) return;
		var state = _clock.CurrentState;
		if (state == ClockState.Active && !_clock.IsPaused) ApplyBehavior(MediaState.Play);
		else if (state == ClockState.Stopped) ApplyBehavior(MediaState.Stop);
		else ApplyBehavior(MediaState.Pause);
	}
}
