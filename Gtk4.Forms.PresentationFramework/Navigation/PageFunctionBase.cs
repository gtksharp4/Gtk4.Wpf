using System.Windows.Controls;

namespace System.Windows.Navigation;

/// <summary>Base of <see cref="PageFunction{T}"/>: a page that returns a result to its caller.</summary>
public abstract class PageFunctionBase : Page
{
	/// <summary>Creates the page function.</summary>
	protected PageFunctionBase()
	{
	}

	/// <summary>Whether the page is dropped from the journal once it returns. Defaults to <c>true</c>.</summary>
	public bool RemoveFromJournal { get; set; } = true;

	// Set by NavigationService when this page is navigated to from another page.
	internal object? Caller { get; set; }

	internal void ReturnToCaller()
	{
		if (Caller == null || NavigationService == null) return;
		NavigationService.ReturnTo(this, Caller, RemoveFromJournal);
	}
}
