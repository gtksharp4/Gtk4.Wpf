namespace System.Windows;

/// <summary>Specifies the button set shown by <see cref="MessageBox"/>.</summary>
/// <remarks>
/// AbortRetryIgnore, RetryCancel and CancelTryContinue come from WPF in .NET 10 (dotnet/wpf main), not .NET 9;
/// the ordinals match WPF.
/// </remarks>
public enum MessageBoxButton
{
	/// <summary>OK button only.</summary>
	OK = 0,
	/// <summary>OK and Cancel buttons.</summary>
	OKCancel = 1,
	/// <summary>Abort, Retry and Ignore buttons.</summary>
	AbortRetryIgnore = 2,
	/// <summary>Yes, No, and Cancel buttons.</summary>
	YesNoCancel = 3,
	/// <summary>Yes and No buttons.</summary>
	YesNo = 4,
	/// <summary>Retry and Cancel buttons.</summary>
	RetryCancel = 5,
	/// <summary>Cancel, Try Again and Continue buttons.</summary>
	CancelTryContinue = 6
}

/// <summary>Icon displayed in a <see cref="MessageBox"/>. Win32 values; the aliases share one value.</summary>
public enum MessageBoxImage
{
	/// <summary>No icon.</summary>
	None = 0,
	/// <summary>Hand (error) icon.</summary>
	Hand = 16,
	/// <summary>Stop (error) icon.</summary>
	Stop = 16,
	/// <summary>Error icon.</summary>
	Error = 16,
	/// <summary>Question icon.</summary>
	Question = 32,
	/// <summary>Exclamation (warning) icon.</summary>
	Exclamation = 48,
	/// <summary>Warning icon.</summary>
	Warning = 48,
	/// <summary>Asterisk (information) icon.</summary>
	Asterisk = 64,
	/// <summary>Information icon.</summary>
	Information = 64
}

/// <summary>Return value from <see cref="MessageBox.Show"/>. Win32 values.</summary>
/// <remarks>
/// Abort, Retry, Ignore, TryAgain and Continue come from WPF in .NET 10 (dotnet/wpf main), not .NET 9;
/// the ordinals match WPF.
/// </remarks>
public enum MessageBoxResult
{
	/// <summary>No result or default close.</summary>
	None = 0,
	/// <summary>OK was clicked.</summary>
	OK = 1,
	/// <summary>Cancel was clicked.</summary>
	Cancel = 2,
	/// <summary>Abort was clicked.</summary>
	Abort = 3,
	/// <summary>Retry was clicked.</summary>
	Retry = 4,
	/// <summary>Ignore was clicked.</summary>
	Ignore = 5,
	/// <summary>Yes was clicked.</summary>
	Yes = 6,
	/// <summary>No was clicked.</summary>
	No = 7,
	/// <summary>Try Again was clicked.</summary>
	TryAgain = 10,
	/// <summary>Continue was clicked.</summary>
	Continue = 11
}

/// <summary>Specifies options for a <see cref="MessageBox"/>. Stored only on GTK: all values are Win32 flags.</summary>
[Flags]
public enum MessageBoxOptions
{
	/// <summary>No options.</summary>
	None = 0,
	/// <summary>Shown by a service notification.</summary>
	ServiceNotification = 0x200000,
	/// <summary>Shown on the default desktop only.</summary>
	DefaultDesktopOnly = 0x20000,
	/// <summary>Text is right-aligned.</summary>
	RightAlign = 0x80000,
	/// <summary>Text reads right to left.</summary>
	RtlReading = 0x100000,
}
