using System.Collections.Specialized;
using System.IO;
using System.Windows.Media.Imaging;

namespace System.Windows;

public static partial class Clipboard
{
	/// <summary>Returns whether the clipboard holds wave audio.</summary>
	public static bool ContainsAudio() => Backend.ContainsData(DataFormats.WaveAudio);

	/// <summary>Returns whether the clipboard holds a bitmap.</summary>
	public static bool ContainsImage() => Backend.ContainsData(DataFormats.Bitmap);

	/// <summary>Returns whether the clipboard holds a list of files.</summary>
	public static bool ContainsFileDropList() => Backend.ContainsData(DataFormats.FileDrop);

	/// <summary>Returns the bitmap on the clipboard, or <see langword="null"/>.</summary>
	public static BitmapSource? GetImage() => Backend.GetData(DataFormats.Bitmap) as BitmapSource;

	/// <summary>Returns the file paths on the clipboard; empty when there are none.</summary>
	public static StringCollection GetFileDropList()
	{
		var list = new StringCollection();
		if (Backend.GetData(DataFormats.FileDrop) is string[] paths)
			list.AddRange(paths);
		return list;
	}

	/// <summary>Returns the wave audio on the clipboard as a stream, or <see langword="null"/>.</summary>
	public static Stream? GetAudioStream() => Backend.GetData(DataFormats.WaveAudio) as Stream;

	/// <summary>Replaces the clipboard content with <paramref name="image"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
	public static void SetImage(BitmapSource image)
	{
		ArgumentNullException.ThrowIfNull(image);
		Backend.SetDataObject(new DataObject(DataFormats.Bitmap, image));
	}

	/// <summary>Replaces the clipboard content with a list of file paths.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="fileDropList"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="fileDropList"/> is empty.</exception>
	public static void SetFileDropList(StringCollection fileDropList)
	{
		ArgumentNullException.ThrowIfNull(fileDropList);
		if (fileDropList.Count == 0)
			throw new ArgumentException("The file drop list is empty.", nameof(fileDropList));
		var paths = new string[fileDropList.Count];
		fileDropList.CopyTo(paths, 0);
		Backend.SetDataObject(new DataObject(DataFormats.FileDrop, paths));
	}

	/// <summary>Replaces the clipboard content with wave audio.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="audioBytes"/> is null.</exception>
	public static void SetAudio(byte[] audioBytes)
	{
		ArgumentNullException.ThrowIfNull(audioBytes);
		SetAudio(new MemoryStream(audioBytes));
	}

	/// <summary>Replaces the clipboard content with wave audio read from <paramref name="audioStream"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="audioStream"/> is null.</exception>
	public static void SetAudio(Stream audioStream)
	{
		ArgumentNullException.ThrowIfNull(audioStream);
		Backend.SetDataObject(new DataObject(DataFormats.WaveAudio, audioStream));
	}
}
