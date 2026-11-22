namespace System.Windows.Navigation;

/// <summary>A page that returns a value of type <typeparamref name="T"/> to the page that navigated to it.</summary>
public class PageFunction<T> : PageFunctionBase
{
	/// <summary>Creates the page function.</summary>
	public PageFunction()
	{
	}

	/// <summary>Raised when the page returns; the caller subscribes before navigating.</summary>
	public event ReturnEventHandler<T>? Return;

	/// <summary>
	/// Navigates back to the caller (dropping this page from the journal when
	/// <see cref="PageFunctionBase.RemoveFromJournal"/> is set), then raises <see cref="Return"/>, so a handler
	/// sees the caller as the current content and a chain of page functions unwinds one level at a time.
	/// </summary>
	protected virtual void OnReturn(ReturnEventArgs<T> e)
	{
		ReturnToCaller();
		Return?.Invoke(this, e);
	}
}
