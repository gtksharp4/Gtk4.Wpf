using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>The text of each templated item of a ListBox, read from the visual tree of its item containers.</summary>
static class ListTexts
{
	public static string[] Of(ListBox list) =>
		Enumerable.Range(0, list.Items.Count)
			.Select(i => list.ContainerAt(i) is { } c ? Texts(c).FirstOrDefault() ?? "" : "")
			.ToArray();

	static IEnumerable<string> Texts(DependencyObject root)
	{
		if (root is TextBlock t) yield return t.Text;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
			foreach (var s in Texts(VisualTreeHelper.GetChild(root, i))) yield return s;
	}
}
