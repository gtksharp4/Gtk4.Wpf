using System.Collections.Generic;

namespace System.Windows.Automation;

/// <summary>Base of the UI Automation identifiers: a numeric id plus a programmatic name.</summary>
public class AutomationIdentifier
{
	internal AutomationIdentifier(int id, string programmaticName)
	{
		Id = id;
		ProgrammaticName = programmaticName;
	}

	/// <summary>The UI Automation numeric id.</summary>
	public int Id { get; }

	/// <summary>The programmatic name, for example <c>ControlType.Button</c>.</summary>
	public string ProgrammaticName { get; }

	internal static T Register<T>(Dictionary<int, T> table, T identifier) where T : AutomationIdentifier
	{
		lock (table) table[identifier.Id] = identifier;
		return identifier;
	}

	internal static T? Find<T>(Dictionary<int, T> table, int id) where T : AutomationIdentifier
	{
		Ids.EnsureAll();
		lock (table) return table.TryGetValue(id, out var found) ? found : null;
	}
}

/// <summary>Identifies a UI Automation property.</summary>
public class AutomationProperty : AutomationIdentifier
{
	static readonly Dictionary<int, AutomationProperty> _table = new Dictionary<int, AutomationProperty>();

	AutomationProperty(int id, string name) : base(id, name) { }

	internal static AutomationProperty Register(int id, string name) =>
		Register(_table, new AutomationProperty(id, name));

	/// <summary>Returns the property with the given id, or <c>null</c>.</summary>
	public static AutomationProperty? LookupById(int id) =>
		Find(_table, id);
}

/// <summary>Identifies a UI Automation event.</summary>
public class AutomationEvent : AutomationIdentifier
{
	static readonly Dictionary<int, AutomationEvent> _table = new Dictionary<int, AutomationEvent>();

	AutomationEvent(int id, string name) : base(id, name) { }

	internal static AutomationEvent Register(int id, string name) =>
		Register(_table, new AutomationEvent(id, name));

	/// <summary>Returns the event with the given id, or <c>null</c>.</summary>
	public static AutomationEvent? LookupById(int id) =>
		Find(_table, id);
}

/// <summary>Identifies a UI Automation control pattern.</summary>
public class AutomationPattern : AutomationIdentifier
{
	static readonly Dictionary<int, AutomationPattern> _table = new Dictionary<int, AutomationPattern>();

	AutomationPattern(int id, string name) : base(id, name) { }

	internal static AutomationPattern Register(int id, string name) =>
		Register(_table, new AutomationPattern(id, name));

	/// <summary>Returns the pattern with the given id, or <c>null</c>.</summary>
	public static AutomationPattern? LookupById(int id) =>
		Find(_table, id);
}
