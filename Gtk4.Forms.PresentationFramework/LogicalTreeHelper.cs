using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>Re-reads <see cref="FrameworkElement.LogicalChildren"/> on every enumeration.</summary>
sealed class LogicalChildrenEnumerable : IEnumerable
{
	readonly FrameworkElement _owner;

	internal LogicalChildrenEnumerable(FrameworkElement owner) => _owner = owner;

	public IEnumerator GetEnumerator() => _owner.LogicalChildren ?? Array.Empty<object>().GetEnumerator();
}

/// <summary>
/// Provides methods to navigate the WPF logical tree.
/// Mirrors <c>System.Windows.LogicalTreeHelper</c>.
/// </summary>
public static class LogicalTreeHelper
{
	// ── Parent ────────────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the logical parent of <paramref name="current"/>, or <c>null</c>
	/// when it has no logical parent.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public static DependencyObject? GetParent(DependencyObject current)
	{
		if (current == null) throw new ArgumentNullException(nameof(current));
		if (current is UIElement el) return (DependencyObject?)el.InlineContainer ?? el.LogicalParent;
		if (current is FrameworkContentElement fce) return fce.Parent;
		return null;
	}

	// ── Children ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the direct logical children of <paramref name="current"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public static IEnumerable GetChildren(DependencyObject current)
	{
		if (current == null) throw new ArgumentNullException(nameof(current));
		if (current is FrameworkElement fe) return new LogicalChildrenEnumerable(fe);
		if (current is Documents.InlineUIContainer { Child: { } hosted }) return new object[] { hosted };
		if (current is Documents.Span span) return span.Inlines;
		return Array.Empty<object>();
	}

	// ── FindLogicalNode ───────────────────────────────────────────────────────────

	/// <summary>
	/// Searches the logical subtree rooted at <paramref name="logicalTreeNode"/> for
	/// an element whose <c>Name</c> matches <paramref name="elementName"/>.
	/// Returns <c>null</c> when not found.
	/// </summary>
	public static DependencyObject? FindLogicalNode(
		DependencyObject logicalTreeNode,
		string elementName)
	{
		if (logicalTreeNode == null) throw new ArgumentNullException(nameof(logicalTreeNode));
		if (elementName == null) throw new ArgumentNullException(nameof(elementName));

		// Check this node.
		if (logicalTreeNode is FrameworkElement fe && fe.Name == elementName)
			return fe;

		// Recurse into children.
		foreach (var child in GetChildren(logicalTreeNode))
		{
			if (child is DependencyObject dobj)
			{
				var found = FindLogicalNode(dobj, elementName);
				if (found != null) return found;
			}
		}
		return null;
	}

	// ── BringIntoView ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Requests that the element be scrolled into view.
	/// Not implemented in this GTK port (stub only).
	/// </summary>
	public static void BringIntoView(DependencyObject current)
	{
		// Phase-2: delegate to a ScrollViewer ancestor's BringIntoView.
		// No-op in Phase-1.
	}
}
