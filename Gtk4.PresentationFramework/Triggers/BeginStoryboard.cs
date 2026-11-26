using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>
/// A <see cref="TriggerAction"/> that starts a <see cref="Storyboard"/> on the
/// element that owns the trigger.  Mirrors WPF's
/// <c>System.Windows.Media.Animation.BeginStoryboard</c>.
/// </summary>
public class BeginStoryboard : TriggerAction
{
	/// <summary>The storyboard to start when this action is invoked.</summary>
	public Storyboard? Storyboard { get; set; }

	/// <inheritdoc/>
	internal override void Invoke(FrameworkElement target)
	{
		if (Storyboard == null) return;
		Storyboard.Begin(target);
	}
}