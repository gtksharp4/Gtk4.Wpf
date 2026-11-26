namespace System.Windows.Media;

/// <summary>A colour profile reference (WPF <c>ColorContext</c>); stored only, never applied.</summary>
public class ColorContext
{
	/// <summary>Creates a context for the profile at <paramref name="profileUri"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="profileUri"/> is null.</exception>
	public ColorContext(Uri profileUri)
	{
		ProfileUri = profileUri ?? throw new ArgumentNullException(nameof(profileUri));
	}

	/// <summary>Creates the default context for <paramref name="pixelFormat"/>.</summary>
	public ColorContext(PixelFormat pixelFormat) => PixelFormat = pixelFormat;

	/// <summary>The profile location, or <see langword="null"/> for a default-profile context.</summary>
	public Uri? ProfileUri { get; }

	internal PixelFormat PixelFormat { get; }
}
