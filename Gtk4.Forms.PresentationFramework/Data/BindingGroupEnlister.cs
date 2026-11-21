using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// Keeps <see cref="BindingGroup.BindingExpressions"/> in step with the tree: an element's group collects the
/// binding expressions of the element and its descendants (nearest group wins) whose
/// <see cref="BindingBase.BindingGroupName"/> is empty or equals the group's name.
/// </summary>
internal static class BindingGroupEnlister
{
	static BindingGroup? FindGroup(UIElement? start)
	{
		for (var e = start; e != null; e = e.LogicalParent)
			if (e is FrameworkElement fe && fe.BindingGroup is { } group) return group;
		return null;
	}

	static bool Joins(BindingGroup group, BindingExpression expression)
	{
		string name = expression.ParentBinding.BindingGroupName;
		return name.Length == 0 || name == group.Name;
	}

	/// <summary>Enlists a binding expression just created on <paramref name="target"/>.</summary>
	internal static void Enlist(FrameworkElement target, BindingExpressionBase expression)
	{
		if (expression is not BindingExpression simple || FindGroup(target) is not { } group) return;
		if (Joins(group, simple) && !group.BindingExpressions.Contains(simple)) group.BindingExpressions.Add(simple);
	}

	/// <summary>Removes <paramref name="expression"/> from the group above <paramref name="target"/>.</summary>
	internal static void Withdraw(FrameworkElement target, BindingExpressionBase expression)
	{
		if (expression is BindingExpression simple && FindGroup(target) is { } group)
			group.BindingExpressions.Remove(simple);
	}

	/// <summary>Enlists the expressions of <paramref name="root"/> and its descendants into the group above.</summary>
	internal static void EnlistSubtree(UIElement root, BindingGroup? group = null)
	{
		group ??= FindGroup(root);
		if (group == null) return;
		foreach (var expression in SubtreeExpressions(root, true))
			if (Joins(group, expression) && !group.BindingExpressions.Contains(expression))
				group.BindingExpressions.Add(expression);
	}

	/// <summary>Removes the subtree's expressions from <paramref name="group"/>.</summary>
	internal static void WithdrawSubtree(UIElement root, BindingGroup group)
	{
		foreach (var expression in SubtreeExpressions(root, true))
			group.BindingExpressions.Remove(expression);
	}

	// Expressions of the subtree, not descending into an element that owns a group of its own.
	static List<BindingExpression> SubtreeExpressions(UIElement root, bool isRoot)
	{
		var found = new List<BindingExpression>();
		if (!isRoot && root is FrameworkElement { BindingGroup: not null }) return found;
		if (root is FrameworkElement fe)
			foreach (var expression in BindingEngine.ExpressionsOf(fe))
				if (expression is BindingExpression simple) found.Add(simple);
		foreach (var child in LogicalTreeHelper.GetChildren(root))
			if (child is UIElement element) found.AddRange(SubtreeExpressions(element, false));
		return found;
	}
}
