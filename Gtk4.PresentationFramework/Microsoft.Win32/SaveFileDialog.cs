using System;
using System.IO;

namespace Microsoft.Win32;

/// <summary>Lets the user choose a file name to save to.</summary>
public sealed class SaveFileDialog : FileDialog
{
	/// <summary>Creates a dialog with default values.</summary>
	public SaveFileDialog()
	{
	}

	/// <summary>Whether to ask before creating a file that does not exist. Default false.</summary>
	public bool CreatePrompt { get; set; }

	/// <summary>Whether to ask before replacing an existing file. Default true.</summary>
	public bool OverwritePrompt { get; set; }

	/// <summary>Whether to create and delete a test file to prove the location is writable. Default false.</summary>
	public bool CreateTestFile { get; set; }

	internal override FileDialogKind Kind => FileDialogKind.Save;

	internal override void OnReset()
	{
		CreatePrompt = false;
		OverwritePrompt = true;
		CreateTestFile = false;
	}

	/// <summary>Creates or truncates the chosen file and opens it for writing.</summary>
	/// <exception cref="InvalidOperationException">No file was chosen.</exception>
	public Stream OpenFile()
	{
		if (string.IsNullOrEmpty(FileName))
			throw new InvalidOperationException("No file name was chosen.");
		return new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite);
	}

	internal override bool ValidateSelection(string path)
	{
		if (!base.ValidateSelection(path))
			return false;
		bool exists = File.Exists(path);
		if (exists && OverwritePrompt && !Confirm(path + " already exists.\nDo you want to replace it?"))
			return false;
		if (!exists && CreatePrompt && !Confirm(path + " does not exist.\nDo you want to create it?"))
			return false;
		return !CreateTestFile || exists || TryCreateTestFile(path);
	}

	bool TryCreateTestFile(string path)
	{
		try
		{
			using (new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
			}
			File.Delete(path);
			return true;
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			return Inform(path + "\nThe file cannot be created at this location.");
		}
	}
}
