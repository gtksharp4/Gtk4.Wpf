namespace System.Windows.Controls;

public partial class ListBox
{
	// An ItemTemplate (or selector choice) or DisplayMemberPath makes the item show through a presenter.
	void ConfigurePresenter(ListBoxItem container, object? item)
	{
		var template = item is UIElement ? null : ResolveItemTemplate(item);
		if (template != null && template.HasContent)
		{
			container.ContentTemplate = template;
			container.UsesPresenter = true;
			return;
		}
		var text = item is UIElement ? null : ResolveDisplayText(item);
		if (text == null) return;
		container.PresenterContent = text;
		container.UsesPresenter = true;
	}

	/// <inheritdoc/>
	protected override bool IsItemItsOwnContainerOverride(object? item) => item is ListBoxItem;
}
