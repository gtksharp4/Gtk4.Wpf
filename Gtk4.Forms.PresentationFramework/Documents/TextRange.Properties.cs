using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace System.Windows.Documents;

public partial class TextRange
{
	static bool IsSupported(DependencyProperty p) =>
		p == TextElement.FontWeightProperty || p == TextElement.FontStyleProperty
		|| p == TextElement.FontSizeProperty || p == TextElement.ForegroundProperty
		|| p == TextElement.FontFamilyProperty;

	/// <summary>
	/// Applies a formatting value to every run in the range, splitting runs at the range edges.
	/// Supports FontWeight, FontStyle, FontSize, FontFamily and Foreground; a font size is stored but not
	/// rendered until <see cref="TextElement"/> carries an instance FontSize.
	/// </summary>
	/// <exception cref="ArgumentException">The property is not supported, or the value has the wrong type.</exception>
	public void ApplyPropertyValue(DependencyProperty formattingProperty, object value)
	{
		if (formattingProperty == null) throw new ArgumentNullException(nameof(formattingProperty));
		if (!IsSupported(formattingProperty))
			throw new ArgumentException("Not a supported formatting property.", nameof(formattingProperty));
		if (value == null || !formattingProperty.PropertyType.IsInstanceOfType(value))
			throw new ArgumentException("The value does not match the property type.", nameof(value));
		if (WholeOwner && _owner!.Document == null) return;
		var doc = Start.Document;
		doc.EditBatch(() =>
		{
			SplitRunAt(doc, End.Offset);
			SplitRunAt(doc, Start.Offset);
			int s = Start.Offset, e = End.Offset;
			foreach (var seg in TextTree.Build(doc))
				if (seg.Kind == TextSymKind.Text && seg.Offset >= s && seg.End <= e)
					Apply((Run)seg.Node, formattingProperty, value);
			doc.RecordTextEdit(s, 0, 0);
		});
		Changed?.Invoke(this, EventArgs.Empty);
	}

	static void Apply(Run run, DependencyProperty p, object value)
	{
		if (p == TextElement.FontWeightProperty) run.FontWeight = (FontWeight)value;
		else if (p == TextElement.FontStyleProperty) run.FontStyle = (FontStyle)value;
		else if (p == TextElement.ForegroundProperty) run.Foreground = (Brush)value;
		else if (p == TextElement.FontFamilyProperty) run.FontFamily = (FontFamily)value;
		else run.FontSize = (double)value;
	}

	/// <summary>
	/// The value shared by the runs in the range, <see cref="DependencyProperty.UnsetValue"/> when they differ.
	/// An empty range reports the value at its start.
	/// </summary>
	public object GetPropertyValue(DependencyProperty formattingProperty)
	{
		if (formattingProperty == null) throw new ArgumentNullException(nameof(formattingProperty));
		if (!IsSupported(formattingProperty))
			throw new ArgumentException("Not a supported formatting property.", nameof(formattingProperty));
		if (WholeOwner && _owner!.Document == null) return formattingProperty.DefaultMetadata.DefaultValue!;
		var doc = Start.Document;
		var syms = TextTree.Build(doc);
		int s = Start.Offset, e = End.Offset;
		var values = new List<object>();
		foreach (var seg in syms)
			if (seg.Kind == TextSymKind.Text && seg.Offset < e && seg.End > s)
				values.Add(Effective(doc, syms, seg.Offset, formattingProperty));
		if (values.Count == 0) return Effective(doc, syms, s, formattingProperty);
		return values.All(v => Equals(v, values[0])) ? values[0] : DependencyProperty.UnsetValue;
	}

	static object Effective(FlowDocument doc, List<TextSym> syms, int offset, DependencyProperty p)
	{
		var open = TextTree.OpenElements(syms, offset);
		for (int i = open.Count - 1; i >= 0; i--)
		{
			var v = Explicit(open[i] as TextElement, p);
			if (v != null) return v;
		}
		return doc.HasNonInheritedValue(p) ? doc.GetValue(p)! : p.DefaultMetadata.DefaultValue!;
	}

	static object? Explicit(TextElement? el, DependencyProperty p)
	{
		if (el == null) return null;
		if (p == TextElement.FontWeightProperty)
			return TextLocalValues.Weight(el) ?? (el is Bold ? FontWeights.Bold : (FontWeight?)null);
		if (p == TextElement.FontStyleProperty)
			return TextLocalValues.Style(el) ?? (el is Italic ? FontStyles.Italic : (FontStyle?)null);
		if (p == TextElement.ForegroundProperty) return TextLocalValues.Brush(el);
		if (p == TextElement.FontFamilyProperty) return TextLocalValues.Family(el);
		return TextLocalValues.Size(el);
	}

	// Splits the Run holding the offset into two Runs when the offset is strictly inside its text.
	static void SplitRunAt(FlowDocument doc, int offset)
	{
		var syms = TextTree.Build(doc);
		var open = TextTree.OpenElements(syms, offset);
		if (open.Count < 2 || !(open[open.Count - 1] is Run run)) return;
		int local = offset - (TextTreeEdit.StartOffset(syms, run) + 1);
		if (local <= 0 || local >= run.Text.Length) return;
		var rest = new Run { Text = run.Text.Substring(local) };
		TextTreeEdit.CopyFormat(run, rest);
		run.Text = run.Text.Substring(0, local);
		var col = open[open.Count - 2] is Span sp ? sp.Inlines : ((Paragraph)open[open.Count - 2]).Inlines;
		var items = col.ToList();
		items.Insert(items.IndexOf(run) + 1, rest);
		TextTreeEdit.Replace(col, items);
		doc.RecordTextEdit(offset, 0, 2);
	}
}
