using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Represents a named state within a <see cref="VisualStateGroup"/>.
	/// Setter-based state application is supported; storyboard application is deferred
	/// to the animation track (a future round).
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
		/// Storyboard for animated transitions. Stored but not applied until the animation
		/// track is wired — a seam for a future round. Set to any object to record the value.
		/// </summary>
		public object? Storyboard { get; set; }
	}
}
