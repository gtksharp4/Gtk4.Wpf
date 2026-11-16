# Porting a Windows WPF App to Gtk4.Forms.Xaml — One Codebase, Three Desktops

## What this gets you

A classic WPF application runs on Windows only. Rehosting its XAML on **Gtk4.Forms** gives
you a single XAML + C# codebase that renders on native **GTK 4** widgets and runs on
**Windows, Linux and macOS** from the same build output.

This is not an automatic converter. WPF XAML and Forms XAML are different dialects of the same
idea, so the port is mechanical but manual: you keep your layout intent, data bindings and
view-model code, and you rewrite the markup and the bootstrap. This article is the step-by-step.

Be clear about the trade first:

| | WPF | Gtk4.Forms |
| --- | --- | --- |
| Platforms | Windows only | Windows, Linux, macOS |
| Widget toolkit | Win32 / DirectX | GTK 4 (via GtkSharp 4) |
| XAML namespace | `.../winfx/2006/xaml/presentation` | `http://xamarin.com/schemas/2014/forms` |
| Top-level UI | `Window` | `Application` + `Page` |
| Property system | `DependencyProperty` | `BindableProperty` |
| Data context | `DataContext` | `BindingContext` |
| Control set | full WPF | Forms control set (smaller, portable) |

If your app leans on WPF-only features — `DataTemplate` triggers with `MultiBinding`, custom
`ControlTemplate` chrome, `DrawingContext`, WPF 3D, `RichTextBox`, `DocumentViewer` — those have
no Forms equivalent and must be redesigned, not translated. For a straightforward
forms-over-data LOB app the port is routine.

## Prerequisites

### 1. .NET SDK

```
.NET SDK 10.0.100 or later
```

Pin it in `global.json` if you want reproducibility (`rollForward: latestFeature`).

### 2. Native GTK 4 runtime

Gtk4.Forms binds **GtkSharp 4 (GTK 4.22)**, published on nuget.org — and the `GtkSharp4` package
carries the native GTK 4 runtime for Windows, Linux and macOS as package assets. So there is
**nothing to install and no special feed to add**: a plain `dotnet restore` pulls the managed binding
and its native libraries together, and the same build runs on all three OSes. A Linux box still needs
a display server (X11/Wayland, or `xvfb-run` for headless CI); macOS renders through the Quartz GDK
backend.

## The project file

A WPF project is `<Project Sdk="Microsoft.NET.Sdk">` with `UseWPF=true` and a Windows-only TFM
(`net10.0-windows`). Replace that with a plain, OS-neutral executable.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Exe, NOT WinExe. WinExe is a Windows-subsystem concept with no meaning on Linux/macOS,
         and it suppresses console output you want for diagnostics. -->
    <OutputType>Exe</OutputType>

    <!-- net10.0, NOT netstandard2.0 and NOT net10.0-windows. An application must name a concrete,
         OS-neutral runtime: netstandard emits no runtimeconfig.json (launch fails with
         "libhostpolicy not found"); a -windows TFM defeats the point of running on Linux/macOS. -->
    <TargetFramework>net10.0</TargetFramework>

    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Gtk4.Forms.Core" Version="5.0.0.*" />
    <PackageReference Include="Gtk4.Forms.Xaml" Version="5.0.0.*" />
    <PackageReference Include="Gtk4.Forms.Platform.Gtk" Version="5.0.0.*" />
    <!-- Gtk4.Forms.Platform.Gtk transitively pulls in GtkSharp4 and the backend. -->
  </ItemGroup>
