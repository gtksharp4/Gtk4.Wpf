using System;
using System.Collections.Generic;

namespace Microsoft.Win32;

/// <summary>
/// Base class of the dialogs that pick file-system items: <see cref="FileDialog"/> and <see cref="OpenFolderDialog"/>.
/// </summary>
/// <remarks>
/// GTK's dialogs have no equivalent for <see cref="AddToRecent"/>, <see cref="DereferenceLinks"/>,
/// <see cref="ShowHiddenItems"/>, <see cref="CustomPlaces"/>, <see cref="ClientGuid"/>,
/// <see cref="RootDirectory"/> and <see cref="ValidateNames"/>; they are stored and
/// have no effect on the native dialog.
/// </remarks>
public abstract class CommonItemDialog : CommonDialog
{
	/// <summary>Creates a dialog with default values.</summary>
	protected CommonItemDialog()
	{
		ResetItemDefaults();
	}

	/// <summary>Whether the chosen item is added to the recent list. Default true. Store-only.</summary>
	public bool AddToRecent { get; set; }

	/// <summary>An id that scopes the dialog's remembered state. Store-only.</summary>
	public Guid? ClientGuid { get; set; }

	/// <summary>Extra folders for the places list. Store-only.</summary>
	public IList<FileDialogCustomPlace> CustomPlaces { get; set; } = new List<FileDialogCustomPlace>();

	/// <summary>The folder used when <see cref="InitialDirectory"/> is empty.</summary>
	public string DefaultDirectory { get; set; } = "";

	/// <summary>Whether shortcuts resolve to their targets. Default true. Store-only.</summary>
	public bool DereferenceLinks { get; set; }

	/// <summary>The folder the dialog opens in.</summary>
	public string InitialDirectory { get; set; } = "";

	/// <summary>The top of the browsable tree. Store-only.</summary>
	public string RootDirectory { get; set; } = "";

	/// <summary>Whether hidden items are listed. Store-only.</summary>
	public bool ShowHiddenItems { get; set; }

	/// <summary>The dialog title; empty keeps the system default.</summary>
	public string Title { get; set; } = "";

	/// <summary>Whether names are validated before the dialog closes. Store-only.</summary>
	public bool ValidateNames { get; set; }

	/// <summary>Restores every property to its default.</summary>
	public override void Reset() => ResetItemDefaults();

	void ResetItemDefaults()
	{
		AddToRecent = true;
		ClientGuid = null;
		CustomPlaces = new List<FileDialogCustomPlace>();
		DefaultDirectory = "";
		DereferenceLinks = true;
		InitialDirectory = "";
		RootDirectory = "";
		ShowHiddenItems = false;
		Title = "";
		ValidateNames = true;
	}
}
