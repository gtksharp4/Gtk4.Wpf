using System.Collections.Generic;

namespace Gtk4.Forms.MarkupCompiler
{
	/// <summary>
	/// Built-in fast-path map from local element name to fully-qualified CLR type for the
	/// WPF presentation namespace (http://schemas.microsoft.com/winfx/2006/xaml/presentation).
	/// Covers all known WPF types; unknown types fall through to the
	/// <c>[assembly: XmlnsDefinition]</c>-driven resolver (<see cref="AssemblyXmlnsDefinitionSource"/>)
	/// injected into <see cref="XamlCodeGenerator"/>.
	/// </summary>
	static class WpfXmlnsMap
	{
		/// <summary>WPF presentation namespace URI.</summary>
		internal const string PresentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

		/// <summary>XAML language namespace URI (x: prefix).</summary>
		internal const string XNs = "http://schemas.microsoft.com/winfx/2009/xaml";

		/// <summary>Alternate XAML 2006 language namespace URI.</summary>
		internal const string XNs2006 = "http://schemas.microsoft.com/winfx/2006/xaml";

		/// <summary>
		/// Markup-compatibility namespace URI (mc: prefix). Carries <c>mc:Ignorable</c>, whose
		/// listed prefixes (e.g. the <c>d:</c> design namespace) are stripped at compile time.
		/// </summary>
		internal const string McNs = "http://schemas.openxmlformats.org/markup-compatibility/2006";

		static readonly Dictionary<string, string> _map = new Dictionary<string, string>
		{
			// System.Windows
			{ "Window",             "System.Windows.Window" },
			{ "Application",        "System.Windows.Application" },
			{ "ResourceDictionary", "System.Windows.ResourceDictionary" },
			{ "Style",              "System.Windows.Style" },
			// VisualStateManager
			{ "VisualStateManager", "System.Windows.VisualStateManager" },
			{ "VisualStateGroup",   "System.Windows.VisualStateGroup" },
			{ "VisualState",        "System.Windows.VisualState" },
			// Triggers
			{ "Trigger",            "System.Windows.Trigger" },
			{ "DataTrigger",        "System.Windows.DataTrigger" },
			{ "MultiTrigger",       "System.Windows.MultiTrigger" },
			{ "EventTrigger",       "System.Windows.EventTrigger" },
			{ "Setter",             "System.Windows.Setter" },
			{ "Condition",          "System.Windows.Condition" },

			// Templates
			{ "ControlTemplate",    "System.Windows.Controls.ControlTemplate" },
			{ "DataTemplate",       "System.Windows.Controls.DataTemplate" },

			// Animation
			{ "Storyboard",         "System.Windows.Media.Animation.Storyboard" },
			{ "DoubleAnimation",    "System.Windows.Media.Animation.DoubleAnimation" },
			{ "ColorAnimation",     "System.Windows.Media.Animation.ColorAnimation" },
			{ "ThicknessAnimation", "System.Windows.Media.Animation.ThicknessAnimation" },
			{ "PointAnimation",     "System.Windows.Media.Animation.PointAnimation" },
			{ "DoubleAnimationUsingKeyFrames", "System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames" },
			{ "DiscreteDoubleKeyFrame", "System.Windows.Media.Animation.DiscreteDoubleKeyFrame" },
			{ "LinearDoubleKeyFrame", "System.Windows.Media.Animation.LinearDoubleKeyFrame" },
			{ "SplineDoubleKeyFrame", "System.Windows.Media.Animation.SplineDoubleKeyFrame" },
			{ "EasingDoubleKeyFrame", "System.Windows.Media.Animation.EasingDoubleKeyFrame" },
			{ "BeginStoryboard",    "System.Windows.BeginStoryboard" },

			// System.Windows.Controls
			{ "Grid",             "System.Windows.Controls.Grid" },
			{ "StackPanel",       "System.Windows.Controls.StackPanel" },
			{ "DockPanel",        "System.Windows.Controls.DockPanel" },
			{ "TextElement",      "System.Windows.Documents.TextElement" },
			{ "WrapPanel",        "System.Windows.Controls.WrapPanel" },
			{ "Canvas",           "System.Windows.Controls.Canvas" },
			{ "TextBlock",        "System.Windows.Controls.TextBlock" },
			{ "TextBox",          "System.Windows.Controls.TextBox" },
			{ "Button",           "System.Windows.Controls.Button" },
			{ "CheckBox",         "System.Windows.Controls.CheckBox" },
			{ "RadioButton",      "System.Windows.Controls.RadioButton" },
			{ "ComboBox",         "System.Windows.Controls.ComboBox" },
			{ "Image",            "System.Windows.Controls.Image" },
			{ "Label",            "System.Windows.Controls.Label" },
			{ "Border",           "System.Windows.Controls.Border" },
			{ "ExceptionValidationRule",       "System.Windows.Controls.ExceptionValidationRule" },
			{ "DataErrorValidationRule",       "System.Windows.Controls.DataErrorValidationRule" },
			{ "NotifyDataErrorValidationRule", "System.Windows.Controls.NotifyDataErrorValidationRule" },
			{ "ContentControl",   "System.Windows.Controls.ContentControl" },
			{ "ContentPresenter", "System.Windows.Controls.ContentPresenter" },
			{ "ItemsControl",     "System.Windows.Controls.ItemsControl" },
			{ "ListBox",          "System.Windows.Controls.ListBox" },
			{ "ListView",         "System.Windows.Controls.ListView" },
			{ "ListBoxItem",      "System.Windows.Controls.ListBoxItem" },
			{ "ScrollViewer",     "System.Windows.Controls.ScrollViewer" },
			{ "SpellCheck",       "System.Windows.Controls.SpellCheck" },
			{ "Mouse",            "System.Windows.Input.Mouse" },
			{ "Keyboard",         "System.Windows.Input.Keyboard" },

			// System.Windows.Shapes
			{ "Shape",            "System.Windows.Shapes.Shape" },
			{ "Ellipse",          "System.Windows.Shapes.Ellipse" },
			{ "Rectangle",        "System.Windows.Shapes.Rectangle" },
			{ "Polygon",          "System.Windows.Shapes.Polygon" },
			{ "Polyline",         "System.Windows.Shapes.Polyline" },
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
