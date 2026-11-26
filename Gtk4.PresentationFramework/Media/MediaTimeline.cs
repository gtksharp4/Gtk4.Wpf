using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>
/// A <see cref="Timeline"/> that describes a media file. <see cref="AllocateClock"/> returns a
/// <see cref="MediaClock"/> that a <see cref="Controls.MediaElement"/> follows when it is set as its Clock.
/// </summary>
public class MediaTimeline : Timeline, IUriContext
{
	/// <summary>Identifies <see cref="Source"/>.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(Uri), typeof(MediaTimeline), new PropertyMetadata(null));

	/// <summary>Creates a timeline without a source.</summary>
	public MediaTimeline()
	{
	}

	/// <summary>Creates a timeline with a begin time.</summary>
	public MediaTimeline(TimeSpan beginTime) => BeginTime = beginTime;

	/// <summary>Creates a timeline with a begin time and duration.</summary>
	public MediaTimeline(TimeSpan beginTime, Duration duration) : this(beginTime) => Duration = duration;

	/// <summary>Creates a timeline with a begin time, duration and repeat behavior.</summary>
	public MediaTimeline(TimeSpan beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: this(beginTime, duration) => RepeatBehavior = repeatBehavior;

	/// <summary>Creates a timeline for <paramref name="source"/>.</summary>
	public MediaTimeline(Uri source) => Source = source;

	/// <summary>Creates a timeline for <paramref name="source"/>, resolved against <paramref name="baseUri"/>.</summary>
	public MediaTimeline(Uri baseUri, Uri source)
	{
		_baseUri = baseUri;
		Source = source;
	}

	/// <summary>Gets or sets the media file.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	Uri? _baseUri;

	/// <summary>The base URI that a relative <see cref="Source"/> is resolved against.</summary>
	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	internal Uri? BaseUriCore => _baseUri;

	/// <summary>Creates the <see cref="MediaClock"/> for this timeline.</summary>
	protected internal override Clock AllocateClock()
	{
		if (Source == null) throw new InvalidOperationException("Must specify URI.");
		return new MediaClock(this);
	}

	// The span of the media is unknown until it opens, so an automatic duration lasts until the clock stops.
	internal override Duration GetAutomaticDuration() => Duration.Forever;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MediaTimeline();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		((MediaTimeline)destination)._baseUri = _baseUri;
	}
}
