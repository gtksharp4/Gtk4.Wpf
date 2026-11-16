using System.Collections.Generic;

namespace Gtk.Forms.MarkupCompiler
{
	/// <summary>
	/// Phase-0 built-in map from local element name to fully-qualified CLR type for the
	/// WPF presentation namespace (http://schemas.microsoft.com/winfx/2006/xaml/presentation).
	/// TODO: later phases resolve this from the referenced assemblies'
	/// [assembly: XmlnsDefinition] attributes instead of this hard-coded table.
	/// </summary>
	static class WpfXmlnsMap
	{
		/// <summary>WPF presentation namespace URI.</summary>
		internal const string PresentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

		/// <summary>XAML language namespace URI (x: prefix).</summary>
		internal const string XNs = "http://schemas.microsoft.com/winfx/2009/xaml";

		/// <summary>Alternate XAML 2006 language namespace URI.</summary>
		internal const string XNs2006 = "http://schemas.microsoft.com/winfx/2006/xaml";

		static readonly Dictionary<string, string> _map = new Dictionary<string, string>
		{
			// System.Windows
			{ "Window",             "System.Windows.Window" },
			{ "Application",        "System.Windows.Application" },
			{ "ResourceDictionary", "System.Windows.ResourceDictionary" },
			{ "Style",              "System.Windows.Style" },

			// System.Windows.Controls
			{ "Grid",           "System.Windows.Controls.Grid" },
			{ "StackPanel",     "System.Windows.Controls.StackPanel" },
			{ "DockPanel",      "System.Windows.Controls.DockPanel" },
			{ "WrapPanel",      "System.Windows.Controls.WrapPanel" },
			{ "Canvas",         "System.Windows.Controls.Canvas" },
			{ "TextBlock",      "System.Windows.Controls.TextBlock" },
			{ "TextBox",        "System.Windows.Controls.TextBox" },
			{ "Button",         "System.Windows.Controls.Button" },
			{ "CheckBox",       "System.Windows.Controls.CheckBox" },
			{ "RadioButton",    "System.Windows.Controls.RadioButton" },
			{ "ComboBox",       "System.Windows.Controls.ComboBox" },
			{ "Image",          "System.Windows.Controls.Image" },
			{ "Label",          "System.Windows.Controls.Label" },
			{ "Border",         "System.Windows.Controls.Border" },
			{ "ContentControl", "System.Windows.Controls.ContentControl" },
		};

		/// <summary>
		/// Returns the fully-qualified CLR type name for a presentation-namespace element, or
		/// <c>null</c> when the element is not in the Phase-0 map.
		/// </summary>
		internal static string Resolve(string localName)
		{
			_map.TryGetValue(localName, out string fqn);
			return fqn;
		}

		/// <summary>
		/// Returns <c>true</c> for type names whose children are added via .Children.Add().
		/// All other mapped types use .Content = child (single-child model).
		/// </summary>
		internal static bool HasChildren(string localName)
		{
			return localName == "Grid"
				|| localName == "StackPanel"
				|| localName == "DockPanel"
				|| localName == "WrapPanel"
				|| localName == "Canvas";
		}
	}
}
