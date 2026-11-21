using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Win32;

internal enum FileDialogKind
{
	Open,
	Save,
	Folder,
}

/// <summary>One entry of a WPF filter string: a display name and its patterns.</summary>
internal sealed class FileDialogFilter
{
	internal FileDialogFilter(string name, IReadOnlyList<string> patterns)
	{
		Name = name;
		Patterns = patterns;
	}

	internal string Name { get; }

	internal IReadOnlyList<string> Patterns { get; }
}

internal sealed class FileDialogRequest
{
	internal FileDialogKind Kind { get; set; }

	internal string Title { get; set; } = "";

	internal string InitialDirectory { get; set; } = "";

	internal string FileName { get; set; } = "";

	internal bool Multiselect { get; set; }

	internal IReadOnlyList<FileDialogFilter> Filters { get; set; } = new FileDialogFilter[0];

	/// <summary>Zero-based index into <see cref="Filters"/>, already range-checked.</summary>
	internal int FilterIndex { get; set; }

	internal Window? Owner { get; set; }
}

/// <summary>Shows the native dialog and returns the chosen paths, or null when it was cancelled.</summary>
internal interface IFileDialogPresenter
{
	string[]? Show(FileDialogRequest request);
}
