using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>
/// Describes how a group of a grouped <see cref="ItemsControl"/> is shown. The header of each group is presented
/// from <see cref="HeaderTemplate"/> (or the selector) with the <see cref="Data.CollectionViewGroup"/> as its
/// content and data context.
/// </summary>
/// <remarks>
/// <see cref="Panel"/>, <see cref="ContainerStyle"/>, <see cref="ContainerStyleSelector"/>,
/// <see cref="HidesIfEmpty"/> and <see cref="AlternationCount"/> are stored only: groups are flattened into the
/// owner's item host with a header row, there is no <c>GroupItem</c> container.
/// </remarks>
public class GroupStyle : INotifyPropertyChanged
{
	static readonly GroupStyle _default = new GroupStyle();

	DataTemplate? _headerTemplate;
	DataTemplateSelector? _headerTemplateSelector;
	string? _headerStringFormat;
	Style? _containerStyle;
	StyleSelector? _containerStyleSelector;
	ItemsPanelTemplate? _panel;
	bool _hidesIfEmpty;
	int _alternationCount;

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>The style used when an <see cref="ItemsControl"/> has none for a level.</summary>
	public static GroupStyle Default => _default;

	/// <summary>Gets or sets the template of the group header.</summary>
	public DataTemplate? HeaderTemplate
	{
		get => _headerTemplate;
		set => Set(ref _headerTemplate, value, nameof(HeaderTemplate));
	}

	/// <summary>Gets or sets the selector of the group header template.</summary>
	public DataTemplateSelector? HeaderTemplateSelector
	{
		get => _headerTemplateSelector;
		set => Set(ref _headerTemplateSelector, value, nameof(HeaderTemplateSelector));
	}

	/// <summary>Gets or sets the string format of the group header.</summary>
	public string? HeaderStringFormat
	{
		get => _headerStringFormat;
		set => Set(ref _headerStringFormat, value, nameof(HeaderStringFormat));
	}

	/// <summary>Gets or sets the style of the group container (stored only).</summary>
	public Style? ContainerStyle
	{
		get => _containerStyle;
		set => Set(ref _containerStyle, value, nameof(ContainerStyle));
	}

	/// <summary>Gets or sets the selector of the group container style (stored only).</summary>
	public StyleSelector? ContainerStyleSelector
	{
		get => _containerStyleSelector;
		set => Set(ref _containerStyleSelector, value, nameof(ContainerStyleSelector));
	}

	/// <summary>Gets or sets the panel that lays out the items of a group (stored only).</summary>
	public ItemsPanelTemplate? Panel
	{
		get => _panel;
		set => Set(ref _panel, value, nameof(Panel));
	}

	/// <summary>Gets or sets whether a group with no items is hidden (stored only).</summary>
	public bool HidesIfEmpty
	{
		get => _hidesIfEmpty;
		set => Set(ref _hidesIfEmpty, value, nameof(HidesIfEmpty));
	}

	/// <summary>Gets or sets the number of alternating group containers (stored only).</summary>
	public int AlternationCount
	{
		get => _alternationCount;
		set => Set(ref _alternationCount, value, nameof(AlternationCount));
	}

	void Set<T>(ref T field, T value, string name)
	{
		if (Equals(field, value)) return;
		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
