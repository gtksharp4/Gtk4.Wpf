using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Encodes frames into an image container (WPF <c>BitmapEncoder</c>).</summary>
public abstract class BitmapEncoder
{
	readonly ImageContainer _container;
	bool _saved;

	/// <summary>Creates an encoder for a derived class; override <see cref="Save"/> to write the frames.</summary>
	protected BitmapEncoder() { }

	internal BitmapEncoder(ImageContainer container) => _container = container;

	/// <summary>The frames to write.</summary>
	public virtual IList<BitmapFrame> Frames { get; } = new List<BitmapFrame>();

	/// <summary>Describes the codec.</summary>
	public virtual BitmapCodecInfo CodecInfo => _container == ImageContainer.Unknown
		? throw new NotSupportedException("A derived encoder must override CodecInfo.")
		: BuiltInCodecInfo.For(_container, false);

	/// <summary>Container level metadata (stored, not written by the built-in codecs).</summary>
	public virtual BitmapMetadata? Metadata { get; set; }

	/// <summary>Container level palette (stored only).</summary>
	public virtual BitmapPalette? Palette { get; set; }

	/// <summary>Container level thumbnail (stored only).</summary>
	public virtual BitmapSource? Thumbnail { get; set; }

	/// <summary>Container level preview (stored only).</summary>
	public virtual BitmapSource? Preview { get; set; }

	/// <summary>Container level colour profiles (stored only).</summary>
	public virtual ReadOnlyCollection<ColorContext>? ColorContexts { get; set; }

	/// <summary>Writes the encoded image to <paramref name="stream"/>.</summary>
	/// <exception cref="NotSupportedException">There are no frames, or the codec cannot write them.</exception>
	/// <exception cref="InvalidOperationException">The encoder already saved.</exception>
	public virtual void Save(Stream stream)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));
		if (_saved)
			throw new InvalidOperationException("The encoder has already saved.");
		if (Frames.Count == 0)
			throw new NotSupportedException("The encoder has no frames to save.");
		var images = new List<DecodedImage>();
		foreach (var frame in Frames)
		{
			var image = DecodedImage.FromSource(frame);
			image.Text = TextOf(frame.Metadata as BitmapMetadata);
			images.Add(image);
		}
		byte[] bytes = Encode(images);
		stream.Write(bytes, 0, bytes.Length);
		_saved = true;
	}

	internal virtual byte[] Encode(IList<DecodedImage> frames) =>
		throw new NotSupportedException("A derived encoder must override Save.");

	internal static void RequireSingle(IList<DecodedImage> frames, string codec)
	{
		if (frames.Count > 1)
			throw new NotSupportedException($"{codec} stores a single frame here; add one frame.");
	}

	static Dictionary<string, string>? TextOf(BitmapMetadata? metadata)
	{
		if (metadata == null)
			return null;
		var text = new Dictionary<string, string>();
		foreach (var kv in metadata.Entries)
		{
			if (kv.Key.StartsWith("/Text/", StringComparison.OrdinalIgnoreCase) && kv.Value is string s)
				text[kv.Key.Substring(6)] = s;
		}
		return text;
	}
}
