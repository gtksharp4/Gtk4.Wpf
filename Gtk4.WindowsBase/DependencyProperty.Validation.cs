using System;

namespace System.Windows;

public sealed partial class DependencyProperty
{
	/// <summary>Callback that rejects invalid values on SetValue and for default values; may be <c>null</c>.</summary>
	public ValidateValueCallback? ValidateValueCallback { get; }

	/// <summary>
	/// Whether <paramref name="value"/> is assignable to <see cref="PropertyType"/>:
	/// <c>null</c> only for reference and nullable types.
	/// </summary>
	public bool IsValidType(object? value)
	{
		if (value == null)
			return !PropertyType.IsValueType || Nullable.GetUnderlyingType(PropertyType) != null;
		return PropertyType.IsInstanceOfType(value);
	}

	/// <summary>Whether <paramref name="value"/> has a valid type and passes <see cref="ValidateValueCallback"/>.</summary>
	public bool IsValidValue(object? value)
		=> IsValidType(value) && (ValidateValueCallback == null || ValidateValueCallback(value));

	internal void ValidateDefault(PropertyMetadata metadata)
	{
		if (ValidateValueCallback != null && !ValidateValueCallback(metadata.DefaultValue))
			throw new ArgumentException(
				$"Default value for the '{Name}' property is not valid because ValidateValueCallback failed.");
	}

	// Coerced results are not re-validated, so this runs only on the caller-supplied value.
	internal void ThrowIfInvalid(object? value)
	{
		if (!IsValidValue(value))
			throw new ArgumentException($"'{value}' is not a valid value for property '{Name}'.");
	}

	/// <summary>Registers an instance dependency property.</summary>
	public static DependencyProperty Register(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? typeMetadata = null)
		=> RegisterCore(name, propertyType, ownerType, typeMetadata, false, false, null);

	/// <summary>Registers an instance dependency property with a value validator.</summary>
	public static DependencyProperty Register(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? typeMetadata,
		ValidateValueCallback? validateValueCallback)
		=> RegisterCore(name, propertyType, ownerType, typeMetadata, false, false, validateValueCallback);

	/// <summary>Registers an attached dependency property.</summary>
	public static DependencyProperty RegisterAttached(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? defaultMetadata = null)
		=> RegisterCore(name, propertyType, ownerType, defaultMetadata, true, false, null);

	/// <summary>Registers an attached dependency property with a value validator.</summary>
	public static DependencyProperty RegisterAttached(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? defaultMetadata,
		ValidateValueCallback? validateValueCallback)
		=> RegisterCore(name, propertyType, ownerType, defaultMetadata, true, false, validateValueCallback);

	/// <summary>
	/// Registers a read-only dependency property; the returned key is required to
	/// call <see cref="DependencyObject.SetValue(DependencyPropertyKey,object?)"/>.
	/// </summary>
	public static DependencyPropertyKey RegisterReadOnly(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? typeMetadata = null)
		=> new DependencyPropertyKey(RegisterCore(name, propertyType, ownerType, typeMetadata, false, true, null));

	/// <summary>Registers a read-only dependency property with a value validator.</summary>
	public static DependencyPropertyKey RegisterReadOnly(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? typeMetadata,
		ValidateValueCallback? validateValueCallback)
		=> new DependencyPropertyKey(
			RegisterCore(name, propertyType, ownerType, typeMetadata, false, true, validateValueCallback));

	/// <summary>Registers a read-only attached dependency property.</summary>
	public static DependencyPropertyKey RegisterAttachedReadOnly(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? defaultMetadata = null)
		=> new DependencyPropertyKey(RegisterCore(name, propertyType, ownerType, defaultMetadata, true, true, null));

	/// <summary>Registers a read-only attached dependency property with a value validator.</summary>
	public static DependencyPropertyKey RegisterAttachedReadOnly(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? defaultMetadata,
		ValidateValueCallback? validateValueCallback)
		=> new DependencyPropertyKey(
			RegisterCore(name, propertyType, ownerType, defaultMetadata, true, true, validateValueCallback));

	static DependencyProperty RegisterCore(
		string name,
		Type propertyType,
		Type ownerType,
		PropertyMetadata? metadata,
		bool isAttached,
		bool isReadOnly,
		ValidateValueCallback? validateValueCallback)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

		metadata ??= new PropertyMetadata();
		metadata.SetImplicitDefault(GetDefaultDefault(propertyType));

		var key = (name, ownerType);
		lock (_registryLock)
		{
			if (_registry.ContainsKey(key))
				throw new InvalidOperationException(
					$"DependencyProperty '{name}' is already registered on '{ownerType.FullName}'.");
			var dp = new DependencyProperty(
				name, propertyType, ownerType, metadata, isAttached, isReadOnly, validateValueCallback);
			dp.ValidateDefault(metadata);
			metadata.Apply(dp, ownerType, null);
			_registry[key] = dp;
			if (metadata is FrameworkPropertyMetadata fmeta && fmeta.Inherits)
				_inheritingRegistry.Add(dp);
			return dp;
		}
	}
}
