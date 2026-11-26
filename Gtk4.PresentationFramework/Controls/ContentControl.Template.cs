namespace System.Windows.Controls;

public partial class ContentControl
{
	UIElement? _templateHosted;

	// A ControlTemplate replaces the control's own content widgets; its ContentPresenter shows the content.
	/// <inheritdoc/>
	protected override void ApplyTemplateRoot()
	{
		if (_box == null) return;
		var root = TemplateRootInternal;
		if (ReferenceEquals(root, _templateHosted)) return;

		if (_templateHosted != null)
		{
			_box.Remove(_templateHosted.Widget);
			_templateHosted.LogicalParent = null;
			_templateHosted = null;
		}
		if (root != null)
		{
			DetachOwnContent();
			root.LogicalParent = this;
			_templateHosted = root;
			_box.Add(root.Widget);
			OnApplyTemplate();
			return;
		}
		_box.Prepend(_label!);
		RefreshPresentation();
	}

	void DetachOwnContent()
	{
		if (_hosted != null)
		{
			UnhostElement(_hosted.Widget);
			ContentResolution.Release(_hosted, _hostedContent);
			_hosted = null;
			_hostedContent = null;
		}
		if (_label != null && ReferenceEquals(_label.Parent, _box)) _box!.Remove(_label);
	}
}
