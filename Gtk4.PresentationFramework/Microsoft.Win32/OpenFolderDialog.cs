using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.Win32;

/// <summary>Lets the user choose one or more folders.</summary>
/// <remarks>
/// GTK's folder dialog has no equivalent for <see cref="CommonItemDialog.AddToRecent"/>, <see cref="ClientGuid"/>,
/// <see cref="DefaultDirectory"/> beyond the start folder, <see cref="DereferenceLinks"/>,
/// <see cref="RootDirectory"/>, <see cref="ShowHiddenItems"/> and <see cref="ValidateNames"/>; they are store-only.
/// </remarks>
public sealed class OpenFolderDialog : CommonItemDialog
{
	string[]? _folderNames;

	/// <summary>Creates a dialog with default values.</summary>
	public OpenFolderDialog()
	{
		Reset();
	}

	/// <summary>The last segment of <see cref="FolderName"/>, or empty.</summary>
	public string SafeFolderName => SafeName(FolderName);

	/// <summary>The last segment of every chosen folder.</summary>
	public string[] SafeFolderNames => FolderNames.Select(SafeName).ToArray();

	// Both separators end a segment on every OS, so a Windows path and a Unix path both give their last name.
	static readonly char[] _separators = { '/', (char)92 };

	static string SafeName(string path)
	{
		var trimmed = path.TrimEnd(_separators);
		return trimmed.Substring(trimmed.LastIndexOfAny(_separators) + 1);
	}

	/// <summary>The first chosen folder, or empty.</summary>
	public string FolderName
	{
		get => _folderNames != null && _folderNames.Length > 0 ? _folderNames[0] : "";
		set => _folderNames = value == null ? null : new[] { value };
	}

	/// <summary>Every chosen folder; a copy, empty when nothing is chosen.</summary>
	public string[] FolderNames => _folderNames == null ? new string[0] : (string[])_folderNames.Clone();

	/// <summary>Whether several folders may be chosen. Default false.</summary>
	public bool Multiselect { get; set; }

	/// <summary>Raised when the user accepts a selection; setting Cancel keeps the dialog open.</summary>
	public event CancelEventHandler? FolderOk;

	/// <summary>Raises <see cref="FolderOk"/>.</summary>
	void OnFolderOk(CancelEventArgs e) => FolderOk?.Invoke(this, e);

	/// <summary>Restores every property to its default.</summary>
	public override void Reset()
	{
		base.Reset();
		_folderNames = null;
		Multiselect = false;
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
