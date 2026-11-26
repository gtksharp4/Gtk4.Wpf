using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class Review47TaskTests : IDisposable
{
	const string Head = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
		+ "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

	readonly string _dir = Path.Combine(Path.GetTempPath(), "gf47-" + Guid.NewGuid().ToString("N"));
	readonly FakeEngine _engine = new FakeEngine();

	public Review47TaskTests() => Directory.CreateDirectory(_dir);

	public void Dispose() => Directory.Delete(_dir, true);

	string Write(string relative, string xaml)
	{
		string path = Path.Combine(_dir, relative.Replace('/', Path.DirectorySeparatorChar));
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, xaml);
		return path;
	}

	static TaskItem Item(string path, string type)
	{
		var item = new TaskItem(path);
		item.SetMetadata("GFItemType", type);
		return item;
	}

	MarkupCompileTask Run(params ITaskItem[] items)
	{
		var task = new MarkupCompileTask
		{
			BuildEngine = _engine, XamlFiles = items, RootNamespace = "A", ProjectDirectory = _dir,
			OutputDirectory = Path.Combine(_dir, "obj", "gen"),
		};
		Assert.True(task.Execute(), string.Join("; ", _engine.Errors));
		return task;
	}

	[Fact]
	public void Same_stem_files_in_different_folders_write_distinct_outputs_without_duplicates()
	{
		string dict = $"<ResourceDictionary {Head}/>";
		string light = Write("Themes/Light/Colors.xaml", dict);
		string dark = Write("Themes/Dark/Colors.xaml", dict);
		string themeStyles = Write("Themes/Styles.xaml", dict);
		string viewStyles = Write("Views/Styles.xaml", $"<Window x:Class=\"A.Views.Styles\" {Head}/>");

		MarkupCompileTask task = Run(Item(light, "Page"), Item(dark, "Page"), Item(themeStyles, "Page"),
			Item(viewStyles, "Page"), Item(themeStyles, "Page"));

		string[] outputs = task.GeneratedFiles.Select(f => f.ItemSpec).ToArray();
		Assert.Equal(4, outputs.Length);
		Assert.Equal(4, outputs.Distinct(StringComparer.OrdinalIgnoreCase).Count());
		Assert.All(outputs, o => Assert.True(File.Exists(o)));
		Assert.Contains("class Themes_S_Light_S_Colors", File.ReadAllText(outputs.Single(o => o.Contains("Light"))));
	}

	[Fact]
	public void Two_different_files_with_the_same_output_path_are_an_error()
	{
		string inside = Write("Dict.xaml", $"<ResourceDictionary {Head}/>");
		string outsideDir = Path.Combine(Path.GetTempPath(), "gf47-out-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(outsideDir);
		try
		{
			string outside = Path.Combine(outsideDir, "Dict.xaml");
			File.WriteAllText(outside, $"<ResourceDictionary {Head}/>");
			var task = new MarkupCompileTask
			{
				BuildEngine = _engine, XamlFiles = new ITaskItem[] { Item(inside, "Page"), Item(outside, "Page") },
				RootNamespace = "A", ProjectDirectory = _dir, OutputDirectory = Path.Combine(_dir, "obj", "gen"),
			};

			Assert.False(task.Execute());
			Assert.Contains(_engine.Errors, e => e.Contains("collides"));
		}
		finally
		{
			Directory.Delete(outsideDir, true);
		}
	}

	[Fact]
	public void Dropped_markup_notes_are_logged_at_high_importance()
	{
		string path = Write("Main.xaml",
			$"<Window x:Class=\"A.Main\" {Head}><Grid><Grid.Bogus><Button/></Grid.Bogus></Grid></Window>");

		Run(Item(path, "Page"));

		BuildMessageEventArgs note = _engine.Messages.Single(m => m.Message!.Contains("Bogus"));
		Assert.Equal(MessageImportance.High, note.Importance);
	}

	[Fact]
	public void Only_an_ApplicationDefinition_gets_Main()
	{
		string app = Write("App.xaml", $"<Application x:Class=\"A.App\" {Head}/>");
		string asPage = Write("Other/App.xaml", $"<Application x:Class=\"A.Other\" {Head}/>");

		MarkupCompileTask task = Run(Item(app, "ApplicationDefinition"), Item(asPage, "Page"));

		string[] code = task.GeneratedFiles.Select(f => File.ReadAllText(f.ItemSpec)).ToArray();
		Assert.Single(code, c => c.Contains("static void Main()"));
	}

	internal sealed class FakeEngine : IBuildEngine
	{
		public List<BuildMessageEventArgs> Messages { get; } = new List<BuildMessageEventArgs>();
		public List<string> Errors { get; } = new List<string>();
		public bool ContinueOnError => false;
		public int LineNumberOfTaskNode => 0;
		public int ColumnNumberOfTaskNode => 0;
		public string ProjectFileOfTaskNode => "test.csproj";
		public bool BuildProjectFile(string a, string[] b, System.Collections.IDictionary c, System.Collections.IDictionary d)
			=> true;
		public void LogCustomEvent(CustomBuildEventArgs e) { }
		public void LogErrorEvent(BuildErrorEventArgs e) => Errors.Add(e.Message!);
		public void LogMessageEvent(BuildMessageEventArgs e) => Messages.Add(e);
		public void LogWarningEvent(BuildWarningEventArgs e) { }
	}
}
