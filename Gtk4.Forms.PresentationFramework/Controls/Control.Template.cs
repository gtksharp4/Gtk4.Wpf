namespace System.Windows.Controls;

/// <summary>
/// Partial extension of <see cref="Control"/> that adds the <see cref="Template"/>
/// dependency property. When a <see cref="ControlTemplate"/> is set, its visual tree
/// is built and replaces the control's default widget.
/// </summary>
public abstract partial class Control
{
	/// <summary>The control template for this control.</summary>
	public static readonly DependencyProperty TemplateProperty =
		DependencyProperty.Register(
			nameof(Template),
			typeof(ControlTemplate),
			typeof(Control),
			new PropertyMetadata(null, OnTemplateChanged));

	static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		// Page shares this property (store-only there), so d is not always a Control.
		if (d is Control control) control.OnTemplateChanged((ControlTemplate?)e.NewValue);
	}

	/// <summary>Gets or sets the control template.</summary>
	public ControlTemplate? Template
	{
		get => (ControlTemplate?)GetValue(TemplateProperty);
		set => SetValue(TemplateProperty, value);
	}

	// The root UIElement built from the current template (null = default widget in use).
	UIElement? _templateRoot;

	/// <summary>
	/// Called when <see cref="Template"/> changes.
	/// Builds the template's visual tree and re-parents it into the control's GTK host.
	/// When <paramref name="template"/> is <c>null</c> the control reverts to its default widget.
	/// </summary>
	protected virtual void OnTemplateChanged(ControlTemplate? template)
	{
		_templateRoot = null;
		if (template == null || !template.HasContent)
		{
			ApplyTemplateRoot();
			return;
		}

		_templateRoot = template.CreateVisualTree();
		if (_templateRoot == null) return;

		// Stamp TemplatedParent on the produced root (and its subtree).
		StampTemplatedParent(_templateRoot, this);

		// Re-activate any RelativeSource=TemplatedParent bindings that the factory
		// registered while TemplatedParent was still null (the compiler-emitted path).
		Data.BindingEngine.RefreshTemplatedParentBindings(_templateRoot);

		// Re-host into the default widget when the control uses it.
		ApplyTemplateRoot();
	}

	/// <summary>
	/// Recursively stamps <paramref name="templatedParent"/> on each
	/// <see cref="FrameworkElement"/> in the subtree rooted at <paramref name="root"/>.
	/// Walks <see cref="UIElement"/> children via <see cref="UIElement.VisualChildren"/>.
	/// </summary>
	static void StampTemplatedParent(UIElement root, FrameworkElement templatedParent)
	{
		if (root is FrameworkElement fe)
			fe.TemplatedParent = templatedParent;
		foreach (var child in root.VisualChildren)
			StampTemplatedParent(child, templatedParent);
	}

	// The default host created by CreateWidget; null for controls that supply their own widget.
	Gtk.Box? _templateHost;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		var host = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		_templateHost = host;
		ApplyTemplateRoot();
		return host;
	}

	/// <summary>
	/// Hosts the current template root in the default widget. Controls that override
	/// <see cref="UIElement.CreateWidget"/> with their own widget keep this a no-op.
	/// </summary>
	protected virtual void ApplyTemplateRoot()
	{
		if (_templateHost == null) return;
		for (var child = _templateHost.FirstChild; child != null; child = _templateHost.FirstChild)
			_templateHost.Remove(child);
		if (_templateRoot == null) return;
		_templateHost.Append(_templateRoot.Widget);
		OnApplyTemplate();
	}

	/// <summary>
	/// Calls <see cref="ApplyTemplateRoot"/> after the backing widget is first created.
	/// Subclasses that override <see cref="UIElement.CreateWidget"/> should call this.
	/// </summary>
	protected void OnWidgetCreated()
	{
		if (_templateRoot != null)
			ApplyTemplateRoot();
	}
}