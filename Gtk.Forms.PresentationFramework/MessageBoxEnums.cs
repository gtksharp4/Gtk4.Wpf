namespace System.Windows;

/// <summary>Specifies the button set shown by <see cref="MessageBox"/>.</summary>
public enum MessageBoxButton
{
	/// <summary>OK button only.</summary>
	OK,
	/// <summary>OK and Cancel buttons.</summary>
	OKCancel,
	/// <summary>Yes and No buttons.</summary>
	YesNo,
	/// <summary>Yes, No, and Cancel buttons.</summary>
	YesNoCancel
}

/// <summary>Icon displayed in a <see cref="MessageBox"/>.</summary>
public enum MessageBoxImage
{
	/// <summary>No icon.</summary>
	None,
	/// <summary>Error icon.</summary>
	Error,
	/// <summary>Question icon.</summary>
	Question,
	/// <summary>Warning icon.</summary>
	Warning,
	/// <summary>Information icon.</summary>
	Information,
	/// <summary>Stop (error) icon.</summary>
	Stop,
	/// <summary>Asterisk (information) icon.</summary>
	Asterisk,
	/// <summary>Exclamation (warning) icon.</summary>
	Exclamation,
	/// <summary>Hand (error) icon.</summary>
	Hand
}

/// <summary>Return value from <see cref="MessageBox.Show"/>.</summary>
public enum MessageBoxResult
{
	/// <summary>No result or default close.</summary>
	None,
	/// <summary>OK was clicked.</summary>
	OK,
	/// <summary>Cancel was clicked.</summary>
	Cancel,
	/// <summary>Yes was clicked.</summary>
	Yes,
	/// <summary>No was clicked.</summary>
	No
}