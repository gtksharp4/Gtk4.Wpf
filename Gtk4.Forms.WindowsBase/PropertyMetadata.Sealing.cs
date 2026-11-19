using System;

namespace System.Windows;

public partial class PropertyMetadata
{
	/// <summary><c>true</c> once the metadata has been applied to a property; its setters then throw.</summary>
	protected bool IsSealed { get; private set; }

	/// <summary>
	/// Merges <paramref name="baseMetadata"/> into this metadata: an unset default is taken from the base,
	/// change callbacks are chained (base first), and an unset coerce callback is taken from the base.
	/// </summary>
	protected virtual void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
	{
		if (baseMetadata == null) throw new ArgumentNullException(nameof(baseMetadata));
		ThrowIfSealed();
		if (!_defaultValueSet) _defaultValue = baseMetadata._defaultValue;
		if (baseMetadata._propertyChangedCallback != null)
			_propertyChangedCallback = (PropertyChangedCallback?)Delegate.Combine(
				baseMetadata._propertyChangedCallback, _propertyChangedCallback);
		if (_coerceValueCallback == null) _coerceValueCallback = baseMetadata._coerceValueCallback;
	}

	/// <summary>Called once after the metadata is sealed against <paramref name="dp"/> and its target type.</summary>
	protected virtual void OnApply(DependencyProperty dp, Type targetType)
	{
	}

	// Fills the unset default of a base registration (default(T) for value types).
	internal bool IsDefaultValueModified => _defaultValueSet;

	internal void SetImplicitDefault(object? value)
	{
		if (!_defaultValueSet) _defaultValue = value;
	}

	internal void Apply(DependencyProperty dp, Type targetType, PropertyMetadata? baseMetadata)
	{
		if (IsSealed)
			throw new ArgumentException(
				"Metadata is already associated with a type and property. A new one must be created.");
		if (baseMetadata != null) Merge(baseMetadata, dp);
		IsSealed = true;
		OnApply(dp, targetType);
	}

	/// <summary>Throws <see cref="InvalidOperationException"/> when the metadata is sealed.</summary>
	protected void ThrowIfSealed()
	{
		if (IsSealed)
			throw new InvalidOperationException(
				"Cannot change property metadata after it has been associated with a property.");
	}
}
