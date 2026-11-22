namespace System.Windows.Controls;

/// <summary>
/// Defines the visual structure of a <see cref="Control"/> via a factory that
/// produces a <see cref="UIElement"/> tree.
/// <para>
/// Set <see cref="FrameworkTemplate.Factory"/> programmatically. XAML
/// <c>&lt;ControlTemplate&gt;</c> parsing is a later compiler round.
/// </para>
/// </summary>
public partial class ControlTemplate : FrameworkTemplate
{
	/// <summary>
	/// The type of control this template is intended for.
	/// Optional in Phase 2; used for validation in future rounds.
	/// </summary>
	public Type? TargetType { get; set; }

	/// <summary>Initialises a template with no target-type constraint.</summary>
	public ControlTemplate() { }

	/// <summary>Initialises a template for a specific control type.</summary>
	public ControlTemplate(Type? targetType)
	{
		TargetType = targetType;
	}
}