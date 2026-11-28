using System.Linq;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Data Binding/XmlDataSource": an XPath-filtered list of the inventory.</summary>
public class XmlDataSourceTests : WpfUiWindowTest<XmlDataSource.MainWindow>
{
	[WpfUiFact]
	public void The_list_shows_the_filtered_titles_in_document_order()
	{
		var titles = App.Invoke(() => ListTexts.Of(ListTexts.FindFirst<System.Windows.Controls.ListBox>(App.Window)));
		Assert.Equal(new[]
		{
			"XML in Action",
			"Programming Microsoft Windows With C#",
			"Inside C#",
			"Microsoft C# Language Specifications",
		}, titles);
		Assert.All(titles, t => Assert.False(string.IsNullOrWhiteSpace(t)));
	}
}
