using System;

namespace Gtk4.Wpf.Testing;

/// <summary>No element matched a locator. The message carries the automation tree around the search scope.</summary>
public sealed class ElementNotFoundException : Exception
{
	public ElementNotFoundException(string message) : base(message)
	{
	}
}

/// <summary>A wait timed out. The message carries the automation tree at the moment it gave up.</summary>
public sealed class WaitTimeoutException : TimeoutException
{
	public WaitTimeoutException(string message) : base(message)
	{
	}
}
