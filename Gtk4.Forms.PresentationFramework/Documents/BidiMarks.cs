namespace System.Windows.Documents;

/// <summary>Unicode bidi controls used to render <see cref="FlowDirection"/> in Pango text.</summary>
internal static class BidiMarks
{
	internal const char Lri = '⁦';
	internal const char Rli = '⁧';
	internal const char Pdi = '⁩';
	internal const char Lrm = '‎';
	internal const char Rlm = '‏';

	/// <summary>The isolate opener for <paramref name="direction"/>.</summary>
	internal static char Isolate(FlowDirection direction) =>
		direction == FlowDirection.RightToLeft ? Rli : Lri;

	/// <summary>The directional mark for <paramref name="direction"/>; it fixes a paragraph's base direction.</summary>
	internal static char Mark(FlowDirection direction) =>
		direction == FlowDirection.RightToLeft ? Rlm : Lrm;
}
