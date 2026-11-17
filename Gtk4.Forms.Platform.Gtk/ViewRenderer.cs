using NativeView = Gtk.Widget;

namespace Gtk4.Forms.Platform.GTK;

public abstract class ViewRenderer : ViewRenderer<View, NativeView>
{

}

public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView, TNativeView>
	where TView : View where TNativeView : NativeView
{
	private string _defaultAccessibilityLabel;
	private string _defaultAccessibilityHint;

	protected override void Dispose(bool disposing)
	{
		// Only tear the native control down on a deterministic Dispose, never from the GC
		// finalizer (disposing == false). Under GtkSharp 4 Gtk.Widget.Destroy() walks to the
		// parent and reconstructs its managed wrapper; on the finalizer thread that both
		// touches GTK off the UI thread and throws MissingIntPtrCtorException for a managed
		// renderer subclass. GObject's own toggle-ref drop during finalization releases the
		// native widget without our help.
		if (disposing && Control != null)
		{
			Control.Destroy();
			Control = null;
		}
		base.Dispose(disposing);
	}

	protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
	{
		base.OnElementChanged(e);

		if (e.NewElement != null)
		{
			UpdateBackgroundColor();
		}
	}

	protected override void SetNativeControl(TNativeView view)
	{
		base.SetNativeControl(view);

		Add(view);
	}

	protected override void SetAccessibilityHint()
	{
		if (Control == null)
		{
			base.SetAccessibilityHint();
			return;
		}

		if (Element == null)
			return;

		if (_defaultAccessibilityHint == null)
			_defaultAccessibilityHint = Control.Accessible.Name;

		var helpText = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;

		if (!string.IsNullOrEmpty(helpText))
		{
			Control.Accessible.Name = helpText;
		}
	}

	protected override void SetAccessibilityLabel()
	{
		if (Control == null)
		{
			base.SetAccessibilityLabel();
			return;
		}

		if (Element == null)
			return;

		if (_defaultAccessibilityLabel == null)
			_defaultAccessibilityLabel = Control.Accessible.Description;

		var name = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

		if (!string.IsNullOrEmpty(name))
		{
			Control.Accessible.Description = name;
		}
	}
}