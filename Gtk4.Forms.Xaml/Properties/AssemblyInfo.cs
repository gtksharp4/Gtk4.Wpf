using System.Runtime.CompilerServices;

using Gtk4.Forms;
using Gtk4.Forms.Internals;

[assembly: InternalsVisibleTo("Gtk4.Forms.Xaml.UnitTests")]
[assembly: InternalsVisibleTo("Gtk4.Forms.Build.Tasks")]
[assembly: Preserve]

[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms", "Gtk4.Forms.Xaml")]
[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms/design", "Gtk4.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Gtk4.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "Gtk4.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: TypeForwardedTo(typeof(Gtk4.Forms.Xaml.Internals.INameScopeProvider))]
#pragma warning restore CS0612 // Type or member is obsolete

[assembly: TypeForwardedTo(typeof(Gtk4.Forms.Xaml.Diagnostics.DebuggerHelper))]
[assembly: TypeForwardedTo(typeof(Gtk4.Forms.Xaml.Diagnostics.VisualDiagnostics))]
[assembly: TypeForwardedTo(typeof(Gtk4.Forms.Xaml.Diagnostics.VisualTreeChangeEventArgs))]
[assembly: TypeForwardedTo(typeof(Gtk4.Forms.Xaml.Diagnostics.XamlSourceInfo))]
