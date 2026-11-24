using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// WPF's <c>ContentPresenter</c> content resolution, shared by every content and header host:
/// a UIElement, the explicit template, the selector, an implicit DataType template, an Inline, then text.
/// </summary>
internal static partial class ContentResolution
{
	/// <summary>
	/// Returns the element that presents <paramref name="content"/> inside <paramref name="owner"/> (text
	/// content becomes a TextBlock or AccessText), with its logical parent set to the owner; null for null content.
	/// </summary>
	internal static UIElement? Resolve(
		FrameworkElement owner, object? content, DataTemplate? template, DataTemplateSelector? selector,
		string? stringFormat, bool recognizesAccessKey)
	{
		if (content == null) return null;
		var element = ResolveStructured(owner, content, template, selector, out _)
			?? FromText(content, stringFormat, recognizesAccessKey);
		element.LogicalParent = owner;
		return element;
	}

	/// <summary>
	/// As <see cref="Resolve"/> but returns null when the content falls through to plain text, which the
	/// hosts show with their own native text widget. <paramref name="applied"/> is the template used, if any.
	/// </summary>
	internal static UIElement? ResolveStructured(
		FrameworkElement owner, object? content, DataTemplate? template, DataTemplateSelector? selector,
		out DataTemplate? applied)
	{
		applied = null;
		if (content == null) return null;

		UIElement? element;
		if (content is UIElement direct)
			element = direct;
		else
		{
			var chosen = ChooseTemplate(owner, content, template, selector);
			element = chosen != null ? FromTemplate(owner, content, chosen) : null;
			if (element != null)
				applied = chosen;
			else if (content is Inline inline)
				element = FromInline(inline);
		}
		if (element != null) element.LogicalParent = owner;
		return element;
	}

	static DataTemplate? ChooseTemplate(
		FrameworkElement owner, object content, DataTemplate? template, DataTemplateSelector? selector)
	{
		if (template != null && template.HasContent) return template;
		if (template == null && selector != null)
		{
			var selected = selector.SelectTemplate(content, owner);
			if (selected != null && selected.HasContent) return selected;
		}
		var implicitTemplate = FrameworkElement.FindTemplateResource(owner, content);
		return implicitTemplate != null && implicitTemplate.HasContent ? implicitTemplate : null;
	}

	static UIElement? FromTemplate(FrameworkElement owner, object content, DataTemplate template)
	{
		var visual = template.CreateVisualTreeForItem(content);
		if (visual == null) return null;
		if (visual is FrameworkElement fe) fe.DataContext = content;
		FrameworkElement.StampTemplatedParentTree(visual, owner);
		return visual;
	}

	/// <summary>Detaches a hosted element: clears its parent and frees an inline it wrapped.</summary>
	internal static void Release(UIElement? hosted, object? content)
	{
		if (hosted == null) return;
		if (content is Inline inline && hosted is TextBlock tb)
			tb.Inlines.Remove(inline);
		hosted.LogicalParent = null;
	}
}
