using System;

namespace System.Windows;

/// <summary>
/// Callback invoked when an effective property value changes on a <see cref="DependencyObject"/>.
/// </summary>
public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

/// <summary>
/// Callback invoked to coerce a value before it is stored.
/// Returns the coerced value (may equal the proposed value).
/// </summary>
public delegate object? CoerceValueCallback(DependencyObject d, object? baseValue);

/// <summary>
/// Provides property-system metadata for a <see cref="DependencyProperty"/>.
/// Setters throw <see cref="InvalidOperationException"/> once the metadata is sealed
/// (passed to Register, OverrideMetadata or AddOwner).
/// </summary>
public partial class PropertyMetadata
{
	object? _defaultValue;
	bool _defaultValueSet;
	PropertyChangedCallback? _propertyChangedCallback;
	CoerceValueCallback? _coerceValueCallback;

	/// <summary>Creates metadata with no default value; one is inherited or derived at registration.</summary>
	public PropertyMetadata()
	{
	}

	/// <summary>Creates metadata with a change callback only.</summary>
	public PropertyMetadata(PropertyChangedCallback? propertyChangedCallback)
	{
		_propertyChangedCallback = propertyChangedCallback;
	}

	/// <summary>Creates metadata with a default value.</summary>
	public PropertyMetadata(object? defaultValue)
	{
		DefaultValue = defaultValue;
	}

	/// <summary>Creates metadata with a default value and change callback.</summary>
	public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
	{
		DefaultValue = defaultValue;
		_propertyChangedCallback = propertyChangedCallback;
	}

	/// <summary>Creates metadata with a default value, change callback, and coerce callback.</summary>
	public PropertyMetadata(
		object? defaultValue,
		PropertyChangedCallback? propertyChangedCallback,
		CoerceValueCallback? coerceValueCallback)
	{
		DefaultValue = defaultValue;
		_propertyChangedCallback = propertyChangedCallback;
		_coerceValueCallback = coerceValueCallback;
	}

	/// <summary>Default value returned when no local value or inherited value is present.</summary>
	/// <exception cref="ArgumentException">Set to <see cref="DependencyProperty.UnsetValue"/>.</exception>
	/// <exception cref="InvalidOperationException">The metadata is sealed.</exception>
	public object? DefaultValue
	{
		get => _defaultValue;
		set
		{
			ThrowIfSealed();
			if (ReferenceEquals(value, DependencyProperty.UnsetValue))
				throw new ArgumentException("Cannot set property metadata's default value to 'Unset'.");
			_defaultValue = value;
			_defaultValueSet = true;
		}
	}

	/// <summary>Callback fired when the effective value changes.</summary>
	public PropertyChangedCallback? PropertyChangedCallback
	{
		get => _propertyChangedCallback;
		set
		{
			ThrowIfSealed();
			_propertyChangedCallback = value;
		}
	}

	/// <summary>Callback to coerce the value before storing.</summary>
	public CoerceValueCallback? CoerceValueCallback
	{
		get => _coerceValueCallback;
		set
		{
			ThrowIfSealed();
			_coerceValueCallback = value;
		}
	}
}
