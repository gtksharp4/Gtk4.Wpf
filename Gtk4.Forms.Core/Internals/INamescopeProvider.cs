using System;
using System.ComponentModel;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Xaml.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}