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

	/// <summary>The first descendant of <paramref name="root"/> of type <typeparamref name="T"/>, in the visual tree.</summary>
	public static T FindFirst<T>(DependencyObject root) where T : DependencyObject
	{
		if (root is T match) return match;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
			{ var found = FindFirst<T>(VisualTreeHelper.GetChild(root, i)); if (found != null) return found; }
		return null!;
	}

	static IEnumerable<string> Texts(DependencyObject root)
	{
		if (root is TextBlock t) yield return t.Text;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
			foreach (var s in Texts(VisualTreeHelper.GetChild(root, i))) yield return s;
	}
}
