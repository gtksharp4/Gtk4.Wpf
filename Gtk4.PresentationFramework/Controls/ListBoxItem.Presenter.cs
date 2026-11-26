using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls;

public partial class ListBoxItem
{
	// The item is hosted through a ContentPresenter, the way WPF's default template does.
	ContentPresenter? _presenter;

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
		_presenter != null ? new UIElement[] { _presenter } : Enumerable.Empty<UIElement>();
}
