using System;

namespace System.Windows.Markup;

/// <summary>The error <see cref="XamlReader"/> throws; carries the 1-based position of the failing node.</summary>
// The protected (SerializationInfo, StreamingContext) ctor of WPF is omitted: base(info, context) is
// SYSLIB0051 (an error under TreatWarningsAsErrors) on the supported TFMs.
[Serializable]
public class XamlParseException : SystemException
{
	/// <summary>Creates an exception with no message.</summary>
	public XamlParseException()
	{
	}

	/// <summary>Creates an exception with <paramref name="message"/>.</summary>
	public XamlParseException(string message) : base(message)
	{
	}

	/// <summary>Creates an exception wrapping <paramref name="innerException"/>.</summary>
	public XamlParseException(string message, Exception innerException) : base(message, innerException)
	{
	}

	/// <summary>Creates an exception pointing at <paramref name="lineNumber"/>/<paramref name="linePosition"/>.</summary>
	public XamlParseException(string message, int lineNumber, int linePosition) : base(message)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	/// <summary>Creates a positioned exception wrapping <paramref name="innerException"/>.</summary>
	public XamlParseException(string message, int lineNumber, int linePosition, Exception innerException)
		: base(message, innerException)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	/// <summary>The 1-based line of the failing node, or 0 when unknown.</summary>
	public int LineNumber { get; internal set; }

	/// <summary>The 1-based column of the failing node, or 0 when unknown.</summary>
	public int LinePosition { get; internal set; }

	/// <summary>The base URI of the document, when known.</summary>
	public Uri? BaseUri { get; internal set; }

	/// <summary>The <c>x:Key</c> of the entry being parsed, when known.</summary>
	public object? KeyContext { get; internal set; }

	/// <summary>The <c>x:Name</c> of the element being parsed, when known.</summary>
	public string? NameContext { get; internal set; }

	/// <summary>The <c>x:Uid</c> of the element being parsed, when known.</summary>
	public string? UidContext { get; internal set; }
}
