using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.Win32;

/// <summary>Lets the user choose one or more folders.</summary>
/// <remarks>
/// GTK's folder dialog has no equivalent for <see cref="AddToRecent"/>, <see cref="ClientGuid"/>,
/// <see cref="DefaultDirectory"/> beyond the start folder, <see cref="DereferenceLinks"/>,
/// <see cref="RootDirectory"/>, <see cref="ShowHiddenItems"/> and <see cref="ValidateNames"/>; they are store-only.
/// </remarks>
public sealed class OpenFolderDialog : CommonDialog
{
	string[]? _folderNames;

	/// <summary>Creates a dialog with default values.</summary>
	public OpenFolderDialog()
	{
		Reset();
	}

	/// <summary>Whether the folder is added to the recent list. Default true. Store-only.</summary>
	public bool AddToRecent { get; set; }

	/// <summary>An id that scopes the dialog's remembered state. Store-only.</summary>
	public Guid? ClientGuid { get; set; }

	/// <summary>The folder used when <see cref="InitialDirectory"/> is empty.</summary>
	public string DefaultDirectory { get; set; } = "";

	/// <summary>Whether shortcuts resolve to their targets. Default true. Store-only.</summary>
	public bool DereferenceLinks { get; set; }

	/// <summary>The first chosen folder, or empty.</summary>
	public string FolderName
	{
		get => _folderNames != null && _folderNames.Length > 0 ? _folderNames[0] : "";
		set => _folderNames = value == null ? null : new[] { value };
	}

	/// <summary>Every chosen folder; a copy, empty when nothing is chosen.</summary>
	public string[] FolderNames => _folderNames == null ? new string[0] : (string[])_folderNames.Clone();

	/// <summary>The folder the dialog opens in.</summary>
	public string InitialDirectory { get; set; } = "";

	/// <summary>Whether several folders may be chosen. Default false.</summary>
	public bool Multiselect { get; set; }

	/// <summary>The top of the browsable tree. Store-only.</summary>
	public string RootDirectory { get; set; } = "";

	/// <summary>Whether hidden folders are listed. Store-only.</summary>
	public bool ShowHiddenItems { get; set; }

	/// <summary>The dialog title; empty keeps the system default.</summary>
	public string Title { get; set; } = "";

	/// <summary>Whether names are validated before the dialog closes. Store-only.</summary>
	public bool ValidateNames { get; set; }

	/// <summary>Raised when the user accepts a selection; setting Cancel keeps the dialog open.</summary>
	public event CancelEventHandler? FolderOk;

	/// <summary>Raises <see cref="FolderOk"/>.</summary>
	void OnFolderOk(CancelEventArgs e) => FolderOk?.Invoke(this, e);

	/// <summary>Restores every property to its default.</summary>
	public override void Reset()
	{
		_folderNames = null;
		AddToRecent = true;
		ClientGuid = null;
		DefaultDirectory = "";
		DereferenceLinks = true;
		InitialDirectory = "";
		Multiselect = false;
		RootDirectory = "";
		ShowHiddenItems = false;
		Title = "";
		ValidateNames = true;
	}

	/// <inheritdoc/>
	protected override bool RunDialog(IntPtr hwndOwner)
	{
		while (true)
		{
			var request = new FileDialogRequest
			{
				Kind = FileDialogKind.Folder,
				Title = Title,
				InitialDirectory = InitialDirectory.Length > 0 ? InitialDirectory : DefaultDirectory,
				FileName = FolderName,
				Multiselect = Multiselect,
				Owner = OwnerWindow,
			};
			var picked = Presenter.Show(request);
			if (picked == null || picked.Length == 0)
				return false;
			var previous = _folderNames;
			_folderNames = picked.ToArray();
			var args = new CancelEventArgs();
			OnFolderOk(args);
			if (!args.Cancel)
				return true;
			_folderNames = previous;
		}
	}
}
