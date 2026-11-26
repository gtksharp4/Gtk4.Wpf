using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Linq;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>ControlsAndLayout snippets load through XamlReader (needs the local WPF-Samples checkout).</summary>
public class XamlReaderSamplesTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => System.Windows.Application.ResetCurrentForTests();

	// Layer gaps, not reader gaps: no InkCanvas / FlowDocumentPageViewer type, no TabItem.IsSelected setter.
	static readonly string[] Known = { "InkCanvas", "FlowDocumentPageViewer", "IsSelected" };

	static string? SamplesFile()
	{
		for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
		{
			string path = Path.Combine(dir.FullName, "WPF-Samples", "Getting Started", "ControlsAndLayout",
				"ControlsAndLayout", "samples.xml");
			if (File.Exists(path)) return path;
		}
		return null;
	}

	public static IEnumerable<object[]> Snippets()
	{
		string? file = SamplesFile();
		if (file == null) yield break;
		foreach (XElement sample in XDocument.Load(file).Descendants("Sample"))
		{
			string text = sample.Element("Syntax")?.Value.Trim() ?? "";
			if (text.Length == 0 || Known.Any(text.Contains)) continue;
			yield return new object[] { (string)sample.Attribute("Title")!, text };
		}
	}

	[Fact]
	public void The_samples_checkout_is_present() =>
		Assert.SkipUnless(SamplesFile() != null, "WPF-Samples not checked out");

	[Theory(SkipTestWithoutData = true)]
	[MemberData(nameof(Snippets))]
	public void Snippet_loads(string title, string xaml) => Run(() =>
	{
		Assert.NotNull(title);
		Assert.NotNull(XamlReader.Parse(xaml));
	});
}