</Project>
```

Remove from the old WPF project: `UseWPF`, `ApplicationDefinition` (the `App.xaml`
auto-wiring), `net*-windows`, and any `PackageReference` to WPF-only libraries.

Keep your `.xaml` files as `<Page>` build items — `Gtk4.Forms.Build.Tasks` (pulled in transitively
by the packages) runs `XamlGTask`/`XamlCTask` to generate `InitializeComponent` and compile the
markup, exactly like WPF's XAML build tasks. SDK-style default globbing already includes `.xaml`;
no explicit `<Compile>` entries are needed.

**Shortcut — `Gtk4.Forms.Base`.** Instead of hand-writing the three package references above *and*
the entry point in Step 4, you can add the single meta-package `Gtk4.Forms.Base`. At build time it
rewrites the `.csproj` (sets `OutputType=Exe` / `TargetFramework=net10.0`, strips any leftover
`Xamarin.Forms`/`net*-windows` bits, and pulls in Core, Xaml, Platform.Gtk and the build tasks) and
injects the GTK `Main` for you, so the manual project file and the Step 4 bootstrap become optional.
Because an MSBuild target cannot retarget the build already running, the **first** build after adding
the package rewrites the project and the **second** is the clean one (the rewrite is idempotent;
`SkipGtkConvert`/`SkipGtkHost` opt out). The manual versions below remain worth reading as a precise
description of what the package does.

## Conceptual mapping

| WPF | Gtk4.Forms | Note |
| --- | --- | --- |
| `App : Application` + `App.xaml` | `App : Xamarin.Forms.Application` | Forms `Application` is not a `Window`; it has a `MainPage`. |
| `Window` | `ContentPage` (inside a `FormsWindow` at runtime) | The native window is created by the host, not by your XAML. |
| `UserControl` | `ContentView` | |
| `Page` (navigation) | `ContentPage` + `NavigationPage` | |
| `Grid` / `StackPanel` / `DockPanel` / `WrapPanel` | `Grid` / `StackLayout` / `FlexLayout` / `AbsoluteLayout` | `Grid` is close; others need reshaping. |
| `TextBlock` / `TextBox` | `Label` / `Entry` (single line), `Editor` (multi-line) | |
| `Button` / `CheckBox` / `RadioButton` | `Button` / `CheckBox` / `RadioButton` | Same names, Forms properties. |
| `ComboBox` | `Picker` | |
| `ListBox` / `ListView` / `DataGrid` | `ListView` / `CollectionView` | No direct `DataGrid`. |
| `Image` | `Image` | `Source="file.png"` → `ImageSource.FromFile`. |
| `DependencyProperty` | `BindableProperty` | Same pattern, different base types. |
| `DataContext` | `BindingContext` | Inherited down the tree the same way. |
| `{Binding Path=X}` | `{Binding X}` | `Path=` is implicit; `Mode`, `Converter` carry over. |
| `{StaticResource K}` / `{DynamicResource K}` | `{StaticResource K}` / `{DynamicResource K}` | `ResourceDictionary` works the same. |
| `Style` / `Setter` / `Trigger` | `Style` / `Setter` / `Trigger` | Supported; `TargetType` is required. |
| `ICommand` + `Command=` | `ICommand` + `Command=` | Your view-model commands move over unchanged. |
| `Visibility` (`Collapsed`/`Visible`) | `IsVisible` (bool) | Drop the `Visibility` enum; bind a bool. |
| `Margin="l,t,r,b"` | `Margin="l,t,r,b"` | Same. `Padding` same. |
| `HorizontalAlignment` | `HorizontalOptions` (`LayoutOptions`) | `Stretch`→`FillAndExpand`, etc. |

Your **view models, commands, converters, and `INotifyPropertyChanged` code generally move over
unchanged** — they never referenced WPF types. The work is in the markup and the bootstrap.

## Step 1 — Convert the XAML root namespaces

Every WPF file opens with the presentation namespace. Swap it for the Forms namespace.

**WPF:**
```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
        Title="My App" Height="450" Width="800">
  <Grid>
    <TextBlock Text="{Binding Greeting}" />
  </Grid>
</Window>
```

**Gtk4.Forms:**
```xml
<ContentPage x:Class="MyApp.MainPage"
             xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             Title="My App">
  <Grid>
    <Label Text="{Binding Greeting}" />
  </Grid>
</ContentPage>
```

Notes:
- `Window` → `ContentPage`. Window size is set at runtime on the host window, not in XAML.
- `xmlns` (default) changes; `xmlns:x` stays identical.
- `clr-namespace` references for your own types add `;assembly=YourAssembly`:
  `xmlns:local="clr-namespace:MyApp;assembly=MyApp"`.
- `TextBlock` → `Label`, `TextBox` → `Entry`/`Editor`, and so on per the mapping table.

## Step 2 — Code-behind

The partial class pattern is identical to WPF; only the base type changes.

```csharp
using Xamarin.Forms;

namespace MyApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();            // generated by XamlGTask, same as WPF
            BindingContext = new MainViewModel();   // was DataContext in WPF
        }
    }
}
```

## Step 3 — The Application class

WPF's `App.xaml` with `StartupUri` has no Forms analogue. Forms' `Application` owns a `MainPage`.

```csharp
using Xamarin.Forms;

