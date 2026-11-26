using System.IO;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Real WPF-Samples XAML (copied under SamplesRound47/) generates code that Roslyn compiles cleanly.</summary>
public class SamplesRound47CompileTests : GeneratorV2TestBase
{
	static readonly string Dir = Path.Combine(
		Path.GetDirectoryName(typeof(SamplesRound47CompileTests).Assembly.Location)!, "SamplesRound47");

	static string Read(string name) => File.ReadAllText(Path.Combine(Dir, name));

	[Fact]
	public void DragDropEvents_MainWindow_compiles()
	{
		const string stub = @"using System; using System.Windows;
namespace DragDropEvents { public partial class MainWindow {
void ClickClearEventHistory(object s, RoutedEventArgs a) { } void EhEventWindowSizeChanged(object s, EventArgs a) { }
void EhDragEnter(object s, DragEventArgs a) { } void EhDragLeave(object s, DragEventArgs a) { }
void EhDragOver(object s, DragEventArgs a) { } void EhDrop(object s, DragEventArgs a) { }
void EhPreviewDragEnter(object s, DragEventArgs a) { } void EhPreviewDragLeave(object s, DragEventArgs a) { }
void EhPreviewDragOver(object s, DragEventArgs a) { } void EhPreviewDrop(object s, DragEventArgs a) { } } }";
		string code = Gen(Read("DragDropEvents.MainWindow.xaml"), sources: new[] { stub });

		AssertCompiles(code, stub);
	}

	[Fact]
	public void StringLocalization_app_window_and_merged_dictionary_compile_together()
	{
		const string stub = @"namespace StringLocalizationSample { public partial class App { }
public partial class MainWindow { void messageBoxButton_Click(object s, System.Windows.RoutedEventArgs a) { } } }";
		string app = Gen(Read("StringLocalization.App.xaml"), sources: new[] { stub }, itemType: "ApplicationDefinition");
		string window = Gen(Read("StringLocalization.MainWindow.xaml"), sources: new[] { stub });
		string dict = Gen(Read("StringLocalization.StringResources.xaml"), out _, false,
			xamlPath: "StringResources.xaml", rootNamespace: "StringLocalizationSample");

		Assert.Contains("TryLoadDictionary", app);
		AssertCompiles(app, dict + "\n" + stub + "\n" + window);
	}

	[Fact]
	public void CustomApplicationClass_App_compiles_against_its_base_class()
	{
		const string stub = @"namespace CustomApplicationClient { public class CustomApplication : System.Windows.Application { } }
namespace CustomApplicationClass { public partial class App : CustomApplicationClient.CustomApplication { } }";
		string app = Gen(Read("CustomApplicationClass.App.xaml"), sources: new[] { stub },
			itemType: "ApplicationDefinition");

		Assert.Contains("static void Main()", app);
		AssertCompiles(app, stub);
	}

	[Fact]
	public void AlternatingAppearanceOfItems_MainWindow_compiles()
	{
		const string stub = @"using System.Collections.Generic; using System.Collections.ObjectModel;
namespace AlternatingAppearanceOfItems {
public partial class MainWindow { }
public class Place { public string CityName { get; set; } public string State { get; set; } }
public class Places : ObservableCollection<Place> { }
public class ListLeagueList : List<object> { } }";
		string code = Gen(Read("AlternatingAppearanceOfItems.MainWindow.xaml"), sources: new[] { stub });

		AssertCompiles(code, stub);
	}
}
