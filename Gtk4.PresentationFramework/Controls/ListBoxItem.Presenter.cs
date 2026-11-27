using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls;

public partial class ListBoxItem
{
	// The item is hosted through a ContentPresenter, the way WPF's default template does.
	ContentPresenter? _presenter;

	// A view (GridView) can replace the content presenter of the row.
	UIElement? _viewPresenter;

	/// <summary>The view-supplied row presenter, or null when the row uses the content presenter.</summary>
	internal UIElement? ViewPresenter => _viewPresenter;

	/// <summary>The element that presents the row in place of the content presenter, or null for the default.</summary>
	internal virtual UIElement? CreateViewPresenter() => null;

	Gtk.Widget BuildRowContent()
	{
		_viewPresenter = CreateViewPresenter();
		if (_viewPresenter == null) return CreatePresenter().Widget;
		_viewPresenter.LogicalParent = this;
		return _viewPresenter.Widget;
	}

	/// <summary>Replaces the row content after the owning view changed.</summary>
	internal void RebuildRowContent()
	{
		if (_row == null) return;
		if (_viewPresenter != null) _viewPresenter.LogicalParent = null;
		_presenter = null;
		_row.Child = BuildRowContent();
	}

	/// <summary>Kept for the generator; every item now goes through the presenter.</summary>
	internal bool UsesPresenter { get; set; }

	/// <summary>What the presenter shows when it is not <see cref="ContentControl.Content"/> itself.</summary>
	internal object? PresenterContent { get; set; }

	internal ContentPresenter? Presenter => _presenter;

	ContentPresenter CreatePresenter()
	{
		_presenter = new ContentPresenter { ContentSource = string.Empty };
		SyncPresenter();
		_presenter.LogicalParent = this;
		return _presenter;
	}

	void SyncPresenter()
	{
		if (_presenter == null) return;
		_presenter.ContentTemplate = ContentTemplate;
		_presenter.ContentTemplateSelector = ContentTemplateSelector;
		_presenter.ContentStringFormat = ContentStringFormat;
		_presenter.Content = PresenterContent ?? Content;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty || e.Property == ContentTemplateProperty
			|| e.Property == ContentTemplateSelectorProperty || e.Property == ContentStringFormatProperty)
			SyncPresenter();
	}

	internal override IEnumerable<UIElement> VisualChildren =>
		_viewPresenter != null ? new[] { _viewPresenter }
		: _presenter != null ? new UIElement[] { _presenter } : Enumerable.Empty<UIElement>();
}
