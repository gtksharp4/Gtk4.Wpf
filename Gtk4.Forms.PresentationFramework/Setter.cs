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

	/// <summary>Creates an empty setter.</summary>
	public Setter() { }

	/// <summary>Creates a setter for <paramref name="property"/> with <paramref name="value"/>.</summary>
	public Setter(DependencyProperty property, object value)
	{
		Property = property;
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
	public DependencyProperty? Property
	{
		get => _property;
		set => _property = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// String property name, resolved against the target when <see cref="Property"/> is null.
	/// Non-WPF; emitted by the markup compiler.
	/// </summary>
	public string? PropertyName { get; set; }

	/// <summary>The value to assign. A string is type-converted on apply.</summary>
	public object? Value { get; set; }

	/// <summary>
	/// Name of a template part this setter targets. Store-only: template triggers apply the setter to
	/// the trigger's own target and do not yet route by name.
	/// </summary>
	public string? TargetName { get; set; }

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
