using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Sets a single <see cref="DependencyProperty"/> value as part of a <see cref="Style"/> or trigger.
/// <para>
/// Either <see cref="Property"/> or <see cref="PropertyName"/> (compiler-facing, resolved against the
/// target at apply time, owner-qualified names such as <c>Grid.Row</c> allowed) may be set;
/// <see cref="Property"/> wins.
/// </para>
/// <para>
/// A string <see cref="Value"/> is type-converted for non-string properties; other objects are applied
/// as-is. Markup-extension values are live: see <c>Setter.Values.cs</c>.
/// </para>
/// </summary>
public partial class Setter : SetterBase
{
	DependencyProperty? _property;
	object? _value;
	string? _propertyName;
	string? _targetName;

	/// <summary>Creates an empty setter.</summary>
	public Setter() { }

	/// <summary>Creates a setter for <paramref name="property"/> with <paramref name="value"/>.</summary>
	public Setter(DependencyProperty property, object value)
	{
		CheckValidProperty(property);
		_property = property;
		Value = value;
	}

	/// <summary>Creates a setter that targets the named template part.</summary>
	public Setter(DependencyProperty property, object value, string targetName)
		: this(property, value)
	{
		TargetName = targetName;
	}

	/// <summary>The dependency property to set (takes precedence over <see cref="PropertyName"/>).</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	/// <exception cref="ArgumentException">The property is read-only.</exception>
	public DependencyProperty? Property
	{
		get => _property;
		set
		{
			CheckValidProperty(value);
			CheckSealed();
			_property = value;
		}
	}

	static void CheckValidProperty(DependencyProperty? property)
	{
		if (property == null) throw new ArgumentNullException("property");
		if (property.IsReadOnly)
			throw new ArgumentException(
				$"'{property.Name}' is a read-only property and cannot be set by a Setter.", "property");
	}

	/// <summary>
	/// String property name, resolved against the target when <see cref="Property"/> is null.
	/// Non-WPF; emitted by the markup compiler.
	/// </summary>
	public string? PropertyName
	{
		get => _propertyName;
		set { CheckSealed(); _propertyName = value; }
	}

	/// <summary>The value to assign. A string is type-converted on apply.</summary>
	/// <exception cref="ArgumentException">The value is <see cref="DependencyProperty.UnsetValue"/>.</exception>
	public object? Value
	{
		get => _value;
		set
		{
			if (ReferenceEquals(value, DependencyProperty.UnsetValue))
				throw new ArgumentException(
					"'UnsetValue' is not a valid value for the 'Setter.Value' property.", nameof(value));
			CheckSealed();
			_value = value;
		}
	}

	/// <summary>
	/// Name of a template part this setter targets. Store-only: template triggers apply the setter to
	/// the trigger's own target and do not yet route by name.
	/// </summary>
	public string? TargetName
	{
		get => _targetName;
		set { CheckSealed(); _targetName = value; }
	}

	/// <inheritdoc/>
	internal override void Apply(DependencyObject target) => ApplyAt(target, Layer.Local, null, null);

	/// <inheritdoc/>
	internal override void ApplyToStyleLayer(DependencyObject target) => ApplyAt(target, Layer.Style, null, null);

	/// <inheritdoc/>
	internal override void ApplyToTriggerLayer(
		DependencyObject target,
		object contributor,
		List<DependencyProperty> appliedDps) => ApplyAt(target, Layer.Trigger, contributor, appliedDps);
}
