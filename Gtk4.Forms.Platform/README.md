# Gtk4.Forms.Platform.Base

The `Xamarin.Forms.Platform` facade. This package exists so that
[`Gtk4.Forms.Core`](https://www.nuget.org/packages/Gtk4.Forms.Core) has
something to compile against; you never reference it directly.

```
dotnet add package Gtk4.Forms.Platform.Base
```

## Why it is empty

`Xamarin.Forms.Platform.dll` is a *platform-flavoured* assembly. Core references it by name, but
its real content differs per backend: on Android, iOS and Tizen it carries internal placeholder
types marked `[RenderWith(typeof(SomeRenderer))]`, which is how Xamarin.Forms resolves a
control's **default** renderer — the ones that are not declared with `[assembly: ExportRenderer]`.

The assembly in this package is the `netstandard2.0` build of that file with the renderer
markers compiled out: type shapes only, no behaviour. At runtime a backend either supplies its
own flavour of the assembly (the "Forwarders" projects) or, as the GTK backend does, registers
every renderer explicitly with `[assembly: ExportRenderer]` and leaves this facade in place.

## About this fork

Maintenance fork of the `1.0.0` branch of [xamarin/Xamarin.Forms](https://github.com/pieroviano/Xamarin.Forms),
published under the `Gtk4.` package prefix. Upstream Xamarin.Forms reached end of support on
**May 1, 2024** and was succeeded by [.NET MAUI](https://github.com/dotnet/maui); new
applications should target .NET MAUI instead.
