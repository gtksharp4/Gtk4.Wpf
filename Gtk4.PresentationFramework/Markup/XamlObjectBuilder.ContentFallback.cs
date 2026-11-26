using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// Mirror of the markup compiler's content-model table (XamlCodeGenerator.ContentModel.cs), used when a
	// layer type carries no [ContentProperty]. Style and the triggers take Setters, as in WPF.
	static readonly Dictionary<string, string> _contentFallback = BuildContentFallback();

	static Dictionary<string, string> BuildContentFallback()
	{
		var d = new Dictionary<string, string>(StringComparer.Ordinal);
		void Add(string ns, string names, string prop)
		{
			foreach (string n in names.Split(' ')) d[ns + "." + n] = prop;
		}
		const string controls = "System.Windows.Controls";
		const string docs = "System.Windows.Documents";
		const string media = "System.Windows.Media";
		Add(controls, "Grid StackPanel DockPanel WrapPanel Canvas UniformGrid", "Children");
		Add(controls, "Border Viewbox", "Child");
		Add(controls, "ListBox ComboBox Menu ContextMenu TabControl StatusBar ToolBar TreeView TreeViewItem "
			+ "MenuItem ListView", "Items");
		Add(controls, "TextBlock", "Inlines");
		Add(controls, "TextBox", "Text");
		Add(controls, "RichTextBox FlowDocumentScrollViewer", "Document");
		Add(controls, "Button ToggleButton CheckBox RadioButton RepeatButton Label GroupBox Expander TabItem "
			+ "ListBoxItem ComboBoxItem ScrollViewer UserControl", "Content");
		Add(docs, "Paragraph Span Bold Italic Hyperlink", "Inlines");
		Add(docs, "Run", "Text");
		Add(docs, "Section ListItem FlowDocument", "Blocks");
		Add(docs, "List", "ListItems");
		Add(media, "TransformGroup DrawingGroup", "Children");
		Add(media, "PathGeometry", "Figures");
		Add(media, "PathFigure", "Segments");
		Add(media, "LinearGradientBrush RadialGradientBrush", "GradientStops");
		Add("System.Windows", "Style Trigger DataTrigger MultiTrigger MultiDataTrigger", "Setters");
		Add("System.Windows", "EventTrigger", "Actions");
		Add("System.Windows", "BeginStoryboard", "Storyboard");
		Add(media + ".Animation", "Storyboard ParallelTimeline", "Children");
		return d;
	}

	static string? FallbackContentProperty(Type type)
	{
		for (Type? t = type; t != null; t = t.BaseType)
			if (t.FullName != null && _contentFallback.TryGetValue(t.FullName, out string? name)) return name;
		return null;
	}

	// A template's content is its visual tree: rebuilt from the markup each time the template is applied.
	bool TryApplyTemplateContent(XElement el, object instance, List<Node> nodes)
	{
		if (instance is not FrameworkTemplate template) return false;
		if (nodes.Count != 1 || nodes[0].Element == null) throw Fail(el, "A template takes one root element.");
		XElement root = nodes[0].Element!;
		object[] chain = _stack.ToArray();
		template.Factory = () =>
		{
			var builder = new XamlObjectBuilder();
			builder._stack.AddRange(chain);
			builder._ctx = root;
			return (UIElement)builder.BuildElement(root, null, null)!;
		};
		return true;
	}
}
