using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Provides methods to navigate the WPF visual tree.
/// Mirrors <c>System.Windows.Media.VisualTreeHelper</c>.
/// </summary>
/// <remarks>
/// Navigation is built on <see cref="Visual.VisualChildrenCount"/>,
/// <see cref="Visual.GetVisualChild"/> and <see cref="Visual.VisualParent"/>, which
/// <see cref="UIElement"/> wires to the existing <c>VisualChildren</c> / <c>LogicalParent</c>
/// infrastructure.
/// </remarks>
public static partial class VisualTreeHelper
{
	// ── Child access ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the number of direct visual children of <paramref name="reference"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="ArgumentException">
	/// Thrown when <paramref name="reference"/> is not a <see cref="Visual"/>.
	/// </exception>
	public static int GetChildrenCount(DependencyObject reference)
	{
		if (reference == null) throw new ArgumentNullException(nameof(reference));
		if (reference is Visual v) return v.VisualChildrenCount;
		throw new ArgumentException(
			$"'{reference.GetType().FullName}' is not a Visual.", nameof(reference));
	}

	/// <summary>
	/// Returns the visual child at <paramref name="childIndex"/> of
	/// <paramref name="reference"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="ArgumentException">
	/// Thrown when <paramref name="reference"/> is not a <see cref="Visual"/>.
	/// </exception>
	public static DependencyObject GetChild(DependencyObject reference, int childIndex)
	{
		if (reference == null) throw new ArgumentNullException(nameof(reference));
		if (reference is Visual v) return v.GetVisualChild(childIndex);
		throw new ArgumentException(
			$"'{reference.GetType().FullName}' is not a Visual.", nameof(reference));
	}

	// ── Parent access ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the visual parent of <paramref name="reference"/>, or <c>null</c>
	/// when it has no visual parent.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public static DependencyObject? GetParent(DependencyObject reference)
	{
		if (reference == null) throw new ArgumentNullException(nameof(reference));
		if (reference is Visual v) return v.VisualParent;
		return null;
	}

}

/// <summary>Result of a hit test. Mirrors <c>HitTestResult</c>.</summary>
public abstract class HitTestResult
{
	/// <summary>The visual that was hit.</summary>
	public Visual VisualHit { get; }

	/// <summary>Creates a hit-test result for the given visual.</summary>
	protected internal HitTestResult(Visual visualHit) => VisualHit = visualHit;
}
