namespace System.Windows.Controls;

/// <summary>Which pages a print job covers.</summary>
/// <remarks>WPF has AllPages and UserPages; CurrentPage and SelectedPages are additions that
/// <see cref="PrintDialog"/> stores and treats as AllPages.</remarks>
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
