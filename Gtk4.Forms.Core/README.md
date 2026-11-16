# Gtk4.Forms.Core

The platform-independent half of Xamarin.Forms — the shared UI abstraction that per-platform
renderers map onto native controls at runtime.

```
dotnet add package Gtk4.Forms.Core
```

You normally do not reference this package directly: a backend package such as
[`Gtk4.Forms.Platform.Gtk`](https://www.nuget.org/packages/Gtk4.Forms.Platform.Gtk)
brings it in.

## What is in it

`Xamarin.Forms.Core.dll` (`netstandard2.0`) contains everything that does not depend on a
specific platform:

- **The property system** — `BindableObject` / `BindableProperty`, backing data bindings,
  styles and triggers.
- **The visual tree** — `Element` → `VisualElement` → `View` / `Page` / `Layout`, plus layout
  and measurement.
- **`Shell`** — flyout/tab navigation and URI-based routing.
- **Resources and styling** — `ResourceDictionary`, `Style`, and CSS `StyleSheets`.
- **App services** — `MessagingCenter`, `DependencyService`, `Device`, the animation system.
- **`PlatformConfiguration`** — the `<Platform>Specific` extension-method surface.
- **`Xamarin.Forms.Internals.Registrar`**, which maps a Forms type to a renderer type (keyed by
  `Visual` and priority) and is populated from `[assembly: ExportRenderer]` and friends during
  each backend's `Forms.Init(...)`.

## Companion packages

| Package | Purpose |
| --- | --- |
| `Gtk4.Forms.Platform.Base` | Facade this package references; the real assembly is supplied per platform |
| `Gtk4.Forms.Xaml` | Runtime XAML loader and markup extensions |
| `Gtk4.Forms.Build.Tasks` | Build-time XAML code generation and IL compilation |
| `Gtk4.Forms.Maps.Base` | `Map` control abstraction |
| `Gtk4.Forms.Platform.Gtk` | GTK backend |

`AssemblyVersion` is deliberately frozen at `2.0.0.0` for binding compatibility; the package
version is the one that moves.

## About this fork

Maintenance fork of the `5.0.0` branch of [xamarin/Xamarin.Forms](https://github.com/pieroviano/Xamarin.Forms),
published under the `Gtk4.` package prefix. Upstream Xamarin.Forms reached end of support on
**May 1, 2024** and was succeeded by [.NET MAUI](https://github.com/dotnet/maui); new
applications should target .NET MAUI instead.
