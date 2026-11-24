using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Round 49a: string converters reached from generated code, and the unhandled-type audit.</summary>
public class Round49aConverterTests
{
	static object? Dash(string raw)
		=> XamlValueConverter.Convert(typeof(Shape), Shape.StrokeDashArrayProperty, raw);

	[Theory]
	[InlineData("2,1")]
	[InlineData("2 1")]
	[InlineData(" 2 , 1 ")]
	[InlineData("2\n1")]
	public void DoubleCollection_splits_on_whitespace_and_commas(string raw)
		=> Assert.Equal(new[] { 2d, 1d }, ((DoubleCollection)Dash(raw)!).ToArray());

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void DoubleCollection_empty_text_is_an_empty_collection(string raw)
		=> Assert.Empty((DoubleCollection)Dash(raw)!);

	[Fact]
	public void DoubleCollection_uses_the_invariant_culture()
		=> Assert.Equal(new[] { 1.5, 2.5 }, ((DoubleCollection)Dash("1.5,2.5")!).ToArray());

	[Fact]
	public void DoubleCollection_bad_token_is_a_FormatException()
		=> Assert.Throws<FormatException>(() => Dash("2,x"));

	static object? Cur(string raw)
		=> XamlValueConverter.Convert(typeof(FrameworkElement), FrameworkElement.CursorProperty, raw);

	[Theory]
	[InlineData("SizeAll")]
	[InlineData("sizeall")]
	[InlineData(" SIZEALL ")]
	public void Cursor_name_maps_to_the_Cursors_static_case_insensitively(string raw)
		=> Assert.Same(Cursors.SizeAll, Cur(raw));

	[Fact]
	public void Cursor_unknown_name_throws()
		=> Assert.Throws<FormatException>(() => Cur("NotACursor"));

	// DP types that no string converter handles and no sample sets from plain text; they reach the raw-string
	// fall-through. Anything else registered as a DP must have a converter, so a gap fails here, not at startup.
	static readonly HashSet<string> Unconverted = new HashSet<string>
	{
		"System.Object", "System.Type", "System.Windows.Shell.WindowChrome", "System.Windows.DependencyProperty", "System.Windows.PropertyPath",
		"System.Collections.IEnumerable", "System.Windows.Style", "System.Windows.Controls.ControlTemplate",
		"System.Windows.Controls.DataTemplate", "System.Windows.Controls.ItemsPanelTemplate",
		"System.Windows.Data.Binding", "System.Windows.Data.IValueConverter", "System.Delegate",
		"System.Windows.UIElement",
		"System.Windows.FrameworkElement",
		// Found by the static-owner walk (attached DPs): object-valued, read-only or culture-typed.
		"System.Collections.IList", "System.Globalization.CultureInfo",
		"System.Collections.ObjectModel.ReadOnlyObservableCollection`1",
		// Object-valued, no string form in WPF either.
		"System.Windows.Controls.ContextMenu", "System.Windows.Controls.DataTemplateSelector",
		"System.Windows.Controls.StyleSelector", "System.Windows.Data.BindingGroup", "System.Windows.IInputElement",
		"System.Xml.XmlNamespaceManager",
		// Object-valued (DataGrid.CurrentColumn, DataGridCell.Column); WPF has no converter or string form.
		"System.Windows.Controls.DataGridColumn",
		// Deferred to the documents/effects work (other lane): no sample sets them from plain text yet.
		// TextDecorationCollection belongs to the documents lane (PerMonitorDPI/TextFormatting sets
		// TextDecorations="Underline").
		"System.Windows.Documents.FlowDocument", "System.Windows.Media.Effects.Effect",
		"System.Windows.TextDecorationCollection",
		// FlowDocument.TextEffects (documents lane): TextEffectCollection has no type converter in WPF.
		"System.Windows.Media.TextEffectCollection",
		// Animation/drawing object model: set through property elements, never from plain text (BeginTime is
		// parsed by the generator).
		"System.TimeSpan", "System.Windows.DependencyObject", "System.Windows.INameScope",
		"System.Windows.Media.Animation.IEasingFunction", "System.Windows.Media.Drawing", "System.Windows.Media.Pen",
		// DocumentViewerBase.Document: an interface, set as content; no type converter in WPF.
		"System.Windows.Documents.IDocumentPaginatorSource",
		// Media3D object model: set through property elements, no string form in WPF either.
		"System.Windows.Media.Media3D.Camera", "System.Windows.Media.Media3D.Geometry3D",
		"System.Windows.Media.Media3D.Material", "System.Windows.Media.Media3D.MaterialCollection",
		"System.Windows.Media.Media3D.Model3D", "System.Windows.Media.Media3D.Model3DCollection",
		"System.Windows.Media.Media3D.Rotation3D", "System.Windows.Media.Media3D.Transform3D",
		"System.Windows.Media.Media3D.Transform3DCollection",
		// Legacy BitmapEffect store-only DPs (UIElement.BitmapEffect/Input): no string form in WPF.
		"System.Windows.Media.Effects.BitmapEffect", "System.Windows.Media.Effects.BitmapEffectInput",
		"System.Windows.Media.Imaging.BitmapSource",
		// BitmapPalette has no converter in WPF.
		"System.Windows.Media.Imaging.BitmapPalette",
		// A Visual is an object reference (VisualBrush.Visual), not a string-convertible value.
		"System.Windows.Media.Visual",
	};

	[Fact]
	public void Every_registered_DP_type_has_a_converter_or_is_on_the_audited_list()
	{
		var registry = (System.Collections.IDictionary)typeof(XamlValueConverter)
			.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var handled = new HashSet<Type>(registry.Keys.Cast<Type>());
		var missing = new SortedSet<string>();
		var types = typeof(UIElement).Assembly.GetTypes()
			.Where(t => t.IsPublic);
		foreach (Type t in types)
		{
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Where(f => f.FieldType == typeof(DependencyProperty));
			foreach (FieldInfo f in fields)
			{
				if (f.GetValue(null) is not DependencyProperty dp) continue;
				Type pt = Nullable.GetUnderlyingType(dp.PropertyType) ?? dp.PropertyType;
				string name = (pt.IsGenericType ? pt.GetGenericTypeDefinition().FullName : pt.FullName) ?? pt.Name;
				if (pt == typeof(string) || pt.IsEnum || handled.Contains(pt) || Unconverted.Contains(name)) continue;
				missing.Add(name);
			}
		}
		Assert.True(missing.Count == 0,
			"DP types without a string converter (add one or list them): " + string.Join(", ", missing));
	}
}
