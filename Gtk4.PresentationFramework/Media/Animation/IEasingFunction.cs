namespace System.Windows.Media.Animation;

/// <summary>
/// Provides a custom easing function for animation progress.
/// Mirrors WPF's <c>System.Windows.Media.Animation.IEasingFunction</c>.
/// </summary>
public interface IEasingFunction
{
	/// <summary>
	/// Transforms the linear normalised progress <paramref name="normalizedTime"/> in [0,1]
	/// into the eased progress, also expected in [0,1].
	/// </summary>
	double Ease(double normalizedTime);
}