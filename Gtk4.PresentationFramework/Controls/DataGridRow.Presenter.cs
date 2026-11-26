using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

public partial class DataGridRow
{
	/// <summary>The presenter holding the row's cells; it is the row's only visual child.</summary>
	internal DataGridCellsPresenter? Presenter { get; set; }

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren =>
		Presenter != null ? new UIElement[] { Presenter } : base.VisualChildren;
}
