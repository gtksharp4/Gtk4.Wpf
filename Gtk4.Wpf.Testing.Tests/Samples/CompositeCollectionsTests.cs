using System.Linq;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Data Binding/CompositeCollections": gods (CLR) and heroes (XML) in one list.</summary>
public class CompositeCollectionsTests : WpfUiWindowTest<CompositeCollections.MainWindow>
{
	[WpfUiFact]
	public void The_hero_names_are_shown_next_to_the_gods()
	{
		var names = App.Invoke(() => ListTexts.Of((System.Windows.Controls.ListBox)App.Window.FindName("myListBox")));
		Assert.Contains("Zeus", names);
		Assert.Contains("Jason", names);
		Assert.Contains("Perseus", names);
		// 12 Greek gods (GreekGods.cs) + 6 heroes (the GreekHeroes XML in MainWindow.xaml).
		Assert.Equal(12 + 6, names.Take(12 + 6).Count(n => !string.IsNullOrWhiteSpace(n)));
	}
}
