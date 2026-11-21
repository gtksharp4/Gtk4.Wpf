using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Microsoft.Win32;

/// <summary>Base class of <see cref="OpenFileDialog"/> and <see cref="SaveFileDialog"/>.</summary>
/// <remarks>
/// GTK's file dialog has no equivalent for <see cref="DereferenceLinks"/>, <see cref="ShowHiddenItems"/>,
/// <see cref="CustomPlaces"/>, <see cref="ClientGuid"/>, <see cref="RootDirectory"/> and
/// <see cref="ValidateNames"/>; they are stored and have no effect on the native dialog.
/// </remarks>
public abstract partial class FileDialog : CommonDialog
{
	string[]? _fileNames;
	string? _filter;
	string _defaultExt = "";

	/// <summary>Creates a dialog with default values.</summary>
	protected FileDialog()
	{
		Initialize();
	}

	/// <summary>Whether the default extension is appended to a name that has none. Default true.</summary>
	public bool AddExtension { get; set; }

	/// <summary>Whether the chosen file must exist. Open dialogs default to true, save dialogs to false.</summary>
	public virtual bool CheckFileExists { get; set; }

	/// <summary>Whether the folder of the chosen file must exist. Default true.</summary>
	public virtual bool CheckPathExists { get; set; }

	/// <summary>An id that scopes the dialog's remembered state. Store-only.</summary>
	public Guid? ClientGuid { get; set; }

	/// <summary>Extra folders for the places list. Store-only.</summary>
	public IList<FileDialogCustomPlace> CustomPlaces { get; set; } = new List<FileDialogCustomPlace>();

	/// <summary>The folder used when <see cref="InitialDirectory"/> is empty.</summary>
	public string DefaultDirectory { get; set; } = "";

	/// <summary>The extension appended by <see cref="AddExtension"/>, without a leading dot.</summary>
	public string DefaultExt
	{
		get => _defaultExt;
		set => _defaultExt = (value ?? "").TrimStart('.');
	}

	/// <summary>Whether shortcuts resolve to their targets. Default true. Store-only.</summary>
	public bool DereferenceLinks { get; set; }

	/// <summary>The first chosen file, or empty. Setting it selects exactly that name.</summary>
	public string FileName
	{
		get => _fileNames != null && _fileNames.Length > 0 ? _fileNames[0] : "";
		set => _fileNames = value == null ? null : new[] { value };
	}

	/// <summary>Every chosen file; a copy, empty when nothing is chosen.</summary>
	public string[] FileNames => _fileNames == null ? new string[0] : (string[])_fileNames.Clone();

	/// <summary>The filter string, <c>Name|pattern;pattern|Name|pattern</c>. Empty means no filter.</summary>
	/// <exception cref="ArgumentException">The string does not hold name and pattern pairs.</exception>
	public string Filter
	{
		get => _filter ?? "";
		set
		{
			string? updated = value;
			if (value != null)
			{
				if (value.Length == 0)
					updated = null;
				else if (value.Split('|').Length % 2 != 0)
					throw new ArgumentException(
						"The filter string is not valid. Use pairs of a name and a pattern separated by a bar.",
						nameof(value));
			}
			_filter = updated;
		}
	}

	/// <summary>The one-based index of the selected filter. Default 1.</summary>
	public int FilterIndex { get; set; }

	/// <summary>The folder the dialog opens in.</summary>
	public string InitialDirectory { get; set; } = "";

	/// <summary>The top of the browsable tree. Store-only.</summary>
	public string RootDirectory { get; set; } = "";

	/// <summary>The chosen file names without their folders.</summary>
	public string[] SafeFileNames => FileNames.Select(n => Path.GetFileName(n) ?? "").ToArray();

	/// <summary>The first chosen file name without its folder, or empty.</summary>
	public string SafeFileName => SafeFileNames.FirstOrDefault() ?? "";

	/// <summary>Whether hidden files are listed. Store-only.</summary>
	public bool ShowHiddenItems { get; set; }

	/// <summary>The dialog title; empty keeps the system default.</summary>
	public string Title { get; set; } = "";

	/// <summary>Whether names are validated before the dialog closes. Store-only.</summary>
	public bool ValidateNames { get; set; }

	/// <summary>Raised when the user accepts a selection; setting Cancel keeps the dialog open.</summary>
	public event CancelEventHandler? FileOk;

	/// <summary>Raises <see cref="FileOk"/>.</summary>
	protected virtual void OnFileOk(CancelEventArgs e) => FileOk?.Invoke(this, e);

	internal void RaiseFileOk(CancelEventArgs e) => OnFileOk(e);

	/// <summary>Restores every property to its default.</summary>
	public override void Reset()
	{
		Initialize();
	}

	void Initialize()
	{
		_fileNames = null;
		_filter = null;
		_defaultExt = "";
		AddExtension = true;
		CheckPathExists = true;
		CheckFileExists = InitialCheckFileExists;
		ClientGuid = null;
		CustomPlaces = new List<FileDialogCustomPlace>();
		DefaultDirectory = "";
		DereferenceLinks = true;
		FilterIndex = 1;
		InitialDirectory = "";
		RootDirectory = "";
		ShowHiddenItems = false;
		Title = "";
		ValidateNames = true;
		OnReset();
	}

	// Derived dialogs pick the CheckFileExists default and reset their own properties.
	internal virtual bool InitialCheckFileExists => false;

	internal virtual void OnReset()
	{
	}

	/// <inheritdoc/>
	public override string ToString() => base.ToString() + ": Title: " + Title + ", FileName: " + FileName;

	internal void SetSelection(string[]? names) => _fileNames = names;

	internal string[]? Selection => _fileNames;
}
