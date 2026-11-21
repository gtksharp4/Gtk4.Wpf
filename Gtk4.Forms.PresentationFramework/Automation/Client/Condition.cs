using System.Collections.Generic;

namespace System.Windows.Automation;

/// <summary>A test an element passes or fails; the argument of searches and tree walkers.</summary>
public abstract class Condition
{
	internal Condition() { }

	/// <summary>Matches every element.</summary>
	public static readonly Condition TrueCondition = new BoolCondition(true);

	/// <summary>Matches no element.</summary>
	public static readonly Condition FalseCondition = new BoolCondition(false);

	/// <summary>Evaluates the condition on the GTK thread, inside a client call.</summary>
	internal abstract bool Matches(AutomationElement element);

	sealed class BoolCondition : Condition
	{
		readonly bool _value;

		internal BoolCondition(bool value) => _value = value;

		internal override bool Matches(AutomationElement element) => _value;
	}
}

/// <summary>Matches elements that match every one of its conditions.</summary>
public class AndCondition : Condition
{
	readonly Condition[] _conditions;

	public AndCondition(params Condition[] conditions) => _conditions = Validate(conditions);

	/// <summary>Returns a copy of the conditions.</summary>
	public Condition[] GetConditions() => (Condition[])_conditions.Clone();

	internal override bool Matches(AutomationElement element)
	{
		foreach (var condition in _conditions)
			if (!condition.Matches(element))
				return false;
		return true;
	}

	internal static Condition[] Validate(Condition[] conditions)
	{
		if (conditions == null) throw new ArgumentNullException(nameof(conditions));
		if (conditions.Length < 2)
			throw new ArgumentException("At least two conditions are required.", nameof(conditions));
		var copy = new List<Condition>(conditions.Length);
		foreach (var condition in conditions)
			copy.Add(condition ?? throw new ArgumentNullException(nameof(conditions)));
		return copy.ToArray();
	}
}

/// <summary>Matches elements that match at least one of its conditions.</summary>
public class OrCondition : Condition
{
	readonly Condition[] _conditions;

	public OrCondition(params Condition[] conditions) => _conditions = AndCondition.Validate(conditions);

	/// <summary>Returns a copy of the conditions.</summary>
	public Condition[] GetConditions() => (Condition[])_conditions.Clone();

	internal override bool Matches(AutomationElement element)
	{
		foreach (var condition in _conditions)
			if (condition.Matches(element))
				return true;
		return false;
	}
}

/// <summary>Matches elements that do not match its condition.</summary>
public class NotCondition : Condition
{
	public NotCondition(Condition condition) =>
		Condition = condition ?? throw new ArgumentNullException(nameof(condition));

	/// <summary>The condition this one negates.</summary>
	public Condition Condition { get; }

	internal override bool Matches(AutomationElement element) => !Condition.Matches(element);
}
