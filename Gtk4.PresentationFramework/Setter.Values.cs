using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>
/// Value application for <see cref="Setter"/>. Plain values go into the style or trigger layer.
/// <see cref="DynamicResourceExtension"/> values stay live (a style-layer link for styles, a
/// contributor-keyed trigger value for triggers; a local setter uses the dynamic-resource layer);
/// <see cref="BindingBase"/> values are bound while the style or trigger is active;
/// <see cref="StaticResourceExtension"/> resolves once at apply.
/// </summary>
/// <remarks>
/// Engine limitation: style/trigger <see cref="BindingBase"/> values are applied as local values.
/// Consequently a trigger setter for the same property never shows over a style binding,
/// <c>ReadLocalValue</c>/<c>ValueSource</c> report Local, and a trigger binding is ignored when the
/// property already has a binding. A layer-targeted binding expression is a backlog item.
/// </remarks>
public partial class Setter
{
	enum Layer { Local, Style, Trigger }

	sealed class Live
	{
		public Setter Owner = null!;
		public object? Contributor;
		public DependencyProperty Dp = null!;
		public BindingExpressionBase? Expr;
		public bool StyleRef;
	}

	static readonly ConditionalWeakTable<DependencyObject, List<Live>> _live =
		new ConditionalWeakTable<DependencyObject, List<Live>>();

	void ApplyAt(DependencyObject target, Layer layer, object? contributor, List<DependencyProperty>? applied)
	{
		var dp = ResolveProperty(target);
		if (dp == null) return;
		applied?.Add(dp);
		Unapply(target, contributor);

		var fe = target as FrameworkElement;
		switch (Value)
		{
			case DynamicResourceExtension dr when dr.ResourceKey != null:
				if (fe != null) { LinkResource(fe, dp, dr.ResourceKey, layer, contributor); return; }
				Store(target, dp, layer, contributor, ResolveOnce(null, dr.ResourceKey));
				return;
			case StaticResourceExtension sr when sr.ResourceKey != null:
				Store(target, dp, layer, contributor, ResolveOnce(fe, sr.ResourceKey));
				return;
			case BindingBase binding:
				if (fe != null && ReferenceEquals(target.ReadLocalValue(dp), DependencyProperty.UnsetValue))
				{
					var expr = BindingOperations.SetBinding(fe, dp, binding);
					Track(target, contributor, dp, expr);
				}
				return;
			default:
				Store(target, dp, layer, contributor, CoerceValue(dp, Value));
				return;
		}
	}

	static object? ResolveOnce(FrameworkElement? fe, object key)
	{
		if (fe != null) return fe.FindResource(key);
		if (Application.Current?.Resources.TryGetValue(key, out var v) == true) return v;
		throw new InvalidOperationException($"Resource with key '{key}' not found.");
	}

	void LinkResource(FrameworkElement fe, DependencyProperty dp, object key, Layer layer, object? contributor)
	{
		if (layer == Layer.Trigger) fe.SetLayerResourceReference(dp, key, contributor!);
		else if (layer == Layer.Style) fe.SetStyleResourceReference(dp, key);
		else fe.SetResourceReference(dp, key);
		Track(fe, contributor, dp, null, layer == Layer.Style);
	}

	static void Store(DependencyObject target, DependencyProperty dp, Layer layer, object? contributor, object? v)
	{
		if (layer == Layer.Style && target is FrameworkElement sfe) sfe.DropStyleResourceReference(dp);
		switch (layer)
		{
			case Layer.Style: target.SetStyleValue(dp, v); break;
			case Layer.Trigger: target.SetTriggerValue(dp, contributor!, v); break;
			default: target.SetValue(dp, v); break;
		}
	}

	void Track(DependencyObject target, object? contributor, DependencyProperty dp,
		BindingExpressionBase? expr, bool styleRef = false) =>
		_live.GetOrCreateValue(target).Add(new Live
		{
			Owner = this, Contributor = contributor, Dp = dp, Expr = expr, StyleRef = styleRef
		});

	/// <inheritdoc/>
	internal override void Unapply(DependencyObject target, object? contributor)
	{
		if (!_live.TryGetValue(target, out var list)) return;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			var l = list[i];
			if (!ReferenceEquals(l.Owner, this) || !ReferenceEquals(l.Contributor, contributor)) continue;
			list.RemoveAt(i);
			if (target is not FrameworkElement fe) continue;
			if (l.Expr != null)
			{
				if (ReferenceEquals(BindingOperations.GetBindingExpressionBase(fe, l.Dp), l.Expr))
				{
					BindingOperations.ClearBinding(fe, l.Dp);
					fe.ClearValue(l.Dp);
				}
			}
			else if (l.StyleRef) fe.DropStyleResourceReference(l.Dp);
			else if (contributor != null) fe.ClearLayerResourceReference(l.Dp, contributor);
			else fe.ClearResourceReference(l.Dp);
		}
	}
}