namespace MyApp
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new MainPage());   // or just new MainPage()
        }
    }
}
```

If you prefer XAML for `App`, make `App.xaml` a `ResourceDictionary` host exactly as in Forms
samples (`<Application ...>` root, `x:Class="MyApp.App"`), and set `MainPage` in the code-behind
constructor after `InitializeComponent()`.

## Step 4 — The entry point (bootstrap)

This is the part with no WPF equivalent. WPF generates `Main` for you from `App.xaml`. Here you
write it: initialise GTK, initialise Forms, host the `Application` in a `FormsWindow`, run the
GTK main loop. (If you took the `Gtk4.Forms.Base` shortcut above, you can skip this section entirely
— the package injects an equivalent `Main` that reflects out your `Application` subclass and runs it.)

```csharp
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

namespace MyApp
{
    static class Program
    {
        [STAThread]   // required: GTK's Win32 backend calls OleInitialize, which needs STA
        static void Main()
        {
            GtkThemes.Init();
            Gtk.Application.Init();
            Forms.Init();

            var window = new FormsWindow();
            window.LoadApplication(new App());
            window.SetApplicationTitle("My App");
            window.SetApplicationIcon("icon.png");
            window.Show();

            Gtk.Application.Run();
        }
    }
}
```

Two bootstrap subtleties:

- **`[STAThread]` is mandatory.** On Windows, GTK's GDK backend calls `OleInitialize`, which
  fails on an MTA thread (`Gdk-ERROR: OleInitialize failed`) and hangs the process.
- **`Forms.Init()` only scans loaded assemblies.** If any renderers, effects or `Maps` live in
  a separate assembly, merely referencing the package is not enough — force the load by naming a
  type in it:

  ```csharp
  Forms.Init(new[] { typeof(Xamarin.Forms.Maps.GTK.MapRenderer).Assembly });
  Xamarin.Forms.Maps.GTK.FormsMaps.Init();
  ```

## Step 5 — Bindings, resources and styles

These are the highest-leverage part of the port because they usually carry over almost verbatim.

**Bindings** — drop the explicit `Path=`, keep everything else:

```xml
<!-- WPF -->      <TextBlock Text="{Binding Path=Name, Mode=OneWay}" />
<!-- Gtk4.Forms --><Label Text="{Binding Name, Mode=OneWay}" />
```

**Resources** — `ResourceDictionary`, `StaticResource` and `DynamicResource` all work:

```xml
<ContentPage.Resources>
  <ResourceDictionary>
    <Color x:Key="Accent">#2196F3</Color>
    <Style TargetType="Button">
      <Setter Property="BackgroundColor" Value="{StaticResource Accent}" />
      <Setter Property="TextColor" Value="White" />
    </Style>
  </ResourceDictionary>
</ContentPage.Resources>
```

Differences to fix by hand:
- `TargetType` is **required** on every Forms `Style` (WPF allows implicit styles without it in
  some cases).
- WPF's `{x:Type Button}` for `TargetType` → bare `TargetType="Button"`.
- `Trigger`, `DataTrigger`, `EventTrigger` exist; `MultiTrigger` exists; WPF `MultiBinding` and
  `PriorityBinding` do **not** — rework them as a converter or a view-model property.

**Triggers** work and are a good example of what survives:

```xml
<Entry x:Name="entry" BackgroundColor="Lime">
  <Entry.Triggers>
    <Trigger TargetType="Entry" Property="Text" Value="red">
      <Setter Property="BackgroundColor" Value="Red" />
    </Trigger>
  </Entry.Triggers>
</Entry>
```

## Step 6 — Assets

WPF `Resource`/`Content` build actions become Forms `Content` with copy-to-output, and
file-based images are loaded by name at runtime:

```xml
<ItemGroup>
  <Content Include="icon.png">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

```xml
<Image Source="icon.png" />           <!-- ImageSource.FromFile, resolved from the app dir -->
```

Image names in `ImageSource.FromFile(...)` are looked up flat in the output directory — keep the
files at the output root, not in subfolders, unless you include the path.

## Building and running on each OS

The build is plain `dotnet`. The **same** published output runs on all three OSes; only the
native GTK 4 runtime differs.

```powershell
dotnet build MyApp.csproj
dotnet run   --project MyApp.csproj
```

### Windows

