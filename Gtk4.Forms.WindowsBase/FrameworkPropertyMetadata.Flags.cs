using System;
using System.Windows.Data;

namespace System.Windows;

public partial class FrameworkPropertyMetadata
{
	/// <summary>Value propagates down the logical tree to children that have no local value.</summary>
	public bool Inherits
	{
		get => Get(FrameworkPropertyMetadataOptions.Inherits);
		set => Set(FrameworkPropertyMetadataOptions.Inherits, value);
	}

	/// <summary>A change to this property triggers a Measure pass (store-only; GTK owns layout).</summary>
	public bool AffectsMeasure
	{
		get => Get(FrameworkPropertyMetadataOptions.AffectsMeasure);
		set => Set(FrameworkPropertyMetadataOptions.AffectsMeasure, value);
	}

	/// <summary>A change to this property triggers an Arrange pass (store-only).</summary>
	public bool AffectsArrange
	{
		get => Get(FrameworkPropertyMetadataOptions.AffectsArrange);
		set => Set(FrameworkPropertyMetadataOptions.AffectsArrange, value);
	}

	/// <summary>A change to this property triggers a Render pass (store-only).</summary>
	public bool AffectsRender
	{
		get => Get(FrameworkPropertyMetadataOptions.AffectsRender);
		set => Set(FrameworkPropertyMetadataOptions.AffectsRender, value);
	}

	/// <summary>A change affects the parent's Measure pass (store-only).</summary>
	public bool AffectsParentMeasure
	{
		get => Get(FrameworkPropertyMetadataOptions.AffectsParentMeasure);
		set => Set(FrameworkPropertyMetadataOptions.AffectsParentMeasure, value);
	}

	/// <summary>A change affects the parent's Arrange pass (store-only).</summary>
	public bool AffectsParentArrange
	{
		get => Get(FrameworkPropertyMetadataOptions.AffectsParentArrange);
		set => Set(FrameworkPropertyMetadataOptions.AffectsParentArrange, value);
	}

	/// <summary>Inheritance crosses template boundaries (store-only; no inheritance-barrier support).</summary>
	public bool OverridesInheritanceBehavior
	{
		get => Get(FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior);
		set => Set(FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, value);
	}

	/// <summary>The property cannot be data bound (store-only; binding does not reject it).</summary>
	public bool IsNotDataBindable
	{
		get => Get(FrameworkPropertyMetadataOptions.NotDataBindable);
		set => Set(FrameworkPropertyMetadataOptions.NotDataBindable, value);
	}

	/// <summary>Bindings with <c>BindingMode.Default</c> resolve to TwoWay.</summary>
	public bool BindsTwoWayByDefault
	{
		get => Get(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault);
		set => Set(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, value);
	}

	/// <summary>The value is journaled across navigation (store-only; there is no journal).</summary>
	public bool Journal
	{
		get => Get(FrameworkPropertyMetadataOptions.Journal);
		set => Set(FrameworkPropertyMetadataOptions.Journal, value);
	}

	/// <summary>Sub-property changes do not re-render (store-only; no Freezable render pass).</summary>
	public bool SubPropertiesDoNotAffectRender
	{
		get => Get(FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender);
		set => Set(FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, value);
	}

	/// <summary>When the binding source is updated for a binding using <c>UpdateSourceTrigger.Default</c>.</summary>
	/// <exception cref="ArgumentException">Set to <c>Default</c>.</exception>
	public UpdateSourceTrigger DefaultUpdateSourceTrigger
	{
		get => _defaultUpdateSourceTrigger;
		set
		{
			ThrowIfSealed();
			if (value == UpdateSourceTrigger.Default)
				throw new ArgumentException("DefaultUpdateSourceTrigger cannot be UpdateSourceTrigger.Default.");
			_defaultUpdateSourceTrigger = value;
			_triggerModified = true;
		}
	}

	/// <summary><c>true</c> unless the property is read-only or marked not data bindable.</summary>
	public bool IsDataBindingAllowed => !IsNotDataBindable && !_readOnly;

	bool Get(FrameworkPropertyMetadataOptions flag) => (_flags & flag) != 0;

	void Set(FrameworkPropertyMetadataOptions flag, bool value)
	{
		ThrowIfSealed();
		_flags = value ? _flags | flag : _flags & ~flag;
		_modified |= flag;
	}

	/// <inheritdoc/>
	protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
	{
		base.Merge(baseMetadata, dp);
		if (baseMetadata is not FrameworkPropertyMetadata b) return;
		_flags = (_flags & _modified) | (b._flags & ~_modified);
		if (!_triggerModified) _defaultUpdateSourceTrigger = b._defaultUpdateSourceTrigger;
	}

	/// <inheritdoc/>
	protected override void OnApply(DependencyProperty dp, Type targetType)
	{
		base.OnApply(dp, targetType);
		_readOnly = dp.IsReadOnly;
	}
}
