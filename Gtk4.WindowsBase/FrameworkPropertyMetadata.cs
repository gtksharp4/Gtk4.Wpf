using System;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Extended metadata that also carries layout/render influence flags, property inheritance
/// across the logical tree, and binding defaults.
/// </summary>
public partial class FrameworkPropertyMetadata : UIPropertyMetadata
{
	FrameworkPropertyMetadataOptions _flags;
	// Flags explicitly set on this instance; the rest are taken from the base metadata on Merge.
	FrameworkPropertyMetadataOptions _modified;
	UpdateSourceTrigger _defaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
	bool _triggerModified;
	bool _readOnly;

	/// <summary>Creates metadata with no default value.</summary>
	public FrameworkPropertyMetadata()
	{
	}

	/// <summary>Creates metadata with a change callback.</summary>
	public FrameworkPropertyMetadata(PropertyChangedCallback? propertyChangedCallback)
		: base(propertyChangedCallback)
	{
	}

	/// <summary>Creates metadata with change and coerce callbacks.</summary>
	public FrameworkPropertyMetadata(
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback)
		: base(propertyChangedCallback)
	{
		CoerceValueCallback = coerceValueCallback;
	}

	/// <inheritdoc cref="PropertyMetadata(object?)"/>
	public FrameworkPropertyMetadata(object? defaultValue) : base(defaultValue)
	{
	}

	/// <summary>Creates metadata with a default value and options.</summary>
	public FrameworkPropertyMetadata(object? defaultValue, FrameworkPropertyMetadataOptions flags)
		: base(defaultValue)
	{
		TranslateFlags(flags);
	}

	/// <summary>Creates metadata with a default value, options and change callback.</summary>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		FrameworkPropertyMetadataOptions flags,
		PropertyChangedCallback? propertyChangedCallback)
		: base(defaultValue, propertyChangedCallback)
	{
		TranslateFlags(flags);
	}

	/// <summary>Creates metadata with a default value, options and callbacks.</summary>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		FrameworkPropertyMetadataOptions flags,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
		TranslateFlags(flags);
	}

	/// <summary>Creates metadata with a default value, options, callbacks and animation gate.</summary>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		FrameworkPropertyMetadataOptions flags,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback,
		bool isAnimationProhibited)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback, isAnimationProhibited)
	{
		TranslateFlags(flags);
	}

	/// <summary>Creates metadata with everything including the default update-source trigger.</summary>
	/// <exception cref="ArgumentException"><paramref name="defaultUpdateSourceTrigger"/> is <c>Default</c>.</exception>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		FrameworkPropertyMetadataOptions flags,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback,
		bool isAnimationProhibited,
		UpdateSourceTrigger defaultUpdateSourceTrigger)
		: this(defaultValue, flags, propertyChangedCallback, coerceValueCallback, isAnimationProhibited)
	{
		DefaultUpdateSourceTrigger = defaultUpdateSourceTrigger;
	}

	/// <inheritdoc cref="PropertyMetadata(object?,PropertyChangedCallback?)"/>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		PropertyChangedCallback? propertyChangedCallback) : base(defaultValue, propertyChangedCallback)
	{
	}

	/// <inheritdoc cref="PropertyMetadata(object?,PropertyChangedCallback?,CoerceValueCallback?)"/>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
	}

	/// <summary>Creates metadata with a default value and inheritance flag (not a WPF overload).</summary>
	public FrameworkPropertyMetadata(object? defaultValue, bool inherits) : base(defaultValue)
	{
		Inherits = inherits;
	}

	/// <summary>Creates metadata with all four basic flags (not a WPF overload).</summary>
	public FrameworkPropertyMetadata(
		object? defaultValue,
		bool inherits,
		bool affectsMeasure,
		bool affectsArrange,
		bool affectsRender,
		PropertyChangedCallback? propertyChangedCallback = null,
		CoerceValueCallback? coerceValueCallback = null)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
		Inherits = inherits;
		AffectsMeasure = affectsMeasure;
		AffectsArrange = affectsArrange;
		AffectsRender = affectsRender;
	}

	// Only flags that are on count as explicitly set, so an override without a flag keeps the base's.
	void TranslateFlags(FrameworkPropertyMetadataOptions flags)
	{
		_flags = flags;
		_modified = flags;
	}
}
