using System;

namespace Gtk4.Forms.Xaml.Internals
{
	public interface INativeValueConverterService
	{
		bool ConvertTo(object value, Type toType, out object nativeValue);
	}
}