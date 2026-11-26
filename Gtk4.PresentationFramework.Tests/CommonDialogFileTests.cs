using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public sealed partial class CommonDialogTests : IDisposable
{
	[Fact]
	public void Save_appends_the_default_extension_and_prompts_before_overwriting()
	{
		var existing = Touch("exists.txt");
		_presenter.Answers.Enqueue(new[] { Path.Combine(_dir, "exists") });
		_box.Answer = MessageBoxResult.Yes;
		var dialog = new SaveFileDialog { DefaultExt = "txt" };
		Assert.True(dialog.ShowDialog());
		Assert.Equal(existing, dialog.FileName);
		Assert.Equal(1, _box.Shown);

		_presenter.Answers.Enqueue(new[] { existing });
		_presenter.Answers.Enqueue(null);
		_box.Answer = MessageBoxResult.No;
		Assert.False(dialog.ShowDialog());
	}

	[Fact]
	public void Save_CreatePrompt_asks_for_a_new_file_and_OpenFile_creates_it()
	{
		var target = Path.Combine(_dir, "new.txt");
		_presenter.Answers.Enqueue(new[] { target });
		var dialog = new SaveFileDialog { CreatePrompt = true, CreateTestFile = true };
		Assert.True(dialog.ShowDialog());
		Assert.Equal(1, _box.Shown);
		Assert.False(File.Exists(target));
		using (var stream = dialog.OpenFile())
			stream.WriteByte(1);
		Assert.True(File.Exists(target));
	}

	[Fact]
	public void OpenFile_without_a_name_throws()
	{
		Assert.Throws<InvalidOperationException>(() => new OpenFileDialog().OpenFile());
		Assert.Throws<InvalidOperationException>(() => new SaveFileDialog().OpenFile());
		Assert.Throws<InvalidOperationException>(() => new OpenFileDialog().OpenFiles());
	}

	[Fact]
	public void Reset_restores_defaults_but_keeps_Tag()
	{
		var dialog = new OpenFileDialog
		{
			Filter = "A|*.a", FilterIndex = 3, Title = "x", FileName = "f", Multiselect = true, Tag = "tag",
		};
		dialog.Reset();
		Assert.Equal("", dialog.Filter);
		Assert.Equal(1, dialog.FilterIndex);
		Assert.Equal("", dialog.Title);
		Assert.Equal("", dialog.FileName);
		Assert.False(dialog.Multiselect);
		Assert.True(dialog.CheckFileExists);
		Assert.Equal("tag", dialog.Tag);
	}

	[Fact]
	public void Folder_dialog_returns_every_chosen_folder_and_raises_FolderOk()
	{
		_presenter.Answers.Enqueue(new[] { _dir, Path.GetTempPath() });
		var dialog = new OpenFolderDialog { Multiselect = true, Title = "pick" };
		int raised = 0;
		dialog.FolderOk += (s, e) => raised++;
		Assert.True(dialog.ShowDialog());
		Assert.Equal(1, raised);
		Assert.Equal(_dir, dialog.FolderName);
		Assert.Equal(2, dialog.FolderNames.Length);
		Assert.Equal(FileDialogKind.Folder, _presenter.Last!.Kind);
		Assert.True(_presenter.Last.Multiselect);
		Assert.False(dialog.ShowDialog());
	}

	[Fact]
	public void ShowDialog_with_a_null_owner_throws()
	{
		Assert.Throws<ArgumentNullException>(() => new OpenFileDialog().ShowDialog(null!));
	}

	[Fact]
	public void ToString_names_title_and_file()
	{
		var dialog = new OpenFileDialog { Title = "T", FileName = "f" };
		Assert.EndsWith(": Title: T, FileName: f", dialog.ToString());
	}
}
