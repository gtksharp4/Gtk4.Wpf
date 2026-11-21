namespace System.Windows.Automation;

/// <summary>Matches elements whose property has a given value.</summary>
public class PropertyCondition : Condition
{
	public PropertyCondition(AutomationProperty property, object? value)
		: this(property, value, PropertyConditionFlags.None) { }

	public PropertyCondition(AutomationProperty property, object? value, PropertyConditionFlags flags)
	{
		Property = property ?? throw new ArgumentNullException(nameof(property));
		var type = PropertyCatalog.LookupElement(property)?.Type ?? PropertyCatalog.LookupPattern(property)?.Type
			?? throw new ArgumentException("The property is not supported.", nameof(property));
		if (value == null ? type.IsValueType
			: !ReferenceEquals(value, AutomationElement.NotSupported) && !type.IsInstanceOfType(value))
			throw new ArgumentException($"The value must be a {type.Name}.", nameof(value));
		if ((flags & PropertyConditionFlags.IgnoreCase) != 0 && type != typeof(string))
			throw new ArgumentException("IgnoreCase applies only to string values.", nameof(flags));
		Value = value;
		Flags = flags;
	}

	/// <summary>The property to compare.</summary>
	public AutomationProperty Property { get; }

	/// <summary>The value the property must have.</summary>
	public object? Value { get; }

	/// <summary>How the comparison is made.</summary>
	public PropertyConditionFlags Flags { get; }

	internal override bool Matches(AutomationElement element)
	{
		var actual = element.ReadPropertyCore(Property, ignoreDefault: true);
		if (ReferenceEquals(actual, AutomationElement.NotSupported)
			|| ReferenceEquals(Value, AutomationElement.NotSupported))
			return ReferenceEquals(actual, Value);
		if (Value is string expected && actual is string text)
			return string.Equals(expected, text, (Flags & PropertyConditionFlags.IgnoreCase) != 0
				? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal);
		return Equals(Value, actual);
	}
}
