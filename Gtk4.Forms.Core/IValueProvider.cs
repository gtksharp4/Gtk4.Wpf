using System;

namespace Gtk4.Forms.Xaml
{
	public interface IValueProvider
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}
}