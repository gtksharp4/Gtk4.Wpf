namespace System.Windows.Media.Animation;

/// <summary>
/// A target path step reached an object that is not a <see cref="DependencyObject"/> in this layer
/// (for example <c>GradientStop</c>). The storyboard skips that animation instead of failing the whole start.
/// </summary>
internal sealed class UnanimatableTargetException : InvalidOperationException
{
	internal UnanimatableTargetException(string message) : base(message)
	{
	}
}
