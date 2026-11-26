using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class DuplicateXamlItemTests : IDisposable
{
	const string Head = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
		+ "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

	readonly string _dir = Path.Combine(Path.GetTempPath(), "gf48-" + Guid.NewGuid().ToString("N"));
	readonly Review47TaskTests.FakeEngine _engine = new Review47TaskTests.FakeEngine();

	public DuplicateXamlItemTests() => Directory.CreateDirectory(_dir);

	public void Dispose() => Directory.Delete(_dir, true);

	string Write(string relative, string xaml)
	{
		string path = Path.Combine(_dir, relative.Replace('/', Path.DirectorySeparatorChar));
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, xaml);
		return path;
	}

	MarkupCompileTask Task(params string[] paths) => new MarkupCompileTask
	{
		BuildEngine = _engine, RootNamespace = "A", ProjectDirectory = _dir,
		XamlFiles = paths.Select(p => (ITaskItem)new TaskItem(p)).ToArray(),
		OutputDirectory = Path.Combine(_dir, "obj", "gen"),
	};

	[Fact]
	public void Differently_spelled_paths_to_one_file_generate_once_and_log_the_duplicate()
	{
		string path = Write("Pages/A.xaml", $"<Window x:Class=\"A.Pages.A\" {Head}/>");
		string dotted = Path.Combine(Path.GetDirectoryName(path)!, "..", "Pages", "A.xaml");
		var task = Task(path, dotted);

		Assert.True(task.Execute(), string.Join("; ", _engine.Errors));
		Assert.Single(task.GeneratedFiles);
		var note = _engine.Messages.Single(m => m.Message!.Contains("duplicate"));
		Assert.Equal(MessageImportance.Low, note.Importance);
	}

	[Fact]
	public void Case_differing_names_are_distinct_files_on_case_sensitive_systems()
	{
		Assert.SkipUnless(
			FileSystemCase.IsCaseSensitive(_dir), "Case-insensitive file system.");
		string upper = Write("Pages/A.xaml", $"<Window x:Class=\"A.Pages.A\" {Head}/>");
		string lower = Write("Pages/a.xaml", $"<Window x:Class=\"A.Pages.Lower\" {Head}/>");
		var task = Task(upper, lower);

		Assert.True(task.Execute(), string.Join("; ", _engine.Errors));
		Assert.Equal(2, task.GeneratedFiles.Length);
		Assert.DoesNotContain(_engine.Messages, m => m.Message!.Contains("duplicate"));
	}

	[Fact]
	public void Case_variant_spellings_of_one_file_generate_once_on_case_insensitive_systems()
	{
		Assert.SkipWhen(FileSystemCase.IsCaseSensitive(_dir), "Case-sensitive file system.");
		string upper = Write("Pages/A.xaml", $"<Window x:Class=\"A.Pages.A\" {Head}/>");
		string lower = Path.Combine(Path.GetDirectoryName(upper)!, "a.xaml");
		var task = Task(upper, lower);

		Assert.True(task.Execute(), string.Join("; ", _engine.Errors));
		Assert.Single(task.GeneratedFiles);
	}

	[Fact]
	public void The_probe_leaves_no_file_behind()
	{
		_ = FileSystemCase.IsCaseSensitive(_dir);
		Assert.Empty(Directory.GetFiles(_dir));
	}
}
