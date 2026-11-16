using System.ComponentModel;
using System.IO;

namespace System.Windows.Media.Imaging
{
	/// <summary>
	/// Phase-1 stand-in for WPF's <c>System.Windows.Media.Imaging.BitmapImage</c> — the workhorse
	/// consumers use to load an image from a URI or stream. It supports the standard WPF shapes:
	/// the <see cref="Uri"/> constructor and the <see cref="ISupportInitialize"/> batch
	/// (<c>BeginInit()</c>/set <see cref="UriSource"/>/<c>EndInit()</c>). Decoding is deferred to a
	/// later phase; the source is carried so <see cref="ImageSource"/>-typed consumers compile and
	/// round-trip their inputs.
	/// </summary>
	public sealed class BitmapImage : BitmapSource, ISupportInitialize
	{
		/// <summary>Creates an empty bitmap; set <see cref="UriSource"/> between
		/// <see cref="BeginInit"/> and <see cref="EndInit"/>.</summary>
		public BitmapImage() { }

		/// <summary>Creates a bitmap whose content is loaded from <paramref name="uriSource"/>.</summary>
		public BitmapImage(Uri uriSource) => UriSource = uriSource;

		/// <summary>The URI the image is loaded from, or <see langword="null"/> when none is set.</summary>
		public Uri? UriSource { get; set; }

		/// <summary>The stream the image is loaded from, or <see langword="null"/> when none is set.</summary>
		public Stream? StreamSource { get; set; }

		/// <summary>Begins a batch of property initialisation (WPF <see cref="ISupportInitialize"/>).</summary>
		public void BeginInit() { }

		/// <summary>Ends a batch of property initialisation (WPF <see cref="ISupportInitialize"/>).</summary>
		public void EndInit() { }
	}
}
