using System;

namespace System.Windows.Input;

/// <summary>Data for <c>UIElement.OnAccessKey</c>: the pressed key and whether other elements share it.</summary>
public class AccessKeyEventArgs : EventArgs
{
	internal AccessKeyEventArgs(string key, bool isMultiple, bool userInitiated)
	{
		Key = key;
		IsMultiple = isMultiple;
		UserInitiated = userInitiated;
	}

	/// <summary>Gets the access key that was pressed.</summary>
	public string Key { get; }

	/// <summary>Gets whether more than one element is registered for <see cref="Key"/>.</summary>
	public bool IsMultiple { get; }

	internal bool UserInitiated { get; }
}
