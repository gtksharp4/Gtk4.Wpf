namespace System.Windows;

/// <summary>
/// Partial extension of <see cref="FrameworkElement"/> that exposes the
/// <see cref="TemplatedParent"/> property: the <see cref="Controls.Control"/> (or any
/// <see cref="FrameworkElement"/>) that instantiated the <see cref="Controls.ControlTemplate"/>
/// this element belongs to.
/// <para>
/// Set internally by <see cref="Controls.Control"/> when it applies its
/// <see cref="Controls.Control.Template"/>. Read by <see cref="Data.RelativeSource"/>
/// when <c>Mode = TemplatedParent</c> to resolve the binding source.
/// The full visual subtree produced by the factory is stamped recursively via
/// <c>StampTemplatedParent</c>, and all <c>RelativeSource=TemplatedParent</c>
/// bindings on the subtree are re-activated afterwards via
/// <c>BindingEngine.RefreshTemplatedParentBindings</c> so they resolve correctly
/// even when the binding was set inside the factory lambda (before <c>TemplatedParent</c>
/// was assigned).
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