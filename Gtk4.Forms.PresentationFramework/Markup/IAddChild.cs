namespace System.Windows.Markup;

/// <summary>Lets the XAML parser (and code) add child objects and text to a parent.</summary>
public interface IAddChild
{
	/// <summary>Adds a child object.</summary>
	void AddChild(object value);

	/// <summary>Adds text to the object.</summary>
	void AddText(string text);
}
