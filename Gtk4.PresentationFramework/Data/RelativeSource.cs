using System.Windows.Controls;

namespace System.Windows.Data;

/// <summary>Specifies the source of a binding relative to the target element.</summary>
public enum RelativeSourceMode
{
	/// <summary>The binding source is the element itself.</summary>
	Self,
	/// <summary>Walk the logical tree for the nearest ancestor of a given type.</summary>
	FindAncestor,
	/// <summary>The element that applied the template containing the bound element.</summary>
	TemplatedParent,
	/// <summary>The item before the current item in a data-bound collection.</summary>
	PreviousData,
}

/// <summary>
/// Describes the source of a <see cref="Binding"/> relative to the binding target.
/// Supports <see cref="RelativeSourceMode.Self"/>, <see cref="RelativeSourceMode.FindAncestor"/>
/// (with <see cref="AncestorType"/> and <see cref="AncestorLevel"/>),
/// <see cref="RelativeSourceMode.TemplatedParent"/>, and
/// <see cref="RelativeSourceMode.PreviousData"/>.
/// </summary>
public sealed class RelativeSource
{
	/// <summary>Mode used by this instance.</summary>
	public RelativeSourceMode Mode { get; }

	/// <summary>
	/// For <see cref="RelativeSourceMode.FindAncestor"/>: the type to search for.
	/// </summary>
	public Type? AncestorType { get; set; }

	/// <summary>
	/// For <see cref="RelativeSourceMode.FindAncestor"/>: 1-based level among matching
	/// ancestors (1 = nearest, default).
	/// </summary>
	public int AncestorLevel { get; set; } = 1;

	/// <summary>Creates a <see cref="RelativeSource"/> with the given mode.</summary>
	public RelativeSource(RelativeSourceMode mode) { Mode = mode; }

	/// <summary>
	/// Creates a <see cref="RelativeSourceMode.FindAncestor"/> source for the given type.
	/// </summary>
	public RelativeSource(RelativeSourceMode mode, Type ancestorType, int ancestorLevel)
	{
		Mode = mode;
		AncestorType = ancestorType;
		AncestorLevel = ancestorLevel;
	}

	/// <summary>Singleton for <see cref="RelativeSourceMode.Self"/>.</summary>
	public static readonly RelativeSource Self =
		new RelativeSource(RelativeSourceMode.Self);

	/// <summary>Singleton for <see cref="RelativeSourceMode.TemplatedParent"/>.</summary>
	public static readonly RelativeSource TemplatedParent =
		new RelativeSource(RelativeSourceMode.TemplatedParent);

	/// <summary>Singleton for <see cref="RelativeSourceMode.PreviousData"/>.</summary>
	public static readonly RelativeSource PreviousData =
		new RelativeSource(RelativeSourceMode.PreviousData);

	/// <summary>
	/// Resolves the source object for a binding given the binding target element.
	/// Returns <c>null</c> when the source cannot be resolved.
	/// </summary>
	internal object? Resolve(FrameworkElement target)
	{
		switch (Mode)
		{
			case RelativeSourceMode.Self:
				return target;

			case RelativeSourceMode.FindAncestor:
				return FindAncestor(target);

			case RelativeSourceMode.TemplatedParent:
				return target.TemplatedParent;

			case RelativeSourceMode.PreviousData:
				return ResolvePreviousData(target);

			default:
				return null;
		}
	}

	/// <summary>
	/// Finds the data item immediately before the item whose container is
	/// <paramref name="target"/> inside a parent <c>ItemsControl</c>.
	/// <para>
	/// Limitation: resolution depends on the target's <c>DataContext</c> matching
	/// an item in the nearest <c>ItemsControl</c> ancestor's <c>Items</c> list.
	/// If the item container uses a template with an intermediate host element,
	/// the DataContext walk may not locate the item and returns <c>null</c>.
	/// </para>
	/// </summary>
	static object? ResolvePreviousData(FrameworkElement target)
	{
		// Walk to the nearest ItemsControl ancestor.
		DependencyObject? current =
			(target as UIElement)?.LogicalParent ?? target.InheritanceParent;

		while (current != null)
		{
			if (current is System.Windows.Controls.ItemsControl ic)
			{
				var dataItem = target.DataContext;
				if (dataItem == null) return null;

				// Find the index of the current item in the flat Items list.
				var items = ic.Items;
				for (int i = 1; i < items.Count; i++)
				{
					if (ReferenceEquals(items[i], dataItem))
						return items[i - 1];
				}
				return null;
			}
			current = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
		}
		return null;
	}

	/// <summary>Walks the logical/inheritance parents of any target (element or content element).</summary>
	internal object? FindAncestor(DependencyObject target)
	{
		if (AncestorType == null) return null;

		int level = AncestorLevel < 1 ? 1 : AncestorLevel;
		int found = 0;

		DependencyObject? current =
			(target as UIElement)?.LogicalParent ?? target.InheritanceParent;

		while (current != null)
		{
			if (AncestorType.IsInstanceOfType(current))
			{
				found++;
				if (found >= level) return current;
			}
			current = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
		}
		return null;
	}
}