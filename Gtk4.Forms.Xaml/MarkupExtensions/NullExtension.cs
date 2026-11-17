using System;

namespace Gtk4.Forms.Xaml
{
	[ProvideCompiled("Gtk4.Forms.Build.Tasks.NullExtension")]
	[AcceptEmptyServiceProvider]
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}
