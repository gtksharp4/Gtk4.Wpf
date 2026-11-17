# Porting a Windows WPF App to Gtk4.Forms.Xaml — One Codebase, Three Desktops

## What this gets you

A classic WPF application runs on Windows only. Rehosting its XAML on **Gtk4.Forms** gives
you a single XAML + C# codebase that renders on native **GTK 4** widgets and runs on
**Windows, Linux and macOS** from the same build output.

The project plumbing — the executable, the target framework, the package references and the GTK
entry point — is done for you by a single package, **`Gtk4.Forms.Base`**. The work that remains is
translating the markup: WPF XAML and Forms XAML are different dialects of the same idea, so you keep
your layout intent, data bindings and view-model code, and rewrite the controls. This article is the
step-by-step.

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

## Set up the project — add one package

Add a single `PackageReference` to **`Gtk4.Forms.Base`**:

```xml
<ItemGroup>
  <PackageReference Include="Gtk4.Forms.Base" Version="1.0.0.*" />
</ItemGroup>
```

That package does the whole plumbing. At build time it:

- **Rewrites the `.csproj`** — sets `OutputType=Exe` and a single OS-neutral `TargetFramework=net10.0`,
  drops the `UseWPF` / `net*-windows` / WPF-only bits and any leftover `Xamarin.Forms` package
  references, and pulls in Core, Xaml, Platform.Gtk and the XAML build tasks. Those tasks run
  `XamlGTask`/`XamlCTask` to generate `InitializeComponent` and compile your markup, exactly like
  WPF's XAML build tasks. SDK-style default globbing already includes `.xaml`; no explicit
  `<Compile>` entries are needed.
- **Injects the GTK entry point** — a generated `Main` that initialises GTK and Forms, finds your
  `Application` subclass, hosts it in a `FormsWindow` and runs the GTK main loop. You write no
  bootstrap.

Because an MSBuild target cannot retarget the build already running, the conversion is **one-time**:
the **first** build after adding the package rewrites the project, and the **second** build is the
clean one. The rewrite is idempotent (re-running changes nothing); `SkipGtkConvert` / `SkipGtkHost`
are the opt-outs if you ever want to take the wheel back.

Everything else below is the markup translation — the part no package can do for you.

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
unchanged** — they never referenced WPF types. The work is in the markup.

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

WPF's `App.xaml` with `StartupUri` has no Forms analogue. Forms' `Application` owns a `MainPage`,
and the injected entry point finds this class and runs it — so this is the only startup code you
write.

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
constructor after `InitializeComponent()`. If more than one `Application` subclass exists, mark the
one to launch with `[assembly: Xamarin.Forms.Platform.GTK.GtkMainApplication(typeof(App))]`.

## Step 4 — Bindings, resources and styles

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

## Step 5 — Assets

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

The build is plain `dotnet`, and the **same** output runs on all three OSes.

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

GTK is not thread-safe. **Everything that touches a widget must run on the main GTK thread** — the
thread running `Gtk.Application.Run()` (set up for you by the entry point). From a background thread,
marshal back:

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

1. Add one `PackageReference` to **`Gtk4.Forms.Base`** — it turns the project into a cross-platform
   GTK executable and writes the entry point for you (build twice the first time; the conversion is
   one-time and idempotent).
2. Rewrite each `Window` as a `ContentPage`; swap the default `xmlns` to the Forms namespace and
   translate the controls.
3. `DataContext` → `BindingContext`; drop `Path=`; keep commands, converters and view models.
4. Run the **same** build everywhere — the native GTK 4 runtime ships with the package, so there is
   nothing to install per OS (Linux still needs a display: X11/Wayland or `xvfb-run`).

The reward is a WPF-shaped codebase — XAML, bindings, styles, MVVM — that is no longer tied to
Windows.

---

*Part of the Gtk4.Forms `1.0.0` maintenance fork. See also
[`article-gtk4-forms-gtk.md`](article-gtk4-forms-gtk.md) for the architecture of the backend,
and the `Gtk4.Forms.ControlGallery` project for a complete runnable example.*
