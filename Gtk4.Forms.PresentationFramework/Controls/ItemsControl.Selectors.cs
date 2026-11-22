using System.Collections;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Selector/path extensions for <see cref="ItemsControl"/>:
/// <see cref="ItemTemplateSelector"/>, <see cref="ItemContainerStyle"/>,
/// <see cref="ItemContainerStyleSelector"/>, <see cref="DisplayMemberPath"/>.
/// <para>
/// Priority when creating a container:
/// 1. <see cref="ItemTemplateSelector"/> (if set, wins over <see cref="ItemTemplate"/>)
/// 2. <see cref="ItemTemplate"/>
/// 3. <see cref="DisplayMemberPath"/> — creates a <see cref="ContentPresenter"/> that
///    shows the value of the named property.
/// 4. Default: <see cref="ContentPresenter"/> bound to the whole item.
/// </para>
/// <para>
/// Container styling priority: <see cref="ItemContainerStyleSelector"/> (if set, wins over
/// <see cref="ItemContainerStyle"/>), then <see cref="ItemContainerStyle"/>.
/// </para>
/// </summary>
public partial class ItemsControl
{
	// ── ItemTemplateSelector DP ───────────────────────────────────────────────

	/// <summary>Selects a template per item; takes priority over <see cref="ItemTemplate"/>.</summary>
	public static readonly DependencyProperty ItemTemplateSelectorProperty =
		DependencyProperty.Register(
			nameof(ItemTemplateSelector),
			typeof(DataTemplateSelector),
			typeof(ItemsControl),
			new PropertyMetadata(null, OnSelectorPropertyChanged));

	/// <summary>Gets or sets the item template selector.</summary>
	public DataTemplateSelector? ItemTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(ItemTemplateSelectorProperty);
		set => SetValue(ItemTemplateSelectorProperty, value);
	}

	// ── ItemContainerStyle DP ─────────────────────────────────────────────────

	/// <summary>Style applied to each item container.</summary>
	public static readonly DependencyProperty ItemContainerStyleProperty =
		DependencyProperty.Register(
			nameof(ItemContainerStyle),
			typeof(Style),
			typeof(ItemsControl),
			new PropertyMetadata(null, OnSelectorPropertyChanged));

	/// <summary>Gets or sets the item container style.</summary>
	public Style? ItemContainerStyle
	{
		get => (Style?)GetValue(ItemContainerStyleProperty);
		set => SetValue(ItemContainerStyleProperty, value);
	}

	// ── ItemContainerStyleSelector DP ────────────────────────────────────────

	/// <summary>Selects a style per container; takes priority over <see cref="ItemContainerStyle"/>.</summary>
	public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
		DependencyProperty.Register(
			nameof(ItemContainerStyleSelector),
			typeof(StyleSelector),
			typeof(ItemsControl),
			new PropertyMetadata(null, OnSelectorPropertyChanged));

	/// <summary>Gets or sets the item container style selector.</summary>
	public StyleSelector? ItemContainerStyleSelector
	{
		get => (StyleSelector?)GetValue(ItemContainerStyleSelectorProperty);
		set => SetValue(ItemContainerStyleSelectorProperty, value);
	}

	// ── DisplayMemberPath DP ─────────────────────────────────────────────────

	/// <summary>
	/// Name of the data-item property whose string value is shown in the container
	/// when no <see cref="ItemTemplate"/> / <see cref="ItemTemplateSelector"/> is set.
	/// </summary>
	public static readonly DependencyProperty DisplayMemberPathProperty =
		DependencyProperty.Register(
			nameof(DisplayMemberPath),
			typeof(string),
			typeof(ItemsControl),
			new PropertyMetadata(null, OnSelectorPropertyChanged));

	/// <summary>Gets or sets the display member path.</summary>
	public string? DisplayMemberPath
	{
		get => (string?)GetValue(DisplayMemberPathProperty);
		set => SetValue(DisplayMemberPathProperty, value);
	}

	// ── Change callback ───────────────────────────────────────────────────────

	static void OnSelectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).RebuildItems();
	}

	// ── Override GetContainerForItem (selector logic) ─────────────────────────

	/// <summary>
	/// Creates an item container honouring <see cref="ItemTemplateSelector"/>,
	/// <see cref="ItemTemplate"/>, <see cref="DisplayMemberPath"/>,
	/// <see cref="ItemContainerStyleSelector"/>, and <see cref="ItemContainerStyle"/>.
	/// </summary>
	protected virtual UIElement GetContainerForItem(object? item)
	{
		UIElement container = CreateContainerForItem(item);
		ApplyContainerStyle(container, item);
		return container;
	}

	UIElement CreateContainerForItem(object? item)
	{
		var template = ResolveItemTemplate(item);
		if (template != null && template.HasContent)
		{
			var visual = template.CreateVisualTreeForItem(item);
			if (visual != null) return visual;
		}

		var text = ResolveDisplayText(item);
		if (text != null) return new ContentPresenter { Content = text };

		// 4. Default.
		return new ContentPresenter { Content = item };
	}

	void ApplyContainerStyle(UIElement container, object? item)
	{
		Style? style = null;
		var styleSelector = ItemContainerStyleSelector;
		if (styleSelector != null)
			style = styleSelector.SelectStyle(item, container as DependencyObject);
		if (style == null)
			style = ItemContainerStyle;
		if (style != null && container is DependencyObject dep)
			style.Apply(dep);
	}
}
