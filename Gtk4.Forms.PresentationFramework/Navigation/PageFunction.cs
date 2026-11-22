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
	/// Raises <see cref="Return"/>, then navigates back to the caller (dropping this page from the
	/// journal when <see cref="PageFunctionBase.RemoveFromJournal"/> is set).
	/// </summary>
	protected virtual void OnReturn(ReturnEventArgs<T> e)
	{
		Return?.Invoke(this, e);
		ReturnToCaller();
	}
}
