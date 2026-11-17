using Gtk4.Forms;
using Gtk4.Forms.ControlGallery.GTK;
using Gtk4.Forms.Controls.Issues.Helpers;
using Gtk4.Forms.Platform.GTK;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Gtk4.Forms.ControlGallery.GTK;

public class SampleNativeControl : ISampleNativeControl
{
	public View View
	{
		get
		{
			return new Label { Text = "NativeViews not supported on GTK" };
		}
	}
}