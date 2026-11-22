using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls;

public partial class ListBoxItem
{
	// A templated item is hosted through a ContentPresenter, the way WPF's default template does.
	ContentPresenter? _presenter;

	/// <summary>Whether the item's visual is a <see cref="ContentPresenter"/> (set before the widget exists).</summary>
	internal bool UsesPresenter { get; set; }

	/// <summary>What the presenter shows when it is not <see cref="ContentControl.Content"/> itself.</summary>
	internal object? PresenterContent { get; set; }

	internal ContentPresenter? Presenter => _presenter;

	ContentPresenter CreatePresenter()
	{
		_presenter = new ContentPresenter
		{
			Content = PresenterContent ?? Content,
			ContentTemplate = ContentTemplate,
		};
		_presenter.LogicalParent = this;
		return _presenter;
	}

	internal override IEnumerable<UIElement> VisualChildren =>
		_presenter != null ? new UIElement[] { _presenter } : base.VisualChildren;
}
