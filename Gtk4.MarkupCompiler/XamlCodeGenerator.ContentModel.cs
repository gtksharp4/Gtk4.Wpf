using System;
using System.Collections.Generic;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string PresControls = "System.Windows.Controls.";
		const string PresDocs = "System.Windows.Documents.";
		const string RunType = "global::System.Windows.Documents.Run";

		/// <summary>Content property of a type: its name and whether it is added to or assigned.</summary>
		sealed class ContentInfo
		{
			internal string Name;
			internal bool IsCollection;
		}

		// Used when no [ContentProperty] can be read: no references, or the layer type omits it.
		static readonly Dictionary<string, ContentInfo> _contentFallback = BuildContentFallback();

		static Dictionary<string, ContentInfo> BuildContentFallback()
		{
			var d = new Dictionary<string, ContentInfo>(StringComparer.Ordinal);
			void Add(string ns, string names, string prop, bool coll)
			{
				foreach (string n in names.Split(' '))
					d[ns + n] = new ContentInfo { Name = prop, IsCollection = coll };
			}
			Add(PresControls, "Grid StackPanel DockPanel WrapPanel Canvas UniformGrid", "Children", true);
			Add(PresControls, "Border Viewbox", "Child", false);
			Add(PresControls, "ListBox ComboBox Menu ContextMenu TabControl StatusBar ToolBar TreeView "
				+ "TreeViewItem MenuItem ListView", "Items", true);
			Add(PresControls, "TextBlock", "Inlines", true);
			Add(PresControls, "TextBox", "Text", false);
			Add(PresControls, "RichTextBox FlowDocumentScrollViewer", "Document", false);
			Add(PresControls, "Button ToggleButton CheckBox RadioButton RepeatButton Label GroupBox Expander "
				+ "TabItem ListBoxItem ComboBoxItem ScrollViewer UserControl", "Content", false);
			Add(PresDocs, "Paragraph Span Bold Italic Hyperlink", "Inlines", true);
			Add(PresDocs, "Run", "Text", false);
			Add(PresDocs, "Section ListItem FlowDocument", "Blocks", true);
			Add(PresDocs, "List", "ListItems", true);
			Add("System.Windows.Media.", "TransformGroup DrawingGroup", "Children", true);
			Add("System.Windows.Media.", "PathGeometry", "Figures", true);
			Add("System.Windows.Media.", "PathFigure", "Segments", true);
			Add("System.Windows.Media.", "LinearGradientBrush RadialGradientBrush", "GradientStops", true);
			return d;
		}

		/// <summary>
		/// Content property of <paramref name="fqType"/>; <c>null</c> keeps the legacy emission.
		/// </summary>
		ContentInfo GetContentInfo(string fqType)
		{
			if (fqType == null) return null;
			string name = fqType.StartsWith("global::", StringComparison.Ordinal) ? fqType.Substring(8) : fqType;
			if (name == MediaNs + "GradientStopCollection")
				return new ContentInfo { Name = "", IsCollection = true };
			if (!_resolver.TypeExists(name) && UserClassExists(name)) return UserContentInfo(name);
			if (_resolver.TryGetContentProperty(name, out string prop, out bool coll))
				return new ContentInfo { Name = prop, IsCollection = coll };
			if (!_contentFallback.TryGetValue(name, out ContentInfo info)) return null;
			if (_resolver.HasReferences)
			{
				Type t = _resolver.FindPropertyType(name, info.Name);
				if (t == null) return null;
			}
			return info;
		}

		/// <summary>Attaches <paramref name="childVar"/> to <paramref name="parentVar"/>.</summary>
		void EmitAttach(IndentedWriter w, string parentVar, string parentLocal, ContentInfo ci,
			string childVar, string childFq)
		{
			if (ci == null)
			{
				if (WpfXmlnsMap.HasChildren(parentLocal))
					w.Line($"{parentVar}.Children.Add({childVar});");
				else
					w.Line($"{parentVar}.Content = {childVar};");
			}
			else if (!ci.IsCollection)
				w.Line($"{parentVar}.{ci.Name} = {childVar};");
			else if (ci.Name == "Inlines"
					&& IsUIElement(childFq))
				w.Line($"{parentVar}.Inlines.Add(new global::{PresDocs}InlineUIContainer({childVar}));");
			else
				w.Line(ci.Name.Length == 0
					? $"{parentVar}.Add({childVar});" : $"{parentVar}.{ci.Name}.Add({childVar});");
		}

		bool IsUIElement(string fq) =>
			fq != null && _resolver.DerivesFrom(fq.Replace("global::", ""), "System.Windows.UIElement");

		/// <summary>Emits a text run of element content into the content property.</summary>
		void EmitText(IndentedWriter w, string parentVar, ContentInfo ci, string text, bool hasElements)
		{
			string lit = "\"" + EscapeString(text) + "\"";
			if (ci.IsCollection)
			{
				if (ci.Name == "Inlines")
					w.Line($"{parentVar}.Inlines.Add(new {RunType} {{ Text = {lit} }});");
				else if (ci.Name == "Items")
					w.Line($"{parentVar}.Items.Add({lit});");
			}
			else if (!hasElements && TextAssignable(ci))
				w.Line($"{parentVar}.{ci.Name} = {lit};");
		}

		static bool TextAssignable(ContentInfo ci) =>
			ci.Name == "Text" || ci.Name == "Content";
	}
}
