using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Chooses the <see cref="GroupStyle"/> of a group at a nesting level.</summary>
public delegate GroupStyle? GroupStyleSelector(CollectionViewGroup group, int level);

public partial class ItemsControl
{
	ObservableCollection<GroupStyle>? _groupStyle;
	readonly List<UIElement> _groupHeaders = new List<UIElement>();

	/// <summary>The group header elements currently shown, in order.</summary>
	internal IReadOnlyList<UIElement> GroupHeaders => _groupHeaders;

	/// <summary>
	/// The styles of the group levels. When empty, a grouped <see cref="ItemsSource"/> shows its items flat, without
	/// headers; a style adds one header row ahead of the items of each group.
	/// </summary>
	public ObservableCollection<GroupStyle> GroupStyle
	{
		get
		{
			if (_groupStyle == null)
			{
				_groupStyle = new ObservableCollection<GroupStyle>();
				_groupStyle.CollectionChanged += OnGroupStyleChanged;
			}
			return _groupStyle;
		}
	}

	/// <summary>Gets or sets the callback that picks the <see cref="GroupStyle"/> of a group.</summary>
	public GroupStyleSelector? GroupStyleSelector { get; set; }

	/// <summary>Whether the items are grouped (the view has group descriptions).</summary>
	public bool IsGrouping => Items.Groups != null;

	void OnGroupStyleChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildItems();

	GroupStyle? ResolveGroupStyle(CollectionViewGroup group)
	{
		var chosen = GroupStyleSelector?.Invoke(group, 0);
		if (chosen != null) return chosen;
		return _groupStyle != null && _groupStyle.Count > 0 ? _groupStyle[0] : null;
	}

	// Index of the first item of each group, when the control shows group headers.
	Dictionary<int, CollectionViewGroup>? GroupHeaderStarts()
	{
		var groups = Items.Groups;
		if (groups == null || groups.Count == 0) return null;
		if (GroupStyleSelector == null && (_groupStyle == null || _groupStyle.Count == 0)) return null;
		var starts = new Dictionary<int, CollectionViewGroup>();
		int index = 0;
		foreach (var g in groups)
		{
			if (g is not CollectionViewGroup group) continue;
			if (group.ItemCount > 0) starts[index] = group;
			index += group.ItemCount;
		}
		return starts;
	}

	UIElement? CreateGroupHeader(CollectionViewGroup group)
	{
		var style = ResolveGroupStyle(group);
		if (style == null) return null;
		var presenter = new ContentPresenter { LogicalParent = this };
		var template = style.HeaderTemplateSelector?.SelectTemplate(group, presenter) ?? style.HeaderTemplate;
		if (template != null)
		{
			presenter.Content = group;
			presenter.ContentTemplate = template;
		}
		else
		{
			// Without a template the header shows the group name.
			presenter.Content = group.Name;
			presenter.ContentStringFormat = style.HeaderStringFormat;
		}
		return presenter;
	}

	void PlaceGroupHeader(Dictionary<int, CollectionViewGroup>? starts, int index)
	{
		if (starts == null || !starts.TryGetValue(index, out var group)) return;
		var header = CreateGroupHeader(group);
		if (header == null) return;
		_groupHeaders.Add(header);
		AddGroupHeader(header);
	}

	/// <summary>Adds a group header ahead of the next item container. The default host stacks it with the items.</summary>
	internal virtual void AddGroupHeader(UIElement header)
	{
		if (_defaultBox == null) return;
		if (_itemsPanelHost != null) _itemsPanelHost.Children.Add(header);
		else _defaultBox.Add(header.Widget);
	}

	/// <summary>Removes every group header added by <see cref="AddGroupHeader"/>.</summary>
	internal virtual void RemoveGroupHeaders()
	{
		foreach (var header in _groupHeaders)
		{
			header.LogicalParent = null;
			if (_defaultBox == null) continue;
			if (_itemsPanelHost != null) _itemsPanelHost.Children.Remove(header);
			else _defaultBox.Remove(header.Widget);
		}
		_groupHeaders.Clear();
	}

	void PlaceRowsInDefaultHost()
	{
		var starts = GroupHeaderStarts();
		for (int i = 0; i < _rows.Count; i++)
		{
			PlaceGroupHeader(starts, i);
			AddDefaultHostItem(_rows[i].Widget);
		}
	}
}
