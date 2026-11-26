using System;

namespace System.Windows;

/// <summary>
/// Flags describing how a property participates in layout, rendering, inheritance and binding.
/// <para>
/// GTK gap: only <see cref="Inherits"/>, <see cref="BindsTwoWayByDefault"/> and
/// <see cref="NotDataBindable"/> have an effect. GTK owns layout invalidation, and there is no
/// navigation journal, Freezable sub-property render pass or animation gate, so the remaining
/// flags are stored faithfully but never read.
/// </para>
/// </summary>
[Flags]
public enum FrameworkPropertyMetadataOptions
{
	/// <summary>No options.</summary>
	None = 0,
	/// <summary>Affects measure (store-only).</summary>
	AffectsMeasure = 0x1,
	/// <summary>Affects arrange (store-only).</summary>
	AffectsArrange = 0x2,
	/// <summary>Affects the parent's measure (store-only).</summary>
	AffectsParentMeasure = 0x4,
	/// <summary>Affects the parent's arrange (store-only).</summary>
	AffectsParentArrange = 0x8,
	/// <summary>Affects rendering (store-only).</summary>
	AffectsRender = 0x10,
	/// <summary>The value is inherited down the tree.</summary>
	Inherits = 0x20,
	/// <summary>Inheritance crosses template/popup boundaries (store-only).</summary>
	OverridesInheritanceBehavior = 0x40,
	/// <summary>The property cannot be data bound.</summary>
	NotDataBindable = 0x80,
	/// <summary>Bindings with <c>BindingMode.Default</c> are TwoWay.</summary>
	BindsTwoWayByDefault = 0x100,
	/// <summary>The value is journaled across navigation (store-only).</summary>
	Journal = 0x400,
	/// <summary>Sub-property changes do not re-render (store-only).</summary>
	SubPropertiesDoNotAffectRender = 0x800,
}
