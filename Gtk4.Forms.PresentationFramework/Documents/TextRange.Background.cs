using System.Windows.Media;

namespace System.Windows.Documents;

public partial class TextRange
{
	/// <summary>
	/// Sets (or, with null, clears) the background of every run in the range, splitting runs at the edges. Used by
	/// annotation highlights. The Pango text path does not paint TextElement.Background, so this is stored only.
	/// </summary>
	internal void ApplyBackground(Brush? brush)
	{
		if (WholeOwner && _owner!.Document == null) return;
		var doc = Start.Document;
		doc.EditBatch(() =>
		{
			SplitRunAt(doc, End.Offset);
			SplitRunAt(doc, Start.Offset);
			int s = Start.Offset, e = End.Offset;
			foreach (var seg in TextTree.Build(doc))
				if (seg.Kind == TextSymKind.Text && seg.Offset >= s && seg.End <= e)
					((Run)seg.Node).Background = brush;
			doc.RecordTextEdit(s, 0, 0);
		});
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
