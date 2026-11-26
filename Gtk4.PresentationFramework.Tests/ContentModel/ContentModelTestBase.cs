using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

public class Place
{
	public string Name { get; set; } = "";
	public string State { get; set; } = "";
	public override string ToString() => "Place:" + Name;
}

public sealed class SpecialPlace : Place { }

public abstract class ContentModelTestBase : WpfGtkTestBase
{
	protected static List<Gtk.Widget> Descendants(Gtk.Widget root)
	{
		var all = new List<Gtk.Widget> { root };
		for (var child = root.FirstChild; child != null; child = child.NextSibling)
			all.AddRange(Descendants(child));
		return all;
	}

	protected static List<string> Texts(Gtk.Widget widget) =>
		Descendants(widget).OfType<Gtk.Label>().Select(l => l.Text).Where(t => t.Length > 0).ToList();

	protected static List<string> Texts(UIElement element) =>
		Descendants(element.Widget).OfType<Gtk.Label>().Select(l => l.Text).Where(t => t.Length > 0).ToList();

	protected static DataTemplate Template(string prefix) => new DataTemplate
	{
		ItemFactory = item => new TextBlock { Text = prefix + ((item as Place)?.Name ?? item) },
	};

	protected static bool IsInside(Gtk.Widget ancestor, Gtk.Widget widget) =>
		Descendants(ancestor).Contains(widget);
}
