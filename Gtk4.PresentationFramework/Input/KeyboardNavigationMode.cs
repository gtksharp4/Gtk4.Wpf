namespace System.Windows.Input;

/// <summary>
/// Specifies how keyboard tab-navigation moves focus through elements within a container.
/// </summary>
public enum KeyboardNavigationMode
{
	/// <summary>Focus moves through all focusable elements, then continues to the parent container.</summary>
	Continue,
	/// <summary>Focus moves into the container once; subsequent tabs exit without cycling.</summary>
	Once,
	/// <summary>Focus cycles through the focusable elements within the container indefinitely.</summary>
	Cycle,
	/// <summary>No keyboard navigation to or within this container.</summary>
	None,
	/// <summary>Focus is contained within the element and does not leave it via Tab.</summary>
	Contained,
	/// <summary>Focus moves through elements in local (declaration) order only.</summary>
	Local,
}
