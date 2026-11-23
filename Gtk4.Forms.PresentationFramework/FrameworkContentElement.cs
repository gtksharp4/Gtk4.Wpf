using System.Windows.Input;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>
/// Framework-level base of flow content: name, tag, data context, style, resources, logical parent, language,
/// tool tip and cursor. Resources are looked up through the logical parents to the hosting
/// <see cref="FrameworkElement"/>.
/// <para>
/// Gaps: only <see cref="Binding"/> is supported on content elements (no MultiBinding or PriorityBinding);
/// validation errors are recorded in Validation.Errors but the Error event is not raised and no BindingGroup
/// is joined; a relative source of TemplatedParent resolves through the hosting element; triggers and templates
/// are not available; <see cref="ToolTip"/> and <see cref="Cursor"/> are stored but not shown (the flow document
/// is one GTK label).
/// </para>
/// </summary>
[RuntimeNameProperty("Name")]
public partial class FrameworkContentElement : ContentElement
{
	DependencyObject? _parent;
	ResourceDictionary? _resources;

	/// <summary>Gets or sets the name used by x:Name (backed by <see cref="NameProperty"/>).</summary>
	public string Name
	{
		get => (string)GetValue(NameProperty)!;
		set => SetValue(NameProperty, value);
	}

	/// <summary>Gets the logical parent (the owning element, or the hosting viewer for a document).</summary>
	public DependencyObject? Parent => _parent;

	/// <summary>
	/// Sets the logical parent and the inheritance parent together, so inherited values and resource lookups
	/// follow it.
	/// </summary>
	internal void SetLogicalParent(DependencyObject? parent)
	{
		_parent = parent;
		InheritanceParent = parent;
		if (parent != null) (this as Documents.TextElement)?.ReleaseTextContainer();
	}

	/// <summary>Gets or sets the local resource dictionary.</summary>
	public ResourceDictionary Resources
	{
		get => _resources ??= NewResources();
		set => SetResources(value);
	}

	/// <summary>Finds a resource in the logical parents, the hosting element and the application.</summary>
	/// <exception cref="InvalidOperationException">The resource is not found.</exception>
	public object? FindResource(object resourceKey)
	{
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));
		if (TryFindResourceCore(resourceKey, out var value)) return value;
		throw new InvalidOperationException($"Resource with key '{resourceKey}' not found.");
	}

	/// <summary>Finds a resource like <see cref="FindResource"/>; returns null when it is absent.</summary>
	public object? TryFindResource(object resourceKey)
	{
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));
		TryFindResourceCore(resourceKey, out var value);
		return value;
	}

	bool TryFindResourceCore(object key, out object? value)
	{
		DependencyObject? current = this;
		for (int steps = 0; current != null && steps < 1000; steps++)
		{
			if (current is FrameworkContentElement fce)
			{
				if (fce._resources != null && fce._resources.TryGetValue(key, out value)) return true;
				current = fce._parent;
			}
			else if (current is FrameworkElement fe)
				return fe.TryFindResourceFromContent(key, out value);
			else
				break;
		}
		if (Application.Current?.Resources.TryGetValue(key, out value) == true) return true;
		return SystemResources.TryFind(key, out value);
	}
}
