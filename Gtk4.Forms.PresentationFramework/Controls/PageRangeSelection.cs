namespace System.Windows.Controls;

/// <summary>Which pages a print job covers.</summary>
/// <remarks>The values match WPF. <see cref="PrintDialog"/> stores CurrentPage and SelectedPages but treats them
/// as AllPages.</remarks>
public enum PageRangeSelection
{
	/// <summary>Every page.</summary>
	AllPages,

	/// <summary>The pages of <see cref="PrintDialog.PageRange"/>.</summary>
	UserPages,

	/// <summary>The page being viewed.</summary>
	CurrentPage,

	/// <summary>The selected pages.</summary>
	SelectedPages
}
