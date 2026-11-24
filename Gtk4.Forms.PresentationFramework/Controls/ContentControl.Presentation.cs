using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Controls;

public partial class ContentControl
{
	UIElement? _hosted;
	object? _hostedContent;

	/// <summary>Whether the GTK widget that shows the content exists (subclasses with another widget override).</summary>
	internal virtual bool PresentationReady => IsWidgetCreated;

	/// <summary>Whether recognized access keys in string content are processed (WPF RecognizesAccessKey).</summary>
	internal virtual bool RecognizesAccessKeyInContent => false;

	/// <summary>Puts the resolved element's widget into the control's GTK widget.</summary>
	internal virtual void HostElement(Gtk.Widget widget)
	{
		ContentLabel.Text = string.Empty;
		ContentContainer.Add(widget);
	}

	/// <summary>Takes a previously hosted widget out again.</summary>
	internal virtual void UnhostElement(Gtk.Widget widget) => ContentContainer.Remove(widget);

	/// <summary>Shows plain text in the control's native text widget.</summary>
	internal virtual void ShowText(string text) => ContentLabel.Text = text;

	/// <summary>The element hosting the content (null while the content is shown as plain text).</summary>
	internal UIElement? HostedElement => _hosted;

	/// <summary>Resolves <see cref="Content"/> through the shared rules and shows it.</summary>
	internal void RefreshPresentation()
	{
		if (!PresentationReady || _templateHosted != null) return;
		var content = Content;
		if (_hosted != null)
		{
			UnhostElement(_hosted.Widget);
			ContentResolution.Release(_hosted, _hostedContent);
			_hosted = null;
			_hostedContent = null;
		}

		var element = ContentResolution.ResolveStructured(
			this, content, ContentTemplate, ContentTemplateSelector, out _);
		if (element != null)
		{
			_hosted = element;
			_hostedContent = content;
			HostElement(element.Widget);
			return;
		}
		ShowContentAsText(content);
	}

	void ShowContentAsText(object? content)
	{
		if (content is string s && string.IsNullOrEmpty(ContentStringFormat))
			ShowContentText(s);
		else
			ShowText(ContentResolution.ToText(content, ContentStringFormat));
	}

	internal override void OnLogicalParentChangedResources()
	{
		base.OnLogicalParentChangedResources();
		// An implicit DataTemplate may only be found once the control is in the tree.
		if (PresentationReady && Content != null && Content is not UIElement && ContentTemplate == null)
			RefreshPresentation();
	}

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren =>
		_templateHosted != null ? new[] { _templateHosted }
		: _hosted != null ? new[] { _hosted }
		: Content is UIElement child ? new[] { child } : Enumerable.Empty<UIElement>();
}
