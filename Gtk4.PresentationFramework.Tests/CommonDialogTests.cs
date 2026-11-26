using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The Microsoft.Win32 dialog object model, driven through a fake presenter.</summary>
public sealed partial class CommonDialogTests : IDisposable
{
	sealed class FakePresenter : IFileDialogPresenter
	{
		public Queue<string[]?> Answers { get; } = new Queue<string[]?>();
		public FileDialogRequest? Last { get; private set; }

		public string[]? Show(FileDialogRequest request)
		{
			Last = request;
			return Answers.Count > 0 ? Answers.Dequeue() : null;
		}
	}

	sealed class FakeBox : IMessageBoxPresenter
	{
		public MessageBoxResult Answer = MessageBoxResult.Yes;
		public int Shown;

		public MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
		{
			Shown++;
			return Answer;
		}
	}

	readonly IFileDialogPresenter _savedPresenter = CommonDialog.Presenter;
	readonly IMessageBoxPresenter _savedBox = MessageBox.Presenter;
	readonly FakePresenter _presenter = new FakePresenter();
	readonly FakeBox _box = new FakeBox();
	readonly string _dir = Path.Combine(Path.GetTempPath(), "cd" + Guid.NewGuid().ToString("N"));

	public CommonDialogTests()
	{
		CommonDialog.Presenter = _presenter;
		MessageBox.Presenter = _box;
		Directory.CreateDirectory(_dir);
	}

	public void Dispose()
	{
		CommonDialog.Presenter = _savedPresenter;
		MessageBox.Presenter = _savedBox;
		Directory.Delete(_dir, true);
	}

	string Touch(string name)
	{
		var path = Path.Combine(_dir, name);
		File.WriteAllText(path, "x");
		return path;
	}

	[Fact]
	public void Defaults_match_wpf()
	{
		var open = new OpenFileDialog();
		Assert.True(open.AddExtension);
		Assert.True(open.CheckFileExists);
		Assert.True(open.CheckPathExists);
		Assert.True(open.DereferenceLinks);
		Assert.True(open.ValidateNames);
		Assert.Equal(1, open.FilterIndex);
		Assert.Equal("", open.Filter);
		Assert.Equal("", open.FileName);
		Assert.Empty(open.FileNames);
		Assert.False(open.Multiselect);
		var save = new SaveFileDialog();
		Assert.False(save.CheckFileExists);
		Assert.True(save.OverwritePrompt);
		Assert.False(save.CreatePrompt);
	}

	[Fact]
	public void Filter_requires_name_and_pattern_pairs()
	{
		var dialog = new OpenFileDialog();
		dialog.Filter = "Text|*.txt|All|*.*";
		Assert.Equal("Text|*.txt|All|*.*", dialog.Filter);
		Assert.Throws<ArgumentException>(() => dialog.Filter = "Text|*.txt|All");
		dialog.Filter = null!;
		Assert.Equal("", dialog.Filter);
	}

	[Fact]
	public void DefaultExt_drops_a_leading_dot()
	{
		var dialog = new SaveFileDialog { DefaultExt = ".txt" };
		Assert.Equal("txt", dialog.DefaultExt);
	}

	[Fact]
	public void Request_carries_filters_with_a_zero_based_clamped_index()
	{
		var dialog = new OpenFileDialog { Filter = "Text|*.txt;*.md|All|*.*", FilterIndex = 2, Title = "T" };
		Assert.False(dialog.ShowDialog());
		var request = _presenter.Last!;
		Assert.Equal(2, request.Filters.Count);
		Assert.Equal(new[] { "*.txt", "*.md" }, request.Filters[0].Patterns);
		Assert.Equal(1, request.FilterIndex);
		Assert.Equal("T", request.Title);
		dialog.FilterIndex = 9;
		dialog.ShowDialog();
		Assert.Equal(0, _presenter.Last!.FilterIndex);
	}

	[Fact]
	public void Cancelling_returns_false_and_keeps_the_name()
	{
		var dialog = new OpenFileDialog { FileName = "keep.txt" };
		Assert.False(dialog.ShowDialog());
		Assert.Equal("keep.txt", dialog.FileName);
	}

	[Fact]
	public void Accepting_sets_names_and_safe_names()
	{
		var a = Touch("a.txt");
		var b = Touch("b.txt");
		_presenter.Answers.Enqueue(new[] { a, b });
		var dialog = new OpenFileDialog { Multiselect = true };
		Assert.True(dialog.ShowDialog());
		Assert.Equal(new[] { a, b }, dialog.FileNames);
		Assert.Equal(a, dialog.FileName);
		Assert.Equal(new[] { "a.txt", "b.txt" }, dialog.SafeFileNames);
		Assert.Equal("a.txt", dialog.SafeFileName);
		Assert.True(_presenter.Last!.Multiselect);
		var streams = dialog.OpenFiles();
		Assert.Equal(2, streams.Length);
		foreach (var s in streams) s.Dispose();
		using (var one = dialog.OpenFile())
			Assert.Equal(1, one.Length);
	}

	[Fact]
	public void FileOk_can_cancel_and_the_dialog_is_shown_again()
	{
		var first = Touch("first.txt");
		var second = Touch("second.txt");
		_presenter.Answers.Enqueue(new[] { first });
		_presenter.Answers.Enqueue(new[] { second });
		var dialog = new OpenFileDialog();
		dialog.FileOk += (s, e) => e.Cancel = dialog.FileName == first;
		Assert.True(dialog.ShowDialog());
		Assert.Equal(second, dialog.FileName);
	}

	[Fact]
	public void CheckFileExists_rejects_a_missing_file_and_asks_again()
	{
		var good = Touch("good.txt");
		_presenter.Answers.Enqueue(new[] { Path.Combine(_dir, "missing.txt") });
		_presenter.Answers.Enqueue(new[] { good });
		var dialog = new OpenFileDialog();
		Assert.True(dialog.ShowDialog());
		Assert.Equal(1, _box.Shown);
		Assert.Equal(good, dialog.FileName);
	}
}
