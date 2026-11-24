namespace System.Windows.Controls.Primitives;

public abstract partial class ButtonBase
{
	/// <summary>Whether the GTK button widget exists.</summary>
	internal virtual bool GtkWidgetReady => false;

	/// <summary>Shows the mnemonic text as the GTK widget's label.</summary>
	internal virtual void SetGtkLabel(string mnemonic) { }

	/// <summary>Replaces the GTK widget's child (null removes it).</summary>
	internal virtual void SetGtkChild(Gtk.Widget? child) { }

	// Plain string content is already on the GTK label; setting it again would replace the styled label node.
	protected void PresentAtCreation()
	{
		if (Content is string && ContentTemplate == null && ContentTemplateSelector == null
			&& string.IsNullOrEmpty(ContentStringFormat))
			return;
		RefreshPresentation();
	}

	internal override bool PresentationReady => GtkWidgetReady;

	internal override bool RecognizesAccessKeyInContent => true;

	internal override void HostElement(Gtk.Widget widget) => SetGtkChild(widget);

	internal override void UnhostElement(Gtk.Widget widget) => SetGtkChild(null);

	internal override void ShowContentText(string text) => SetGtkLabel(AccessKeyBinding.Apply(this, text));

	internal override void ShowText(string text)
	{
		AccessKeyBinding.Register(this, null);
		SetGtkLabel(text.Replace("_", "__"));
	}
}
