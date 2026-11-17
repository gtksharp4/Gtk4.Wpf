using System;
using System.Reflection;

namespace Gtk4.Forms.Xaml
{
	interface IValueConverterProvider
	{
		object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider);
	}
}