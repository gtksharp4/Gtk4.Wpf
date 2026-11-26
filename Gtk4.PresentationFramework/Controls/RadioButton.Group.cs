using System.Collections.Generic;
using System.Windows.Media;

namespace System.Windows.Controls;

public partial class RadioButton
{
	/// <summary>Identifies the <see cref="GroupName"/> property.</summary>
	public static readonly DependencyProperty GroupNameProperty =
		DependencyProperty.Register(
			nameof(GroupName), typeof(string), typeof(RadioButton),
			new PropertyMetadata(string.Empty, (d, e) =>
				((RadioButton)d).UpdateGroup((string?)e.OldValue, (string?)e.NewValue)));

	/// <summary>Gets or sets the group name; empty groups by parent.</summary>
	public string GroupName
	{
		get => (string?)GetValue(GroupNameProperty) ?? string.Empty;
		set => SetValue(GroupNameProperty, value);
	}

	// Named groups are per-process (no root scoping yet).
	static readonly Dictionary<string, List<WeakReference<RadioButton>>> _groups =
		new Dictionary<string, List<WeakReference<RadioButton>>>();

	void UpdateGroup(string? oldName, string? newName)
	{
		if (!string.IsNullOrEmpty(oldName) && _groups.TryGetValue(oldName!, out var old))
			old.RemoveAll(r => !r.TryGetTarget(out var rb) || ReferenceEquals(rb, this));
		if (!string.IsNullOrEmpty(newName))
		{
			if (!_groups.TryGetValue(newName!, out var list))
				_groups[newName!] = list = new List<WeakReference<RadioButton>>();
			list.Add(new WeakReference<RadioButton>(this));
		}
		if (_button != null)
			ApplyGtkGroup();
	}

	void ApplyGtkGroup()
	{
		if (string.IsNullOrEmpty(GroupName) || _button == null) return;
		foreach (var other in Peers())
		{
			if (other._button == null) continue;
			_button.Group = other._button;
			return;
		}
	}

	bool HasCheckedPeer()
	{
		foreach (var peer in Peers())
			if (peer.IsChecked == true)
				return true;
		return false;
	}

	// Other live radio buttons exclusive with this one.
	IEnumerable<RadioButton> Peers()
	{
		var name = GroupName;
		if (name.Length > 0)
		{
			if (!_groups.TryGetValue(name, out var list)) yield break;
			foreach (var wr in list.ToArray())
				if (wr.TryGetTarget(out var rb) && !ReferenceEquals(rb, this))
					yield return rb;
			yield break;
		}
		// Ungrouped: siblings under the visual parent (templated/generated); when the parent does not
		// expose this button as a visual child, the logical parent and its children.
		var siblings = new List<RadioButton>();
		bool found = false;
		var visualParent = VisualTreeHelper.GetParent(this);
		int count = visualParent == null ? 0 : VisualTreeHelper.GetChildrenCount(visualParent);
		for (int i = 0; i < count; i++)
		{
			if (VisualTreeHelper.GetChild(visualParent!, i) is not RadioButton rb) continue;
			if (ReferenceEquals(rb, this)) found = true;
			else if (rb.GroupName.Length == 0) siblings.Add(rb);
		}
		if (found)
		{
			foreach (var sibling in siblings) yield return sibling;
			yield break;
		}
		var parent = LogicalTreeHelper.GetParent(this);
		if (parent == null) yield break;
		foreach (var child in LogicalTreeHelper.GetChildren(parent))
			if (child is RadioButton rb && !ReferenceEquals(rb, this) && rb.GroupName.Length == 0)
				yield return rb;
	}
}
