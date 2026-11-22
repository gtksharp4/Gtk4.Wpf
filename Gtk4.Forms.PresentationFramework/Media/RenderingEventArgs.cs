namespace System.Windows.Media;

/// <summary>Arguments for <see cref="CompositionTarget.Rendering"/>.</summary>
public class RenderingEventArgs : EventArgs
{
	internal RenderingEventArgs(TimeSpan renderingTime) => RenderingTime = renderingTime;

	/// <summary>Monotonic time of the frame being rendered.</summary>
	public TimeSpan RenderingTime { get; }
}
