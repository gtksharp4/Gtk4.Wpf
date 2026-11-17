namespace Gtk4.Forms
{
	internal interface IAttachedObject
	{
		void AttachTo(BindableObject bindable);
		void DetachFrom(BindableObject bindable);
	}
}