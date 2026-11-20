namespace System.Windows;

public partial class Window
{
	bool _showingAsDialog;
	bool? _dialogResult;

	/// <summary>
	/// Gets or sets the dialog result. Setting it closes a window shown with <see cref="ShowDialog"/> and
	/// makes <see cref="ShowDialog"/> return the value. A <see cref="Closing"/> handler that cancels the
	/// close leaves the previous value in place.
	/// </summary>
	/// <exception cref="InvalidOperationException">Set while the window is not shown as a dialog.</exception>
	public bool? DialogResult
	{
		get => _dialogResult;
		set
		{
			if (!_showingAsDialog)
				throw new InvalidOperationException(
					"DialogResult can be set only after the window is shown as a dialog.");
			if (_dialogResult == value) return;
			bool? previous = _dialogResult;
			_dialogResult = value;
			// Inside a close already in progress (a Closing handler) the value is only recorded.
			if (!_closingInProgress && !RequestClose())
				_dialogResult = previous; // vetoed: undo the tentative set
		}
	}
}
