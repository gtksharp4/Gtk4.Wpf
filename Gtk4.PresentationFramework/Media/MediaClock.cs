using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>
/// The <see cref="Clock"/> of a <see cref="MediaTimeline"/>. A <see cref="Controls.MediaElement"/> that has this
/// clock plays while the clock is active and reports the length of the opened media in <see cref="NaturalDuration"/>.
/// </summary>
public class MediaClock : Clock
{
	Duration _naturalDuration = Duration.Automatic;

	protected internal MediaClock(MediaTimeline media) : base(media)
	{
	}

	/// <summary>The media timeline this clock runs.</summary>
	public new MediaTimeline Timeline => (MediaTimeline)base.Timeline;

	/// <summary>The length of the opened media; Automatic until the media opens.</summary>
	public Duration NaturalDuration => _naturalDuration;

	internal void SetNaturalDuration(Duration duration) => _naturalDuration = duration;
}
