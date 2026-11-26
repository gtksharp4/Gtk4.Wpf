using System;

namespace System.Windows;

/// <summary>
/// Metadata that adds an animation gate to <see cref="PropertyMetadata"/>.
/// GTK gap: <see cref="IsAnimationProhibited"/> is stored but no animation gate reads it.
/// </summary>
public class UIPropertyMetadata : PropertyMetadata
{
	bool _isAnimationProhibited;

	/// <summary>Creates metadata with no default value.</summary>
	public UIPropertyMetadata()
	{
	}

	/// <summary>Creates metadata with a default value.</summary>
	public UIPropertyMetadata(object? defaultValue) : base(defaultValue)
	{
	}

	/// <summary>Creates metadata with a change callback.</summary>
	public UIPropertyMetadata(PropertyChangedCallback? propertyChangedCallback) : base(propertyChangedCallback)
	{
	}

	/// <summary>Creates metadata with a default value and change callback.</summary>
	public UIPropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
		: base(defaultValue, propertyChangedCallback)
	{
	}

	/// <summary>Creates metadata with a default value, change callback and coerce callback.</summary>
	public UIPropertyMetadata(
		object? defaultValue,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
	}

	/// <summary>Creates metadata with callbacks and an animation gate.</summary>
	public UIPropertyMetadata(
		object? defaultValue,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback,
		bool isAnimationProhibited)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
		_isAnimationProhibited = isAnimationProhibited;
	}

	/// <summary>Whether animating the property is prohibited.</summary>
	public bool IsAnimationProhibited
	{
		get => _isAnimationProhibited;
		set
		{
			ThrowIfSealed();
			_isAnimationProhibited = value;
		}
	}
}
