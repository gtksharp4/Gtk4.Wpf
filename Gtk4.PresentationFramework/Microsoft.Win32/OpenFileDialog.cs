using System;
using System.IO;
using System.Linq;

namespace Microsoft.Win32;

/// <summary>Lets the user choose one or more existing files.</summary>
/// <remarks>
/// <see cref="ForcePreviewPane"/>, <see cref="ReadOnlyChecked"/> and <see cref="ShowReadOnly"/> are store-only on GTK.
/// </remarks>
public sealed class OpenFileDialog : FileDialog
{
	/// <summary>Creates a dialog with default values.</summary>
	public OpenFileDialog()
	{
	}

	/// <summary>Whether several files may be chosen. Default false.</summary>
	public bool Multiselect { get; set; }

	/// <summary>Whether the read-only box is checked. Default false.</summary>
	public bool ReadOnlyChecked { get; set; }

	/// <summary>Whether the read-only box is shown. Default false.</summary>
	public bool ShowReadOnly { get; set; }

	/// <summary>Whether the preview pane is always shown. Default false.</summary>
	public bool ForcePreviewPane { get; set; }

	internal override bool InitialCheckFileExists => true;

	internal override bool AllowMultiple => Multiselect;

	internal override void OnReset()
	{
		Multiselect = false;
		ReadOnlyChecked = false;
		ShowReadOnly = false;
		ForcePreviewPane = false;
	}

	/// <summary>Opens the chosen file for reading.</summary>
	/// <exception cref="InvalidOperationException">No file was chosen.</exception>
	public Stream OpenFile()
	{
		if (string.IsNullOrEmpty(FileName))
			throw new InvalidOperationException("No file name was chosen.");
		return new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Opens every chosen file for reading.</summary>
	/// <exception cref="InvalidOperationException">No file was chosen.</exception>
	public Stream[] OpenFiles()
	{
		var names = FileNames;
		if (names.Length == 0)
			throw new InvalidOperationException("No file name was chosen.");
		return names.Select(n => (Stream)new FileStream(n, FileMode.Open, FileAccess.Read, FileShare.Read)).ToArray();
	}
}
