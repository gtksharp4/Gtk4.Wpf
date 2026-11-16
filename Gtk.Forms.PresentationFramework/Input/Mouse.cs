namespace System.Windows.Input
{
	/// <summary>
	/// Provides mouse state services. State is updated by GTK gesture controllers.
	/// </summary>
	public static class Mouse
	{
		/// <summary>Current state of the left mouse button.</summary>
		public static MouseButtonState LeftButton { get; internal set; } = MouseButtonState.Released;

		/// <summary>Current state of the right mouse button.</summary>
		public static MouseButtonState RightButton { get; internal set; } = MouseButtonState.Released;

		/// <summary>
		/// Returns the mouse position relative to <paramref name="relativeTo"/>.
		/// Returns <see cref="System.Windows.Point"/> origin when tracking is unavailable.
		/// </summary>
		public static System.Windows.Point GetPosition(System.Windows.UIElement? relativeTo)
		{
			// GTK does not give a global cursor position without a GDK device query on
			// the GTK thread.  Return the last tracked position from the motion controller.
			return _lastPosition;
		}

		internal static System.Windows.Point _lastPosition;
	}
}
