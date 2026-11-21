using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.Win32;

public abstract partial class FileDialog
{
	internal virtual FileDialogKind Kind => FileDialogKind.Open;

	internal virtual bool AllowMultiple => false;

	/// <inheritdoc/>
	protected override bool RunDialog(IntPtr hwndOwner)
	{
		while (true)
		{
			var picked = Presenter.Show(CreateRequest());
			if (picked == null || picked.Length == 0)
				return false;
			if (Accept(picked))
				return true;
		}
	}

	internal FileDialogRequest CreateRequest()
	{
		var filters = ParseFilter(Filter);
		string directory = InitialDirectory.Length > 0 ? InitialDirectory : DefaultDirectory;
		return new FileDialogRequest
		{
			Kind = Kind,
			Title = Title,
			InitialDirectory = directory,
			FileName = FileName,
			Multiselect = AllowMultiple,
			Filters = filters,
			FilterIndex = FilterIndex >= 1 && FilterIndex <= filters.Count ? FilterIndex - 1 : 0,
			Owner = OwnerWindow,
		};
	}

	// Applies the extension, validates each name and raises FileOk; false keeps the dialog open.
	bool Accept(string[] picked)
	{
		var names = picked.Select(AppendDefaultExtension).ToArray();
		if (!names.All(ValidateSelection))
			return false;
		var previous = Selection;
		SetSelection(names);
		var args = new CancelEventArgs();
		RaiseFileOk(args);
		if (args.Cancel)
			SetSelection(previous);
		return !args.Cancel;
	}

	string AppendDefaultExtension(string name) =>
		AddExtension && DefaultExt.Length > 0 && !Path.HasExtension(name) ? name + "." + DefaultExt : name;

	/// <summary>Checks one chosen path; shows a message and returns false when it is not acceptable.</summary>
	internal virtual bool ValidateSelection(string path)
	{
		if (CheckPathExists)
		{
			var folder = Path.GetDirectoryName(Path.GetFullPath(path));
			if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
				return Inform(path + "\nThe folder does not exist. Check the path and try again.");
		}
		if (CheckFileExists && !File.Exists(path))
			return Inform(path + "\nThe file does not exist. Check the file name and try again.");
		return true;
	}

	internal bool Inform(string message)
	{
		MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
		return false;
	}

	internal bool Confirm(string message) =>
		MessageBox.Show(message, Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
}
