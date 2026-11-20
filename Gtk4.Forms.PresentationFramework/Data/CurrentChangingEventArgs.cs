namespace System.ComponentModel;

/// <summary>Represents the method that handles <see cref="ICollectionView.CurrentChanging"/>.</summary>
public delegate void CurrentChangingEventHandler(object? sender, CurrentChangingEventArgs e);

/// <summary>Event data for <see cref="ICollectionView.CurrentChanging"/>.</summary>
public class CurrentChangingEventArgs : EventArgs
{
	bool _cancel;

	/// <summary>Initializes a cancelable instance.</summary>
	public CurrentChangingEventArgs() => IsCancelable = true;

	/// <summary>Initializes an instance that is or is not cancelable.</summary>
	public CurrentChangingEventArgs(bool isCancelable) => IsCancelable = isCancelable;

	/// <summary>Gets whether the change can be canceled.</summary>
	public bool IsCancelable { get; }

	/// <summary>Gets or sets whether the change is canceled.</summary>
	/// <exception cref="InvalidOperationException">Set to <see langword="true"/> when not cancelable.</exception>
	public bool Cancel
	{
		get => _cancel;
		set
		{
			if (!IsCancelable && value)
				throw new InvalidOperationException("Cannot cancel a CurrentChanging event that is not cancelable.");
			_cancel = value;
		}
	}
}
