using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>Re-evaluates all bindings whose source is the inherited DataContext.</summary>
	internal static void RefreshBindings(DependencyObject target)
	{
		RefreshMentored(target);
		if (!_bindings.TryGetValue(target, out var list)) return;
		foreach (var record in list)
		{
			if (!record.UsesDataContext) continue;
			record.Activate();
		}
	}

	/// <summary>
	/// Re-activates all <see cref="Binding"/>s whose
	/// <c>RelativeSource.Mode == RelativeSourceMode.TemplatedParent</c> on every element
	/// in the subtree rooted at <paramref name="root"/> (including <paramref name="root"/>
	/// itself). Called by <see cref="Controls.Control.OnTemplateChanged"/> immediately
	/// after <c>StampTemplatedParent</c> stamps the new <c>TemplatedParent</c> value so
	/// that the bindings can resolve the source that was null at activation time.
	/// </summary>
	internal static void RefreshTemplatedParentBindings(UIElement root)
	{
		RefreshTemplatedParentOnElement(root);
		foreach (var child in root.VisualChildren)
			RefreshTemplatedParentBindings(child);
	}

	static void RefreshTemplatedParentOnElement(UIElement element)
	{
		if (element is not FrameworkElement fe) return;
		if (!_bindings.TryGetValue(fe, out var list)) return;
		foreach (var record in list)
		{
			if (record.UsesTemplatedParent)
				record.Activate();
		}
	}
}
