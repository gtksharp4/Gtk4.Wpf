using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// Keeps <see cref="BindingGroup.BindingExpressions"/> in step with the tree. A binding with a
/// <see cref="BindingBase.BindingGroupName"/> joins the nearest ancestor group of that name. One without a name
/// joins the nearest group, provided it reads the group owner's DataContext (no Source, ElementName or
/// RelativeSource, and the same DataContext as the owner).
/// </summary>
internal static class BindingGroupEnlister
{
	static BindingGroup? Resolve(FrameworkElement? target, BindingExpression expression)
	{
		string name = expression.ParentBinding.BindingGroupName;
		for (UIElement? e = target; e != null; e = e.LogicalParent)
		{
			if (e is not FrameworkElement { BindingGroup: { } group } owner) continue;
			if (name.Length > 0)
			{
				if (name == group.Name) return group;
				continue;
			}
			return UsesOwnerDataContext(owner, target!, expression.ParentBinding) ? group : null;
		}
		return null;
	}

	static bool UsesOwnerDataContext(FrameworkElement owner, FrameworkElement target, Binding binding) =>
		binding.Source == null && binding.ElementName == null && binding.RelativeSource == null
		&& Equals(owner.DataContext, target.DataContext);

	static List<BindingGroup> GroupsAbove(UIElement? start)
	{
		var groups = new List<BindingGroup>();
		for (var e = start; e != null; e = e.LogicalParent)
			if (e is FrameworkElement fe && fe.BindingGroup is { } group) groups.Add(group);
		return groups;
	}

	static void Place(FrameworkElement target, BindingExpression expression)
	{
		var home = Resolve(target, expression);
		foreach (var group in GroupsAbove(target))
		{
			if (ReferenceEquals(group, home)) continue;
			group.BindingExpressions.Remove(expression);
		}
		if (home != null && !home.BindingExpressions.Contains(expression)) home.BindingExpressions.Add(expression);
	}

	/// <summary>Enlists a binding expression just created on <paramref name="target"/>.</summary>
	internal static void Enlist(FrameworkElement target, BindingExpressionBase expression)
	{
		if (expression is BindingExpression simple) Place(target, simple);
	}

	/// <summary>Removes <paramref name="expression"/> from every group above <paramref name="target"/>.</summary>
	internal static void Withdraw(FrameworkElement target, BindingExpressionBase expression)
	{
		if (expression is not BindingExpression simple) return;
		foreach (var group in GroupsAbove(target)) group.BindingExpressions.Remove(simple);
	}

	/// <summary>Places the subtree's expressions into the groups above <paramref name="root"/>.</summary>
	internal static void EnlistSubtree(UIElement root)
	{
		foreach (var (element, expression) in SubtreeExpressions(root)) Place(element, expression);
	}

	/// <summary>Removes the subtree's expressions from <paramref name="group"/>.</summary>
	internal static void WithdrawSubtree(UIElement root, BindingGroup group)
	{
		foreach (var (_, expression) in SubtreeExpressions(root)) group.BindingExpressions.Remove(expression);
	}

	static List<(FrameworkElement, BindingExpression)> SubtreeExpressions(UIElement root)
	{
		var found = new List<(FrameworkElement, BindingExpression)>();
		if (root is FrameworkElement fe)
			foreach (var expression in BindingEngine.ExpressionsOf(fe))
				if (expression is BindingExpression simple) found.Add((fe, simple));
		foreach (var child in LogicalTreeHelper.GetChildren(root))
			if (child is UIElement element) found.AddRange(SubtreeExpressions(element));
		return found;
	}
}
