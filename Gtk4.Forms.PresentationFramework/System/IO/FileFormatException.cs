namespace System.IO;

/// <summary>The file or stream is not in the expected format (WPF <c>FileFormatException</c>).</summary>
public class FileFormatException : FormatException
{
	/// <summary>Creates the exception.</summary>
	public FileFormatException() { }

	/// <summary>Creates the exception for <paramref name="sourceUri"/>.</summary>
	public FileFormatException(Uri? sourceUri) : base(Describe(null, sourceUri)) => SourceUri = sourceUri;

	/// <summary>Creates the exception with a message.</summary>
	public FileFormatException(string? message) : base(message) { }

	/// <summary>Creates the exception for <paramref name="sourceUri"/> with a message.</summary>
	public FileFormatException(Uri? sourceUri, string? message) : base(Describe(message, sourceUri))
		=> SourceUri = sourceUri;

	/// <summary>Creates the exception with a message and inner exception.</summary>
	public FileFormatException(string? message, Exception? innerException) : base(message, innerException) { }

	/// <summary>The URI of the offending file, or <see langword="null"/>.</summary>
	public Uri? SourceUri { get; }

	static string Describe(string? message, Uri? uri) =>
		message ?? (uri == null ? "The file or stream has an unexpected format."
			: $"The file '{uri}' has an unexpected format.");
}