- Nothing to install — the native GTK 4 runtime restores with the `GtkSharp4` package.
- No `DISPLAY`, no X server — the Win32 GDK backend is used directly.
- `dotnet run` just works.

### Linux

```bash
dotnet run --project MyApp.csproj
```

- Needs a display server (X11 or Wayland). On Wayland, GTK 4 picks the Wayland backend
  automatically; force X11 with `GDK_BACKEND=x11` if a renderer misbehaves.
- **Headless/CI** needs a virtual framebuffer, and the screen must be large enough for your
  windows (the 640×480 default clamps an 800×600 window):

  ```bash
  xvfb-run -a --server-args="-screen 0 1280x1024x24" \
    env GDK_BACKEND=x11 dotnet run --project MyApp.csproj
  ```

### macOS

```bash
dotnet run --project MyApp.csproj
```

- Nothing to install — the native GTK 4 runtime restores with the `GtkSharp4` package.
- GTK renders through the Quartz GDK backend; no XQuartz required for GTK 4.
- On Apple Silicon, make sure the GTK 4 arch matches the .NET runtime arch (both arm64, or both
  x64 under Rosetta) or the native load fails.

## Threading

GTK is not thread-safe. **Everything that touches a widget must run on the thread that called
`Forms.Init()`** — the thread running `Gtk.Application.Run()`. From a background thread, marshal
back:

```csharp
Device.BeginInvokeOnMainThread(() => label.Text = "done");
```

This is the direct analogue of WPF's `Dispatcher.Invoke`.

## What does not port automatically

| WPF feature | Status in Gtk4.Forms | Do instead |
| --- | --- | --- |
| `ControlTemplate` chrome / full templating | Limited | `ControlTemplate` exists for `TemplatedView`/`TemplatedPage`; complex WPF templates must be redesigned. |
| `DataGrid` | None | `CollectionView` / `ListView` with a `DataTemplate`. |
| `MultiBinding`, `PriorityBinding` | None | A value converter or a computed view-model property. |
| `Visibility` enum | None | Bind a `bool` to `IsVisible`. |
| `RichTextBox`, `FlowDocument`, 3D | None | Redesign; no equivalent. |
| WPF `Trigger` on arbitrary `RoutedEvent` | Partial | `EventTrigger` + `TriggerAction`. |
| `x:Static` to arbitrary CLR statics | Supported, but narrower | Verify each reference compiles. |
| Pixel-exact custom drawing (`OnRender`) | Via renderers | Write an `IVisualElementRenderer` or a custom renderer in `Xamarin.Forms.Platform.GTK`. |

A pragmatic order of work: port the view models and converters first (they usually compile as-is),
then one page at a time starting with the simplest, running the app after each so GTK layout
problems surface early rather than all at once.

## Summary

1. New SDK-style project: `OutputType=Exe`, `TargetFramework=net10.0` (OS-neutral).
2. Reference `Gtk4.Forms.Core` + `Gtk4.Forms.Xaml` + `Gtk4.Forms.Platform.Gtk` from nuget.org (the
   `GtkSharp4` dependency, and its native runtime, come along automatically).
3. Rewrite each `Window` as a `ContentPage`; swap the default `xmlns` to the Forms namespace and
   translate the controls.
4. `DataContext` → `BindingContext`; drop `Path=`; keep commands, converters and view models.
5. Write the `Main` bootstrap: `GtkThemes.Init()` → `Gtk.Application.Init()` → `Forms.Init()` →
   `FormsWindow.LoadApplication(new App())` → `Gtk.Application.Run()`, on an `[STAThread]`.
6. Run the **same** build everywhere — the native GTK 4 runtime ships with the package, so there is
   nothing to install per OS (Linux still needs a display: X11/Wayland or `xvfb-run`).

Steps 1, 2 and 5 can be collapsed into a single `PackageReference` to **`Gtk4.Forms.Base`**, which
rewrites the project and injects the `Main` for you (see the shortcut under *The project file*); the
XAML translation in steps 3–4 is the part that stays manual.

The reward is a WPF-shaped codebase — XAML, bindings, styles, MVVM — that is no longer tied to
Windows.

---

*Part of the Gtk4.Forms `5.0.0` maintenance fork. See also
[`article-gtk4-forms-gtk.md`](article-gtk4-forms-gtk.md) for the architecture of the backend,
and the `Gtk4.Forms.ControlGallery` project for a complete runnable example of the bootstrap
described here.*
