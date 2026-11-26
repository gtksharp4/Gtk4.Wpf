using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>
/// Represents a named state within a <see cref="VisualStateGroup"/>.
/// Setter-based and storyboard-based state transitions are both supported.
/// </summary>
public class VisualState
{
	/// <summary>The name of this state (e.g. <c>"Normal"</c>, <c>"Focused"</c>).</summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Optional setters applied when this state becomes active.
	/// Each setter is applied to the <see cref="Controls.Control"/> passed to
	/// <see cref="VisualStateManager.GoToState"/>.
	/// </summary>
	public IList<Setter> Setters { get; } = new List<Setter>();

	/// <summary>
	/// Optional storyboard started when this state becomes active and stopped when
	/// the state is exited.  Null means no animation.
	/// </summary>
	public Storyboard? Storyboard { get; set; }
}