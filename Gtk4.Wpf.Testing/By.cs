using System;
using System.Linq;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace Gtk4.Wpf.Testing;

/// <summary>A locator: a UI Automation <see cref="Condition"/> with a description for failure messages.</summary>
public sealed class By
{
	By(Condition condition, string description)
	{
		Condition = condition;
		Description = description;
	}

	/// <summary>The condition handed to <c>FindFirst</c>/<c>FindAll</c>.</summary>
	public Condition Condition { get; }

	/// <summary>Human-readable form, for example <c>AutomationId == "Ok"</c>.</summary>
	public string Description { get; }

	public override string ToString() => Description;

	public static implicit operator Condition(By by) => by.Condition;

	/// <summary>Matches <c>AutomationProperties.AutomationId</c> (or the element name WPF falls back to).</summary>
	public static By AutomationId(string id) =>
		Property(AutomationElement.AutomationIdProperty, id, "AutomationId == " + Quote(id));

	/// <summary>Matches the accessible name.</summary>
	public static By Name(string name, bool ignoreCase = false) =>
		new By(new PropertyCondition(AutomationElement.NameProperty, name,
			ignoreCase ? PropertyConditionFlags.IgnoreCase : PropertyConditionFlags.None),
			"Name == " + Quote(name) + (ignoreCase ? " (ignore case)" : ""));

	/// <summary>Matches the UIA control type, for example <c>ControlType.Button</c>.</summary>
	public static By ControlType(ControlType type) =>
		Property(AutomationElement.ControlTypeProperty, type, "ControlType == " + type.ProgrammaticName);

	/// <summary>Matches the peer class name (the WPF type name of the element).</summary>
	public static By ClassName(string className) =>
		Property(AutomationElement.ClassNameProperty, className, "ClassName == " + Quote(className));

	/// <summary>Matches elements every locator matches.</summary>
	public static By AllOf(params By[] locators)
	{
		Validate(locators);
		return new By(locators.Length == 1 ? locators[0].Condition
			: new AndCondition(locators.Select(l => l.Condition).ToArray()),
			string.Join(" AND ", locators.Select(l => l.Description)));
	}

	/// <summary>Matches elements any locator matches.</summary>
	public static By AnyOf(params By[] locators)
	{
		Validate(locators);
		return new By(locators.Length == 1 ? locators[0].Condition
			: new OrCondition(locators.Select(l => l.Condition).ToArray()),
			"(" + string.Join(" OR ", locators.Select(l => l.Description)) + ")");
	}

	/// <summary>Narrows this locator: the element must also match <paramref name="other"/>.</summary>
	public By And(By other) => AllOf(this, other);

	static By Property(AutomationProperty property, object value, string description) =>
		new By(new PropertyCondition(property, value), description);

	static string Quote(string value) => "\"" + value + "\"";

	static void Validate(By[] locators)
	{
		if (locators == null || locators.Length == 0 || locators.Any(l => l == null))
			throw new ArgumentException("At least one locator is required.", nameof(locators));
	}
}
