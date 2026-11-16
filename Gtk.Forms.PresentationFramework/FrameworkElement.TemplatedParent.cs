namespace System.Windows
{
	/// <summary>
	/// Partial extension of <see cref="FrameworkElement"/> that exposes the
	/// <see cref="TemplatedParent"/> property: the <see cref="Controls.Control"/> (or any
	/// <see cref="FrameworkElement"/>) that instantiated the <see cref="Controls.ControlTemplate"/>
	/// this element belongs to.
	/// <para>
	/// Set internally by <see cref="Controls.Control"/> when it applies its
	/// <see cref="Controls.Control.Template"/>. Read by <see cref="Data.RelativeSource"/>
	/// when <c>Mode = TemplatedParent</c> to resolve the binding source.
	/// Only the visual-tree root produced by the factory is stamped; child elements further
	/// down the factory tree are reachable via their own <c>TemplatedParent</c> (stamped
	/// when each sub-element is reparented into the same template scope, though the full
	/// subtree walk is not implemented — documented limitation: only the factory root is
	/// guaranteed to have this set).
	/// </para>
	/// </summary>
	public abstract partial class FrameworkElement
	{
		/// <summary>
		/// The control that instantiated the <see cref="Controls.ControlTemplate"/> this
		/// element is part of. <c>null</c> when this element is not inside a template.
		/// </summary>
		public FrameworkElement? TemplatedParent { get; internal set; }
	}
}
