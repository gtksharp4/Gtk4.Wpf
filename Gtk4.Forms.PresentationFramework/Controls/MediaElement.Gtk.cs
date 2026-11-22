using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Controls;

public partial class MediaElement
{
	const long TicksPerMicrosecond = 10;

	Gtk.Picture? _picture;
	Gtk.MediaFile? _stream;
	bool _prepared;
	bool _failed;
	bool _wantPlay;

	/// <summary>Creates a media element that plays its <see cref="Source"/> when loaded.</summary>
	public MediaElement()
	{
		Loaded += (_, _) => { if (_clock == null) ApplyBehavior(LoadedBehavior); };
		Unloaded += (_, _) => { if (_clock == null) ApplyBehavior(UnloadedBehavior); };
	}

	internal Gtk.MediaFile? Stream => _stream;

	bool IsOpened => _stream != null && _prepared;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_picture = new Gtk.Picture();
		ApplyStretch();
		OpenStream();
		return _picture;
	}

	void OnSourceChanged()
	{
		CloseStream();
		_failed = false;
		if (!IsWidgetCreated) return;
		OpenStream();
		if (_clock == null && IsLoaded) ApplyBehavior(LoadedBehavior);
	}

	void ApplyStretch()
	{
		if (_picture == null) return;
		_picture.ContentFit = ImageStretchMap.ToContentFit(Stretch);
		_picture.CanShrink = Stretch != Stretch.None;
	}

	void ApplyAudio()
	{
		if (_stream == null) return;
		_stream.Volume = Math.Max(0.0, Math.Min(1.0, Volume));
		_stream.Muted = IsMuted;
	}

	void ApplyBehavior(MediaState state)
	{
		switch (state)
		{
			case MediaState.Play:
				_ = Widget;
				if (_stream == null && !_failed) OpenStream();
				_wantPlay = true;
				if (IsOpened) _stream!.Play();
				break;
			case MediaState.Pause:
				_wantPlay = false;
				if (IsOpened) _stream!.Pause();
				break;
			case MediaState.Stop:
				_wantPlay = false;
				_position = TimeSpan.Zero;
				if (IsOpened)
				{
					_stream!.Pause();
					SeekStream(TimeSpan.Zero);
				}
				break;
			case MediaState.Close:
				_wantPlay = false;
				CloseStream();
				break;
		}
	}

	Uri? ActiveSource() => _clock != null ? _clock.Timeline.Source : Source;

	Uri? ActiveBase() => _clock != null ? _clock.Timeline.BaseUriCore : _baseUri;

	void CloseStream()
	{
		var stream = _stream;
		_stream = null;
		_prepared = false;
		if (stream == null) return;
		if (_picture != null) _picture.Paintable = null;
		stream.Pause();
		stream.Clear();
	}

	void OnPrepared(Gtk.MediaFile stream)
	{
		if (!ReferenceEquals(stream, _stream) || _prepared) return;
		_prepared = true;
		_clock?.SetNaturalDuration(NaturalDurationCore());
		if (_position > TimeSpan.Zero) SeekStream(_position);
		if (_wantPlay) stream.Play();
		RaiseOpened();
	}

	void OnEnded(Gtk.MediaFile stream)
	{
		if (ReferenceEquals(stream, _stream) && stream.Ended) RaiseEnded();
	}

	void SeekStream(TimeSpan value)
	{
		if (IsOpened && _stream!.Seekable)
			_stream.Seek(value.Ticks / TicksPerMicrosecond);
	}

	TimeSpan PositionCore() =>
		IsOpened ? TimeSpan.FromTicks(_stream!.Timestamp * TicksPerMicrosecond) : _position;

	Duration NaturalDurationCore() =>
		IsOpened && _stream!.Duration > 0
			? new Duration(TimeSpan.FromTicks(_stream.Duration * TicksPerMicrosecond))
			: Duration.Automatic;

	int NaturalVideoWidthCore() => IsOpened ? _stream!.IntrinsicWidth : 0;

	int NaturalVideoHeightCore() => IsOpened ? _stream!.IntrinsicHeight : 0;

	bool HasAudioCore() => IsOpened && _stream!.HasAudio;

	bool HasVideoCore() => IsOpened && _stream!.HasVideo;
}
