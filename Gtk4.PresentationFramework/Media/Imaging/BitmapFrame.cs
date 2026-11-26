using System.Collections.ObjectModel;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>One frame of an image (WPF <c>BitmapFrame</c>).</summary>
public abstract class BitmapFrame : BitmapSource
{
	/// <summary>Creates a frame for a derived class.</summary>
	protected BitmapFrame() { }

	/// <summary>The decoder that produced this frame, or <see langword="null"/> for an in-memory frame.</summary>
	public virtual BitmapDecoder? Decoder => null;

	/// <summary>The thumbnail, or <see langword="null"/>.</summary>
	public virtual BitmapSource? Thumbnail => null;

	/// <summary>The frame metadata, or <see langword="null"/> when the format has none.</summary>
	public override ImageMetadata? Metadata => null;

	/// <summary>The colour profiles of the frame.</summary>
	public virtual ReadOnlyCollection<ColorContext>? ColorContexts => null;

	/// <summary>The base URI the frame was created from.</summary>
	public virtual Uri? BaseUri { get; set; }

	/// <summary>Creates a writer for the metadata of this frame. Only decoder frames have one.</summary>
	/// <exception cref="InvalidOperationException">The frame did not come from a decoder.</exception>
	public virtual InPlaceBitmapMetadataWriter CreateInPlaceBitmapMetadataWriter()
	{
		if (Decoder == null || !(Metadata is BitmapMetadata metadata))
			throw new InvalidOperationException("The frame has no metadata that can be written in place.");
		return new InPlaceBitmapMetadataWriter(metadata);
	}

	/// <summary>Creates a frame over <paramref name="source"/>.</summary>
	public static BitmapFrame Create(BitmapSource source) => Create(source, null, null, null);

	/// <summary>Creates a frame over <paramref name="source"/> with a thumbnail.</summary>
	public static BitmapFrame Create(BitmapSource source, BitmapSource? thumbnail) =>
		Create(source, thumbnail, null, null);

	/// <summary>Creates a frame over <paramref name="source"/> with thumbnail, metadata and colour contexts.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static BitmapFrame Create(BitmapSource source, BitmapSource? thumbnail, BitmapMetadata? metadata,
		ReadOnlyCollection<ColorContext>? colorContexts)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		return new SourceBitmapFrame(source, thumbnail, metadata, colorContexts, null);
	}

	/// <summary>Decodes the first frame of the image at <paramref name="bitmapUri"/>.</summary>
	public static BitmapFrame Create(Uri bitmapUri) =>
		Create(bitmapUri, BitmapCreateOptions.None, BitmapCacheOption.Default);

	/// <summary>Decodes the first frame of the image at <paramref name="bitmapUri"/>.</summary>
	public static BitmapFrame Create(Uri bitmapUri, BitmapCreateOptions createOptions,
		BitmapCacheOption cacheOption) =>
		BitmapDecoder.Create(bitmapUri, createOptions, cacheOption).Frames[0];

	/// <summary>Decodes the first frame of the image in <paramref name="bitmapStream"/>.</summary>
	public static BitmapFrame Create(Stream bitmapStream) =>
		Create(bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

	/// <summary>Decodes the first frame of the image in <paramref name="bitmapStream"/>.</summary>
	public static BitmapFrame Create(Stream bitmapStream, BitmapCreateOptions createOptions,
		BitmapCacheOption cacheOption) =>
		BitmapDecoder.Create(bitmapStream, createOptions, cacheOption).Frames[0];
}

/// <summary>A frame that wraps a bitmap source plus optional extras.</summary>
sealed class SourceBitmapFrame : BitmapFrame
{
	readonly BitmapSource _source;

	internal SourceBitmapFrame(BitmapSource source, BitmapSource? thumbnail, ImageMetadata? metadata,
		ReadOnlyCollection<ColorContext>? colorContexts, BitmapDecoder? decoder)
	{
		_source = source;
		Thumbnail = thumbnail;
		Metadata = metadata;
		ColorContexts = colorContexts;
		Decoder = decoder;
		PixelWidth = source.PixelWidth;
		PixelHeight = source.PixelHeight;
		DpiX = source.DpiX;
		DpiY = source.DpiY;
		Format = source.Format;
		Palette = source.Palette;
	}

	public override BitmapDecoder? Decoder { get; }

	public override BitmapSource? Thumbnail { get; }

	public override ImageMetadata? Metadata { get; }

	public override ReadOnlyCollection<ColorContext>? ColorContexts { get; }

	internal override byte[] GetPixelData() => _source.GetPixelData();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() =>
		new SourceBitmapFrame(_source, Thumbnail, Metadata, ColorContexts, Decoder);
}

/// <summary>Writes metadata back to the file it was read from (WPF <c>InPlaceBitmapMetadataWriter</c>).</summary>
public sealed class InPlaceBitmapMetadataWriter : BitmapMetadata
{
	readonly BitmapMetadata _baseline;

	internal InPlaceBitmapMetadataWriter(BitmapMetadata source) : base(source.Format)
	{
		_baseline = source;
		foreach (var kv in source.Entries)
			SetQuery(kv.Key, kv.Value);
		source.CopyPolicy(this);
	}

	/// <summary>
	/// Returns <see langword="true"/> when there is nothing to write. Like WIC, which reserves no padding
	/// in PNG and JPEG files, it returns <see langword="false"/> once the metadata has changed, and the
	/// file is left untouched.
	/// </summary>
	public bool TrySave() => SameEntriesAs(_baseline);
}
