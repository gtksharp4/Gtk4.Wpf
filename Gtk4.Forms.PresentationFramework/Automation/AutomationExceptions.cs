namespace System.Windows.Automation;

/// <summary>Thrown when a pattern is used on a disabled element.</summary>
public class ElementNotEnabledException : InvalidOperationException
{
	public ElementNotEnabledException() : base("The element is not enabled.") { }

	public ElementNotEnabledException(string message) : base(message) { }

	public ElementNotEnabledException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Thrown when an element is no longer available in the automation tree.</summary>
public class ElementNotAvailableException : SystemException
{
	public ElementNotAvailableException() : base("The element is not available.") { }

	public ElementNotAvailableException(string message) : base(message) { }

	public ElementNotAvailableException(string message, Exception innerException) : base(message, innerException) { }
}
