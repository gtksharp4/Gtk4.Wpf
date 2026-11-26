namespace System.Windows.Input;

/// <summary>Represents the method that handles <see cref="AccessKeyManager.AccessKeyPressedEvent"/>.</summary>
public delegate void AccessKeyPressedEventHandler(object sender, AccessKeyPressedEventArgs e);

/// <summary>
/// Data for <see cref="AccessKeyManager.AccessKeyPressedEvent"/>: handlers name the element to invoke.
/// </summary>
public class AccessKeyPressedEventArgs : RoutedEventArgs
{
	/// <summary>Creates args with no key.</summary>
	public AccessKeyPressedEventArgs() : base(AccessKeyManager.AccessKeyPressedEvent)
	{
	}

	/// <summary>Creates args for <paramref name="key"/>.</summary>
	public AccessKeyPressedEventArgs(string key) : this()
	{
		Key = key;
	}

	/// <summary>Gets the pressed access key (null for the parameterless constructor).</summary>
	public string? Key { get; }

	/// <summary>Gets or sets the element whose access keys should be searched instead (e.g. an open menu).</summary>
	public object? Scope { get; set; }

	/// <summary>Gets or sets the element to invoke; null means the sender is no candidate.</summary>
	public UIElement? Target { get; set; }
}
