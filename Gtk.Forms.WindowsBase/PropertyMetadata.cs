using System;

namespace System.Windows
{
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
	/// </summary>
	public class PropertyMetadata
	{
		/// <summary>Default value returned when no local value or inherited value is present.</summary>
		public object? DefaultValue { get; protected set; }

		/// <summary>Callback fired when the effective value changes.</summary>
		public PropertyChangedCallback? PropertyChangedCallback { get; protected set; }

		/// <summary>Callback to coerce the value before storing.</summary>
		public CoerceValueCallback? CoerceValueCallback { get; protected set; }

		/// <summary>Creates metadata with a default value.</summary>
		public PropertyMetadata(object? defaultValue)
		{
			DefaultValue = defaultValue;
		}

		/// <summary>Creates metadata with a default value and change callback.</summary>
		public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
		{
			DefaultValue = defaultValue;
			PropertyChangedCallback = propertyChangedCallback;
		}

		/// <summary>Creates metadata with a default value, change callback, and coerce callback.</summary>
		public PropertyMetadata(
			object? defaultValue,
			PropertyChangedCallback? propertyChangedCallback,
			CoerceValueCallback? coerceValueCallback)
		{
			DefaultValue = defaultValue;
			PropertyChangedCallback = propertyChangedCallback;
			CoerceValueCallback = coerceValueCallback;
		}
	}

	/// <summary>
	/// Extended metadata that also carries layout/render influence flags and property
	/// inheritance across the logical tree.
	/// </summary>
	public class FrameworkPropertyMetadata : PropertyMetadata
	{
		/// <summary>Value propagates down the logical tree to children that have no local value.</summary>
		public bool Inherits { get; set; }

		/// <summary>A change to this property triggers a Measure pass.</summary>
		public bool AffectsMeasure { get; set; }

		/// <summary>A change to this property triggers an Arrange pass.</summary>
		public bool AffectsArrange { get; set; }

		/// <summary>A change to this property triggers a Render pass.</summary>
		public bool AffectsRender { get; set; }

		/// <inheritdoc cref="PropertyMetadata(object?)"/>
		public FrameworkPropertyMetadata(object? defaultValue) : base(defaultValue) { }

		/// <summary>Creates metadata with a default value and inheritance flag.</summary>
		public FrameworkPropertyMetadata(object? defaultValue, bool inherits) : base(defaultValue)
		{
			Inherits = inherits;
		}

		/// <inheritdoc cref="PropertyMetadata(object?,PropertyChangedCallback?)"/>
		public FrameworkPropertyMetadata(
			object? defaultValue,
			PropertyChangedCallback? propertyChangedCallback) : base(defaultValue, propertyChangedCallback) { }

		/// <inheritdoc cref="PropertyMetadata(object?,PropertyChangedCallback?,CoerceValueCallback?)"/>
		public FrameworkPropertyMetadata(
			object? defaultValue,
			PropertyChangedCallback? propertyChangedCallback,
			CoerceValueCallback? coerceValueCallback)
			: base(defaultValue, propertyChangedCallback, coerceValueCallback) { }

		/// <summary>Creates metadata with all options.</summary>
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
	}
}
