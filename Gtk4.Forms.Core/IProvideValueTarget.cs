namespace Gtk4.Forms.Xaml
{
	public interface IProvideValueTarget
	{
		object TargetObject { get; }
		object TargetProperty { get; }
	}
}