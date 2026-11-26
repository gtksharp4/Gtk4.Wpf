namespace System.Windows.Automation;

public delegate void AutomationEventHandler(object sender, AutomationEventArgs e);

public delegate void AutomationPropertyChangedEventHandler(object sender, AutomationPropertyChangedEventArgs e);

public delegate void StructureChangedEventHandler(object sender, StructureChangedEventArgs e);

public delegate void AutomationFocusChangedEventHandler(object sender, AutomationFocusChangedEventArgs e);

/// <summary>Event data for a property that changed on an element.</summary>
public class AutomationPropertyChangedEventArgs : AutomationEventArgs
{
	public AutomationPropertyChangedEventArgs(AutomationProperty property, object? oldValue, object? newValue)
		: base(AutomationElementIdentifiers.AutomationPropertyChangedEvent)
	{
		Property = property ?? throw new ArgumentNullException(nameof(property));
		OldValue = oldValue;
		NewValue = newValue;
	}

	public AutomationProperty Property { get; }

	public object? OldValue { get; }

	public object? NewValue { get; }
}

/// <summary>Event data for a change in the structure of the automation tree.</summary>
public class StructureChangedEventArgs : AutomationEventArgs
{
	readonly int[] _runtimeId;

	public StructureChangedEventArgs(StructureChangeType structureChangeType, int[] runtimeId)
		: base(AutomationElementIdentifiers.StructureChangedEvent)
	{
		StructureChangeType = structureChangeType;
		_runtimeId = runtimeId ?? throw new ArgumentNullException(nameof(runtimeId));
	}

	public StructureChangeType StructureChangeType { get; }

	/// <summary>Returns the runtime id of the element whose structure changed.</summary>
	public int[] GetRuntimeId() => (int[])_runtimeId.Clone();
}

/// <summary>Event data for a change of the focused element.</summary>
public class AutomationFocusChangedEventArgs : AutomationEventArgs
{
	public AutomationFocusChangedEventArgs(int idObject, int idChild)
		: base(AutomationElementIdentifiers.AutomationFocusChangedEvent)
	{
		ObjectId = idObject;
		ChildId = idChild;
	}

	public int ObjectId { get; }

	public int ChildId { get; }
}
