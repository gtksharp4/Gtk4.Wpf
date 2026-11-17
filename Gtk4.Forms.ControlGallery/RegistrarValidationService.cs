using Gtk4.Forms;
using Gtk4.Forms.ControlGallery.GTK;
using Gtk4.Forms.Controls;
using Gtk4.Forms.Platform.GTK;

[assembly: Dependency(typeof(RegistrarValidationService))]
namespace Gtk4.Forms.ControlGallery.GTK;

public class RegistrarValidationService : IRegistrarValidationService
{
	public bool Validate(VisualElement element, out string message)
	{
		message = "Success";

		if (element == null)
			return true;

		var renderer = Platform.GTK.Platform.CreateRenderer(element);

		if (renderer == null
		    || renderer.GetType().Name == "DefaultRenderer"
		   )
		{
			message = $"Failed to load proper GTK renderer for {element.GetType().Name}";
			return false;
		}

		return true;
	}
}