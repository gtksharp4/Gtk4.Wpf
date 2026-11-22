using System.Windows.Media.Animation;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>Starts <paramref name="storyboard"/> on this element.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="storyboard"/> is null.</exception>
	public void BeginStoryboard(Storyboard storyboard) =>
		BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, false);

	/// <summary>Starts <paramref name="storyboard"/> with the given hand-off behavior.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="storyboard"/> is null.</exception>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior) =>
		BeginStoryboard(storyboard, handoffBehavior, false);

	/// <summary>Starts <paramref name="storyboard"/>; <paramref name="isControllable"/> allows Pause/Resume.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="storyboard"/> is null.</exception>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
	{
		if (storyboard == null) throw new ArgumentNullException(nameof(storyboard));
		storyboard.Begin(this, handoffBehavior, isControllable);
	}
}
