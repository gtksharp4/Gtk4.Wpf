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

	Uri? ActiveBase() => _clock != null ? _clock.Timeline.BaseUri : BaseUri;

	void OpenStream()
	{
		if (_stream != null || _picture == null) return;
		var uri = ActiveSource();
		if (uri == null) return;
		try
		{
			var baseUri = ActiveBase();
			if (!uri.IsAbsoluteUri && baseUri != null && baseUri.IsAbsoluteUri)
				uri = new Uri(baseUri, uri);
			var stream = CreateStream(uri);
			if (stream == null) return;
			_stream = stream;
			_prepared = false;
			_picture.Paintable = stream;
			ApplyAudio();
			stream.AddNotification("prepared", (_, _) => Guard(() => OnPrepared(stream)));
			stream.AddNotification("error", (_, _) => Guard(() => OnError(stream)));
			stream.AddNotification("ended", (_, _) => Guard(() => OnEnded(stream)));
			if (stream.Error != IntPtr.Zero) OnError(stream);
			else if (stream.IsPrepared) OnPrepared(stream);
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	Gtk.MediaFile? CreateStream(Uri uri)
	{
		if (uri.IsAbsoluteUri && !uri.IsFile && !string.Equals(uri.Scheme, "pack", StringComparison.OrdinalIgnoreCase))
			return new Gtk.MediaFile(GLib.FileFactory.NewForUri(uri));
		var path = ImageSourceLocation.ResolveUri(uri);
		if (!File.Exists(path))
		{
			Fail(new FileNotFoundException("The media file was not found.", path));
			return null;
		}
		return new Gtk.MediaFile(path);
	}

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

	void OnError(Gtk.MediaFile stream)
	{
		if (!ReferenceEquals(stream, _stream) || stream.Error == IntPtr.Zero || _failed) return;
		Fail(new InvalidOperationException(ReadError(stream.Error)));
	}

	void OnEnded(Gtk.MediaFile stream)
	{
		if (ReferenceEquals(stream, _stream) && stream.Ended) RaiseEnded();
	}

	// MediaFailed is asynchronous, as in WPF, so handlers attached after Source is set still see it.
	void Fail(Exception error)
	{
		_failed = true;
		Dispatcher.BeginInvoke(new Action(() => RaiseFailed(error)));
	}

	// An exception must never escape a GTK callback.
	void Guard(Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			try { RaiseFailed(ex); }
			catch { }
		}
	}

	// GError layout: guint domain, gint code, gchar* message.
	static string ReadError(IntPtr error)
	{
		var message = Marshal.ReadIntPtr(error, 8);
		return (message == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(message)) ?? "The media could not be played.";
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
