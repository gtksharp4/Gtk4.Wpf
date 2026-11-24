using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A simple content label backed by <see cref="Gtk.Label"/>.
/// When <see cref="ContentControl.Content"/> is a string the GTK label text
/// is updated directly; <see cref="UIElement"/> content is not supported
/// (use <see cref="Border"/> or a layout panel for that case).
/// </summary>
public partial class Label : ContentControl
{
	static Label()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(Label),
			new PropertyMetadata(typeof(Label)));
	}

	/// <inheritdoc/>
	protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
		new System.Windows.Automation.Peers.LabelAutomationPeer(this);

	/// <summary>The inner label that carries the accessible node; the box around it is only a wrapper.</summary>
	internal Gtk.Widget AccessibleTarget => _label ?? Widget;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		_label = new Gtk.Label(string.Empty)
		{
			Xalign = 0f
		};
		if (Content is string text)
			AccessKeyBinding.SetLabel(_label, AccessKeyBinding.Apply(this, text));
		else
			_label.Text = Content?.ToString() ?? string.Empty;
		_box.Add(_label);
		return _box;
	}
}