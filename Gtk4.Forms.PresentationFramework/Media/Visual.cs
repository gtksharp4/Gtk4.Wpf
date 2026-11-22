using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Base class for objects that have visual rendering in the WPF tree.
/// Mirrors <c>System.Windows.Media.Visual</c>.
/// </summary>
/// <remarks>
/// In this implementation <see cref="Visual"/> is a thin wrapper over the existing
/// <see cref="UIElement"/> visual-child / logical-parent infrastructure. The concrete
/// <see cref="UIElement"/> class already holds <c>VisualChildren</c> and
/// <c>LogicalParent</c>; <see cref="Visual"/> exposes those through the WPF-standard
/// protected API so that <see cref="VisualTreeHelper"/> can walk them generically.
/// </remarks>
public abstract partial class Visual : DependencyObject
{
	// ── Visual parent ────────────────────────────────────────────────────────────

	/// <summary>
	/// The visual parent of this element, or <c>null</c> when this is a root.
	/// Set by the parent when it adopts this element as a visual child.
	/// </summary>
	protected internal Visual? VisualParent { get; internal set; }

	// ── Visual children ──────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the number of direct visual children.
	/// Override in concrete types that host children (e.g. panels, content controls).
	/// </summary>
	protected internal virtual int VisualChildrenCount => 0;

	/// <summary>
	/// Returns the visual child at <paramref name="index"/>.
	/// Override in concrete types that host children.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"/>
	protected internal virtual Visual GetVisualChild(int index) =>
		throw new ArgumentOutOfRangeException(nameof(index));
}
