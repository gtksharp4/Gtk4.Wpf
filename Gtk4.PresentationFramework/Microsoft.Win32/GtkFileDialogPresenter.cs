using System;
using System.IO;
using System.Windows;

namespace Microsoft.Win32;

/// <summary>Drives <c>Gtk.FileDialog</c> synchronously: the async call runs inside a nested GLib loop.</summary>
/// <remarks>Needs a running GTK display and the GTK thread; no test opens it.</remarks>
sealed class GtkFileDialogPresenter : IFileDialogPresenter
{
	public string[]? Show(FileDialogRequest request)
	{
		var dialog = new Gtk.FileDialog { Title = request.Title, Modal = true };
		if (request.InitialDirectory.Length > 0)
			dialog.InitialFolder = GLib.FileFactory.NewForPath(request.InitialDirectory);
		if (request.Kind == FileDialogKind.Save && request.FileName.Length > 0)
			dialog.InitialName = Path.GetFileName(request.FileName);
		if (request.Kind != FileDialogKind.Folder)
			AddFilters(dialog, request);

		string[]? result = null;
		var loop = new GLib.MainLoop();
		GLib.AsyncReadyCallback done = (source, async, data) =>
		{
			try { result = Finish(dialog, request, async); }
			catch (GLib.GException) { result = null; } // dismissed by the user
			finally { loop.Quit(); }
		};
		var parent = request.Owner?.Widget as Gtk.Window;
		Start(dialog, request, parent, done);
		loop.Run();
		return result;
	}

	static void AddFilters(Gtk.FileDialog dialog, FileDialogRequest request)
	{
		if (request.Filters.Count == 0)
			return;
		var store = new GLib.ListStore(Gtk.FileFilter.GType);
		Gtk.FileFilter? selected = null;
		for (int i = 0; i < request.Filters.Count; i++)
		{
			var filter = new Gtk.FileFilter { Name = request.Filters[i].Name };
			foreach (var pattern in request.Filters[i].Patterns)
				AddPattern(filter, pattern);
			store.Append(filter.Handle);
			if (i == request.FilterIndex)
				selected = filter;
		}
		dialog.Filters = store;
		if (selected != null)
			dialog.DefaultFilter = selected;
	}

	// "*.png" becomes a case-insensitive suffix; anything else is a glob.
	static void AddPattern(Gtk.FileFilter filter, string pattern)
	{
		if (pattern.StartsWith("*.", StringComparison.Ordinal) && pattern.IndexOfAny(new[] { '*', '?' }, 2) < 0)
			filter.AddSuffix(pattern.Substring(2));
		else
			filter.AddPattern(pattern);
	}

	static void Start(Gtk.FileDialog dialog, FileDialogRequest request, Gtk.Window? parent, GLib.AsyncReadyCallback done)
	{
		switch (request.Kind)
		{
			case FileDialogKind.Save:
				dialog.Save(parent, null!, done);
				break;
			case FileDialogKind.Folder when request.Multiselect:
				dialog.SelectMultipleFolders(parent, null!, done);
				break;
			case FileDialogKind.Folder:
				dialog.SelectFolder(parent, null!, done);
				break;
			default:
				if (request.Multiselect) dialog.OpenMultiple(parent, null!, done);
				else dialog.Open(parent, null!, done);
				break;
		}
	}

	static string[]? Finish(Gtk.FileDialog dialog, FileDialogRequest request, GLib.IAsyncResult async)
	{
		if (request.Kind == FileDialogKind.Save)
			return One(dialog.SaveFinish(async));
		if (request.Kind == FileDialogKind.Folder)
			return request.Multiselect ? Many(dialog.SelectMultipleFoldersFinish(async)) : One(dialog.SelectFolderFinish(async));
		return request.Multiselect ? Many(dialog.OpenMultipleFinish(async)) : One(dialog.OpenFinish(async));
	}

	static string[]? One(GLib.IFile? file) => file?.Path is { } path ? new[] { path } : null;

	static string[] Many(GLib.IListModel model)
	{
		var paths = new System.Collections.Generic.List<string>();
		for (uint i = 0; i < model.NItems; i++)
		{
			var path = GLib.FileAdapter.GetObject(model.GetObject(i))?.Path;
			if (path != null)
				paths.Add(path);
		}
		return paths.ToArray();
	}
}
