using System.IO;

namespace System.Windows.Resources;

/// <summary>A resource stream and its MIME content type, as returned by <see cref="Application.GetResourceStream"/>.</summary>
public class StreamResourceInfo
{
	/// <summary>Creates an empty instance.</summary>
	public StreamResourceInfo()
	{
	}

	/// <summary>Creates an instance for <paramref name="stream"/> with <paramref name="contentType"/>.</summary>
	public StreamResourceInfo(Stream stream, string contentType)
	{
		Stream = stream;
		ContentType = contentType;
	}

	/// <summary>The MIME content type of the stream.</summary>
	public string ContentType { get; } = string.Empty;

	/// <summary>The resource stream.</summary>
	public Stream Stream { get; } = Stream.Null;
}
