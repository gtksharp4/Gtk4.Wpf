namespace Gtk4.Forms.Controls
{
	public interface IRegistrarValidationService
	{
		bool Validate(VisualElement element, out string message);
	}
}
