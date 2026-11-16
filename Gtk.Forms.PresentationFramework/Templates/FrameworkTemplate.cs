namespace System.Windows
{
	/// <summary>
	/// Abstract base for WPF templates (<see cref="Controls.ControlTemplate"/>,
	/// <see cref="Controls.DataTemplate"/>). A template is a factory that produces a
	/// <see cref="UIElement"/> tree; the XAML compiler fills this via
	/// <see cref="VisualTree"/> (a later round) or callers set <see cref="Factory"/>
	/// directly (Phase-2 programmatic API).
	/// </summary>
	public abstract class FrameworkTemplate
	{
		/// <summary>
		/// A factory that creates the root <see cref="UIElement"/> of the template's
		/// visual tree. Set programmatically; XAML compilation support is a later round.
		/// </summary>
		public Func<UIElement>? Factory { get; set; }

		/// <summary>
		/// Creates a new instance of the visual tree defined by this template.
		/// Returns <c>null</c> when no <see cref="Factory"/> has been set.
		/// </summary>
		public UIElement? CreateVisualTree() => Factory?.Invoke();

		/// <summary>
		/// Returns <c>true</c> when the template has a factory and can produce a tree.
		/// </summary>
		public virtual bool HasContent => Factory != null;
	}
}
