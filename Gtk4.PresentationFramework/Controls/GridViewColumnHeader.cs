namespace System.Windows.Controls;

/// <summary>The header of a <see cref="GridViewColumn"/>; a button whose content is the column header.</summary>
/// <remarks>
/// Derives from <see cref="Button"/> (WPF: <c>ButtonBase</c>). Resizing and reordering by drag are not implemented.
/// </remarks>
public class GridViewColumnHeader : Button
{
	/// <summary>Creates a header with no column.</summary>
	public GridViewColumnHeader()
	{
	}

	/// <summary>Gets the column this header belongs to.</summary>
	public GridViewColumn? Column { get; internal set; }
}
