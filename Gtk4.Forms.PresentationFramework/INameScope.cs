namespace System.Windows;

/// <summary>A scope in which names map to objects.</summary>
public interface INameScope
{
	/// <summary>Finds the object registered under <paramref name="name"/>, or null.</summary>
	object? FindName(string name);

	/// <summary>Registers <paramref name="scopedElement"/> under <paramref name="name"/>.</summary>
	void RegisterName(string name, object scopedElement);

	/// <summary>Removes <paramref name="name"/>.</summary>
	void UnregisterName(string name);
}
