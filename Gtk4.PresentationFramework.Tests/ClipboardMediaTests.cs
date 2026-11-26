using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Image, file-drop and audio presence and round trips, on the in-memory backend.</summary>
public sealed class ClipboardMediaTests : IDisposable
{
	readonly IClipboardBackend _original;

	public ClipboardMediaTests()
	{
		_original = Clipboard.Backend;
		Clipboard.Backend = new InMemoryClipboardBackend();
	}

	public void Dispose() => Clipboard.Backend = _original;

	[Fact]
	public void Nothing_is_contained_on_an_empty_clipboard()
	{
		Assert.False(Clipboard.ContainsAudio());
		Assert.False(Clipboard.ContainsImage());
		Assert.False(Clipboard.ContainsFileDropList());
		Assert.Null(Clipboard.GetImage());
		Assert.Null(Clipboard.GetAudioStream());
		Assert.Empty(Clipboard.GetFileDropList());
	}

	[Fact]
	public void SetImage_round_trips_and_is_found_by_format()
	{
		var image = new BitmapImage();
		Clipboard.SetImage(image);
		Assert.True(Clipboard.ContainsImage());
		Assert.True(Clipboard.ContainsData(DataFormats.Bitmap));
		Assert.Same(image, Clipboard.GetImage());
		Assert.False(Clipboard.ContainsFileDropList());
	}

	[Fact]
	public void SetFileDropList_round_trips()
	{
		var list = new StringCollection { "a.txt", "b.txt" };
		Clipboard.SetFileDropList(list);
		Assert.True(Clipboard.ContainsFileDropList());
		Assert.True(Clipboard.ContainsData(DataFormats.FileDrop));
		Assert.Equal(new[] { "a.txt", "b.txt" }, Clipboard.GetFileDropList().Cast());
	}

	[Fact]
	public void SetAudio_bytes_and_stream_are_found_as_wave_audio()
	{
		Clipboard.SetAudio(new byte[] { 1, 2, 3 });
		Assert.True(Clipboard.ContainsAudio());
		Assert.True(Clipboard.ContainsData(DataFormats.WaveAudio));
		using (var stream = Clipboard.GetAudioStream()!)
			Assert.Equal(3, stream.Length);
		Clipboard.SetAudio(new MemoryStream(new byte[] { 9 }));
		using (var stream = Clipboard.GetAudioStream()!)
			Assert.Equal(1, stream.Length);
	}

	[Fact]
	public void Setting_one_kind_replaces_the_previous_content()
	{
		Clipboard.SetFileDropList(new StringCollection { "a" });
		Clipboard.SetImage(new BitmapImage());
		Assert.False(Clipboard.ContainsFileDropList());
		Clipboard.Clear();
		Assert.False(Clipboard.ContainsImage());
	}

	[Fact]
	public void Setters_validate_their_arguments()
	{
		Assert.Throws<ArgumentNullException>(() => Clipboard.SetImage(null!));
		Assert.Throws<ArgumentNullException>(() => Clipboard.SetFileDropList(null!));
		Assert.Throws<ArgumentException>(() => Clipboard.SetFileDropList(new StringCollection()));
		Assert.Throws<ArgumentNullException>(() => Clipboard.SetAudio((byte[])null!));
		Assert.Throws<ArgumentNullException>(() => Clipboard.SetAudio((Stream)null!));
	}
}

static class StringCollectionExtensions
{
	internal static string[] Cast(this StringCollection collection)
	{
		var items = new string[collection.Count];
		collection.CopyTo(items, 0);
		return items;
	}
}
