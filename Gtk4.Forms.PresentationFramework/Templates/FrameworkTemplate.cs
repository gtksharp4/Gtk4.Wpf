namespace System.Windows;

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
	/// Gets whether the template is sealed. Store-only: WPF also locks a sealed template against modification,
	/// which this layer does not enforce.
	/// </summary>
	public bool IsSealed { get; private set; }

	/// <summary>Seals the template (see <see cref="IsSealed"/>; the lock itself is not enforced).</summary>
	public void Seal() => IsSealed = true;

	/// <summary>
	/// Creates a new instance of the visual tree defined by this template.
	/// Returns <c>null</c> when no <see cref="Factory"/> has been set.
	/// </summary>
	public UIElement? CreateVisualTree() => Factory?.Invoke();

	/// <summary>
	/// Returns <c>true</c> when the template has a factory and can produce a tree.
	/// </summary>
	public virtual bool HasContent => Factory != null;

	/// <summary>
	/// Returns the element named <paramref name="name"/> in the template instance applied to
	/// <paramref name="templatedParent"/>, or null.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="templatedParent"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The template was not applied to the parent.</exception>
	public object? FindName(string name, FrameworkElement templatedParent)
	{
		if (templatedParent == null) throw new ArgumentNullException(nameof(templatedParent));
		if (!ReferenceEquals(templatedParent.TemplateInternal, this))
			throw new InvalidOperationException("The template was not applied to the given templatedParent.");
		var root = templatedParent.TemplateRootInternal;
		return root == null ? null : FrameworkElement.FindInTemplate(root, name, templatedParent);
	}
}