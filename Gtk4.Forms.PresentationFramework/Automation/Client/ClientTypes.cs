namespace System.Windows.Automation;

/// <summary>The part of the automation tree a search or subscription covers.</summary>
[Flags]
public enum TreeScope
{
	Element = 1,
	Children = 2,
	Descendants = 4,
	Parent = 8,
	Ancestors = 16,
	Subtree = Element | Children | Descendants,
}

/// <summary>Modifies how a <see cref="PropertyCondition"/> compares values.</summary>
[Flags]
public enum PropertyConditionFlags
{
	None = 0,
	IgnoreCase = 1,
}

/// <summary>Whether cached elements keep a reference to the live element.</summary>
public enum AutomationElementMode
{
	None,
	Full,
}

/// <summary>Thrown when an element has no point a click can reach.</summary>
public class NoClickablePointException : Exception
{
	public NoClickablePointException() : base("The element has no clickable point.") { }

	public NoClickablePointException(string message) : base(message) { }

	public NoClickablePointException(string message, Exception innerException) : base(message, innerException) { }
}
