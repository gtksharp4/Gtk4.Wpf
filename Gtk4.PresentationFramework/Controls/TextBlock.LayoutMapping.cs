namespace System.Windows.Controls;

/// <summary>
/// Pure WPF→GTK mapping helpers for <see cref="TextBlock"/> layout properties
/// (split out of <c>TextBlock.Layout.cs</c> to keep each file ≤200 lines).
/// </summary>
public partial class TextBlock
{
	/// <summary>
	/// Maps a WPF <see cref="TextAlignment"/> to the GTK <see cref="Gtk.Justification"/>
	/// and <c>Xalign</c> float pair.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Left → (Left, 0f)</item>
	/// <item>Center → (Center, 0.5f)</item>
	/// <item>Right → (Right, 1f)</item>
	/// <item>Justify → (Fill, 0f)</item>
	/// </list>
	/// </remarks>
	internal static (Gtk.Justification Justify, float Xalign) MapTextAlignment(TextAlignment alignment)
	{
		switch (alignment)
		{
			case TextAlignment.Center:  return (Gtk.Justification.Center, 0.5f);
			case TextAlignment.Right:   return (Gtk.Justification.Right, 1f);
			case TextAlignment.Justify: return (Gtk.Justification.Fill, 0f);
			default:                    return (Gtk.Justification.Left, 0f);
		}
	}

	/// <summary>
	/// Maps a WPF <see cref="TextTrimming"/> to a Pango <see cref="Pango.EllipsizeMode"/>.
	/// Both ellipsis modes map to <c>End</c> — Pango has no word/character distinction.
	/// </summary>
	internal static Pango.EllipsizeMode MapTextTrimming(TextTrimming trimming)
	{
		switch (trimming)
		{
			case TextTrimming.CharacterEllipsis:
			case TextTrimming.WordEllipsis:
				return Pango.EllipsizeMode.End;
			default:
				return Pango.EllipsizeMode.None;
		}
	}
}
