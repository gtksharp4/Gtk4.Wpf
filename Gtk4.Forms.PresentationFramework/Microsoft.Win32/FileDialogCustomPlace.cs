using System;

namespace Microsoft.Win32;

/// <summary>A folder offered in the dialog places list. GTK has no places API for its file dialog, so this is store-only.</summary>
public sealed class FileDialogCustomPlace
{
	/// <summary>Creates a place from a folder path.</summary>
	public FileDialogCustomPlace(string path)
	{
		Path = path ?? "";
		KnownFolder = Guid.Empty;
	}

	/// <summary>Creates a place from a Windows known-folder id.</summary>
	public FileDialogCustomPlace(Guid knownFolder)
	{
		Path = "";
		KnownFolder = knownFolder;
	}

	/// <summary>The folder path, or empty for a known folder.</summary>
	public string Path { get; set; }

	/// <summary>The known-folder id, or <see cref="Guid.Empty"/> for a path.</summary>
	public Guid KnownFolder { get; set; }
}
