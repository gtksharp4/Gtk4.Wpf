using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>
	/// The node an XPath binding is evaluated against (WPF XmlBindingWorker): the source itself when it is a node,
	/// else the current item of the default view of a node collection (a DataSourceProvider's Data).
	/// </summary>
	static XmlNode? ContextNode(object? source)
	{
		if (source is XmlNode node) return node;
		if (source is string || source is not IEnumerable) return null;
		var view = CollectionViewSource.GetDefaultView(source);
		if (view?.CurrentItem is XmlNode current) return current;
		foreach (var item in (IEnumerable)source)
			return item as XmlNode;
		return null;
	}

	static List<XmlNode>? SelectXmlNodes(object? source, string xpath, DependencyObject target)
	{
		var context = ContextNode(source);
		if (context == null) return null;
		try
		{
			var manager = Binding.GetXmlNamespaceManager(target);
			var selected = manager != null ? context.SelectNodes(xpath, manager) : context.SelectNodes(xpath);
			if (selected == null) return null;
			var result = new List<XmlNode>(selected.Count);
			foreach (XmlNode n in selected) result.Add(n);
			return result;
		}
		catch (XPathException) { return null; }
		catch (ArgumentException) { return null; }
	}

	internal sealed partial class BindingRecord
	{
		/// <summary>
		/// Evaluates the binding's XPath. A collection target (ItemsSource) gets the node list; any other target the
		/// first node, as text (attribute value or element text) unless it takes the node itself. A binding Path
		/// then applies to the result. <paramref name="failed"/> is set when the query matched nothing usable.
		/// </summary>
		object? ResolveXPath(object? source, DependencyObject target, out bool failed)
		{
			failed = false;
			var nodes = SelectXmlNodes(source, _binding.XPath!, target);
			if (nodes == null) { failed = true; return null; }

			var type = _dp.PropertyType;
			bool pathed = !string.IsNullOrEmpty(_binding.PathString);
			if (!pathed && type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type)) return nodes;
			if (nodes.Count == 0) { failed = true; return null; }

			object? value = nodes[0];
			if (pathed) return ResolvePath(value, _binding.PathString);
			if (_binding.Converter != null) return value;
			bool wantsText = _binding.StringFormat != null || (type != typeof(object) && !type.IsInstanceOfType(value));
			return wantsText ? ((XmlNode)value).InnerText : value;
		}

		bool WriteXPath(object? source, DependencyObject target, object? value)
		{
			var nodes = SelectXmlNodes(source, _binding.XPath!, target);
			if (nodes == null || nodes.Count == 0) return false;
			nodes[0].InnerText = Convert.ToString(value, ResolveCulture(_binding.ConverterCulture)) ?? "";
			return true;
		}
	}
}
