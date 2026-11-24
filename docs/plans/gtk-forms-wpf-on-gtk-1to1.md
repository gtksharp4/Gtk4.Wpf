# Plan — `Gtk4.Forms`: run **unmodified WPF** on the Gtk4.Forms GTK engine (aim 1:1)

**Goal.** An existing WPF application — its `System.Windows.*` code, its WPF XAML (the
`…/winfx/2006/xaml/presentation` namespace), its `App.xaml`/`Window.xaml` — compiles and runs on
**Windows, Linux and macOS** against a GTK-backed compatibility layer, with **no namespace changes
and no XAML edits**. The WinForms analogue (`Gtk.Windows.Forms` in `C:\Temp\GtkWindowsForms`) proves
the pattern for an API that has no XAML; this adds the hard part WPF has and WinForms does not — the
XAML stack, the property system, templates, triggers and the binding engine.

## Decisions (answered — do not re-ask)

| Topic | Decision |
|---|---|
| Fidelity target | **Aim for true 1:1 WPF** (the north star; delivered in phases — see below) |
| XAML → objects | **Build-time markup compiler** (like WPF's `PresentationBuildTasks`), not runtime loading |
| Rendering substrate | **Hybrid** — reuse Gtk4.Forms GTK plumbing + control mappings; implement WPF-faithful layout/templating on GtkSharp 4 where the Forms model cannot express it |
| Location | **This repo** (the Xamarin.Forms→GTK fork), next to the engine it depends on |
| Delivery | A one-package drop-in, **`Gtk4.Forms.Wpf`**, reusing the `Gtk4.Forms.Base` pattern just built |

## Reality check — 1:1 is a program, not a task

WPF is ~3 assemblies and thousands of public types (the full property system, routed events, the
logical+visual trees, `Measure`/`Arrange`, templating, triggers, the binding engine, the control
library, media/geometry). Avalonia is the closest existing "WPF-on-everything" and it **deliberately
is not 1:1** because exact parity is enormous. "Aim for 1:1" here means: **the public surface and
semantics are the target, and the work is sequenced so real apps run early and parity grows by
demand.** Nothing below is a weekend; the plan's job is to make the first vertical slice small,
provable, and architecturally load-bearing so the rest is fill-in, not rework.

## The key enabler — why "no XAML / no namespace changes" is possible

1. **Reimplement WPF types in their real namespaces.** `System.Windows.Window`, `System.Windows.Controls.Grid`,
   `System.Windows.Controls.TextBlock`, `System.Windows.DependencyObject`, … live in a compat assembly
   set. A recompile of the WPF app binds `using System.Windows;` to our assemblies (the project drops
   `<UseWPF>`, so the real WPF framework reference — Windows-only anyway — is gone and cannot clash).
2. **Claim the WPF xmlns.** `[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Controls")]`
   (and for `System.Windows`, `System.Windows.Shapes`, `System.Windows.Data`, …). The markup compiler
   resolves the unchanged presentation-namespace XAML to our types through exactly this attribute —
   the same mechanism `Gtk4.Forms.Xaml` already uses for its own xmlns
   (`Gtk4.Forms.Xaml/Properties/AssemblyInfo.cs:10`).
3. **Compile the XAML, don't translate it.** A build task consumes the `Page`/`ApplicationDefinition`
   build items and emits `InitializeComponent` + field wiring, exactly as WPF's markup compiler does —
   so the `.xaml` and the `x:Class` code-behind are untouched.

## Architecture

### A. Assembly & namespace layering (mirror WPF's three layers)

Own assembly names, **real `System.Windows.*` namespaces**. Mirroring WPF's split keeps the layering
honest and the dependency graph acyclic:

| Our assembly (package `Gtk4.Forms`) | Mirrors | Holds (namespaces) |
|---|---|---|
| `Gtk4.Forms.WindowsBase` | `WindowsBase.dll` | `System.Windows` base (`DependencyObject`, `DependencyProperty`, `PropertyMetadata`, `Freezable`, `DispatcherObject`), `System.Windows.Threading` (`Dispatcher`), `System.Windows.Markup` base, primitives (`Point`/`Rect`/`Size`/`Thickness`/`Duration`) |
| `Gtk4.Forms.PresentationCore` | `PresentationCore.dll` | `System.Windows` (`UIElement`, `FrameworkPropertyMetadata`, `RoutedEvent`, input), `System.Windows.Media` (`Brush`, `Pen`, `Geometry`, `Color`, `Visual`), `System.Windows.Input` |
| `Gtk4.Forms.PresentationFramework` | `PresentationFramework.dll` | `System.Windows` (`FrameworkElement`, `Window`, `Application`, `Style`, `ResourceDictionary`), `System.Windows.Controls`, `System.Windows.Controls.Primitives`, `System.Windows.Data` (`Binding`), `System.Windows.Shapes`, `System.Windows.Documents` |
| `Gtk4.Forms.MarkupCompiler` | `PresentationBuildTasks.dll` | The build task + `Gtk4.Forms.props/.targets` (packed, consumer-imported) |

Carrying our own assembly names avoids an identity clash with the real WPF assemblies if a developer
builds on Windows; the `System.Windows.*` namespaces are what the app actually binds to.

### B. The property & event system (`Gtk4.Forms.WindowsBase`)

The backbone — every WPF semantic rests on it, so it is Phase-0 work and must be faithful:
`DependencyObject` + `DependencyProperty` (registration, attached properties, value precedence,
`PropertyMetadata` with coercion + change callbacks, **property-value inheritance** down the tree),
`RoutedEvent` (tunnel/bubble) and `Dispatcher` (marshal to the GTK main thread — bridge to
`Device.BeginInvokeOnMainThread`/the GTK main loop). This is a from-scratch reimplementation; it does
**not** reuse Gtk4.Forms' `BindableProperty` (different precedence/inheritance rules), though that code
is a useful reference for the mechanics.

### C. The build-time markup compiler (`Gtk4.Forms.MarkupCompiler`) — **fork, don't reinvent**

Fork `Gtk4.Forms.Build.Tasks` and retarget it to the WPF dialect. The whole shape transfers:

| Reuse from `Gtk4.Forms.Build.Tasks` | Role | Retarget to |
|---|---|---|
| `XamlGTask.cs:10` (+ `XamlGenerator.cs`) | Generate `InitializeComponent` + `x:Name`/`FindName` partial | WPF: `x:Class`, `x:Name` fields, `Application`/`Window` roots, `[STAThread] Main` from `ApplicationDefinition` |
| `XamlCTask.cs:17` + the `*Visitor.cs` tree (`CreateObjectVisitor`, `SetPropertiesVisitor`, `ExpandMarkupsVisitor`, `SetNamescopesAndRegisterNamesVisitor`) | Walk the XAML node tree, emit IL (Mono.Cecil) | Resolve `presentation` xmlns via `XmlnsDefinition`; emit `DependencyProperty.SetValue`/setters; WPF markup extensions (`{Binding}`, `{StaticResource}`, `{x:Type}`, `{TemplateBinding}`) |
| `ErrorMessages.resx` / `BuildException` | MSBuild-surfaced XAML errors | same |

It is **System.Xaml-free** (parses XAML via `XmlReader`, emits via Cecil) — important, because
`System.Xaml.dll` ships only in the Windows Desktop pack and is **absent on Linux/macOS**. The task
hooks `Page`/`ApplicationDefinition`/`Resource` build actions so an unmodified WPF `.csproj` layout
compiles.

### D. The rendering bridge (hybrid) — WPF visual tree → GTK

Two tiers, chosen per element:

- **Tier 1 — map onto Gtk4.Forms.** Where a WPF control has a faithful Forms equivalent
  (`TextBlock`→`Label`, `Button`→`Button`, `TextBox`→`Entry`, `Grid`→`Grid`), render it by delegating
  to the existing `Gtk4.Forms.Platform.Gtk` renderer for the mapped Forms element. Reuses `FormsWindow.cs`
  as the top-level `Window` host and `Platform.cs` for renderer creation/layout.
- **Tier 2 — WPF-faithful on GtkSharp 4.** Where the Forms model can't express WPF semantics
  (two-pass `Measure`/`Arrange` with `DesiredSize`, `ControlTemplate`/`DataTemplate`, `Panel`s like
  `DockPanel`/`Canvas` with attached layout props, triggers, visual brushes), implement the element
  directly on a GtkSharp 4 widget with a WPF layout system, hosted in the same GTK window. `UIElement`
  owns a backing `Gtk.Widget`; a custom `Gtk.Widget` measures/arranges children per WPF rules.

The seam is `UIElement.OnRender`/`MeasureCore`/`ArrangeCore` → a GTK widget. Tier 1 is how apps run
early; Tier 2 is where parity is won.

### E. Binding, resources, styles, templates, triggers (`PresentationFramework`)

Reimplement per WPF semantics: the `Binding` engine (`Path`, `Mode`, `Converter`, `RelativeSource`,
`ElementName`, `UpdateSourceTrigger`, `INotifyPropertyChanged`/`DependencyProperty` sources),
`ResourceDictionary` with `{StaticResource}`/`{DynamicResource}` + merged dictionaries, `Style`
(setters/triggers, `BasedOn`, implicit by `TargetType`), `ControlTemplate`/`DataTemplate`,
`Trigger`/`DataTrigger`/`MultiTrigger`/`EventTrigger`. Gtk4.Forms has analogues to study but different
semantics; this is new code.

### F. Drop-in delivery (`Gtk4.Forms.Wpf`)

Reuse the pattern shipped in `Gtk4.Forms.Base` (see `Gtk4.Forms.Base/` + `Gtk4.Forms.ProjectConverter/`):
one `PackageReference` that (1) rewrites the consumer `.csproj` — strip `<UseWPF>`/`net*-windows`,
set `OutputType=Exe`/`net10.0`, reference the `Gtk4.Forms.*` assemblies + markup compiler — and (2)
needs no injected `Main` (WPF's `Main` comes from the compiled `ApplicationDefinition`, which the
markup compiler emits). The converter's `CsprojConverter` gains WPF rules alongside its existing ones.

## Reuse map (concrete anchors in this repo)

| Need | Reuse | Anchor |
|---|---|---|
| Markup compiler (codegen) | `XamlGTask` + `XamlGenerator` | `Gtk4.Forms.Build.Tasks/XamlGTask.cs:10` |
| Markup compiler (IL) | `XamlCTask` + `*Visitor.cs` + Mono.Cecil | `Gtk4.Forms.Build.Tasks/XamlCTask.cs:17` |
| xmlns→CLR resolution | `[assembly: XmlnsDefinition]` | `Gtk4.Forms.Xaml/Properties/AssemblyInfo.cs:10` |
| GTK window host | `FormsWindow` | `Gtk4.Forms.Platform.Gtk/FormsWindow.cs` |
| Renderer creation / layout pass | `Platform` | `Gtk4.Forms.Platform.Gtk/Platform.cs` |
| GTK 4 binding + native runtime | `GtkSharp4` (nuget.org, carries the runtime) | `Directory.Build.props` `$(GtkSharp4Version)` |
| One-package drop-in | `Gtk4.Forms.Base` + `ProjectConverter` | `Gtk4.Forms.Base/`, `Gtk4.Forms.ProjectConverter/CsprojConverter.cs` |

## ⛔ Blocking verifications — Phase 0 vertical slice (build before committing to the program)

Prove the architecture end-to-end on the smallest real thing. Until these pass, do not build out the
control library.

| # | Prove | Pass criterion |
|---|---|---|
| B1 | `[assembly: XmlnsDefinition]` + the forked compiler resolves **unmodified** presentation-namespace XAML to our `System.Windows.Controls.*` types | A `<Window><Grid><TextBlock Text="hi"/></Grid></Window>` (verbatim WPF XAML, WPF `x:Class` code-behind) compiles with our markup task — `InitializeComponent` generated, no XAML edits |
| B2 | `DependencyObject`/`DependencyProperty` carries a bound value with inheritance + change notification | A `{Binding}` on `TextBlock.Text` to an `INotifyPropertyChanged` VM updates the widget |
| B3 | The hybrid bridge paints on GTK via the engine | That window shows a GTK label reading "hi" on Windows **and** headless Linux (Xvfb) |
| B4 | The drop-in works | A stock WPF project + `<PackageReference Include="Gtk4.Forms.Wpf">` builds to a cross-platform exe (two-build conversion) and runs |

Phase 0 = `WindowsBase` property system (subset) + the forked markup compiler (minimal) + `Window`/`Grid`/`TextBlock`/`Button` on Tier 1 + a one-way `Binding` + `Gtk4.Forms.Wpf`. One window, one binding, running on two OSes.

## Progress (living)

- **Phase 0 — DONE.** `Gtk4.Forms.WindowsBase` (property/event system), `Gtk4.Forms.MarkupCompiler`
  (build-time compiler), `Gtk4.Forms.PresentationFramework` (`UIElement`→`Window`/`Grid`/`StackPanel`/
  `TextBlock`/`Button` + one-way binding on GtkSharp 4), `Gtk4.Forms.Wpf.ProjectConverter` + `Gtk4.Forms.Wpf`
  (drop-in), and `Gtk4.Forms.WpfSample` (unmodified WPF, compiles through the compiler). ~104 tests;
  blocking checks B1–B4 pass. Reviewed (no foundational defect); inheritance-on-reparent, leaks and
  compile-compat fixed.
- **Phase 1 Round 1 — DONE** (binding + controls + layout; **203 tests total**, sample still compiles):
  - *Two-way binding*: `TwoWay`/`OneTime`/`OneWayToSource`, `ConvertBack`, `ElementName`, reentrancy
    guard (`FrameworkElement.Binding.cs` + `TargetChangeTracker`); compiler `{Binding Path=X, Mode=…,
    ElementName=…}` syntax.
  - *Controls*: `TextBox`/`CheckBox`/`RadioButton`/`ComboBox`/`Image`/`Border`/`Label` on real GtkSharp 4
    widgets (`Gtk.Entry`/`Gtk.CheckButton`/`Gtk.DropDown`/`Gtk.Image`/`Gtk.Frame`), two-way where WPF is.
  - *Layout*: `DockPanel`/`Canvas`/`WrapPanel` + attached props (`DockPanel.Dock`, `Canvas.Left/Top`);
    `Grid` row/col `Definitions` + `Grid.Row/Column` attached props; `GridLength`/`GridUnitType`.
    GTK-backing approximations noted (Star/Auto sizing, dock geometry) — faithful `Measure`/`Arrange`
    is a later refinement.
- **Phase 2 Kickoff — DONE** (templating + items controls; **232 tests total**, all pass):
  - `FrameworkTemplate` (abstract) + `ControlTemplate` (`TargetType`, `Factory`) + `DataTemplate`
    (`ItemFactory`) in `Gtk4.Forms.PresentationFramework/Templates/`.
  - `ContentPresenter` — hosts `UIElement` directly, applies `DataTemplate`, or falls back to
    string label; backed by `Gtk.Box`.
  - `ItemsControl` — `ItemsSource` (IEnumerable, INCC-reactive), `ItemTemplate`, `Items` snapshot;
    packed in a vertical `Gtk.Box`; extensible via `CreateItemsWidget`/`AddItemWidget`/`RemoveItemWidget`.
  - `ListBox` — single-selection, `SelectedItem`/`SelectedIndex` (bidirectional, pure + GTK),
    `SelectionChanged`; backed by `Gtk.ListBox` + `Gtk.ListBoxRow` containers per item.
  - `ListView` — thin `ListBox` subclass; column/view support is a later phase.
  - `Control.Template.cs` — `Template` DP added to `Control` as a partial file; `OnTemplateChanged`
    builds the visual tree; `ApplyTemplateRoot` is a virtual hook for concrete controls.
  - `SelectionChangedEventArgs` extended via partial to add an `object`-source constructor for
    non-`ComboBox` selection controls.
  - 48 new tests: 34 pure DP + 14 GTK smoke (all native, none skipped on Windows).
- **Phase 1 Round 2 — DONE** (resources + styles, runtime + compiler):
  - `ResourceDictionary` (own keys → `MergedDictionaries` in reverse/last-wins order), lookup walking
    the **logical-parent** chain then `Application.Current.Resources`; `FindResource`/`TryFindResource`.
  - `Style`/`Setter`/`SetterBase`/`BasedOn` (outermost-first, cycle-guarded) + implicit styles keyed by
    `TargetType`; style setters write a dedicated **style-value precedence layer** (below local, above
    inherited) and are **un-applied** when `Style` is replaced or set null.
  - Compiler: `<X.Resources>`/`<ResourceDictionary>` → `Style` construction + `Resources["k"]=…` emitted
    before the element tree; `{StaticResource}`/`{DynamicResource}` on any attribute and on `Style=`
    emit `FindResource` calls. (`{DynamicResource}` resolves at load for now; `Setter.Value` stays a
    string pending typed type-converter resolution — tracked as carry-over.)
- **Round 2 review (Opus) + fixes — DONE.** Independent review found 7 real defects the passing tests
  missed; all fixed with fail-before/pass-after pins: resource walk now follows `LogicalParent`
  (set by `ItemsControl`/`ContentPresenter` on hosted children) not only `InheritanceParent`;
  stored-`null` resources no longer read as absent; `SelectionChangedEventArgs.Source` retyped `object`
  (was a null `ComboBox` cast); `ListBox.SelectionChanged` fires exactly once per transition;
  programmatic `SelectedItem` syncs the GTK row; style precedence below local with un-apply on change;
  `BasedOn` cycle guard + `ReadLocalValue`-based implicit-style guard. **304 tests total** across the
  four suites (35 WindowsBase + 213 PresentationFramework + 40 MarkupCompiler + 16 ProjectConverter),
  all pass; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles through the compiler.
- **Round 3 — triggers + commands/input (DONE).**
  - *Track A — triggers*: `TriggerBase`/`Trigger` (property), `DataTrigger` (binding+INPC), `MultiTrigger`
    (`Condition` set), `EventTrigger` (+`TriggerAction`/`SetPropertyAction`); `Style.Triggers` and
    element-level `FrameworkElement.Triggers`; a **trigger-value precedence layer** above style, below
    local (full order: local > trigger > style > inherited > default), with per-DP contributor stacking
    so sibling triggers on one property don't stomp each other, and un-apply on `Style` change/null.
    Compiler parses `<Style.Triggers>` (all four trigger kinds) → construction + `Triggers.Add`.
  - *Track B — commands/input*: `System.Windows.Input.ICommand` (BCL; no duplicate),
    `RoutedCommand`/`RoutedUICommand`, `CommandBinding`, `CommandManager` (**weak** `RequerySuggested`,
    `InvalidateRequerySuggested`, tree-walking `Execute`/`CanExecute` over `LogicalParent`),
    `ICommandSource`/`CommandSourceHelper`, `UIElement.CommandBindings`, `Button.Command`/`CommandParameter`
    (sensitivity tracks `CanExecute`; deterministic `Dispose` unsubscribes), `MenuItem` (backed by
    `Gtk.Button`), `InputGesture`/`KeyGesture`/`MouseGesture` + `KeyBinding`/`MouseBinding` +
    `UIElement.InputBindings` with an `InvokeInputBinding` dispatch entry point.
- **Round 3 review (Opus) + fixes — DONE.** Review found 2 Critical + High/Medium defects; all fixed with
  pins: multi-trigger same-DP corruption (per-contributor trigger stack, re-resolve on un-apply);
  **strong-ref command leak** (`RequerySuggested` now a weak handler list, pruned on raise) + dead
  `DisposeCommandSource` now wired into `Button`/`MenuItem` `Dispose`; style/trigger layer writes now
  **coerced**; `DataTrigger` restricts to single-segment paths with an explicit contract (throws on nested);
  `SetPropertyAction` writes the trigger layer not a permanent local; command routing walks `LogicalParent`.
  Plus a crash fix found in re-verification: `CommandManager.CanExecute`/`Execute` **infinitely recursed**
  for a `RoutedCommand` with no binding and a null target (unconditional `command.CanExecute` fallback) —
  now a RoutedCommand with no matching binding is simply non-executable (WPF semantics). **368 tests total**
  (40 WindowsBase + 265 PresentationFramework + 47 MarkupCompiler + 16 ProjectConverter), all pass, PF full
  run completes with no stack overflow; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 4 Track B — shapes & media primitives (DONE).** `System.Windows.Media`: `Color` (ARGB bytes +
  scaled floats, `ToGdkRgba`/`ToCairoColor`), `Colors`/`Brushes` (named statics), `Brush` (abstract,
  `Opacity`) + `SolidColorBrush` (`ToGdkRgba` + Cairo `SetSourceRGBA`), `GradientBrush`/`LinearGradientBrush`/
  `RadialGradientBrush` (`GradientStopCollection`; full Cairo `LinearGradient`/`RadialGradient` paint),
  `Pen` (`Brush` + `Thickness`), `Geometry` base + `RectangleGeometry`/`EllipseGeometry`/`LineGeometry`/
  `PathGeometry` (`PathFigure`/`LineSegment`/`BezierSegment`; `Bounds` on all four), `Transform` base +
  `TranslateTransform`/`ScaleTransform`/`RotateTransform`/`MatrixTransform` + `Matrix` (multiply,
  transform-point, `IsIdentity`, `ToCairo`).  `System.Windows.Shapes`: `Shape : FrameworkElement`
  (`Fill`/`Stroke`/`StrokeThickness`/`Stretch` DPs; `Gtk.DrawingArea` backing; `SetDrawFunc` Cairo
  callback; deterministic `Dispose`), `Rectangle` (`RadiusX`/`RadiusY`; rounded-rect Cairo path),
  `Ellipse`, `Line` (`X1/Y1/X2/Y2`), `Polygon`/`Polyline` (`PointCollection`, `FillRule`), `Path`
  (`Data` = `Geometry`).  71 new tests (18 Color/Brushes, 9 Geometry bounds, 15 Transform matrix math,
  20 Shape DP defaults/round-trips, 10 GTK smoke). **439 tests total** (40 WindowsBase + 336
  PresentationFramework + 47 MarkupCompiler + 16 ProjectConverter), all pass.
  *Gradient rendering limitation*: `LinearGradientBrush`/`RadialGradientBrush` use full Cairo gradient
  paint when `DrawFunc` is active; `ToGdkRgba` returns the first stop colour as a widget-hint fallback
  (noted limitation, full GTK CSS gradient is not wired). `Stretch` is stored as a DP but the
  Cairo path always paints to the full `DrawingArea` size (stretch transform is a Phase-5 addition).
- **Round 4 Track A — navigation, windows & dialogs (DONE).** (65 new PF tests):
  - `System.Windows.Controls.Page` (`FrameworkElement` subclass, `Content`/`Title`/`WindowTitle` DPs,
    internal `NavigationService` setter, backed by `Gtk.Box`).
  - `System.Windows.Navigation.NavigationService` (cursor-based `NavigationJournal` —
    pure, back/forward with forward-trim on `Push`), `Navigate(object)`/`Navigate(Uri)`,
    `GoBack`/`GoForward`, `Navigating`/`Navigated`/`NavigationStopped` events, internal
    `ContentChanged` event, pluggable `IContentResolver`/`DictionaryContentResolver` for URI lookup.
  - `System.Windows.Controls.Frame` (`ContentControl` subclass, `Source` DP, owns `NavigationService`,
    swaps `Gtk.Box` child on `ContentChanged`).
  - `System.Windows.Navigation.NavigationWindow` (`Window` subclass, owns `NavigationService`,
    wires `ContentChanged` to set `Content` and push `Page.WindowTitle` to `Title`,
    exposes `CanGoBack/CanGoForward`/`GoBack`/`GoForward`/`Navigate` pass-throughs).
  - `System.Windows.MessageBox` (static `Show(…)` overloads, injectable `IMessageBoxPresenter`,
    `GtkMessageBoxPresenter` using deprecated-but-available `Gtk.MessageDialog` with `GLib.MainLoop`
    synchrony, `MessageBoxButtonMapping` pure static — `ButtonLabels`/`ResultFromLabel`/`DefaultResult`).
  - `Window.Owner`/`Window.DialogResult`/`Window.ShowDialog()`/`Window.Close(bool?)` — modal semantics
    via nested `GLib.MainLoop` (GTK 4 equivalent of `gtk_dialog_run`).
  - **Root cause of the long investigation**: `Gtk.Window` exists in GtkSharp4; inside namespace
    `Gtk4.Forms.PresentationFramework.Tests` Roslyn resolves bare `Window` to `Gtk.Window` (namespace
    chain lookup beats using-directive lookup). Fix: `using WpfWindow = System.Windows.Window;` in
    affected test files, matching the pattern already used in `GtkSmokeTests.cs`.
  - New tests: 10 `NavigationJournalTests` + 13 `NavigationServiceTests` + 7 `NavigationWindowTests`
    + 7 `FrameGtkSmokeTests` + 22 `MessageBoxTests` (button mapping) + 6 `WindowDialogTests`.
  - *Fix applied during integration*: `Window.ShowDialog()` originally ran a **blocking** nested
    `GLib.MainLoop` and `WindowDialogTests` quit it from a `Task.Run` thread — calling GTK off the single
    GTK thread **segfaulted the test host** (and would deadlock the harness regardless). `ShowDialog` now
    **pumps** `Gtk.Application.RunIteration(true)` on the GTK thread until `Close(bool?)` sets a flag, and
    the tests schedule `Close` via a same-thread `GLib.Timeout`. GTK must only be touched on its own thread.
- **Round 4 review (Opus) + fixes — DONE.** Review found 2 Critical latent host-hangs + High/Medium defects;
  all fixed with pins: `Window.Close()` (no-arg) and a window-manager/`CloseRequest` close now set the
  closed flag so `ShowDialog` can't spin forever; the default `GtkMessageBoxPresenter` switched from a
  blocking nested `GLib.MainLoop` to the same-thread `RunIteration` pump; **MessageBox button→result
  mapping made total** (distinct per-button response id → label → `ResultFromLabel`; previously every click
  returned the default); rounded `Rectangle`/`RectangleGeometry` corners use `rx` horizontally and `ry`
  vertically (were both `ry`); `GoBack`/`GoForward` now raise the `Navigating`/`NavigationStopped` pipeline
  with `NavigationMode` and honour `Cancel`; `Color.Sc*` documented as normalized bytes (not linear-light
  scRGB). **Round 4 totals: 556 tests** (40 WindowsBase + 453 PresentationFramework + 47 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) still
  compiles.
- **Round 5 Track B — common WPF controls (DONE).** `HeaderedContentControl` (base for `Expander`/`GroupBox`/`TabItem`);
  `ScrollViewer` (`Gtk.ScrolledWindow`, `HorizontalScrollBarVisibility`/`VerticalScrollBarVisibility` → `PolicyType`);
  `TabControl`/`TabItem` (`Gtk.Notebook`; `SelectedIndex`/`SelectedItem`/`SelectionChanged`, two-way with reentrancy guard);
  `Expander` (`Gtk.Expander`; `IsExpanded` two-way via `AddNotification("expanded",…)`, `Expanded`/`Collapsed` events);
  `GroupBox` (`Gtk.Frame` with label-widget header); `Slider` (`Gtk.Scale`; `Minimum`/`Maximum`/`Value` clamped via
  `CoerceValueCallback` lambda, `Orientation`, `ValueChanged`, two-way); `ProgressBar` (`Gtk.ProgressBar`;
  `Minimum`/`Maximum`/`Value` as fraction, `IsIndeterminate` → `Pulse`); `Separator` (`Gtk.Separator`, `Orientation`);
  `ToggleButton` (`Gtk.ToggleButton`; `IsChecked` two-way, `Checked`/`Unchecked`; derives from `ContentControl`
  — no `ButtonBase` in this phase). **66 new tests** (40 pure DP + 26 GTK smoke). **Round 5 totals: 519
  PresentationFramework** (622 total across all suites); all pass, no hang/crash on Windows.
  *GTK note*: `Gtk.ScrolledWindow.Child` wraps the content in a `Gtk.Viewport` automatically — tests assert
  `viewport.Child` not `sw.Child` directly.
- **Round 5 Track A — typed attribute values + type-converter system (DONE).**
  - `System.Windows.Markup.XamlValueConverter` (central runtime registry in `PresentationFramework`):
    `Convert(ownerType, dp, rawString)` dispatches on `dp.PropertyType`; `Register<T>` for custom converters.
  - Built-in converters: `Thickness` (1/2/4-part), `CornerRadius` (1/4-part), `GridLength`
    (`Auto`/`*`/`2*`/px), `Color` (named via `Colors` statics + `#RGB`/`#ARGB`/`#RRGGBB`/`#AARRGGBB`),
    `Brush`/`SolidColorBrush` (named + hex), `Point`/`Size`/`Rect` (comma-separated pairs/quads),
    `double`/`float`/`int`/`long`/`bool`, arbitrary `Enum` (case-insensitive), `string` passthrough.
    `PrimitiveConverters` (internal, WindowsBase) + `MediaConverters` (internal, PresentationFramework).
  - `CornerRadius` struct added to `Gtk4.Forms.WindowsBase`.
  - `Visibility` enum added to `Gtk4.Forms.PresentationFramework`.
  - `Setter.CoerceValue`: string values converted to target DP type at apply time (style, trigger, direct).
  - Compiler (`XamlCodeGenerator`): plain DP attributes emit
    `SetValue(T.PropProperty, XamlValueConverter.Convert(typeof(T), T.PropProperty, "raw"))` instead of
    bare string literals; CLR-only properties (`Title`, `StartupUri`, `Name`, `Text`) keep the existing
    direct-assignment path. `{Binding}`, `{StaticResource}` still take their own paths (unchanged).
  - Compiler emits `XamlValueConverter.Convert(...)` for plain DP attributes; CLR-only props keep direct
    assignment; `{Binding}`/`{StaticResource}` unchanged. (+29 PF, +8 MarkupCompiler tests.)
- **Round 5 defect fixes (Opus review G1–G4, DONE).**
  - **G1** (`Slider.ValueChanged` silent on GTK drag): `OnValueChanged` now raises `ValueChanged`
    unconditionally; `_updating` guards only the GTK↔DP write loop, not the event.
  - **G2** (`TabControl.SelectionChanged` fires twice): all three entry points (`SelectedIndex`,
    `SelectedItem`, GTK page switch) now raise `SelectionChanged` exactly once via a single-choke-point
    `_updating` guard; `OnSelectedIndexChanged` promoted to full coordinator.
  - **G3** (attached-property attributes emit invalid C#): `EmitChildren` now detects `Owner.Prop`
    dotted attr names and emits `OwnerType.SetProp(el, (castType)XamlValueConverter.Convert(...))`;
    runtime form is the static setter (`Grid.SetRow`, `Canvas.SetLeft`, `DockPanel.SetDock`).
    `InternalsVisibleTo` added to `Gtk4.Forms.MarkupCompiler.csproj`.
  - **G4** (`EscapeString` omits `\n`/`\r`/`\t`): all three added. `EscapeString` made `internal`
    for direct unit testing (XML parsers normalize whitespace in attribute values before the generator
    sees them, making XAML-round-trip tests impossible).
  - **+18 PF tests** (3 Slider, 10 TabControl, 5 GTK smoke); **+8 MarkupCompiler tests**
    (12 attached-prop / escape). **Round 5 totals: 682 tests** (40 WindowsBase + 558 PresentationFramework
    + 68 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash;
    `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 6 Track B — hierarchical & popup controls (DONE).** Runtime only; no compiler changes.
  - `HeaderedItemsControl : ItemsControl` base class (`Header` DP; base for `ToolBar`/`TreeViewItem`).
  - `MenuItem` extended: `Items` (sub-menu list; parent items backed by `Gtk.MenuButton`+`Gtk.Popover`,
    leaf items by `Gtk.Button`), `IsCheckable`/`IsChecked` (toggle, `Checked` event), `InputGestureText`.
  - `Menu : ItemsControl` — horizontal `Gtk.Box` of top-level `MenuItem`s.
  - `ContextMenu : ItemsControl` — `Gtk.Popover`+vertical `Gtk.Box`; `IsOpen` (two-way, widget-creation-
    gated), `Placement`; `FrameworkElement.ContextMenu` attached/instance DP.
  - `ToolTip`/`ToolTipService` — `FrameworkElement.ToolTipProperty` attached DP (deferred to widget
    creation via `UIElement.OnWidgetCreated` hook); `ToolTipService` static accessor. `UIElement` gains
    `IsWidgetCreated` and `OnWidgetCreated(Gtk.Widget)` virtual hook.
  - `ToolBar : HeaderedItemsControl` — horizontal `Gtk.Box`.
  - `TreeViewItem : HeaderedItemsControl` — `Gtk.Expander`+vertical `Gtk.Box` children; `IsExpanded`
    two-way (`AddNotification("expanded",…)`), `IsSelected` (DP, `Selected`/`Unselected` events).
  - `TreeView : ItemsControl` — vertical `Gtk.Box` of `TreeViewItem`s; `SelectedItem` (two-way,
    deselects previous), `SelectedItemChanged`; wires `TreeViewItem.Selected` to sync.
  - `Popup (System.Windows.Controls.Primitives)` — `Gtk.Popover`; `IsOpen` (two-way, widget-creation-
    gated), `Child`, `PlacementTarget`, `StaysOpen` (→ `Autohide`).
  - **Existing `IsWidgetCreated` in `ContentControl`/`ContentPresenter`/`Panel`** shadowed with `new`
    to suppress CS0108 (pre-existing pattern; equivalent to base).
  - **GTK-widget choices + limitations**: `Menu` uses `Gtk.Box` not `Gtk.PopoverMenuBar` (requires
    `Gio.Menu` model — deferred); `ContextMenu` uses plain `Gtk.Popover` not `Gtk.PopoverMenu`
    (keyboard nav/accelerators deferred); `TreeView` uses nested `Gtk.Expander`/`Gtk.Box` not
    `Gtk.TreeView`/`Gtk.ListView`+`Gtk.TreeExpander` (requires `Gio.ListModel` tree model — deferred).
    `ToolBar` uses `Gtk.Box` not `Gtk.ActionBar` (start/center/end sections deferred).
  - **67 new tests** (44 pure DP + 23 GTK smoke).
- **Round 6 Track A — `<ControlTemplate>`/`<DataTemplate>` XAML compiler support (DONE).**
  - `WpfXmlnsMap` gains `ControlTemplate`, `DataTemplate`, `ContentPresenter`, `ItemsControl`, `ListBox`, `ListView`.
  - `XamlCodeGenerator` refactored: `EmitChildren` split into `EmitElement` (reusable per-element construction)
    + `EmitPropertyElement` (dispatches dotted property-element children); property-element `<Button.Template>`,
    `<ListBox.ItemTemplate>`, `<ContentControl.ContentTemplate>` etc. emit templates inline.
  - `EmitResources` handles keyed `<ControlTemplate x:Key="k">` and `<DataTemplate x:Key="d">` entries.
  - `EmitControlTemplateInline`: constructs `ControlTemplate(typeof(T))`, sets `Factory = () => { … return root; }`
    using the same `EmitElement` codegen as the page body.
  - `EmitDataTemplateInline`: constructs `DataTemplate()`, sets `ItemFactory = __item => { … DataContext = __item; return root; }`.
  - `Template="{StaticResource k}"` resolves via the existing `{StaticResource}` → `SetValue`/`FindResource` path; confirmed correct for template objects.
  - `ContentControl` made `partial`; `ContentControl.ContentTemplate.cs` adds `ContentTemplateProperty` DP +
    `ApplyContentTemplate` to swap the hosted widget when the template changes.
  - `DependencyProperty.FindByName` now calls `RuntimeHelpers.RunClassConstructor` for each type in the chain
    before searching (fixes `beforefieldinit` types whose static DP registrations weren't guaranteed to run before
    `FindByName` was called — caused `Setter_StringValue_UniformThickness_Applied` to fail silently).
  - **+19 MarkupCompiler tests** (`XamlCodeGeneratorTemplateTests`): inline `<Button.Template>` / keyed
    `<ControlTemplate x:Key>` / `<DataTemplate>` as `ItemTemplate` / `{Binding}` inside template / `{StaticResource}` on `Template`.
  - **+11 PF pure tests** (`TemplatingXamlDpTests`): `ContentControl.ContentTemplateProperty`, `ControlTemplate` factory, `DataTemplate` factory with `DataContext` wiring, keyed resource resolution.
- **Round 6 review (Opus) + fixes — DONE.** Review found 1 Critical (masked by Contains-only tests) + fixes:
  keyed `<ControlTemplate>`/`<DataTemplate>` in a `.Resources` block was processed twice, emitting an
  uncompilable `this.Resources = …` and dropping sibling templates — `EmitPropertyElement` now returns for
  the `Resources` property-element (owned solely by `EmitResources`); pinned by tests asserting both keyed
  entries present and **no** `Resources =` assignment. Also: `MenuItem` now raises `Checked` only on
  false→true and `Unchecked` on true→false (added `Unchecked`); `MenuItem` split into `MenuItem.cs` +
  `MenuItem.Commanding.cs` (≤200 lines); `FrameworkTemplate.HasContent` and `UIElement.IsWidgetCreated`
  converted from `new`-shadows to `virtual`/`override` so a base-typed reference sees the correct value.
  **Round 6 totals: 787 tests** (40 WindowsBase + 642 PresentationFramework + 89 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 7 Track A — advanced data binding (DONE).**
  - `RelativeSource` (`RelativeSourceMode.Self`, `FindAncestor`+`AncestorType`+`AncestorLevel`,
    `TemplatedParent` declared with limitation, `PreviousData` stub); `Binding.RelativeSource`; engine
    resolves via `LogicalParent`/`InheritanceParent` walk.
  - `MultiBinding` (`Bindings`, `IMultiValueConverter` `Convert`/`ConvertBack`, `Mode`, `StringFormat`);
    `MultiBindingEngine` subscribes to per-child INPC, calls converter, re-pushes on any child change;
    `MultiBindingExpression` live object returned by `GetBindingExpression`.
  - `BindingExpression` live object: `UpdateTarget()`/`UpdateSource()`, `ResolvedSource`, `DataItem`;
    `FrameworkElement.SetBinding(dp, Binding)` returns it; `GetBindingExpression(dp)` returns it.
    Engine refactored: each binding is now backed by a `BindingRecord` + `BindingExpression` pair;
    expression map kept in a `ConditionalWeakTable` per element.
  - `StringFormat` on `Binding`/`MultiBinding` (format with `string.Format(InvariantCulture, fmt, v)`;
    WPF `{0}`-less shortcut handled); `FallbackValue` (applied when source unresolvable);
    `TargetNullValue` (applied when resolved+converted value is null).
  - `UpdateSourceTrigger` (`PropertyChanged` auto write-back; `Explicit` defers until
    `BindingExpression.UpdateSource()` is called; `LostFocus`/`Default` fall back to auto).
  - `BindingBase` updated with `StringFormat`, `FallbackValue`, `TargetNullValue` (inherited by
    both `Binding` and `MultiBinding`).
  - Compiler (`XamlCodeGenerator.Binding.cs` partial): `{Binding RelativeSource={RelativeSource Self}}` →
    `RelativeSource.Self` singleton; `{RelativeSource FindAncestor, AncestorType=X}` → constructor call;
    `{RelativeSource TemplatedParent}` → singleton; `Converter={StaticResource k}` → real
    `(IValueConverter)FindResource("k")` (carry-over TODO removed); `StringFormat=`, `FallbackValue=`,
    `TargetNullValue=`, `UpdateSourceTrigger=` emitted in initializer; `<MultiBinding>` property element →
    `MultiBinding` construction + `Bindings.Add` per child + `SetBinding(dp, mb)`. Fixed `TrimEnd('}')`
    bug (was stripping nested-brace values like `StringFormat=Total: {0}`; now removes exactly the outer
    closing brace). `XamlCodeGenerator` split into `partial` with `XamlCodeGenerator.Binding.cs`
    (binding/resource helpers) to stay ≤200 lines per file.
  - **+16 PF tests** (`AdvancedBindingTests`); **+12 MarkupCompiler tests**
    (`XamlCodeGeneratorBindingR7Tests` + updated `Converter_StaticResource` test in Phase1Tests).
  - **+16 PF + 12 MarkupCompiler tests.**
- **Round 7 Track B — remaining common controls (DONE).** Runtime only; no compiler changes.
  `PasswordBox` (`Gtk.PasswordEntry`; `Password` CLR prop + `PasswordChanged`), `RepeatButton`
  (`Gtk.Button` + timer, repeats `Click`), `Thumb` (`Gtk.GestureDrag`; `DragStarted`/`DragDelta`/
  `DragCompleted`), `GridSplitter` (`Gtk.GestureDrag`), `Viewbox` (`Gtk.AspectFrame`; `Child`/`Stretch`),
  `UniformGrid : Panel` (`Gtk.Grid`; `Rows`/`Columns` auto-computed via `ComputeGrid`), `Calendar`
  (`Gtk.Calendar`; `SelectedDate` two-way, `SelectedDatesChanged`), `DatePicker` (`Gtk.Entry`+`Gtk.MenuButton`
  →`Gtk.Calendar` popover; `SelectedDate`/`Text`), `StatusBar : ItemsControl` (`Gtk.Box`), and a
  **functional-minimal `DataGrid`** (`Gtk.ScrolledWindow`+`Gtk.Grid` of labels; `ItemsSource`, explicit
  `Columns`/`DataGridTextColumn`, `AutoGenerateColumns` by reflection, `SelectedItem` — no editing/sorting/
  virtualization yet). **+65 tests** (42 pure DP + 23 GTK smoke).
- **Round 7 review (Opus) + fixes — DONE.** Review found 1 Critical + 3 High + Mediums, all fixed with
  pins (the defects lived in paths the tests missed — binding-kind swap, DataContext-after-binding,
  RepeatButton press, non-string FallbackValue): **R1** `RepeatButton`'s threadpool `Timer` now disposed on
  teardown + before re-press (was a latent native crash surviving widget teardown); **R2** `SetBinding`
  clears ANY prior binding (simple or multi) on the DP before installing (was leaking the old INPC handler,
  fighting the new binding); **R3** `MultiBinding` re-subscribes on a `DataContext` set after the binding;
  **R4** `FallbackValue`/`TargetNullValue` string values now converted to the target DP type via
  `XamlValueConverter` (were breaking non-string DPs); **R5** Fallback applies on unresolvable path (even
  non-null source), converter runs before `TargetNullValue`; **R6** `MultiBinding Mode=TwoWay` throws
  `NotSupportedException` (was silently one-way); **R7** `UniformGrid` only detaches attached children +
  clamps overflow; **R8** `DataGrid` observes `INotifyCollectionChanged` (split into `DataGrid.cs` +
  `DataGrid.Columns.cs`, ≤200 lines); **R9** MultiBinding child `<Binding>` emits `Converter`/`StringFormat`,
  `Binding.DoNothing` sentinel added. **Round 7 totals: 901 tests** (40 WindowsBase + 741 PresentationFramework
  + 104 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 8 Track B — animation & storyboards runtime (DONE).** `System.Windows.Media.Animation`:
  - `Duration` (finite/Forever/Automatic), `RepeatBehavior` (count/duration/Forever), `FillBehavior` (HoldEnd/Stop).
  - `Timeline` (abstract; `Duration`/`BeginTime`/`AutoReverse`/`RepeatBehavior`/`FillBehavior`) +
    `AnimationTimeline` (abstract; `GetCurrentValue(origin, dest, clock)`).
  - **Animation value layer** added to `DependencyObject` (D10) above local in
    `ResolveEffectiveValue`: `SetAnimationValue(dp,v)` / `ClearAnimationValue(dp)` in
    `Gtk4.Forms.WindowsBase/DependencyObject.Animation.cs` (new partial file).
  - `AnimationClock` — per-timeline-per-target clock; `Tick(TimeSpan)` test seam (no GLib loop required);
    handles `BeginTime`, finite/Forever repeat, `AutoReverse`, `FillBehavior.HoldEnd`/`Stop`, `Pause`/`Resume`/`Stop`.
  - `AnimationScheduler` — single `GLib.Timeout` at ~16ms/60fps; registers/unregisters clocks;
    advances active clocks on the GTK thread and removes completed ones.
  - `IEasingFunction` interface + `EasingFunction` property on `DoubleAnimation`.
  - Concrete animations: `DoubleAnimation` (`From`/`To`/`By`/`EasingFunction`), `ColorAnimation`
    (per-channel ARGB lerp), `ThicknessAnimation` (per-side lerp).
  - `Storyboard : Timeline` — `Children` (`TimelineCollection`); per-child `SetTargetName`/`SetTargetProperty`
    (instance dictionary, reference-equality keyed); `Begin(FrameworkElement)` creates+registers clocks,
    `Stop`/`Pause`/`Resume`; `internal ActiveClocks` for test inspection.
  - `BeginStoryboard : TriggerAction` — `Storyboard` property; `Invoke` calls `Storyboard.Begin(target)`;
    `EventTrigger` already invokes any `TriggerAction` — no changes to `EventTrigger.cs`.
  - **13 new pure tests** (`AnimationTests`): at t=0.5s→5, at t=1s→10/Filling, HoldEnd holds, Stop reverts,
    animation beats local, local returns after Stop, AutoReverse@1.5s, AutoReverse HoldEnd at From, Forever Active,
    ColorAnimation channels, BeginStoryboard fires on EventTrigger, null Storyboard noop.
  - **+13 pure animation tests**; limitation: `Storyboard`-in-XAML (`<Storyboard>` property element,
    storyboard keyed resource) is not compiler-wired yet — runtime (code-behind) use works now.
- **Round 8 Track A — TemplatedParent + TemplateBinding + VisualStateManager (DONE).**
  - `FrameworkElement.TemplatedParent` (internal setter): set on the visual-tree root (and subtree,
    via `UIElement.VisualChildren` walk through `Panel.Children`, `ContentControl.Content`,
    `Border.Child`) when `Control.OnTemplateChanged` applies a `ControlTemplate`. Full subtree
    guaranteed for `Panel`/`ContentControl`/`Border`; documented limitation for custom hosts.
  - `RelativeSource.Resolve` for `TemplatedParent` mode now returns `target.TemplatedParent`
    (was null). `{Binding Path, RelativeSource={RelativeSource TemplatedParent}}` resolves correctly.
  - `TemplateBindingExtension` (runtime): `new TemplateBindingExtension("Prop").ToBinding()` →
    `new Binding("Prop") { RelativeSource = RelativeSource.TemplatedParent, Mode = OneWay }`.
  - Compiler: `IsTemplateBindingExpression` parses `{TemplateBinding Prop}` inside any element
    (including `EmitControlTemplateInline` factory body) and emits the equivalent `Binding` with
    `RelativeSource.TemplatedParent` + `BindingMode.OneWay`; checked before `{Binding}` in both
    `EmitElement` and `EmitRootAttributes`.
  - `VisualStateManager` (functional-minimal): `GoToState(control, stateName, useTransitions)` —
    finds the state in attached groups, raises `VisualStateGroup.CurrentStateChanging`/`CurrentStateChanged`,
    applies `VisualState.Setters` via `Setter.Apply`, records `CurrentState`. Storyboard application
    is a seam (stored in `VisualState.Storyboard`, applied by a future animation track).
    `VisualStateGroup`/`VisualState`/`VisualStateChangedEventArgs` in `System.Windows` namespace.
    `GetVisualStateGroups`/`SetVisualStateGroups` attached-property pattern (dictionary, not DP).
  - **+20 PF tests** (`TemplatedParentTests` — 8 tests; `VisualStateManagerTests` — 12 tests);
    **+13 MarkupCompiler tests** (`XamlCodeGeneratorTemplateBindingTests`).
- **Round 8 review (Opus) + fixes — DONE.** Review found 1 Critical + 2 High + Mediums, all fixed with
  pins driving the real paths the tests bypassed: **T1** `{TemplateBinding}` now resolves on the real
  compiler path — the factory activated template bindings against a still-null `TemplatedParent`, so
  `Control.OnTemplateChanged` now calls `BindingEngine.RefreshTemplatedParentBindings` after stamping the
  subtree (the whole feature was silently inert before); **T2** `VisualStateManager`'s state registry is a
  `ConditionalWeakTable` (was a leaking static `Dictionary<Control,…>`); **T3** `AnimationScheduler` guards
  that `GLib.Timeout` registration runs on the GTK thread (off-thread `Storyboard.Begin` now throws instead
  of corrupting GLib); **T4** `VisualState` setters revert on state exit; **T5** `CurrentStateChanged.OldState`
  reports the previous state; **T6** `Duration.Automatic` resolves to 1 s (a `DoubleAnimation` with no explicit
  Duration now animates); **T7** `{TemplateBinding Prop, …}` uses only the property path + stale doc fixed.
  **Round 8 totals: 961 tests** (40 WindowsBase + 786 PresentationFramework + 119 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 9 Track B — data validation + CollectionView runtime (DONE).** No compiler changes.
  - **Validation**:
    - `ValidationRule` (abstract; `Validate(value, culture) → ValidationResult`), `ValidationResult`
      (`IsValid`, `ErrorContent`), `ValidationError` (`RuleInError`, `ErrorContent`, `Exception`).
    - `Validation` static: read-only `Errors`/`HasError` attached DPs, stub `ErrorTemplate` attached DP,
      `Error` attached routed event (Bubble); `SetErrors`/`ClearErrors` internal helpers.
    - `ExceptionValidationRule` and `DataErrorValidationRule` singletons.
    - `Binding` gains `ValidationRules`, `ValidatesOnDataErrors`, `ValidatesOnExceptions`, `NotifyOnValidationError`.
    - Binding engine: pre-write `ValidationRules` gate; post-write `IDataErrorInfo`/`INotifyDataErrorInfo`
      check (`ValidatesOnDataErrors`); `TargetInvocationException` unwrap for `ValidatesOnExceptions`.
      BCL `IDataErrorInfo`/`INotifyDataErrorInfo` used (net10.0 ships them; no redefinition needed).
  - **CollectionView**:
    - `ICollectionView` interface (`SourceCollection`, `Filter`, `SortDescriptions`, `GroupDescriptions`,
      `Groups`, `CurrentItem`/`CurrentPosition`, `MoveCurrentTo*`, `Refresh`, `CurrentChanged`, `CollectionChanged`).
    - `ListCollectionView` over `IEnumerable`/INCC: applies filter → sort → group; `BuildGroups` (single-level);
      `Refresh` raises `CollectionChanged(Reset)`.
    - `CollectionViewSource` (`Source`/`View` instance; `GetDefaultView(source)` stable cache via
      `ConditionalWeakTable`; returns existing `ICollectionView` unchanged).
    - `SortDescription` struct (BCL has `ListSortDirection` in `System.ComponentModel.TypeConverter`; only
      `SortDescription` is defined here — `ListSortDirection` reused from BCL).
    - `GroupDescription` abstract + `PropertyGroupDescription` (single-property grouping).
    - `CollectionViewGroup` (`Name`, `Items`, `ItemCount`).
    - `ItemsControl` wired: `OnItemsSourceChanged` now calls `CollectionViewSource.GetDefaultView` and
      subscribes to the view's `CollectionChanged`; `RebuildItems` iterates `EffectiveItems` (view or raw source).
  - **18 new tests** (6 validation + 12 CollectionView; all pure, all pass):
    - `ValidationTests`: rule rejects/clears, `IDataErrorInfo` surfaced, `INotifyDataErrorInfo` surfaced,
      `ExceptionValidationRule`, `NotifyOnValidationError` routed event.
    - `CollectionViewTests`: filter hides items, sort ascending/descending, `PropertyGroupDescription` groups,
      `MoveCurrentToNext`/`CurrentItem`, `CollectionViewSource.GetDefaultView` stable cache, `ICollectionView`
      passthrough, `ItemsControl` items reflect filtered+sorted view order, plain collection defaults.
  - **18 new tests** (6 validation + 12 CollectionView).
- **Round 9 Track A — Storyboards in XAML + VSM storyboard-states (DONE).**
  - **Converters**: `Duration` (`"0:0:1"` / `"Forever"` / `"Automatic"`) and `RepeatBehavior`
    (`"Forever"` / `"3x"` / `"0:0:5"`) added to `XamlValueConverter` via `AnimationConverters`
    (new file `Markup/XamlValueConverter.Animation.cs`). `XamlValueConverter.ConvertByType(Type, string)`
    overload added for type-only conversion (no DP required — used by compiler-emitted animation attributes).
    `XamlValueConverter.Convert(ownerType, null, targetType, raw)` overload delegates to `ConvertByType`.
  - **`Storyboard.SetTargetPropertyName`**: new method stores a string property name for lazy DP
    resolution at `Begin()` time (compiler-generated path; owner type unknown at code-gen time).
    `Begin()` falls back to `DependencyProperty.FindByName(propertyName, target.GetType())` when no
    pre-set DP is found.
  - **Compiler — keyed `<Storyboard>` resource**: `EmitResources` now handles `<Storyboard x:Key="k">` items,
    calling the new `EmitStoryboardResource` → `EmitStoryboardInline` routines in
    `XamlCodeGenerator.Storyboard.cs`. Emits: `Storyboard` construction, Timeline properties
    (`Duration` via `XamlValueConverter.Convert`, `RepeatBehavior`, `AutoReverse`, `BeginTime`,
    `FillBehavior`), child `DoubleAnimation`/`ColorAnimation`/`ThicknessAnimation` construction with
    `From`/`To`/`By` CLR properties, `Storyboard.TargetName` → `SetTargetName(anim, name)`,
    `Storyboard.TargetProperty` → `SetTargetPropertyName(anim, propName)`, `Children.Add(anim)`,
    `Resources["k"] = sb`.
  - **Compiler — `<EventTrigger><BeginStoryboard><Storyboard>` in `<Style.Triggers>`**: `EmitEventTrigger`
    now delegates to `EmitEventTriggerFull` (new method in `XamlCodeGenerator.Storyboard.cs`), which
    emits `BeginStoryboard` construction, inline `Storyboard` via `EmitStoryboardInline`, wires
    `.Storyboard = sbVar`, and calls `.Actions.Add(bsVar)`. `EmitStyleTriggers` passes `xNs`
    through to `EmitEventTrigger`.
  - **VSM storyboard wiring**: `VisualState.Storyboard` typed from `object?` to real
    `System.Windows.Media.Animation.Storyboard?`. `VisualStateManager.GoToState` now: stops the
    previous state's storyboard (if any) before transitioning, starts the new state's storyboard
    (if any and `useTransitions=true`) after applying setters. No storyboard is started when
    `useTransitions=false` (WPF semantics — instant snap without animation).
  - **`WpfXmlnsMap`** gains `Storyboard`, `DoubleAnimation`, `ColorAnimation`, `ThicknessAnimation`,
    `BeginStoryboard`.
  - **New tests**: 11 `XamlCodeGeneratorStoryboardTests` (compiler; keyed resource + EventTrigger path);
    15 `StoryboardAnimationTests` (PF; Duration/RepeatBehavior converters + VSM storyboard wiring).
  - **Carry-over (VSM compiler)**: `<VisualStateManager.VisualStateGroups>` / `<VisualStateGroup>` /
    `<VisualState><Storyboard>` XAML compilation is not wired — VSM groups must be built in code-behind.
    Runtime (programmatic) storyboard-based states work. Deferred to a future round.
- **Round 9 totals (both tracks merged): 1008 tests** (40 WindowsBase + 820 PresentationFramework +
  132 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 9 review (Opus) + fixes — DONE.** 2 High + Mediums (regressions the CollectionView wiring and
  string-target storyboards introduced), all fixed with pins on the real runtime path: **U1** `ListBox`/
  `TabControl` **preserve selection** across INCC collection changes (Add/Remove/filter/sort no longer silently
  clear the selection; only a genuinely-removed selected item clears); **U2** a real `UIElement.Opacity` DP is
  registered (→ `Gtk.Widget.Opacity`) so string-`TargetProperty` storyboards actually animate (was a silent
  no-op — `FindByName("Opacity")` returned null); `Storyboard.Begin` now **throws** a clear diagnostic on an
  unresolvable target property; **U3** parenthesized/dotted `TargetProperty` (`(UIElement.Opacity)`) normalized;
  **U4** a plain two-way binding does **zero** validation work (was allocating + writing `Validation.Errors`/
  `HasError` every write-back); **U5** `ListCollectionView` keeps null items (WPF parity); **U6**
  `CollectionViewSource` instance `SortDescriptions`/`GroupDescriptions` forwarded into its `View`; **U7** nits.
  **Round 9 totals: 1024 tests** (40 WindowsBase + 836 PresentationFramework + 132 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 10 Track B — Dispatcher/DispatcherTimer + TextBlock inlines (DONE).** Runtime only; no compiler changes.
  - **`Dispatcher`** extended (`System.Windows.Threading`): `CurrentDispatcher` per-thread singleton, `Invoke(Action)`/`Invoke<T>(Func<T>)` (sync, marshals via `InvokeAction` delegate), `BeginInvoke(Action, DispatcherPriority)` (async via `BeginInvokeAction`), `CheckAccess()`/`VerifyAccess()` (via `IsOnDispatcherThread` predicate), `Thread` property. `DispatcherObject` base in `Threading/DispatcherObject.cs`. `DispatcherPriority` enum (Invalid→Send, 12 values). All bridge delegates replaced with real GLib calls in `Application.Run()`.
  - **`DispatcherTimer`**: `Interval`, `Tick`, `IsEnabled`/`Start`/`Stop`, `Priority`, `Tag`. Backed by pluggable `ScheduleAction`/`RemoveSourceAction` delegates (wired to `GLib.Timeout.Add`/`GLib.Source.Remove` in `Application.Run`). **Test seam**: `FireActionOverride` (internal static) + `FireForTest()` (internal instance) allow firing without a GLib loop. Stop calls `RemoveSourceAction` before clearing `_isEnabled` so no post-stop fire occurs.
  - **Inline hierarchy** (`System.Windows.Documents`): `TextElement` (base; `FontFamily`/`FontWeight`/`FontStyle`/`Foreground`), `Inline` (abstract; `AppendMarkup`), `Run` (`Text`), `Span` (child `InlineCollection`), `Bold`, `Italic`, `Underline`, `LineBreak`, `Hyperlink` (`NavigateUri`/`Click`/`RequestNavigate`). `InlineCollection` (typed, `Changed` event). `PangoMarkup.Escape` (internal, `&`/`<`/`>`/`"`). `FontWeight`/`FontWeights` + `FontStyle`/`FontStyles` structs added to PresentationFramework.
  - **`TextBlock`** extended: `Inlines` (`InlineCollection`, `[ContentProperty]`), `Text` ↔ `Inlines` interplay (setting `Text` creates one `Run`, clears prior inlines; adding inlines clears `Text`). `BuildMarkup()` (internal) assembles Pango markup; `ApplyToLabel()` sets `Gtk.Label.UseMarkup = true` and `Markup` when inlines are rich, plain `Text` otherwise.
  - **44 new tests**: 20 `DispatcherTests` (pure, WindowsBase) + 20 `TextBlockInlineTests` (pure, PF) + 4 `DispatcherTimerGtkSmokeTests` (GTK smoke, PF).
  - **44 new tests** (20 Dispatcher + 20 TextBlock inlines + 4 GTK smoke).
- **Round 10 Track A — real assembly-symbol DP resolution + `x:` markup extensions (DONE).**
  *(This resolves the long-standing Round-5 carry-over: compiler DP-vs-CLR is no longer a name heuristic.)*
  - **`DpSymbolResolver`** (`XamlCodeGenerator.SymbolResolver.cs`): loads the referenced assemblies
    via `Assembly.LoadFrom` (MSBuild task path; avoids `System.Runtime.Loader` which is unavailable on
    `netstandard2.0` target), searches for a public static `<Name>Property` field whose type name
    contains "DependencyProperty", walks the full inheritance chain (inherited DPs + attached DPs on
    base types), caches results in a `ConcurrentDictionary`. Falls back to the previous name heuristic
    when no reference paths are supplied (all existing tests keep passing unchanged).
  - **Reference threading**: `MarkupCompileTask` gains an optional `ReferencePaths` task property
    (`ITaskItem[]`); `Gtk4.Forms.MarkupCompiler.targets` passes `@(ReferencePath)` to it; the task
    calls `generator.SetReferencePaths(paths)` before processing. `XamlCodeGenerator` implements
    `IDisposable` (disposes the resolver's loaded assembly list).
  - **`IsKnownDp` replaced**: now delegates to `_resolver.IsDP(fqType, propName)` instead of the
    pure heuristic; `EmitRootAttributes` made non-static to access the resolver.
  - **`x:Null`** (`IsXNull`): `{x:Null}` on a DP prop → `SetValue(…Property, null)`; on CLR →
    `this.Prop = null`.
  - **`x:Type`** (`IsXType`): `{x:Type Button}` → `typeof(global::System.Windows.Controls.Button)`;
    resolved via `WpfXmlnsMap`; unknown types fall back to as-is.
  - **`x:Static`** (`IsXStatic`): `{x:Static local:Foo.Bar}` / `{x:Static SystemColors.ControlBrush}`
    → `global::Ns.Type.Member`; xmlns prefix stripped; type part resolved via `WpfXmlnsMap`.
  - **`x:Array`** (`TryEmitXArray`): `<x:Array Type="sys:String"><sys:String>a</sys:String>…</x:Array>`
    → `new System.String[] { "a", … }`. Supports `String`, `Int32`, `Int64`, `Double`, `Single`,
    `Boolean`, `Object`, and any type mapped by `WpfXmlnsMap`. Limitation: child elements must be
    simple value wrappers (no nested XAML elements).
  - All x: extensions are dispatched **before** the DP/CLR attribute-value paths and checked for
    `IsKnownDp` to route SetValue vs direct assignment; none interfere with
    `{Binding}`/`{StaticResource}`/`{TemplateBinding}` (each starts with a distinct prefix).
  - **+26 tests**: 9 `XamlCodeGeneratorSymbolResolverTests` (resolver: no-ref fallback ×3, known DP,
    CLR prop, inherited DP, unknown type fallback, generator integration ×2) + 17
    `XamlCodeGeneratorXExtensionTests` (x:Null ×3, x:Type ×5, x:Static ×5, x:Array ×2, dispatch ×2).
  - **+26 MarkupCompiler tests.**
- **Round 10 totals (both tracks merged): 1094 tests** (60 WindowsBase + 860 PresentationFramework +
  158 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 10 review (Opus) + fixes — DONE.** 2 High + Mediums, all fixed with pins: **V1** the
  `DpSymbolResolver` **locked consumer DLLs** (`Assembly.LoadFrom` holds a file lock for the MSBuild-node
  lifetime → later targets fail "file in use" — the lock seen this round) → now `Assembly.Load(File.ReadAllBytes)`
  (lock-free) + an `AssemblyResolve` handler serving dependencies from an in-memory map, with heuristic
  fallback on any load failure; pinned by a test asserting the resolved DLL can be deleted afterward; **V2**
  `{x:Static}`/`{x:Type}` now resolve a `local:` prefix against the element's `xmlns:local="clr-namespace:…"`
  scope and emit the **fully-qualified** `global::MyApp.Type.Member` (were dropping the user namespace → invalid
  C#); **V3** an unresolvable `{x:Type}` raises a clear compiler diagnostic (was silent invalid output); **V4**
  `{x:Null}` on a non-nullable value-type CLR property emits `default(T)` (was `= null` → CS0037); **V5** three
  oversized test files split to ≤200 lines; **V6** non-Bold font weights emit a Pango weight span. **Round 10
  totals: 1108 tests** (60 WindowsBase + 863 PresentationFramework + 169 MarkupCompiler + 16 ProjectConverter),
  all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles through the lock-free
  real-symbol-resolution compiler.
- **Round 11 Track B — FlowDocument + RichTextBox + block documents (DONE).** Runtime only; no compiler changes.
  - **Block hierarchy** (`System.Windows.Documents`): `Block : TextElement` (base; `Margin`/`Padding`/`TextAlignment`/`LineHeight`),
    `Paragraph` (`Inlines` — `InlineCollection`; `AppendMarkup`/`AppendPlainText`), `Section` (`Blocks` — `BlockCollection`; nests blocks),
    `List` (`ListItems`; `MarkerStyle`; bullet/number marker rendering; `ToAlpha`/`ToRoman` helpers), `ListItem` (`Blocks`),
    `BlockUIContainer` (`Child : UIElement`; emits nothing in markup/plain-text — no text representation for a hosted widget).
    `BlockCollection` (typed, `Changed` event; mirrors `InlineCollection` pattern). `TextMarkerStyle` enum (9 values).
  - **`TextAlignment` enum** (`System.Windows`) added to `PresentationFramework` (Left/Right/Center/Justify).
  - **`FlowDocument`** (`[ContentProperty("Blocks")]`; `FontFamily`/`FontSize`/`Foreground`/`TextAlignment`/`PagePadding`):
    `BuildMarkup()` — paragraphs joined by `\n\n`, block `AppendMarkup` per block; `BuildPlainText()` — same with
    `AppendPlainText`; `GetAllText()` (internal) — single-newline joined for buffer loading.
  - **`RichTextBox : Control`** (`System.Windows.Controls`): `Document` DP (`FlowDocument`), `IsReadOnly` DP
    (`→ Gtk.TextView.Editable`), `Selection` (`TextSelection`), `AppendText`; backed by `Gtk.TextView` +
    `Gtk.ScrolledWindow`. `SyncBufferFromDocument` populates the buffer from the document, applying `Gtk.TextTag`s
    for bold/italic/underline. `OnBufferChanged` writes plain text back to the first paragraph (minimal two-way;
    full rich round-trip is a later phase). `TextRange`/`TextSelection` cover the whole document (Phase-1 subset).
  - **`FlowDocumentScrollViewer : Control`** (`System.Windows.Controls`): `Document` DP, read-only; backed by
    `Gtk.ScrolledWindow` + `Gtk.Label` with Pango markup from `FlowDocument.BuildMarkup()`.
  - **GTK-widget choices + limitations**: `Gtk.TextView.Buffer.Insert(TextIter, string)` is a GtkSharp4 compat shim
    (`[Obsolete]`, demoted via `WarningsNotAsErrors CS0618`); the `ref TextIter` overload is the idiomatic API —
    switching deferred (shim is functionally correct). `FlowDocumentScrollViewer` uses `Gtk.Label` (Pango markup) for
    read-only display; a `Gtk.TextView` with read-only tags would be more faithful but is a later refinement.
    Two-way editing (buffer → Document) covers only the plain-text content of the first paragraph; re-parsing
    `Gtk.TextTag` state back into `Bold`/`Italic` runs is explicitly deferred.
  - **39 new tests** (24 `FlowDocumentTests` pure + 9 `RichTextBoxTests` pure + 6 `RichTextBoxGtkSmokeTests` GTK):
    - `FlowDocumentTests`: block hierarchy, two-paragraph plain-text/markup with blank-line separation, bold markup,
      markup escaping, disc/decimal list markers, section nesting, `BlockUIContainer` emits nothing, document property round-trips.
    - `RichTextBoxTests`: `Document` get/set, `AppendText` creates paragraph+run, two `AppendText` → two paragraphs,
      `Selection.Text` returns/sets document text, `TextRange.Text` round-trip, `IsReadOnly` toggle, `GetAllText` join.
    - `RichTextBoxGtkSmokeTests`: `CreateWidget` → `Gtk.ScrolledWindow`, scrolled window contains `Gtk.TextView`,
      `IsReadOnly=true` → `Editable=false`, `Document` syncs buffer text, `FlowDocumentScrollViewer` creates/realizes.
  - **+39 tests** (24 FlowDocument + 9 RichTextBox pure + 6 GTK smoke).
- **Round 11 Track A — faithful WPF two-pass Measure/Arrange layout (DONE).**
  - **`UIElement.Layout.cs`** (new partial): proper `Measure(Size)` / `Arrange(Rect)` with dirty flags
    (`_measureDirty`/`_arrangeDirty`); `RenderSize` property; `ArrangeRect` (internal, for tests);
    `InvalidateMeasure`/`InvalidateArrange`/`UpdateLayout`; `MeasureCore` / `ArrangeCore` virtual seam.
    Old Phase-0 stubs removed from `UIElement.cs`.
  - **`FrameworkElement.Layout.cs`** (new partial): `HorizontalAlignment`/`VerticalAlignment` enums +
    DPs (default `Stretch`); `MinWidth`/`MaxWidth`/`MinHeight`/`MaxHeight` DPs (defaults 0/∞/0/∞).
    `MeasureCore`: deflates available by `Margin`, clamps by explicit `Width`/`Height`/`Min*/Max*`,
    delegates to `MeasureOverride`, adds `Margin` back; `DesiredSize` includes margin.
    `ArrangeCore`: removes margin, computes alignment offset (Left/Center/Right/Stretch × Top/Center/Bottom/Stretch),
    delegates to `ArrangeOverride`, sets `RenderSize` to the aligned content size (not the slot).
  - **Panel `MeasureOverride`/`ArrangeOverride`** in new partial files (pure managed math, no GTK):
    - `StackPanel.Layout.cs`: sum along orientation + max across; stack children sequentially.
    - `Grid.Layout.cs`: two-pass `ResolveColumns`/`ResolveRows` (Absolute → fixed; Auto → 0 initially,
      then `UpdateAutoFromChildren` measures span-1 children and expands; Star → `ResolveStarDistribution`
      divides remaining space by total star weight, proportional to each star value); `BuildOffsets` for x/y;
      child clamped to `[0, count-1]` row/col.
    - `DockPanel.Layout.cs`: subtracts each child's size from the remaining slot in order; last child
      fills if `LastChildFill`.
    - `Canvas.Layout.cs`: `MeasureOverride` returns (0,0); `ArrangeOverride` resolves `Left`/`Top` (or
      `Right`/`Bottom` relative to canvas size) per child.
    - `WrapPanel.Layout.cs`: line-breaking algorithm (accumulate main-axis, wrap when limit exceeded);
      `ArrangeOverride` rebuilds lines and places each child at `(mainOffset, crossOffset)`.
    - `UniformGrid.Layout.cs`: `ComputeGrid(rows,cols,n)` for auto-sizing; equal cell dimensions; child
      at `(i%cols * cellW, i/cols * cellH)`.
    - `Border.Layout.cs`: deflates/inflates by `Padding + BorderThickness`; `ArrangeOverride` insets child.
  - **GTK backing**: native GTK containers (`Gtk.Box`/`Gtk.Grid`/`Gtk.Fixed`/`Gtk.FlowBox`/`Gtk.Frame`)
    still used for rendering. Managed layout runs on top: `Measure`/`Arrange` compute correct
    `DesiredSize`/`RenderSize`/`ArrangeRect` (authoritative for unit tests and future GTK fixed-layout
    replacement); existing GTK smoke tests remain green (no GTK structural changes).
  - **33 new pure tests** (`LayoutContractTests.cs`): `FrameworkElement` Width/Margin/MinWidth/MaxWidth/
    MinHeight/MaxHeight; Stretch/Left alignment; InvalidateMeasure/UpdateLayout; `StackPanel` vertical sum +
    max width + stacked y-offsets + horizontal sum; `Grid` 1*+1* equal split, 2*+1* 2:1 split, Auto column
    sizes to child, Absolute column fixed, Row/Column placement; `DockPanel` Top/Left strips + fill remainder;
    `Canvas` desired=(0,0) + Left/Top placement; `WrapPanel` wrap to new line + 2+1 split + y offsets;
    `UniformGrid` auto 2-col + explicit 2×2; `Border` padding/border-thickness inflate + child inset.
  - **+33 pure Measure/Arrange tests** (`LayoutContractTests`). Note: managed layout is authoritative for the
    contract + tests; native GTK containers still render — wiring the computed arrange rects onto a `Gtk.Fixed`
    is a later refinement (carry-over).
- **Round 11 totals (both tracks merged): 1180 tests** (60 WindowsBase + 935 PresentationFramework +
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 11 review (Opus) + fixes — DONE.** 2 High correctness bugs in the new layout contract (hidden by
  constant-size test children) + 2 medium WPF-fidelity gaps, implemented faithfully (not just documented):
  **W1** `Measure`/`Arrange` now re-run when the constraint/rect changes even if not dirty (WPF
  `previousAvailableSize` semantics) — Grid's resolved-cell second measure pass was a silent no-op;
  **W2** `DesiredSize` is clamped by Min/Max only, not by `availableSize` (content may exceed its slot —
  a 500-tall StackPanel in a 200 slot now reports 500); **W3** Grid star distribution is **iterative** —
  a star hitting its `Min`/`MaxWidth` fixes at the bound and the surplus redistributes among remaining stars
  (1*+1* in 100 with col0 Max=20 → 20/80); **W4** spanning children grow the `Auto` tracks in their span;
  **W5** `RichTextBox.cs` split into `RichTextBox.cs` + `RichTextBox.Buffer.cs` (≤200 lines); **W6** the
  managed-layout-vs-live-GTK gap documented on `UIElement.UpdateLayout` + carry-over. **Round 11 totals:
  1187 tests** (60 WindowsBase + 942 PresentationFramework + 169 MarkupCompiler + 16 ProjectConverter),
  all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 12 Track A — input system + TextBox completeness (DONE).** No compiler changes.
  - **`UIElement` focus + input** (`UIElement.Input.cs` new partial):
    `Focusable`/`IsFocused` (read-only) DPs; `IsKeyboardFocused` alias; `Focus()` (→ `GrabFocus` when
    realised, deferred flag otherwise); `GotFocus`/`LostFocus` bubble events;
    full keyboard routed events (`PreviewKeyDown`/`KeyDown`, `PreviewKeyUp`/`KeyUp`, tunnel+bubble);
    full mouse routed events (`PreviewMouseDown`/`MouseDown`, `PreviewMouseUp`/`MouseUp`,
    `MouseMove`, `MouseEnter`/`MouseLeave`); `PreviewTextInput`/`TextInput`.
    Event arg types: `KeyEventArgs` (`Key`, `KeyStates`, `Modifiers`, `SystemKey`),
    `MouseEventArgs`/`MouseButtonEventArgs` (`GetPosition`, `ChangedButton`, `ClickCount`),
    `TextCompositionEventArgs` (`Text`); delegate types `KeyEventHandler` / `MouseEventHandler` /
    `MouseButtonEventHandler` / `TextCompositionEventHandler`. `IInputElement` interface.
    Internal raise seam `RaiseKeyDownForTest(Key, ModifierKeys)` / `RaiseKeyUpForTest` for pure tests.
    `InvokeInputBinding` wired into `RaiseKeyDown` so `KeyBinding` Ctrl+S dispatches on key events.
  - **GTK wiring** (`UIElement.GtkInput.cs` new partial): `partial void AttachInputControllersOnCreated`
    called from `UIElement.cs OnWidgetCreated`; wires `Gtk.EventControllerFocus` (Enter/Leave →
    `GotFocus`/`LostFocus` + `IsFocused`), `Gtk.EventControllerKey` (`Gtk.KeyPressedArgs` →
    `PreviewKeyDown`/`KeyDown` + `TextInput`; `Gtk.KeyReleasedArgs` → `PreviewKeyUp`/`KeyUp`),
    `Gtk.GestureClick` (`Gtk.PressedArgs`/`ReleasedArgs` → `MouseDown`/`MouseUp`),
    `Gtk.EventControllerMotion` (`Gtk.MotionArgs`/`EnterArgs`/`Leave` → `MouseMove`/`Enter`/`Leave`).
    **GDK→Key map** (`GdkKeyMap.cs`): `FromKeyval(uint)` covers A-Z, 0-9, function keys, numpad,
    special/navigation keys; `FromGdkModifier(Gdk.ModifierType)` → `ModifierKeys` bitmask.
  - **Statics**: `Keyboard` (`Modifiers`, `IsKeyDown`, `Focus`, `FocusedElement`, internal
    `SetFocused`/`ClearFocused`/`SetKeyDown`/`SetKeyUp`); `Mouse` (`LeftButton`/`RightButton`,
    `GetPosition`, `_lastPosition`); `FocusManager` (`FocusedElementProperty` attached DP,
    `GetFocusedElement`/`SetFocusedElement`).
  - **`TextBox` completeness** (`TextBox.cs` pure model, `TextBox.Widget.cs` GTK backing):
    `AcceptsReturn` (single-line = `Gtk.Entry`; multiline = `Gtk.TextView`+`Gtk.ScrolledWindow`),
    `TextWrapping` (→ `Gtk.WrapMode`), `SelectionStart`/`SelectionLength`/`SelectedText`,
    `CaretIndex`, `MaxLength` (→ `Entry.MaxLength`), `IsReadOnly` (→ `Entry.IsEditable`/`TextView.Editable`),
    `SelectionChanged` event, `Select`/`SelectAll`/`Clear`. Original `Text` two-way binding preserved.
  - **36 new tests** (978 PF total; +36):
    - `InputRoutedEventTests` (16 pure): `KeyDown` bubble child→parent; `PreviewKeyDown` tunnel
      parent→child before `KeyDown`; `Handled=true` stops bubble; PreviewKeyDown Handled stops KeyDown;
      `Focus()`/`IsFocused` managed state; `GotFocus`/`LostFocus`; `Keyboard.Modifiers` updated by seam;
      `Keyboard.IsKeyDown` during handler; `Keyboard.FocusedElement`; `FocusManager.GetFocusedElement`;
      `KeyBinding` Ctrl+S invokes command; `KeyBinding` wrong modifier skipped.
    - `TextBoxPureTests` (16 pure): selection round-trips, `SelectedText` get/set, `Select`/`SelectAll`,
      `SelectionChanged` fires, `CaretIndex` round-trip + clears selection, `MaxLength` default/round-trip,
      `TextChanged` fires + carries source, `Clear` empties, `AcceptsReturn`/`IsReadOnly` defaults.
    - `InputGtkSmokeTests` (5 GTK): `KeyDown` fires on realised element (seam); `Focus()` sets `IsFocused`;
      single-line widget is `Gtk.Entry`; multiline widget is `Gtk.ScrolledWindow`; multiline child is `Gtk.TextView`.
  - **GTK note**: `Gtk.EventControllerFocus.Enter/Leave` have `System.EventHandler` signatures (no args);
    `Gtk.EventControllerKey` uses `Gtk.KeyPressedArgs`/`KeyReleasedArgs` (not nested sub-types); `Gtk.GestureClick`
    uses `Gtk.PressedArgs`/`ReleasedArgs`; `Gtk.EventControllerMotion` uses `Gtk.MotionArgs`/`EnterArgs`/no-arg Leave.
  - **Carry-over**: `Mouse.GetPosition(relativeTo)` returns last tracked position (relativeTo ignored; correct
    relative positioning requires a Gdk.Device query on the GTK thread — deferred); `TextBox.MaxLength` in
    multiline (`Gtk.TextView`) mode is not enforced at the widget level (buffer-change guard deferred).
- **Round 12 Track B — default control templates + theming (DONE).** Runtime only; no compiler changes.
  - `FrameworkElement.DefaultStyleKeyProperty` (protected DP a control sets to `typeof(itself)` in its static
    ctor) + `OverridesDefaultStyle`; `ApplyDefaultStyle()` resolves the lowest-precedence style.
  - `Themes/ThemeManager` — a `Type`-keyed default-`Style` registry; `FindDefaultStyle` walks base types so a
    subclass inherits its base's default style unless it registers its own key.
  - `Themes/Generic.cs` — the code-side `generic.xaml`: seeds default `Style`s for `Button` (with a default
    `ControlTemplate` = `Border`→`ContentPresenter`), `CheckBox`, `RadioButton`, `Label`, `Border`; registered
    from `Application`'s ctor. (XAML `generic.xaml` compilation is a later round.)
  - Precedence wired into the existing style path: **explicit `Style` > implicit `TargetType` style > theme
    default** (`ApplyImplicitStyle` falls through to `ApplyDefaultStyle`); `OverridesDefaultStyle` skips the theme.
  - `Media/SystemColors` — `ControlBrush`/`WindowBrush`/`HighlightBrush`/`ControlTextBrush` (fixed values; GTK
    theme query deferred); `Control.Padding`/`HorizontalContentAlignment`/`VerticalContentAlignment` DPs.
  - **+19 tests** (8 precedence + 11 Generic/SystemColors).
- **Round 12 totals (both tracks merged): 1242 tests** (60 WindowsBase + 997 PresentationFramework +
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 12 review (Opus) + fixes — DONE.** 2 Critical + Highs, all fixed with pins driving the real path:
  **X1** the theme/default-style mechanism was **inert** (nothing called it) — `Control`'s ctor now applies the
  default style and `Button`/`CheckBox`/`RadioButton`/`Label` set `DefaultStyleKey` via `OverrideMetadata`
  (default-style setters land below local values); **X2** the GTK input-translation layer (`GdkKeyMap`) had
  **zero coverage** — added 11 pure keyval/modifier-mapping tests + a seam smoke test; **X3** `Focus()` now
  clears the previously-focused element (single source of truth, raises its `LostFocus`); **X4** removed the
  inert/empty default `Button` `ControlTemplate` (template-over-native-widget is a later round); **X5** multiline
  `TextBox` attaches input/focus controllers to the inner `Gtk.TextView` (via a `FocusWidget` hook), not the
  `ScrolledWindow`; **X6** TextBox selection model fixed (`MaxLength` truncates, `SelectionStart` re-clamps
  `SelectionLength`, `SelectedText` setter leaves a consistent caret); **X7** oversized files split to ≤200 lines.
  Full-suite total now **1289 tests** (60 WindowsBase + 1041 PresentationFramework + 173 MarkupCompiler +
  15 ProjectConverter — the tree also compiles concurrent imaging/markup-NuGet work landed in `feb7c90b`),
  all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **`WebBrowser` control — DONE (Photino.NET).** `System.Windows.Controls.WebBrowser : Control` with the full
  WPF surface: `Source` DP, read-only `CanGoBack`/`CanGoForward`, `Navigate`/`NavigateToString`/`GoBack`/
  `GoForward`/`Refresh`/`InvokeScript`, `Navigating`(cancelable, with `NavigationMode`)/`Navigated`/
  `LoadCompleted` (fires exactly once), and a back/forward journal. Backed by **Photino.NET 4.0.16**
  (WebView2/WebKitGTK/WKWebView) via an injectable **`IWebBrowserHost`** seam (`PhotinoWebBrowserHost`
  default; tests inject a fake — **no native window is ever created headless**). Native-absent / no-`DISPLAY`
  degradation is handled by the `new PhotinoWindow(...)` try/catch. **Windowing limitation**: Photino owns a
  native top-level window (no public API to embed as a child GTK widget), so the in-GTK area is a
  placeholder/launcher and the page renders in Photino's own window — true in-widget embedding is a later
  refinement. `InvokeScript` JSON-encodes args and sends a web message (Photino has no synchronous JS-eval,
  so it is best-effort/returns null). Opus-reviewed + fixed (single-fire `LoadCompleted`, dead native-probe
  removed, cancelable `Navigating` on journal navigations, files split ≤200 lines). **39 tests**
  (model + history + InvokeScript + GTK smoke); PF suite **1111 pass**, warning-clean; sample + the
  build-and-run integration test still pass with the transitive Photino dependency.
- **Round 13 — infra + command libraries/selectors — DONE.** Two parallel tracks (agents hit a session
  limit mid-run; recovered next session: built, tested, reviewed, fixed, merged).
  - *Track A — Visual/tree/Freezable foundation*: `Freezable` (Freeze/IsFrozen/Clone/Changed, frozen
    rejects mutation) + `DependencyObjectType` (`FromSystemType`, IsInstanceOfType/IsSubclassOf) in
    `WindowsBase`; `Visual`/`VisualTreeHelper` (GetChildrenCount/GetChild/GetParent over the existing
    `UIElement.VisualChildren`), `LogicalTreeHelper` (walks `LogicalParent` + per-container logical children),
    `DrawingVisual`/`DrawingContext`/`VisualCollection`.
  - *Track B — command libraries + selectors*: `ApplicationCommands`/`NavigationCommands`/`ComponentCommands`/
    `MediaCommands`/`EditingCommands` (`RoutedUICommand` statics with default gestures, e.g. Copy=Ctrl+C);
    `DataTemplateSelector`/`StyleSelector`; `HierarchicalDataTemplate` (ItemsSource/ItemTemplate/
    ItemTemplateSelector + `TreeView` hierarchy population); `ItemsControl` `ItemTemplateSelector`/
    `ItemContainerStyle`/`ItemContainerStyleSelector`/`DisplayMemberPath` (selector wins over template).
  - *Recovery fixes*: a `HierarchicalDataTemplate` test poked an internal static (`TreeView.PopulateHierarchicalChildren`)
    that the repo's **package-based PF consumption** doesn't surface cross-assembly — rewritten to exercise the
    public `ResolveChildren`/`ItemTemplate` surface; and an `ItemsControlSelector` test's `ref int` counter
    (unusable in a closure) was replaced with the selector's own `InvokeCount`.
  - **Full suite: 1466 tests** (83 WindowsBase + 1195 PresentationFramework + 173 MarkupCompiler +
    15 ProjectConverter), all pass; warning-clean.
- **Round 14 Track A — data providers + advanced binding (DONE).**
  - **`DataSourceProvider`** (abstract base): `Data`, `DataChanged`, `IsInitialLoadEnabled`, `InitialLoad()`, `Refresh()`, `BeginInit()`/`EndInit()` (ISupportInitialize shape), `OnQueryFinished` protected hook.
  - **`ObjectDataProvider`**: `ObjectType`/`ObjectInstance`, `MethodName`/`MethodParameters`/`ConstructorParameters` — instantiates the type (or uses the instance) and optionally invokes `MethodName` via reflection, exposing result as `Data`.
  - **`XmlDataProvider`**: `Source`(Uri)/`Document`/`XmlSerializer` (inline XML string), `XPath` — loads an `XmlDocument`, evaluates the XPath expression via `XPathNavigator`, exposes matching nodes as `List<XmlNode>` in `Data`.
  - **`PriorityBinding : BindingBase`**: `Bindings` collection; `PriorityBindingEngine` resolves child bindings in order, uses first non-null result, re-evaluates on INPC; `PriorityBindingExpression` live object; wired into `BindingEngine.SetBinding` and the expression map.
  - **`BindingGroup`**: `Name`, `BindingExpressions`, `ValidationRules`, `Items` (deduplicated source objects), `BeginEdit()`/`CommitEdit()`/`CancelEdit()`/`UpdateSources()`/`ValidateWithoutUpdate()`; validates source objects via rules; writes sources on commit via `BindingExpression.UpdateSource`.
  - **`FrameworkElement.BindingGroupProperty`** DP — new partial `FrameworkElement.BindingGroup.cs`.
  - **`BindingOperations`** (static): `SetBinding`, `ClearBinding`, `ClearAllBindings`, `GetBinding`, `GetBindingExpression`, `GetMultiBindingExpression`, `GetBindingExpressionBase`, `IsDataBound`; routes all kinds through `BindingEngine`; `BindingEngine.ClearBinding`/`ClearAllBindings`/`MultiBindingEngine.ClearAll` added as new partials.
  - **`MultiBinding` TwoWay**: `ConvertBack` now writes each child source via `TargetChangeTracker`; `Binding.DoNothing`/`DependencyProperty.UnsetValue` sentinels skip that slot; reentrancy-guarded. The old `NotSupportedException` contract is replaced by functional TwoWay. Existing test updated.
  - **`RelativeSource.PreviousData`**: resolves by walking to the nearest `ItemsControl` ancestor and returning the item at `index - 1` in `Items`. Documented limitation: depends on `DataContext` matching a flat `Items` entry; nested templates with intermediate hosts may not resolve.
  - **`CollectionViewSource` grouping/live-shaping**: already complete from Round 9 (U6) — `SortDescriptions`/`GroupDescriptions`/`Filter` flow into `View` on `Refresh`.
  - **34 new PF tests** (DataSourceProvider/PriorityBinding/BindingGroup/BindingOperations/MultiBinding-TwoWay).
- **Round 14 Track B — MarkupExtension system + trigger actions/MultiDataTrigger (DONE).**
  - **`MarkupExtension`** base (`ProvideValue(IServiceProvider)`) + `IProvideValueTarget`/`IXamlTypeResolver`/
    `IUriContext` services; concrete `StaticResourceExtension` (resolves via the element resource chain +
    `Application.Current.Resources`), `DynamicResourceExtension`, `TypeExtension` (`{x:Type}`), `StaticExtension`
    (`{x:Static}`, reflection), `NullExtension` (`{x:Null}`), `ArrayExtension` (`{x:Array}`). `Binding`/
    `TemplateBindingExtension` gain a `ProvideValue` (plain method — the build-time compiler path is untouched;
    this is the runtime object model for programmatic use / a future runtime XAML loader).
  - **`Trigger`/`DataTrigger`/`MultiTrigger` `EnterActions`/`ExitActions`** run on the active↔inactive transition;
    **`MultiDataTrigger`** (data-bound `Condition`s, AND semantics, INPC-watched, re-evaluates on DataContext change).
  - **+24 PF tests** (MarkupExtension, EnterActions/ExitActions, MultiDataTrigger).
- **Round 14 review (Opus) + fixes — DONE.** No Critical; 3 High + Mediums, all fixed with public-surface pins:
  **V1** `BindingEngine.ClearBinding` now clears multi/priority records (was leaking a PriorityBinding/MultiBinding
  record + its INPC handlers — the Round-7 cross-engine-leak class); **V2** `ClearAllBindings` clears priority
  records via a new `PriorityBindingEngine.ClearAll`; **V3** `PriorityBinding`'s INPC handler filters by the bound
  path segment (no over-fire) and rebuilds subscriptions on re-resolve; **V4** trigger `ExitActions` no longer
  double-fire on `Style` teardown (`UnApply(runActions:false)` from `Dispose`; actions run only on a genuine
  condition flip); **V5** `XmlDataProvider` pins `XmlResolver = null` (no XXE), converter-less TwoWay MultiBinding
  installs no dead target listener, + doc notes (ObjectDataProvider no auto-requery, BindingGroup non-atomic commit).
  **Round 14 totals: 1561 tests** (83 WindowsBase + 1290 PresentationFramework + 173 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 15 Track A — `VisualStateManager.VisualStateGroups` XAML compilation (DONE).**
  The markup compiler now emits `InitializeComponent` code for the `<VisualStateManager.VisualStateGroups>`
  attached-property element form on any `FrameworkElement` (was code-behind only). Multiple groups; states with
  or without a `<Storyboard>` child (both `<Storyboard>` direct-child and `<VisualState.Storyboard>`
  property-element forms); `x:Name` on groups/states declares typed fields. Storyboard construction is delegated
  to the existing `EmitStoryboardInline` (no duplication). Runtime: `VisualStateManager.Get/SetVisualStateGroups`
  widened `Control` → `FrameworkElement` (backward-compatible; `GoToState` still typed `Control`). Files:
  `XamlCodeGenerator.VisualStateManager.cs` (new), `XamlCodeGenerator.cs` hook, `WpfXmlnsMap.cs` (+3 types).
  **+25 tests** (14 compiler + 11 runtime). Note: WPF `VisualState` is storyboard-only in XAML; the runtime
  `VisualState.Setters` extension has no XAML path (programmatic only).
- **Round 15 Track B — `Window` chrome (DONE).** Five WPF `Window` DPs mapped to GTK4 with graceful
  pre-realize deferral: `WindowState` (Normal/Minimized/Maximized → Minimize/Maximize/Unmaximize),
  `WindowStartupLocation` (Manual/CenterScreen/CenterOwner), `ResizeMode`
  (NoResize/CanMinimize/CanResize/CanResizeWithGrip → `Resizable`+title-button affordances),
  `WindowStyle` (None/Single/ThreeD/ToolWindow → `Decorated`), `SizeToContent`
  (Manual/Width/Height/WidthAndHeight, honored in `ApplyExplicitSize`). Enum defaults match WPF exactly.
  Files: `Window.Chrome.cs`, `Window.Chrome.Style.cs`, five enum files, `Window.cs` edits. **+29 tests**
  (20 pure DP + 9 GTK smoke). GTK4 limits documented in the enum XML docs: no `gtk_window_move` (CenterScreen/
  CenterOwner store-only), no per-button title-bar visibility, border variants collapse to `Decorated=true`,
  `WindowState` round-trip is one-way (DP→native; WM-initiated changes not reflected back).
- **Round 15 review (Opus) + fixes — DONE.** Both tracks correct (enum values/defaults, VSM widening,
  deferral guards, no leaked handlers, codegen delegation all verified). Two **Low** test-only findings fixed:
  the 259-line `XamlCodeGeneratorVsmTests.cs` split by behaviour into a shared `XamlCodeGeneratorVsmTestBase.cs`
  + `XamlCodeGeneratorVsmTests.cs` (structure) + `XamlCodeGeneratorVsmStoryboardTests.cs` (storyboard/fields),
  all ≤200 lines; the brittle whole-file `DoesNotContain("Storyboard")` assertion scoped to
  `new …Storyboard()` + `.Storyboard =`.
  **Round 15 totals: 1615 tests** (83 WindowsBase + 1330 PresentationFramework + 187 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 16 Track A — element-level `FrameworkElement.Triggers` XAML compilation + runtime attach (DONE).**
  The markup compiler now emits the `<{element}.Triggers>` / `<FrameworkElement.Triggers>` property-element form
  (triggers added to `element.Triggers`, then `AttachElementTriggers()`). The shared trigger-emit methods were
  renamed (`EmitPropertyTrigger`→`EmitTriggerToCollection`, …; `styleVar`→`containerVar`) and reused for both
  `Style.Triggers` and element triggers (no duplication; Style output unchanged). Runtime
  `FrameworkElement.Triggers.cs` attaches/detaches ALL `TriggerBase` subtypes. Files:
  `XamlCodeGenerator.cs`, `XamlCodeGenerator.ElementTriggers.cs` (new). **+18 tests** (11 compiler + 7 runtime).
  Fidelity note: real WPF permits only `EventTrigger` in `FrameworkElement.Triggers`; this is a documented
  **superset** (property triggers also attach) — harmless for valid WPF XAML, carried over as a future compiler
  diagnostic.
- **Round 16 Track B — Popup/ContextMenu deferred-open replay (DONE).** Fixes the dropped-open defect: an
  `IsOpen` set before the backing GTK widget exists is stored (`_pendingOpen`) and replayed in an
  `OnWidgetCreated` override; a later `IsOpen=false` before realize cancels it (stores the false, no stale open).
  General (replays whatever value was set), not sample-specific. Files: `Controls/Popup.cs`, `Controls/ContextMenu.cs`.
  **+6 tests** (4 pure-DP + 2 GTK cancel-path smoke).
- **Round 16 review (Opus) + fixes — DONE.** No Critical/High. Fixed: **H1** split the 230-line
  `XamlCodeGeneratorElementTriggerTests.cs` into a shared base + EventTrigger/form file +
  property-trigger file (all ≤200 lines); **A4** (pre-existing latent leak, widened by element reuse)
  `Trigger.Attach`/`EventTrigger.Attach` are now idempotent — a re-attach detaches the prior state/handler
  instead of orphaning it (pinned by 2 failing-first reattach tests). Carried over (Low, non-blocking):
  **A1** compiler diagnostic for non-`EventTrigger` element triggers; **B2** positive open-replay path is proven
  only by pure-DP inspection (GTK `Popover.Popup()` crashes without a parent window), cancel path is asserted.
  **Round 16 totals: 1641 tests** (83 WindowsBase + 1345 PresentationFramework + 198 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 17 Track A — `[assembly:XmlnsDefinition]`-driven xmlns resolution (markup compiler) (DONE).**
  The compiler now resolves element/attribute types from referenced assemblies'
  `[assembly: XmlnsDefinition(xmlnsUri, clrNamespace)]` attributes, merged with the built-in `WpfXmlnsMap`
  (probed first, so nothing regresses). New injectable seam `IXmlnsDefinitionSource` + production
  `AssemblyXmlnsDefinitionSource` (reads via `CustomAttributeData`, no new NuGet package); `ResolveType` is now
  an instance method, gating the xmlns-def probe with a new `TypeExists`. `XmlnsDefinitionAttribute` already
  existed in `WindowsBase/MarkupAttributes.cs` with four `[assembly: XmlnsDefinition]` entries on the PF assembly.
  Files: `IXmlnsDefinitionSource.cs`, `AssemblyXmlnsDefinitionSource.cs` (new), `XamlCodeGenerator.cs`,
  `XamlCodeGenerator.SymbolResolver.cs`, `MarkupCompileTask.cs`. **+12 tests** (6 compiler + 6 WindowsBase attr).
- **Round 17 Track B — true `{DynamicResource}` live change-tracking (DONE).** New
  `FrameworkElement.SetResourceReference(dp, key)`/`ClearResourceReference` establish a live link;
  `ResourceDictionary` raises `ResourceChanged` on add/replace/remove/clear; a **new D7 dynamic-resource
  precedence layer** sits between trigger (D8) and style (D6):
  `animation > local > trigger > dynamic-resource > style > inherited > default`. `DynamicResourceExtension`
  routes a `FrameworkElement`+DP target through `SetResourceReference`. Files: `ResourceDictionary.cs`,
  `DependencyObject.DynamicResourceLayer.cs` (D7 layer, split out), `DependencyObject.cs`,
  `FrameworkElement.ResourceReferences.cs` (new). **+6 tests**.
- **Round 17 review (Opus) + fixes — DONE.** No Critical/High; precedence order verified unchanged for
  non-dynamic values (no regression). Fixed: **M2** merged-dictionary live-updates — `CollectResourceChain` now
  recurses into `MergedDictionaries`, so a key resolved from a merged dict also live-updates (pinned by a
  failing-first mutate test); **L1** the modified `DependencyObject.Layers.cs` (238 lines) D7 block extracted to
  `DependencyObject.DynamicResourceLayer.cs` (Layers back to 197), class-doc priority line corrected.
  Carried over (Medium): **M1** app-lifetime subscription leak — a dictionary holds a strong delegate to the
  element, so `SetResourceReference` pins the element until `ClearResourceReference` (no unload hook yet);
  documented in the `SetResourceReference` XML doc, weak-event fix deferred to a dedicated round.
  **Round 17 totals: 1661 tests** (89 WindowsBase + 1353 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 18 Track A — weak `ResourceChanged` subscription (fixes Round-17 M1 leak) (DONE).**
  `ResourceDictionary.ResourceChanged` is backed by a weak-handler list (`WeakResourceChangedHandlers.cs`,
  `List<WeakReference<Action>>`, pruned on raise/subscribe/remove). `FrameworkElement` caches its callback in a
  field so the delegate lives exactly as long as the element; the dictionary holds it weakly — a collected
  element is no longer pinned by a long-lived (e.g. `Application`-level) dictionary. `ClearResourceReference`
  still detaches deterministically. **+2 tests** (a deterministic GC leak test + a live-update-after-GC
  regression). The Round-17 `SetResourceReference` "must clear to avoid pinning" caveat is removed.
- **Round 18 Track B — nested binding paths in `DataTrigger` (DONE).** Removed the `NotSupportedException`
  nested-path guard (the E5 gap); added a reusable internal `PathWatcher` (`BindingEngine.PathWatcher.cs`) that
  subscribes per-segment INPC and re-subscribes from a changed segment forward. `DataTriggerState` (split into
  `DataTrigger.State.cs`) owns one `PathWatcher`, recreated on DataContext change. Leaf change, intermediate
  reassignment, DataContext re-root, null-along-path, single-segment regression, and Enter/ExitActions all
  covered. **+12 tests** (the inverted contract test now asserts "no throw").
- **Round 18 review (Opus) + fixes — DONE.** Both tracks correct (weak scheme verified: no reverse strong path,
  raise-reentrancy index-safe, GC test deterministic; PathWatcher per-segment resubscription/null-safety/
  DataContext re-root all clean; D8 precedence and the Round-14 no-double-ExitActions contract preserved). No
  Critical/High/Medium. Two **Low** house-rule nits fixed: the 217-line `DataTriggerNestedPathTests.cs` split
  into a shared base + core + actions files (all ≤200); a `private set` in a test changed to `internal set`.
  **Round 18 totals: 1676 tests** (89 WindowsBase + 1368 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 19 Track A — `Clipboard` + data-object model (DONE).** New `System.Windows.IDataObject`/`DataObject`
  (format-keyed store, Text↔UnicodeText auto-convert), `DataFormats`, and static `Clipboard` behind an internal
  `IClipboardBackend` seam: `InMemoryClipboardBackend` (headless/tests) + `GtkClipboardBackend` (in-memory mirror
  for synchronous reads + best-effort push to `Gdk.Display.Default.Clipboard`, degrades when no display).
  **+33 tests** (incl. a headless round-trip through the real `GtkClipboardBackend`). Documented limits:
  cross-process reads not supported (GTK4 clipboard read is async-only), `StoreAsync` not called on
  `SetDataObject(copy:true)`, no arbitrary-object serialization, `DataFormats.StringFormat` not auto-converted
  to UnicodeText (only Text↔UnicodeText).
  - **CI hotfix (macOS):** `GtkClipboardBackend` disables the native GTK clipboard side-channel on macOS —
    GTK4's Quartz `GdkClipboard` SIGSEGVs (139) on `SetContent`/`Text=` writes in this GtkSharp build (the
    `gdk_content_provider`/`gdk_content_formats` path), crashing the whole macOS test process. A native crash is
    not catchable, so the only safe fix is not to call it; macOS runs entirely off the in-memory mirror
    (within-process clipboard works; cross-app push is a documented no-op there). Linux/Windows unaffected.
- **Round 30 Track A — Popup placement (DONE).** `Popup.Placement` DP (`PlacementMode`, default Bottom; 6 missing
  enum members added to the existing `PlacementMode`), `HorizontalOffset`/`VerticalOffset` DPs, applied to the GTK
  popover via pure `PopupPlacementMap.ToGtkPosition` (Bottom/Top/Left/Right exact; Center/Absolute/Relative/Mouse →
  Bottom fallback, documented) + `Gtk.Popover.SetOffset` (double→int). **+37 tests**.
- **Round 30 Track B — DataGrid column sorting (DONE).** `DataGrid.CanUserSortColumns` + `DataGridColumn.CanUserSort`/
  `SortMemberPath`/`SortDirection`; an `internal ToggleSort(column)` seam cycles null→Asc→Desc, clears sibling
  columns, and re-sorts rows via pure `DataGridSort.SortItems` (reflection member read, single + recursive dotted
  path, nulls-first, stable via explicit index tiebreaker on `List.Sort`). Header-click wired when realized. **+23 tests**.
- **Round 30 review (Opus) + fixes — DONE.** Both tracks correct and **netstandard2.0-safe** (no MaxBy/Order/span
  APIs; built on all 5 TFMs). Fixed two **Low**: split the 286-line `DataGridSortTests.cs` into a shared base +
  SortItems/DP/ToggleSort files (all ≤150); corrected a stale `DataGridSort` doc (dotted paths resolve recursively,
  not top-level fallback). Carry-over: replacing `ItemsSource` resets rows to unsorted while a column's
  `SortDirection` stays non-null (stale sort state across source swaps — WPF re-sorts via its collection view).
  **Round 30 totals: 2385 tests** (89 WindowsBase + 2077 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across `netstandard2.0;net8.0;net10.0` (+`-windows`).
  (Constraint adopted this round: PF new code must compile on netstandard2.0 — the owner multi-targeted the
  assembly to ship a netstandard2.0-compatible NuGet package.)
- **Round 31 Track A — ListBox multi-select (DONE).** `SelectionMode` enum (Single/Multiple/Extended) + DP
  (default Single), `SelectedItems` (IList), mapped to `Gtk.ListBox.SelectionMode` (Single→Single;
  Multiple/Extended→Multiple — GTK doesn't distinguish, documented). `SelectionChangedEventArgs` gained
  `AddedItems`/`RemovedItems` (non-null by default — existing ComboBox/TreeView/TabControl consumers unaffected).
  GTK `SelectedRowsChanged`→delta→single `SelectionChanged`, guarded against DP↔GTK re-entry. **+28 tests**.
- **Round 31 Track B — tab navigation (DONE).** `UIElement.IsTabStop` (default true) + `TabIndex`
  (default `int.MaxValue`) DPs → `widget.CanFocus = IsTabStop && Focusable`; `System.Windows.Input.KeyboardNavigation`
  static class (`TabNavigation`/`DirectionalNavigation`/`IsTabStop`/`TabIndex` attached DPs + `KeyboardNavigationMode`
  enum); pure `TabOrderHelper.Order` (stable by TabIndex then declaration, skips non-tab-stops). **+40 tests**.
- **Round 31 review (Opus) + fixes — DONE.** Top risks clean (shared `SelectionChangedEventArgs` non-null → no
  consumer NRE; selection-sync single `_updating` guard → no re-entry; netstandard2.0-safe). Fixed two **Minor**:
  **F1** create-time `CanFocus` applied to the outer widget instead of `FocusWidget` — a multiline TextBox with
  pre-realize `IsTabStop=false` left its inner TextView focusable; now uses `FocusWidget` (pinned by a new
  multiline-TextBox GTK smoke test). **F2** corrected the `KeyboardNavigation.IsTabStop`/`TabIndex` docs (they are
  independent attached DPs, not mirrors that cross-propagate).
  **Round 31 totals: 2452 tests** (89 WindowsBase + 2144 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 31)**: GTK can't distinguish `SelectionMode.Multiple` vs `Extended` (keyboard-modifier
  range/toggle semantics not reproduced); `TabIndex` has no per-widget GTK order API (GTK uses tree order — the
  managed `TabOrderHelper` computes WPF order); `KeyboardNavigation.IsTabStop` attached DP does not propagate to
  `UIElement.IsTabStop`.
- **Round 32 Track A — TextBlock layout (DONE).** `TextAlignment` (reused enum) → `Gtk.Label.Justify`+`Xalign`,
  `TextTrimming` (new enum; Char & Word both → `Pango.EllipsizeMode.End`), `Padding` → label margins, `LineHeight`
  (store-only — GtkSharp4 doesn't surface the Pango line-spacing factory). Pure `MapTextAlignment`/`MapTextTrimming`
  helpers. **+39 tests**.
- **Round 32 Track B — SelectedValue/SelectedValuePath (DONE).** Added to BOTH `ListBox` and `ComboBox` via a
  shared pure `SelectorValueSupport` (`GetValue` reflection single+dotted path, `FindByValue`); two-way
  `SelectedValue`↔`SelectedItem` sync (`SelectedValuePath`-resolved; set selects the matching item or clears on
  no-match), guarded against loops by a per-instance `_updatingValue` flag. **+30 tests**.
- **Round 32 review (Opus) + fixes — DONE.** No correctness defects (two-way sync-loop guard verified symmetric
  on both controls; netstandard2.0-safe; mappings correct, no default-left regression). Fixed house-rule/doc:
  split the 202-line `TextBlock.Layout.cs` (mapping helpers → `TextBlock.LayoutMapping.cs`), split the 330-line
  `SelectedValueTests.cs` (→ SelectorValueSupport/ListBox/ComboBox files) and the 204-line
  `TextBlockLayoutDpTests.cs` (→ + mapping-tests file) — all ≤160; corrected a false `LineHeight` doc claim about
  re-application.
  **Round 32 totals: 2521 tests** (89 WindowsBase + 2213 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 32)**: `TextBlock.LineHeight` is store-only (no GTK line-spacing applied); `TextTrimming`
  Char vs Word both ellipsize at End; `SelectedValue` retains a non-matching value after a no-match clear
  (WPF behaviour, untested).
- **Round 33 Track A — Slider ticks (DONE).** `SmallChange`/`LargeChange` (→ adjustment step/page),
  `TickFrequency`/`TickPlacement` (→ `Gtk.Scale.AddMark` per tick; `Both` adds both sides), `IsSnapToTickEnabled`
  (snap in Value coercion via pure `SliderSnap.SnapToTick`, round-half-away-from-zero), `IsDirectionReversed`
  (→ `Range.Inverted`), `Orientation`. **+44 tests**.
- **Round 33 Track B — PasswordBox completeness (DONE).** `PasswordChanged` (`RoutedEventHandler`, fire-once,
  `_updating` guard), `MaxLength` (managed clamp — no native GTK API), `Clear()`, `SecurePassword` (fresh
  read-only `SecureString`), `PasswordChar` (store-only — `Gtk.PasswordEntry` has no echo-char API). **+38 tests**.
- **Round 33 review (Opus) + fixes — DONE.** No functional defects (snap↔GTK loop guarded, clamp-after-snap,
  PasswordChanged fire-once + MaxLength clamp + SecurePassword all verified; netstandard2.0-safe). Fixed **F1**
  (house rule): split the 222-line `Slider.Ticks.cs` (GTK apply → `Slider.TicksGtk.cs`) and the two 260/216-line
  test files (snap-math and SecurePassword into their own files) — all ≤165. **F3** aligned `SnapToTick` to the
  documented round-half-away-from-zero (`Math.Round(..., AwayFromZero)`); **F2** corrected the `Clear()` doc
  (fires only when non-empty).
  **Round 33 totals: 2595 tests** (89 WindowsBase + 2287 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 33)**: `PasswordChar` and `MaxLength` have no native `Gtk.PasswordEntry` API (managed-only);
  `Slider.AddMark` is one call per tick (no uniform-mark GTK API).
- **Round 34 Track A — AutomationProperties (DONE).** `System.Windows.Automation.AutomationProperties` attached
  DPs (`Name`/`HelpText`/`AutomationId`/`LabeledBy`/`IsRequiredForForm`/`LiveSetting`); `Name`→GTK
  `AccessibleProperty.Label` and `HelpText`→`AccessibleProperty.HelpText` pushed via
  `Gtk.AccessibleExtensions.UpdateProperty` on realize (API reachable in GtkSharp4); the rest object-model-only.
  **+28 tests**.
- **Round 34 Track B — ToolTipService timing/placement (DONE).** Attached DPs `InitialShowDelay`/`ShowDuration`/
  `BetweenShowDelay`/`ShowOnDisabled`/`Placement`/`PlacementTarget`/`HorizontalOffset`/`VerticalOffset` (store-only
  — GTK tooltip timing/placement is toolkit-controlled) + `IsEnabled` (actionable: suppresses `HasTooltip` on the
  realized widget). **+26 tests**.
- **Round 34 review (Opus) + fixes — DONE.** No functional/netstandard2.0 defect; `IsEnabled` gate doesn't
  regress the default tooltip apply. Fixed three **house-rule** >200 files (split `AutomationProperties.cs`→
  `+ .Semantics.cs`; split the 351-line `ToolTipDpTests.cs`→control + service-defaults + service-roundtrip;
  extracted `AutomationTestBase` stubs to trim `AutomationPropertiesTests.cs`). Fixed a latent bug Opus flagged:
  `OnToolTipChanged` now consults `ToolTipService.IsEnabled`, so setting a new ToolTip on an already-realized
  disabled element no longer re-shows it (pinned by a new GTK smoke test).
  **Round 34 totals: 2648 tests** (89 WindowsBase + 2340 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 34)**: ToolTip timing/placement DPs are store-only on GTK; `AutomationProperties`
  LabeledBy/IsRequiredForForm/LiveSetting/AutomationId are object-model-only (no GTK accessible push).
- **Round 35 Track A — RenderTargetBitmap (DONE).** `Media.Imaging.RenderTargetBitmap : BitmapSource` +
  `Media.PixelFormat`/`PixelFormats.Pbgra32`; `Render(Visual)` rasterises a `DrawingVisual`'s recorded
  `DrawingContext` into an off-screen `Cairo.ImageSurface(Argb32)` (in-memory — works headless) via the Round-28
  `DrawingContextRenderer`; `CopyPixels`/`GetPixel` read back premultiplied BGRA (wrong-size buffer throws per WPF).
  DrawingVisual-only (documented). **+29 tests** (real pixel read-back, not degrade path).
- **Round 35 Track B — Typography (DONE).** `System.Windows.Documents.Typography` attached DPs
  (`Capitals`/`NumeralStyle`/`Fraction`/`StandardLigatures`/`Kerning`/`Variants` + 4 enums) mapped to Pango
  `font_features` tokens (smcp/c2sc/onum/lnum/frac/afrc/sups/subs/ordn/… ; `liga=0`/`kern=0` only when explicitly
  false; all-default → no span) and woven into `TextBlock.BuildMarkup` (fast-path still skips markup when no
  features). **+33 tests**.
- **Round 35 review (Opus) + fix — DONE.** No functional/netstandard2.0 defect (CopyPixels contract + layout
  correct; TextBlock static-ctor metadata override + markup change don't regress the 74 existing TextBlock tests;
  `font_features` mapping correct). Fixed one **Low** house-rule breach: split the 318-line `TypographyTests.cs`
  into defaults/round-trips + `TypographyFontFeaturesTests` (both ≤188).
  **Round 35 totals: 2704 tests** (89 WindowsBase + 2396 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 35)**: `RenderTargetBitmap` rasterises only `DrawingVisual` content (general live visuals
  render nothing); Pango `font_features` requires Pango ≥ 1.38 at runtime.
- **Round 36 Track A — Element lifecycle + size events (DONE).** `FrameworkElement.Loaded`/`Unloaded` (Direct,
  pre-order raise-once broadcast over logical + attached children, `IsLoaded`; deferred attach/detach at
  `DispatcherPriority.Loaded` re-checks tree state), `Initialized`/`IsInitialized` + `ISupportInitialize`
  (`BeginInit`/`EndInit` with WPF's exception messages, lazy fire on attach/before Loaded), read-only
  `ActualWidth`/`ActualHeight` DPs + `SizeChanged` (`SizeChangedInfo`/`SizeChangedEventArgs`, pure
  `SizeChangeMath` = `DoubleUtil.AreClose`), `Window.SourceInitialized` (GTK realize) / `ContentRendered`
  (frame-clock `after-paint`); live GTK allocation fed into `RenderSize` from the toplevel frame-clock `layout`
  signal (`GtkAllocationSync`). **+66 tests**.
- **Round 36 Track B — Mouse input completeness (DONE).** `EventManager.RegisterClassHandler` (both overloads,
  most-derived first, before instance handlers, `handledEventsToo`) + `GetRoutedEvents`/`GetRoutedEventsForOwner`;
  tunnel no longer stops on Handled; button cracking (`(Preview)MouseLeft/RightButtonDown/Up`, Direct, via
  UIElement class handlers), `(Preview)MouseWheel` (`Gtk.EventControllerScroll`, `RetVal` = Handled so
  `ScrolledWindow` still scrolls), `IsMouseOver`/`IsMouseDirectlyOver` (`contains-pointer`/`is-pointer`),
  `CaptureMouse`/`Mouse.Capture`/`CaptureMode`/`IsMouseCaptured`/`Got`/`LostMouseCapture` (managed re-targeting),
  `Control.(Preview)MouseDoubleClick`; `GestureClick.Button = 0`. **+64 tests**.
- **Round 36 review (Opus) + fixes — DONE.** Track A: **R36A-1 (High)** live `ActualWidth` was GTK's *content*
  box (`gtk_widget_get_width`, CSS border+padding removed — a themed Button read ~22px narrow) → now the border
  box via `gtk_widget_compute_bounds` (`GtkAllocationSync.BorderBoxSize`); the tautological real-GTK assertion
  replaced by one against `ComputeBounds` plus `ActualWidth > Widget.Width` on a padded Button. **R36A-2**
  `ContentRendered` re-armed by every `Content` change (WPF `OnContentChanged`/`PostContentRendered`: next
  `after-paint` with a queued redraw, or dispatcher work at `Input` headless), never raised for a window whose
  Content was never set; the "GTK limitation" wording dropped. **R36A-3** `Arrange` now only queues
  `SizeChangedInfo` on a thread-static chain fired after the outermost arrange pass in prepend order (parent
  first; repeat arranges merge axes — WPF `SizeChangedInfo.Update`), so handlers see final sizes and never
  re-enter mid-pass (new `UIElement.SizeChanged.cs`). **R36A-4** one writer at a time: a managed arrange no
  longer writes `RenderSize` once the element is loaded with a mapped widget (`IsRenderSizeOwnedByGtk`) —
  `UpdateLayout()` on a shown window raises zero SizeChanged; a subtree attached to a live window syncs its
  allocation right after its deferred Loaded. **R36A-5** the MarkupCompiler brackets every non-Application
  root with `ISupportInitialize.BeginInit`/`EndInit` (warning-free `(object)this is` check), so a XAML root is
  `IsInitialized` (children first) before its constructor returns. **R36A-6** `RenderSize` setter is
  store-only (internal `SetRenderSizeAndNotify` for the live path); test flipped. **R36A-7** `GetChildren`
  dedupes with a reference `HashSet`; the per-frame sync walks attached children linearly (`EnumerateAttached`).
  **R36A-8** FE-only/single-segment DP-source tracking documented as a gap in XML docs. **R36A-9** house rule:
  path resolution split to `BindingEngine.Path.cs` (546→491), `CloseCore`/`OnCloseRequest`/`Close(bool?)` moved
  to `Window.Closing.cs` (`Window.cs` 236→191). Track B: **F1 (High)** same-GDK-event dedupe now unconditional
  (pure `MouseDispatchDedupe` + `MouseEventKey`, `DispatchGtkButton`/`DispatchGtkMotion` seams): capturing in
  `MouseLeftButtonDown` (or releasing in `MouseUp`) no longer re-delivers the event via an ancestor's
  controller, and uncaptured nesting no longer double-raises press/motion. **F2** pure `MouseButtonMap.TryFromGdk`:
  4/8→XButton1, 5/9→XButton2 (Win32 `WM_XBUTTON*`/macOS `buttonNumber+1` report 4/5), anything else raises
  nothing (was Left). **F3** scroll is unit-aware: `GDK_SCROLL_UNIT_SURFACE` pixels → notches at 48 px (WPF
  ScrollViewer 3 lines × 16 px) before ×120. **F4** the dispatch decision is now covered by pure + nested
  real-widget tests. **F5** `PreviewMouseMoveEvent` (Tunnel, shared Handled flow with MouseMove) +
  `IsMouseCaptureWithin` read-only DP / `IsMouseCaptureWithinChanged` (captor + logical ancestors).
  **F6** no-auto-release documented in `Mouse` capture remarks. **F8** release passes `NPress`. **F9** handler
  invocation unwraps `TargetInvocationException` (`RoutedEventHandler` invoked typed; `ExceptionDispatchInfo`
  rethrow; `RoutedEventHost.Invoke.cs` split keeps the host ≤200). **F7** deferred (see carry-over).
  **+55 tests** (PF +49: SizeChanged chain, ContentRendered re-arm/none, border box, UpdateLayout-no-SizeChanged,
  generated-root Initialized, GDK dispatch/dedupe/button/wheel maps, PreviewMouseMove/IsMouseCaptureWithin,
  nested real-widget dispatch; MarkupCompiler +3; WindowsBase +3).
  **Round 36 totals: 2889 tests** (105 WindowsBase + 2543 PresentationFramework + 226 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 36)**: mouse/capture/wheel routed events are still owned by `UIElement` (WPF: registered on
  `Mouse`, `AddOwner`ed to UIElement, with `Mouse.Add*Handler` helpers — needs `RoutedEvent.AddOwner`; **F7**);
  capture is never auto-released (disable/hide/remove/OS grab loss) and has no explicit grab without a held
  button; DP-source binding tracking is FrameworkElement + single-segment only; `Dispatcher.BeginInvoke`
  ignores `DispatcherPriority`; no `UIElement.Visibility`/`IsVisible`/`IsEnabled` DPs yet (capture gate and
  Loaded semantics read GTK `visible`/`sensitive`); `BindingEngine.cs` still 491 lines.
- **Round 37 Track A — UIElement Visibility/IsEnabled/IsHitTestVisible (DONE).** `Visibility` (Visible/Hidden/
  Collapsed) + read-only `IsVisible`/`IsVisibleChanged`, `IsEnabled`/`IsEnabledCore`/`IsEnabledChanged` (coerced
  through the logical tree and command sources), `IsHitTestVisible`/`IsHitTestVisibleChanged`, `CoerceValue`/
  `InvalidateProperty`, `BooleanToVisibilityConverter`. GTK mapping in pure `VisibilityGtkMap`: Collapsed hides the
  widget (containers skip it), Hidden keeps it mapped at opacity 0, untargetable and unfocusable; focus/capture
  released on disable/collapse/hide. A `Window`'s Visibility is its show state (default Collapsed, `Show`/`Hide`).
- **Round 37 Track B — Brush/Font DPs (DONE).** `Background`/`Foreground`/`BorderBrush`/`BorderThickness`/
  `CornerRadius` + `Font*` on Control/TextBlock/Panel/Border (`TextElement`-inherited via `DependencyProperty.
  AddOwner`), rendered as a lazy per-widget `Gtk.CssProvider` (priority 600) through pure `BrushCss`/`FontCss`/
  `ElementCssStyle`; only non-inherited values emit CSS, GTK CSS inheritance carries the rest. Image/Visual/Drawing
  brushes store-only; gradient foreground falls back to the first stop.
- **Round 37 review (Opus) + fixes — DONE.** Track A: **F1 (High)** a Window's own Visibility (show state)
  drove the GTK mapping: a widget touched before `Show()` (as `ShowDialog` does) got `CanFocus=false`, and
  Hide→Opacity→Show left opacity 0; new `internal virtual WidgetVisibility` (Window: always Visible) used by
  CanFocus/CanTarget/Opacity/created-state; `Opacity` now also applies to a Window (it was gated on a content
  box a Window never has). **F2 (High)** `ClearValue` never re-ran coercion (cleared child under a disabled
  parent read enabled). **F3** style/trigger/dynamic-resource/animation layers stored already-coerced values, so
  coercion could not relax: all layers now store raw values and every mutation goes through one
  `CommitLayerChange` (old → layer → `RecomputeCoerced` → fire → `NotifyValueSource`), which removed
  `_baseValues`; `ClearStyleValues`/`ClearTriggerValue` re-coerce and compute old from the resolved value (the
  snapshot value is wrong when a local shadows it). **F4** `Show`/`ShowDialog`/`Visibility=Visible` after `Close`
  throw `InvalidOperationException` (WPF text); `CloseCore` sets Hidden so the re-set is a change. **F5**
  `ReadLocalValue` returns the raw local (automatic with F3). Track B: **B37-1 (High)** CSS emission depends on the
  value source but refreshed only on effective-value change (local==inherited rendered the ancestor's later
  value): static `DependencyObject.ValueSourceChanged` hook fired after every layer set/clear (and for children
  shadowed during inheritance propagation) refreshes appearance DPs; `ElementCssStyle.Update` skips unchanged CSS.
  **Also found by the real-GTK tests**: GTK 4 `Widget.Name` returns the type name (`GtkLabel`) for an unnamed
  widget, so the `#name` selector matched nothing and no appearance/effect/render-transform CSS ever applied;
  new `ElementCssStyle.IsUnnamed` (used by Effect and RenderTransform too). **B37-2** CSS builders pattern-match
  instead of hard-casting; `BindingEngine.CoerceToTargetType` converts int→double, string→struct (XamlValueConverter),
  IConvertible and TypeConverter sources, falling back instead of throwing. **B37-3** `FromOpenTypeWeight`/
  `FromOpenTypeStretch` moved onto the `FontWeight`/`FontStretch` structs (+ FontStretch `<`/`>`/`Compare`).
  **B37-4** `TextElement.*` attached attributes: `TextElement` in `WpfXmlnsMap`, setter value type resolved from
  the referenced assembly (`DpSymbolResolver.GetAttachedSetterValueType`) with a hardcoded TextElement fallback.
  **B37-5** `ElementCssStyle.ParseErrors` (`CssProvider.ParsingError`) → `AppearanceParseErrors`; theory over
  every builder output ×4 element types plus measure-based effect assertions (font-size height, border width).
  **B37-6** Generic styles set `Foreground` (ControlTextBrush for Button/CheckBox/RadioButton/Label,
  `SystemColors.WindowTextBrush` (new) for TextBox) as WPF themes do; a theme brush the style layer merely repeats
  is not emitted (`HasOnlyStyleValue`/`GetInheritedValue`), so untouched controls stay provider-free. **B37-7**
  a border with thickness but no brush emits `border-style: solid; border-color: transparent`. **B37-8** linear
  stop offsets remapped onto the CSS gradient line (partial spans keep their extent); GradientOrigin ignored and
  only `SpreadMethod.Pad` honoured, documented. **B37-9** TextBlock padding-vs-background gap documented.
  **B37-10** `FontFamily` locations reduced to the name after `#` (font files are not loaded). **B37-11**
  `FontCss` skips non-positive/NaN/infinite sizes; DP validation (`ValidateValueCallback`) documented as a gap,
  flag alignment skipped (no code reads `AffectsMeasure`/`AffectsRender`). **B37-12** stale Border docs and the
  five long `TextElement.Properties.cs` lines + `FontCss` weight line wrapped. Rejected: none; B37-8's offset
  remap done (optional), B37-11's flag alignment and validation overload deferred.
  **Round 37 totals: 3140 tests** (126 WindowsBase + 2764 PresentationFramework + 235 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 37)**: no `ValidateValueCallback` (invalid-enum/FontSize/FontFamily validation); Focus and
  capture gates skip the presentation-source half of `IsVisible`; Window ignores Opacity/CanTarget suppression for
  Hidden; radial `GradientOrigin` and non-Pad `SpreadMethod` unsupported; linear remap assumes
  `RelativeToBoundingBox`; `TextBlock.Background` does not cover Padding (margins); font files by URI not loaded
  and a `pack://` URI containing commas is split by the family list; `AffectsMeasure`/`AffectsRender` flags not
  aligned with WPF; Image/Visual/DrawingBrush backgrounds store-only; the on-screen CSS result was only verified
  through GTK measurement, not visually.
- **Round 38 Track A — FrameworkPropertyMetadataOptions + ValidateValueCallback + two-way defaults (DONE).**
  `FrameworkPropertyMetadataOptions` (WPF numeric values), `UIPropertyMetadata`, metadata sealing and base-merge
  (only explicitly set flags win), `ValidateValueCallback` (`IsValidType`/`IsValidValue`, direct `SetValue` throws
  `ArgumentException`), and `BindsTwoWayByDefault`/`DefaultUpdateSourceTrigger` resolved by `BindingDefaults`
  (`TextBox.Text` → LostFocus, `IsChecked`/`Value`/`SelectedItem`/… two-way); `UpdateSourceTrigger` moved to
  WindowsBase with a `TypeForwardedTo`. Store-only on GTK: `Affects*` layout/render flags, `Journal`,
  `OverridesInheritanceBehavior`, `SubPropertiesDoNotAffectRender`, `IsAnimationProhibited`, `NotDataBindable`.
  **+107 tests**.
- **Round 38 Track B — Application lifecycle (DONE).** `Startup`/`Exit`/`ShutdownMode`/`Windows`/`MainWindow`,
  `Activated`/`Deactivated`, `FindResource`/`Properties`, `StartupUri` resolution and
  `DispatcherUnhandledException`. `SessionEnding` is store-only (GTK reports it only through a `GtkApplication`).
  **+74 tests**.
- **Round 38 review (Opus) + fixes — DONE.** Track A (BLOCK): **A1 (High)** bindings pushed null into value-type
  DPs and threw now that `SetValue` validates → `BindingEngine.SetTargetSafe`/`SetFallbackOrDefault` (valid value,
  else converted `FallbackValue`, else metadata default; never throws) wired into `PushToTarget`, `ApplyFallback`,
  `ApplyTargetNullValue` and `MultiBindingEngine.PushCore`. **A2 (High)** `WriteBack` handed the raw target value
  to `PropertyInfo.SetValue`, so `TextBox.Text` on an `int` threw at LostFocus → `TryResolveWriteTarget` +
  `TryConvertToSourceType` (empty→null for `Nullable`, TypeConverter/`Convert.ChangeType`); a failed conversion
  skips the write and sets an `ExceptionValidationRule` error, cleared by the next good write. **A3** the
  `FrameworkPropertyMetadata(callback, coerce)` ctor no longer marks the default as explicit null. **A4** Window and
  the `AddOwner` appearance metadata (Control/Border/TextBlock Background, Control BorderBrush/BorderThickness)
  passed a callback the base already chains, so it ran twice → dropped. **A5** `ConvertBack` exceptions (single and
  multi) are caught (error only with `ValidatesOnExceptions`), `DoNothing`/`UnsetValue` skip the write, and
  `MultiBinding.UpdateSourceTrigger` now resolves like `Binding` (LostFocus dirty flag, Explicit never writes);
  stale `MultiBinding` docs fixed. **A6** reused/duplicate metadata and a second `OverrideMetadata` for one type
  throw `ArgumentException`; the default is validated before sealing. **A7** `BindingEngine.cs` 518→185 lines
  (`.Push/.Refresh/.Target/.SourceConvert/.TargetChanges/.WriteBack/.Validation` partials), `MultiBindingEngine`
  split, `TabItem`/`ComboBox.Items` extracted. Track B (SHIP-WITH-FIXES): **F1 (High)** `CloseForShutdown` now
  raises `Closing` once and ignores `Cancel` (no re-entry from a handler). **F2 (High)** `StartupUri` loads
  after `Startup` (a Startup-requested `Shutdown` is async, so the window is still created; see follow-up).
  **F3** `DispatcherUnhandledException` forwards to `Dispatcher.UnhandledException` (dispatcher
  sender, all handlers run, active from subscription); the invented `ApplicationUnhandledException` hook removed.
  **F4** `DoStartup` and the initial `Show` route exceptions through `RaiseUnhandledException`. **F5**
  `Shutdown` is thread-checked and posted via `Dispatcher.BeginInvoke` (first request wins). **F6** `Deactivated`
  is deferred and cancelled when another window activates. **F7** `Run(Window)` keeps an existing `MainWindow`;
  second `Run` throws. **F8** a finished/shut-down app stops tracking windows; `ResetCurrentForTests` in
  lifecycle test teardown. **F9** `Window.Activation.cs`, `Application.StartupUri.cs`. Carry-in: **CI-1** a
  throwing `CoerceValueCallback` stores nothing (`SetValueCore` rolls back) and `Window` rejects Visible-after-close
  from a coerce callback; **CI-2** `ToggleButton`/`RepeatButton`/`PasswordBox` get a `DefaultStyleKey` and theme
  Foreground (`Generic.Buttons.cs`); **CI-3** every layer above Inherited now shadows inheritance propagation
  (coerced values compare effective values); **CI-4** a failed conversion reverts to the default instead of a
  stale value; **CI-5** `BrushCss`/`ClosedMessage` lines wrapped. Rejected: none.
  **Round 38 totals: 3364 tests** (177 WindowsBase + 2930 PresentationFramework + 242 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Round 38 follow-up (Opus-verified fixes) — DONE.** **R38C-1** `WriteBack` now runs validation rules on the
  RAW target value before `ConvertBack`/default conversion (WPF `RawProposedValue`; every rule is treated as raw, no
  `ValidationStep` yet), so a string rule on an `int` source sees the text and a failing rule blocks conversion.
  **R38C-2** `Binding.DoNothing` from a converter (single and multi) leaves the target unchanged and `UnsetValue`
  uses `FallbackValue`/the default, both before `TargetNullValue`/`StringFormat`; `SetTargetSafe` never passes either
  sentinel to `SetValue`. **R38C-3** `PriorityBinding` pushes through `SetTargetConverted` (value converted to the
  DP type, invalid → fallback/default) instead of a throwing raw `SetValue`. **R38C-4** the throwing-`ConvertBack`
  `MultiBinding` test was vacuous (never focused); now `…_does_not_escape_LostFocus` plus a PropertyChanged-trigger
  test, both verified to fail without the catch. **R38C-5** `Application.OnWindowActivationChanged` ignores windows
  not in `Windows`, so a window created after the app finished cannot raise `Activated`. **R38C-6** `StartupUri`
  docs and tests now pin WPF-style asynchronous `Shutdown` from a Startup handler (window still created, then
  closed). Open: `DependencyObject.SetValue(dp, UnsetValue)` stores the sentinel instead of clearing (WPF clears).
  **Follow-up totals: 3382 tests** (177 WindowsBase + 2948 PresentationFramework + 242 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 38)**: `SessionEnding` is never raised on GTK; a second `new Application()` does not throw;
  `Application` Activated/Deactivated depend on window-manager `notify::is-active` ordering (verified only through
  the `NotifyActivation` seam); `LostFocus` tests drive managed `Focus()`, not the native focus controller;
  registered defaults are only run through `ValidateValueCallback` (no type check); `Binding` has no
  `ConverterCulture` (conversions use the current culture); metadata `IsAnimationProhibited` is not merged.
- **Round 39 Track B — Style/Trigger Setter values (DONE).** `Setter.Value` objects (`<Setter.Value>` elements, inline
  `ControlTemplate`/`DataTemplate`/`MultiBinding`), markup-extension values (`DynamicResource` live, `StaticResource`
  resolved once, `Binding`/`TemplateBinding`, `x:Static`/`x:Null`), `Setter(dp, value[, targetName])` ctors, and
  `EventSetter` (handler added at apply, removed on unapply). Gaps on GTK: `Setter.TargetName` is store-only;
  EventSetter handlers run in registration order (WPF runs them after instance handlers); no control hosts a template
  root as its widget, so the `Template` setter reaches `Button.Template` and the tree is built, but the widget is
  unchanged. **+41 tests**.
- **Round 39 Track D — Binding markup + validation pipeline (DONE).** `ConverterParameter`/`ConverterCulture`/`Source`/
  `ValidatesOn*`/`NotifyOnValidationError` in XAML and element-syntax `<Binding>` with `ValidationRules`;
  `ValidationStep` (Raw/Converted/Updated/Committed) + `ValidatesOnTargetUpdated`; `INotifyDataErrorInfo`
  (`NotifyDataErrorValidationRule`, `ValidatesOnNotifyDataErrors`); `ConverterCulture` on `Binding`/`MultiBinding`;
  `SetValue(dp, UnsetValue)` clears the local value. No GTK work. Gaps: CommittedValue runs right after UpdatedValue
  (no BindingGroup/IEditableObject cycle); culture falls back to en-US (no `FrameworkElement.Language`);
  `MultiBinding` has no validation pipeline; the INDEI subscription is re-resolved on push/write-back, not on every
  intermediate path-object change. **+52 tests**.
- **Round 39 review (Opus) + fixes — DONE.** Track B (SHIP-WITH-FIXES): **B-F1 (High)** the compiler emitted
  `new RoutedEventHandler(this.H)` for every `<EventSetter>`, so `MouseDown`/`KeyDown`/`MouseDoubleClick` handlers
  failed with CS0123 → `Delegate.CreateDelegate(Owner.XEvent.HandlerType, this, GetType().GetMethod("H", ...))` (binds non-public and contravariant handlers like
  C# method groups; runtime test with private handlers of both shapes). **B-F2 (High)** a style `DynamicResource`
  wrote the dynamic-resource layer, which outranks the style layer: a base-style link beat a derived plain value, it
  replaced a user's own `SetResourceReference`, and `Unapply` deleted it → style-layer contributor
  (`StyleLayerContributor`, `SetStyleResourceReference`/`DropStyleResourceReference`, `ClearStyleValue`); a derived
  style's plain value drops the base link; only `Layer.Local` still uses `SetResourceReference`. **B-F3** trigger
  setter `StaticResource` resolved on `this` instead of the style's owner element → receiver threaded through the
  trigger emitters. **B-F4** style/trigger `Binding` values are local values (engine limitation, not GTK): a trigger
  setter never shows over a style binding, `ValueSource` reports Local, a trigger binding is ignored when the DP
  already has one → documented, pinned by `Trigger_setter_does_not_show_over_style_Binding_known_gap`; layer-targeted
  `BindingExpression` is a backlog item. **B-F5** `Setter.Value = UnsetValue` threw only at apply, after earlier setters
  applied → throws at assignment; `Setter.Property`/ctors validate (`ArgumentNullException("property")`, read-only DP
  `ArgumentException`); the NameProperty check is skipped (no `NameProperty` DP here). **B-F6** `EventSetter` ctor
  uses WPF parameter names, validates the handler type against `RoutedEvent.HandlerType`, and a trigger containing an
  `EventSetter` throws `InvalidOperationException` on attach. **B-F7** nested `Setter.Value` emission restarted from a
  stale counter (duplicate `__svN`/`__eN`) → counter passed by `ref`. **B-F9** `FrameworkElement.ResourceChain.cs`,
  `MultiDataTrigger.State.cs`, `Style.Unapply.cs` extracted. Track D (SHIP-WITH-FIXES): **D-F1 (High)** `Publish`
  replaced the element's whole error collection, so a sibling INDEI binding wiped other bindings' errors and swapped
  the `Validation.Errors` instance on every push → per-element, per-binding error state merged into one stable
  collection (`Validation.SetErrorsFor`); only changed errors raise events. **D-F2** `ClearBinding` left INDEI/step
  errors on the element → `Deactivate` clears them. **D-F3** rule attributes went through the type-blind
  `ClrValueExpression` (`ValidationStep="UpdatedValue"` → string, CS0029) → `RuleAttrExpression` (enum/string/
  decimal/float/double/bool; `ValidationStep` special-cased); the built-in rule types are in `WpfXmlnsMap`. **D-F4**
  rules stop at the first failure and `IDataErrorInfo` runs only when nothing failed (WPF `Validate`).
  **D-F5** binding arguments: `{x:Null}`, `{x:Type}`, leading `{}` escape; `{DynamicResource}` and unknown
  extensions are build errors. **D-F6** `<Binding.Converter>` inline object; unresolved rule types and unsupported
  `Binding.*` property elements are build errors (`XamlCodeGeneratorException`). **D-F7** the push reuses the raw value
  already resolved (the write-back path is still resolved once more). **D-F8** `StringFormat` without braces is `{0:fmt}` (`N2` → `1,234.50`);
  `MultiBinding` without a converter formats the values array (`"{0} {1}"`). The old
  `StringFormat_without_placeholder_prepends_value` test pinned the non-WPF `"fmt{0}"` shortcut and was corrected.
  Rejected in triage: **B-F8** (default/theme style never unapplied when an explicit Style is set): wrong for WPF, where
  the theme style stays a separate lower-precedence layer whose setters/handlers/triggers keep applying; unapplying it
  would drop theme Templates and defaults. The real harm (a default-style `DynamicResource` beating the explicit
  style) was the B-F2 symptom and is gone. Separating theme-style from Style precedence is a larger pre-existing
  design item. **D-F9** (house-rule size of `XamlCodeGenerator.cs` 1180 / `.Binding.cs` 446 lines): not a defect of the
  round; new code was in new partials. Scheduled with the backlog split below.
  **Round 39 totals: 3509 tests** (182 WindowsBase + 3014 PresentationFramework + 298 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs. `gf39/xaml-content-model` and
  `gf39/combobox-itemscontrol` were NOT merged in this round (not part of the merge set that was reviewed); their
  branches and worktrees are kept untouched for a later round.
- **Round 39 follow-up — DONE.** **B-F3** trigger/style-setter `StaticResource` is emitted before the owner is parented,
  so `owner.FindResource` threw for keys in outer scopes (also broke element-level triggers) → emits
  `(owner.TryFindResource(k) ?? this.FindResource(k))`. **B-F1** `CreateDelegate(Type, object, string)` rejects
  contravariant handlers (`(object, RoutedEventArgs)` on MouseDown) → MethodInfo overload; `{init}` in the emit string
  was not interpolated and the old tests missed it. The runtime test never ran the compiler's expression shape before;
  it now does for both handler shapes (the string overload's rejection is pinned). **D-F3** typed rule attributes now
  tested with a real reference assembly; `NaN`/`Infinity` emit `double.NaN`-style constants and `[Flags]` lists emit
  `A | B`. **D-F1** `SetErrorsFor` diffs per error: unchanged errors keep their instance, add-before-remove, events only
  for changes. **D-F6** `<Binding.Converter><StaticResource/></Binding.Converter>` is unsupported (build error; use the
  `Converter` attribute). Style nits fixed. **Totals: 3523 tests** (182 + 3017 + 309 + 15).
- **Carry-over (Round 39)**: backlog: layer-targeted `BindingExpression` (style/trigger bindings as style/trigger
  layer values, B-F4); split `XamlCodeGenerator.cs` and `XamlCodeGenerator.Binding.cs` (house line limit); separate
  theme-style and Style precedence layers (B-F8); `Setter.TargetName` store-only; EventSetter handlers run in
  registration order, not after instance handlers; `<ControlTemplate.Resources>` is not emitted by the compiler;
  trigger `StaticResource` resolves on the owning element first, then `this` (see follow-up);
  CommittedValue has no BindingGroup cycle; `MultiBinding` has no validation; INDEI
  subscription is not refreshed on intermediate path-object changes.
- **Round 40 track — ComboBox as an ItemsControl (DONE, merged; Round 39 track reworked after a BLOCK).**
  `gf39/combobox-itemscontrol` was blocked in the Round 39 review and reworked (store-only `IsDropDownOpen`, scoped inline
  rebuilds, text-search semantics, item logical parents); merged as `e56b5957`. `ComboBoxItem`, inline `Items`, item text,
  `IsDropDownOpen`, `IsEditable`/`Text`. `gf39/xaml-content-model` is **still not merged**: it was not part of the
  reviewed set this round and its worktree is kept untouched for a later round.
- **Round 40 review (Opus) + fixes — DONE.** **C-F1/F2/F3 (Medium)** selection reconciliation only ran with a widget,
  restored by index, and `CreateWidget` relied on a GTK notify that never fires when GTK already shows item 0. Now
  `ReconcileSelection` runs always: the selection follows its item (insert/remove elsewhere shifts `SelectedIndex`, no
  `SelectionChanged`), removing the selected item or `Clear()` gives -1/null/`""` plus one `SelectionChanged`, and an index
  set before the items is kept pending and applied when they arrive. Setting `SelectedIndex` on a realized box GTK already
  shows now syncs `SelectedItem` (and `-1` clears it). **C-F4 (Low)** doc/test corrected: GtkDropDown cannot show "no
  selection" while its model is non-empty (keeps the last shown item), so picking the shown item while `SelectedIndex` is
  -1 fires no notify; documented in the `ComboBox` remarks (no GtkSharp4 signal reconciles it). The old
  "tells GTK no selection" test only proved an empty model and was renamed. **C-F5 (Low)** `IsTextSearchCaseSensitive`
  uses `FrameworkPropertyMetadata`; `IsTextSearchEnabled` summary reworded. **C-F6 (Nit)** long lines wrapped.
  **CI-1 (Medium)** setter `StaticResource` skipped intermediate ancestors' `Resources`, and `TryFindResource` on the
  unparented owner let an app key shadow a closer one → the compiler tracks the enclosing element variables and emits
  `StaticResourceScope.Find(k, this, innermost, ..., outermost)` (scopes innermost-first, then the root, then the
  application; throws like `FindResource`). **CI-2 (Medium)** `EventSetter` handler lookup used `GetType().GetMethod`, which
  never finds a private base-class method once the x:Class is subclassed → `typeof(<x:Class>).GetMethod`.
  Rejected in triage: none.
  **Round 40 totals before follow-up: 3607 tests** (182 WindowsBase + 3099 PresentationFramework + 311 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Round 40 follow-up — DONE.** `SelectedItem = null` (or an item not in `Items`) now sets `SelectedIndex` to -1, so a
  later `Items` edit no longer resurrects the old index as a pending one. A realized box given an out-of-range
  `SelectedIndex` clears `SelectedItem` like the headless path. Long lines wrapped. The `StaticResourceScope` and
  `EventSetter` runtime tests exercise the helper and reflection lookup directly, not generated output; only the
  MarkupCompiler emission tests guard the generator change (there is no compile-and-run harness).
  **Totals: 3611 tests** (182 + 3103 + 311 + 15), all pass.
- **Carry-over (Round 40)**: `Style.Resources` is not emitted, so setter `StaticResource` does not probe a Style's own
  dictionary; a ComboBox with `SelectedIndex` -1 over a non-empty model still shows an item (GTK limitation); out-of-range
  pending `SelectedIndex` is kept as-is until items arrive (WPF coerces); `gf39/xaml-content-model` still to be reviewed and
  merged.
- **Round 41 track — Dispatcher priorities (DONE, merged).** `gf41/dispatcher-priority`: `Dispatcher.BeginInvoke`
  honours `DispatcherPriority` on the GLib main loop through a priority-aware bridge
  (`BeginInvokePriorityAction`, `Application.ToGLibPriority`); Loaded and above stay non-idle. `gf39/xaml-content-model`
  is **still not merged**: it was not part of the reviewed set again and its worktree is kept untouched.
- **macOS CI fix already on main (`e1f9a678`).** Dispatcher work is posted at `G_PRIORITY_DEFAULT`: the Quartz event
  source and frame clock starved idle sources, so `Loaded` never ran on macOS.
- **Round 41 review (Opus) + fixes — DONE.** **B-F1 (Medium)** `BeginInvoke(Action, Inactive)` threw
  `ArgumentException`; real WPF only validates the range there (only `Invoke` rejects Inactive). It is now accepted and
  dropped (this layer returns no operation, so it can never be promoted); out-of-range still throws
  `InvalidEnumArgumentException`. **B-F2 (Low)** `Background` mapped to 100, before `GDK_PRIORITY_REDRAW` (120), so it
  ran before GTK layout/paint; WPF runs it after Render. Now 130 (`GdkPriorityRedraw + 10`), still idle. **B-F3 (Low)**
  assigning `BeginInvokeAction` was ignored once the priority bridge was installed; the last bridge set now wins
  (the setter clears `BeginInvokePriorityAction`), and every place that sets both assigns the legacy one first.
  **B-F4 (Nit)** stale bridge XML docs rewritten. **CI-1 (Low)** `ComboBox.SelectedItem` set to an item not in `Items` is
  rejected as in WPF: old value kept, no `SelectionChanged`, GTK selection unchanged (restored under a guard, not via
  coercion, which this DP engine does not treat as revert); `null` still clears. **CI-2 (Low)** an out-of-range
  `ComboBox.SelectedIndex` on a non-empty list is ignored (previously cleared a selection, or read back 7 with none);
  an empty list keeps the pending index; `SelectedIndex < -1` throws `ArgumentException` via a `ValidateValueCallback`.
  The old `Out_of_range_SelectedIndex_clears_item_like_headless` asserted non-WPF behaviour and was rewritten.
  **CI-3 (Low)** three over-long ComboBox lines wrapped.
  Rejected in triage: Window `ContentRendered` priority not re-audited (no defect: it is driven by GTK map/frame signals,
  not by a `BeginInvoke` priority this branch changed; no concrete divergence); starvation test not mutation-verified
  and macOS Quartz untestable here (a coverage limitation, not a defect; `Input_and_above_is_never_idle` enforces the
  rule and B-F2 keeps Background idle).
  **Round 41 totals: 3641 tests** (190 WindowsBase + 3125 PresentationFramework + 311 MarkupCompiler + 15 ProjectConverter),
  all pass; warning-clean. Failing-first was observed for none of the new tests individually in this wrap-up (fixes and
  tests were written together); they exercise the changed paths.
- **Carry-over (Round 41)**: `ListBox` still deselects on a foreign `SelectedItem` (its `SelectionDefectTests`
  `notInList` test encodes it) - align with WPF in a later round; no `Invoke(Action, DispatcherPriority)` overload;
  `Inactive` work is dropped, not queued; `gf39/xaml-content-model` still to be reviewed and merged.
- **Round 42 (first round chosen from `docs/samples-coverage.md`).** Two fixes were already on main before the round:
  the samples-coverage report (`fedd13c8`: 0/206 portable samples build) and the macOS segfault fix (`581861a2`: the
  dispatcher-priority GTK tests pumped GLib off the GTK thread; `GtkPump` now throws off-thread use and the tests run
  inside `Run(() => ...)`).
- **Round 42 track - desktop framework deps (DONE, merged).** `gf42/desktop-framework-deps`: `Gtk4.Forms.Wpf` depends on
  `System.Configuration.ConfigurationManager` 10.x, replacing what the WindowsDesktop shared framework supplied implicitly
  (`Settings.Designer.cs` compiles). `gf42/xtype-qualified-names` and `gf42/colors-text-gaps` are **not merged**: they
  were not part of the reviewed set this round; both branches are kept untouched for a later review.
- **Round 42 review (Opus) + fixes - DONE.** **A-F1 (High)** CM 10 brings `System.Diagnostics.EventLog` and
  `System.Security.Cryptography.ProtectedData` >= 10.0.0 into the consumer graph, so a lower direct pin (the sample
  `UnhandledExceptionHandling` pins EventLog 4.6.0-preview8) raised NU1605. Under WindowsDesktop these came from the shared
  framework, so no downgrade arose. `CsprojConverter` now raises literal direct pins of those three packages below 10 to
  10.0.0 (attribute or `<Version>` child, case-insensitive; ranges, `$(...)`, missing and >= 10 untouched; textual,
  idempotent). Limitation: `Directory.Packages.props`, property versions and ranges are not rewritten. The first-build
  flow works with a plain `dotnet build` (see the Round 42 follow-up). The D2 audit note "EventLog is fine because CM pulls it in" was wrong: a lower
  direct pin caused NU1605. **A-F2 (Nit)** the two CM version facts were merged
  (`Wpf_package_depends_on_ConfigurationManager_10x`); the nuspec check moved from `Integration.Tests` (not run in CI, skipped
  when no package) into `ProjectConverter.Tests` (ProjectReference for build order; fails instead of skipping).
  Rejected in triage: **A-F3** (not a repository defect: the "consumers float up to 10.0.15" claim exists only in the
  implementer's report; lowest-applicable resolution gives CM 10.0.0, AssemblyVersion 10.0.0.0, the same as
  Microsoft.WindowsDesktop.App.Ref lists).
  **Round 42 totals: 3657 tests** (190 WindowsBase + 3125 PresentationFramework + 311 MarkupCompiler + 31 ProjectConverter),
  all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. A-F1 tests were observed failing first (5 of 13 failed before the fix).
- **Round 42 follow-up (DONE).** The "Verified limitation" (plain `dotnet build` fails restore with NU1605 on the first
  pass) was wrong for the real flow: the unconverted project is still evaluated against WindowsDesktop, so build 1 restores
  cleanly, the converter raises the pin and stops with exit 10, and build 2 succeeds (re-verified on the untouched
  `UnhandledExceptionHandling` sample). NU1605 on restore only arises when a lower pin is added to an already-converted
  project. `IsBelowFloor` now also treats `10.0.0-*` prereleases as below the floor (NuGet orders them below 10.0.0); the
  class comment names the unhandled forms (`PackageReference Update`, single-quoted `Version=`, `VersionOverride`).
  **Totals: 3659 tests** (190 + 3125 + 311 + 33), all pass, 0 warnings; the new prerelease test was observed failing first.
- **Carry-over (Round 42)**: NU1605 for a lower pin added to an already-converted project (restore-time only; not a
  first-build issue); `gf42/xtype-qualified-names`, `gf42/colors-text-gaps` and `gf39/xaml-content-model` still to be
  reviewed and merged.
- **Round 43 track B - xtype-qualified-names (DONE, merged; the Round 42 track B reworked).**
  `gf42/xtype-qualified-names`: `{x:Type X}` / `{x:TypeExtension}` in `TargetType`/`DataType`, plain-string `DataType`
  kept as a string, unparseable type refs throw, layer type names emitted `global::`-qualified.
- **Round 43 track C - colors-text-gaps (DONE, merged; the Round 42 track C reworked).** `gf42/colors-text-gaps`: the full
  Colors/Brushes tables, `TextBlock`/`TextBox` `TextWrapping`, `TextBox` scrolling members and scroll-bar visibility,
  `UIElement.ClipToBounds`.
- **Round 43 review (Opus) + fixes - DONE.** **B-N-A (Medium)** `DataType` emitted every non-`{x:Type` value as a string, so
  `{Binding Foo}` became a literal. `{}Hero` now emits `"Hero"` (escape stripped), `{x:Type}`/`{x:TypeExtension}` stay
  `typeof`, `{x:Static}` emits the member, and any other markup extension (`{Binding}`, `{StaticResource}`,
  `{DynamicResource}`) - see the follow-up for the corrected `{StaticResource}` handling. **B-N-B (Medium)** `WpfXmlnsMap`
  mapped `SpellCheck` but the type did not exist; added a store-only `System.Windows.Controls.SpellCheck`
  (`IsEnabled` default false, `CustomDictionaries` created lazily per element; GTK spell checking is not applied), and the
  sample-compile filter now counts CS0234. **B-N-C (Low)** the `Emitted()` doc promised that a user namespace with a `System`
  segment cannot shadow emitted names; reworded to what it does. **B-N-D (Medium)** the presentation xmlns now also maps
  `Controls.Primitives`, `Documents`, `Input`, `Media`, `Media.Animation`, `Media.Effects`, `Media.Imaging` and `Navigation`
  (no short-name collisions across the 447 public types; `System.Windows.Automation` was missed, added by the follow-up), so `TargetType="RepeatButton"`/`"Paragraph"` emit qualified
  `typeof`. **C-N1** stray trailing CR removed from `TextBox.Widget.cs`, `RichTextBox.Buffer.cs`, `UIElement.cs`. **C-N2**
  `TextGapsTests`/`TextScrollingTests` class summary and over-120 lines wrapped (the box-drawing section comments measure
  under 120 characters and were left). **C-N3** the static `Get/Set{Horizontal,Vertical}ScrollBarVisibility` docs now say
  only `ScrollViewer`, `TextBox` and `RichTextBox` apply the value. **C-N4** `TextBox.AppendText` on the multiline backing
  inserts at the buffer end under the `_updating` guard instead of rewriting the buffer, so the caret survives (the follow-up restores the MaxLength clamp).
  Rejected in triage: none.
  Consequence of B-N-D: `PropertyAnimation.App.xaml` (a `Clean` sample) now gets 5 semantic errors, because the
  `DrawingBrush` it used to drop silently now resolves and the generator emits string literals for `Rect`/enum
  properties (`Viewport`, `ViewportUnits`, `TileMode`) and `AlignmentX/Y` is missing. It was removed from the `Clean` set (restored by the follow-up).
  In-repo sample compile is otherwise unchanged. The official WPF-Samples coverage was **not** re-measured (no checkout
  or harness in this environment); `docs/samples-coverage.md` is still the Round 42 measurement.
  **Round 43 totals: 4331 tests** (190 WindowsBase + 3730 PresentationFramework + 378 MarkupCompiler + 33 ProjectConverter),
  all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. Failing-first was observed for the `AppendText` caret test and the
  RepeatButton/Brush namespace cases; the DataType, SpellCheck and namespace tests were written alongside their fixes.
- **Round 43 follow-up - DONE.** **B-N-A** `DataType="{StaticResource k}"` now emits the resource lookup (`DataType` is a CLR
  property; only `{Binding}` and `{DynamicResource}` still throw). **B-N-B** `SpellCheck.CustomDictionaries` and
  `GetCustomDictionaries` are the non-generic `IList` as in WPF (a `List<Uri>` instance). **B-N-D** `System.Windows.Automation`
  added to the presentation xmlns. **Regression** non-DP CLR properties of struct or enum type now emit a typed conversion
  (`XamlValueConverter.ConvertByType` / enum member) instead of a string, and `TileBrush` gained store-only
  `AlignmentX`/`AlignmentY` (new enums); `PropertyAnimation.App.xaml` is `Clean` again. **C-N4** `TextBox.AppendText` falls
  back to `Text += text` when the result would exceed `MaxLength`, so the clamp and `TextChanged` run.
  **Follow-up totals: 4335 tests** (190 + 3732 + 380 + 33), all pass, 0 warnings.
- **Carry-over (Round 43)**: B-N-C follow-up (emit `global::` on every generated `System.*` reference, then a
  `x:Class="MyApp.System.MainWindow"` compile test expecting zero CS0234; today 7 CS0234; about 110 literal sites);
  `ScrollViewer.*ScrollBarVisibility` not applied to `ListBox`/`DataGrid`; re-measure samples coverage;
  `gf39/xaml-content-model` still to be reviewed and merged.
- **Pre-round CI fixes (already on main).** `d5100042` CI packs `Gtk4.Forms.Wpf` before `PackedNuspecTests` so the test
  checks the real package; `39c894b5` forward-slash `PackagePath`s so packing works on Linux/macOS (NU5129).
- **Round 44 track A - content-model (DONE, merged).** `gf44/content-model`: the XAML content model honours
  `[ContentProperty]` for element children and text (`Border.Child`, `ListBox.Items`, `TextBlock.Inlines` with `Run`
  text, `Button.Content`, transform/geometry/gradient collections, `FlowDocument.Blocks`), with a name-keyed fallback when
  no reference is available; 410 MarkupCompiler tests at merge. Samples evidence: the `.Content` artefacts listed in
  `docs/samples-coverage.md` are generator output, not API gaps; the in-repo sample compile is the evidence (no
  official-samples re-measure).
- **Round 44 track B - startup-robustness (DONE, merged).** `gf44/startup-robustness`: string to `Transform` /
  `ImageSource` / `ICommand` conversion, `LengthConverter` units (`px`/`in`/`cm`/`pt`/`Auto`), and a `Page` named by
  `StartupUri` hosted in a `NavigationWindow`. Samples evidence (corrected by the review): of the 6 targeted samples
  (DropShadow, ComplexLayout, LocalizationResources, HandlingEventOnCommand, TargetValues, MultiPage) **3 are fixed**;
  TargetValues and MultiPage are **still hangs**. Not re-measured against the official checkout here.
- **Round 44 track C - event-surface (DONE, merged).** `gf44/event-surface`: `RoutedEventHandler` for
  `Checked`/`Unchecked`/`Indeterminate`/`Click`, `ButtonBase`/`ToggleButton` hierarchy, `RadioButton : ToggleButton`,
  routed `ButtonBase.Click`; 14 tests at merge.
- **Round 44 review (Opus) + fixes - DONE.** **C-F1 (Major)** `CheckBox`/`RadioButton` raised `Click` only from
  `Gtk.CheckButton.Activate`, which GTK 4 does not emit for a mouse click; both now raise it from `Toggled` through
  `ToggleButton.RaisesClickOnToggled(active)` (after `Checked`; a radio only when it becomes active; programmatic
  `IsChecked` raises none; the plain `ToggleButton` keeps `Clicked` so it does not double-raise). GTK ignores a click on an
  already-active radio that shares a GTK group, where WPF raises `Click` (documented). **C-F2** `OnToggle` is `protected virtual`.
  **C-F3** ungrouped `RadioButton` peers come from the visual parent's children, falling back to the logical parent
  when the visual parent does not list the button; here `LogicalParent` and `VisualParent` are the same object, so the
  two paths agree except where logical children omit visual ones (see the follow-up). **C-F4** the
  `OnClick` test pins `["OnClick", "handler"]`; the radio tests moved to `RadioButtonEventTests.cs`. **C-F5** blank lines
  in `MenuItem.cs`. **A-F1** `InlineUIContainer` (store-only; rendering inside text not implemented) now exists, so the
  `Inlines` wrap is emitted unconditionally for a `UIElement` child. **A-F2** `MenuItem.Items` is `IList<object>`: a
  `MenuItem` is an item, a `Separator` is a `Gtk.Separator` in the popover box, other objects are store-only (the menu is a
  `Gtk.Box` popover, not a `Gio.Menu`). **A-F3** content-model tests assert `.Children.Add(`/`.Figures.Add(` and
  `.Document = `; `SimpleBinding`/`InvokeTarget` joined `Clean`; `HeightProperties` asserts its exact residual
  diagnostics (5 missing code-behind handlers). **A-F4** whitespace next to `LineBreak` (or any
  `[TrimSurroundingWhitespace]` type) is trimmed. **A-F5** `xml:lang` is a directive (dropped until the layer has
  `FrameworkElement.Language`). **A-F6** `[ContentProperty(Name = "...")]` is read and the reflection helpers no longer
  throw. **A-F7** the wrap condition fits 120 columns. **B-F1 (High)** a `Page`/`Window` XAML with no `x:Class` gets a
  generated `internal sealed partial class <File>` in the project root namespace with an `[assembly:
  GtkFormsXamlRegistration("<path>", typeof(...))]` entry; `CreateWindowFromStartupUri` asks `GtkFormsXamlRegistry`
  (explicit factories, then the assembly attributes; exact path, then leaf name) before the `x:Class` name lookup.
  **B-F2** a root-relative image path (`/Images/a.png`) stays relative instead of becoming `file:///` on Unix and
  resolves under `AppContext.BaseDirectory`. **B-F3** `ParseTransform` tokenizes like WPF (one comma or whitespace; an
  empty, missing or extra token is `InvalidOperationException`, a non-number `FormatException`); `Identity` is
  `Transform.Identity`. **B-F4** `CommandConverters` is case-sensitive, `""` is null, whitespace or a namespace-qualified
  name throws `NotSupportedException`, and the search order is WPF's. **B-F5** `1,000` parses to 1000 for lengths and
  doubles. **B-F6** `NavigationWindow` no longer dereferences a null `WindowTitle`. **B-F7** `ImageBrush` and
  `Window.Icon` use `ImageSourceLocation.ResolveUri` (relative and `pack://application` paths resolve under the base
  directory; the old relative-icon test now expects that full path). **B-F8** `XamlValueConverter.Units.cs` renamed
  `UnitConverters.cs`; `pack://application:,,,` is normalised so `UriSource` does not round-trip the original string.
  Rejected in triage: none.
  **Round 44 totals: 4455 tests** (190 WindowsBase + 3811 PresentationFramework + 421 MarkupCompiler + 33
  ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
- **Round 44 follow-up - DONE.** **C-F3** now has a failing-first test (a panel whose logical children omit its visual
  children; the logical-only code left the radios non-exclusive); a `UIElement` item in this layer's `ItemsControl` sits
  in a `ContentPresenter`, so ungrouped radio items there are not exclusive, unlike WPF (carry-over). **B-F1**
  `StartupUri` resolves exact registry path, then `x:Class` name, then leaf name, so an `x:Class`-less page in another
  folder no longer wins; absolute URIs are unescaped before matching (`My%20Page.xaml`). **C-F1** an ungrouped radio
  clicked while checked raises `Click` and stays checked (named groups: GTK still swallows it, documented). **A-F5**
  `xml:lang` also compiles through Roslyn via the `XmlLang.MainWindow.xaml` Clean sample (the directive test itself cannot
  fail-first). **Follow-up totals: 4461 tests** (190 + 3816 + 422 + 33), all pass; build 0 warnings.
- **Carry-over (Round 44)**: two generated Page/Window classes with the same file name in different folders collide in
  the root namespace; `NavigationService` does not yet use `GtkFormsXamlRegistry` for `NavigateUri`; the C-F3 visual-peer
  path was untested (now covered, see the follow-up); `xml:lang`/`FrameworkElement.Language` missing; `ItemsControl` wraps a
  `UIElement` item in a `ContentPresenter` although `IsItemItsOwnContainerOverride` says it is its own container (WPF: an
  ungrouped radio item is then exclusive with its siblings; only wrapped data items are not);
  `MenuItem` is still a `Gtk.Box` popover (no `Gio.Menu`/`Gtk.PopoverMenu`); official samples coverage not re-measured.
- **Round 45 track A - generator-v2 (DONE, merged).** `gf45/generator-v2` (merge `50731e8b`): public
  `FrameworkElement.AttachElementTriggers` hook, `Uri`/`ImageSource`/`PointCollection` CLR conversion, `x:Class` base
  type and constructor when there is no code-behind, keyed resource objects (user types, `XmlDataProvider`,
  `ObjectDataProvider`, `SolidColorBrush`) and parse-time `{StaticResource}`; 4487 tests at merge. Samples evidence: the
  generator classes in `docs/samples-coverage.md` (no base type, `AttachElementTriggers`, `CS0029`, resource
  placeholders) are the targets; the two Round 44 regressions (TargetValues, MultiPage) were the trigger. Not
  re-measured per track; the end-of-round gate below shows no regression.
- **Round 45 track B - items-data-dialog (DONE, merged).** `gf45/items-data-dialog` (merge `f7938bd5`):
  `Selector.IsSynchronizedWithCurrentItem`, `CollectionView`/`ListCollectionView`, `ValueConversionAttribute`,
  `BindingExpressionBase.UpdateSource`, `DataTransferEventArgs`, `Button.IsDefault`/`IsCancel`, settable
  `Window.DialogResult`, `HeaderedContentControl`. Samples evidence: closes the `Selector.IsSynchronizedWithCurrentItem`
  and `ValueConversion` gaps listed under "Real gaps" in `docs/samples-coverage.md`. Not re-measured per track.
- **Round 45 track C - long-tail-members (DONE, merged).** `gf45/long-tail-members` (merge `4c6d28e9`): `SpellCheck`,
  `TextBox` clipboard, `Grid.IsSharedSizeScope`/`ShowGridLines`, definition actual sizes, `HasContent`, `IsItemsHost`,
  `IAddChild`, `MessageBox` overloads, `Window.ShowActivated` and other single members that alone block apps. Not
  re-measured per track.
- **Round 45 review (Opus) + fixes - DONE.** **A-F1 (High)** a keyed resource whose type is not a `DependencyObject`
  (`SolidColorBrush.Color`) went through `SetValue`/`FindResource`, which does not compile; a `{StaticResource}` now
  assigns the CLR property through `XamlValueConverter.CoerceResource` (a `<Color>` resource is stored as text), and a
  `{DynamicResource}`/binding on a non-DP keeps the plain emission. **A-F2** attributes of a user type the resolver
  cannot see (every type in the project itself) go through the layer hook `XamlValueConverter.SetClrProperty`/`SetClrValue`
  instead of guessing a literal type (`EmployeeNumber="57304"` was `CS0029`). **A-F3** `ObjectDataProvider`
  `MethodParameters`/`ConstructorParameters` emit `typeof` for `x:Type`, the member for `x:Static`, `null` for `x:Null`;
  one parameter that cannot be emitted keeps the plain emission for the whole provider. `ObjectDataProvider` now invokes a
  static method without constructing the type (`Enum.GetValues(typeof(DayOfWeek))` yields 7) and picks the overload
  by parameter types (follow-up; first by count). **A-F4** the code-behind scan skips files under the output directory
  (a stale `.g.cs` dropped the base type and constructor). **A-F5** `DetachElementTriggers` is `internal` again.
  **A-F6** `ParsePointCollection` tokenizes like WPF
  (one comma or whitespace; `1,,2` is `FormatException`, a missing y `InvalidOperationException`). **A-F7** the
  ancestor-resources-visible-regardless-of-order leniency is documented on the generator and pinned by tests (a
  same-dictionary forward reference stays in document order). **A-F8** the `[ThreadStatic]` scope list and the static
  binding prefix map are gone; `StaticResourceLookup` and its callers are instance methods. **A-F9** the base-type and
  constructor logic moved to `XamlCodeGenerator.CodeBehind.cs`; over-long lines wrapped. **B-F1** a `Button` override of
  `OnClick` dismisses the dialog (`DialogResult = false`) for `IsCancel` without a command. **B-F2** currency sync ignores
  the selection clears of an item rebuild (`ItemsControl.IsRebuildingItems`), then re-selects the view's current item;
  switching `ItemsSource` no longer moves the old view. **B-F3** a window `Gtk.EventControllerKey` (capture phase, only when nothing has focus; a focused element's own `KeyDown` runs first and the window clicks only if it was left unhandled)
  routes Return/KP_Enter/Esc to the default/cancel button (not a multi-line `TextBox`/`RichTextBox`, not a focused
  button for Enter). **B-F4/B-F11** `DataTransferEventArgs` is in `System.Windows.Data`, `GroupDescription` in
  `System.ComponentModel`; `SortDescriptionCollection.Empty` is read-only. **B-F5** `ValueConversion` allows multiples.
  **B-F6** a binding path resolves against the current item of the default view (`/`, `X/Y`, `/Y`, and a step not found
  on a collection) and re-evaluates on `CurrentChanged`; an empty path keeps the collection. **B-F7** `DialogResult` follows
  WPF (a `Closing` handler can set it; `ShowDialog` returns `false`, not null, when closed without a result). **B-F8**
  `Selector` has no abstract internal member; an unset flag syncs only a non-default view (and a single-selection `ListBox`).
  **B-F9** `GetDefaultView(null)` is null and a string has a view over its characters. **B-F10/C-C6** `ContentSource` and
  `ShowActivated` are dependency properties. **B-F12** `IsDefaulted` needs a window that holds the focus;
  `ItemsControl.ItemsSource` moved to a partial; `CollectionViewTests` split. **C-C1** a second `ContentControl` child is
  `InvalidOperationException`. **C-C2** `TextElement.Background` is an instance property (stored only; `TextElement` is not
  a `DependencyObject` here, so the registered DP is not its backing store). **C-C3** `TextBox.Copy`/`Cut`/`Paste` read the
  live widget selection. **C-C4** `HasContent` is get-only. **C-C5** `SpellingReform` values match WPF. **C-C7**
  `SpellCheck.GetSpellingReform` is internal. **C-C8** `Panel.AddText` ignores blank text; `AddChild` on an items host throws.
  Rejected in triage: none. Failing-first was done by writing the test with the fix for the generator and layer hooks;
  the pre-fix failure was not re-run for each test.
  **Round 45 totals: 4610 tests** (190 WindowsBase + 3935 PresentationFramework + 452 MarkupCompiler + 33
  ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
  **Samples gate (after the fixes, Gtk4.Forms.Wpf packed from the fixed tree):** all 60 `build=pass` projects of
  `docs/samples-coverage.json` (59 portable apps plus 1 Windows-only) build through the two-build recipe, and all 47
  `run=runs` apps stay alive for 10 s (process alive; the window itself was not re-checked).
- **Round 45 follow-up (DONE).** **B-F3** Return/Esc no longer pre-empt the focused element: the window controller acts only with no focused element, otherwise the focused element raises `PreviewKeyDown`/`KeyDown` first and the default/cancel button is clicked only if it left them unhandled; `RichTextBox` and an open `ComboBox` keep Enter/Esc; tests drive the window's real key controller. **B-F6** write-back and validation owner resolve the current item when the path is a property of the collection's items (`Text="{Binding Name}"` on a master/detail collection). **A-F3** `ObjectDataProvider` picks the overload whose parameter types fit the arguments (`Math.Max` with two doubles), falling back to the first with the same count. **B-F1** dialog tests have a 5 s watchdog that closes and fails instead of hanging. **B-F2** the new view is attached before the items rebuild, so an `ItemsSource` switch never selects (or pushes to a `SelectedItem` binding) the old view's current item. **A-F1** `{StaticResource}` on a real DP with a CLR-only name (`Content`, `Text`, `Tag`) keeps `SetValue`; only non-`DependencyObject` owners assign the CLR property. **A-F6** the `1,,2` assertion now uses `1,,2 3 4` (the old `1,,2 3` threw before the fix too); whether WPF `TokenizerHelper` reports an empty token as `InvalidOperationException` rather than `FormatException` is unverified (no reference source here), so `FormatException` stays. **B-F12** the 125-column line is wrapped. **B-F7** a dialog closed without a result reads `DialogResult == false`. **C-C2 (pre-existing)** the generator emits a CLR assignment for attributes of a resolvable non-`DependencyObject` owner (`Run Background/Foreground` compile; `Brush` joined the converter-backed classes). Not fixed: `DataGrid` has no edit mode here, so Enter/Esc in a cell edit are not yet protected from the buttons.
  **Follow-up totals: 4619 tests** (190 WindowsBase + 3942 PresentationFramework + 454 MarkupCompiler + 33 ProjectConverter), all pass; build 0 warnings. **Samples gate:** all 60 `build=pass` projects build through the two-build recipe from the packed fixed tree; all 47 `run=runs` apps stay alive 10 s (40 with a main window handle, 7 alive without one).
- **Carry-over (Round 45)**: `TextElement` is not a `DependencyObject`, so `Background` is not DP-backed;
  `PathWatcher` (DataTrigger paths) still splits on `.` only; GTK's own `Entry` activate may still fire alongside a
  default-button click; a focused element
  in another window is treated by `Keyboard.FocusedElement` only; `docs/samples-coverage.md` is still the Round 44
  measurement.
- **Round 46 track A - generator-v3 (DONE, merged).** `gf46/generator-v3` (merge `ab58191b`): media resource objects
  (`DrawingBrush`, `DrawingGroup`, gradients) with nullable CLR conversions, `{StaticResource {x:Type}}` and nested
  `x:Type` keys, per-template `x:Name` scopes, attached routed-event syntax (`Button.Click` on a panel), own-type
  qualified properties, a missing base type, `PreviewMouseDown`/`PreviewMouseUp`, `Localization.*` attributes ignored,
  space-separated `Rect`/`Point`/`Size`, data providers loaded on first `Data` read. Samples evidence: targets the
  name-scope (10 apps), routed-delegate (6 apps) and root-base-type (3 apps) classes of `docs/samples-coverage.md`;
  the end-of-round gate below is the only re-measure (all build, none stopped running).
- **Round 46 track B - startup-runtime (DONE, merged).** `gf46/startup-runtime` (merge `ce09926c`): string to `Uri`
  on the DP converter path (`Frame.Source`), routed `Selector.SelectionChanged` held back during `BeginInit`/`EndInit`,
  `SelectionChangedEventHandler`, `ResourceDictionary.Source` with merged and localised dictionaries, `.resx` names,
  exceptions that escape GTK callbacks routed to `Dispatcher.UnhandledException` and printed; 25 tests at merge.
  Samples evidence (measured on `a37d3b17` plus this branch): Linq and OpacityMasking now run (76 apps run, was 74),
  DragDropEvents still crashes but now says why; no regression.
- **Round 46 track C - input-focus (NOT merged).** `gf46/input-focus` (5 commits, tip `4ca99c69`, based on `a37d3b17`):
  keyboard-focus events, `MoveFocus`/`PredictFocus`, `KeyConverter`, `RangeBase.ValueChanged`, `EditingCommands`,
  `Cursors`, and a rule that Enter/Esc click the default/cancel button regardless of focus. It stays on its branch (no
  merge decision or review verdict is recorded in the repository): it rewrites the Enter/Esc controller rule that
  Round 45 B-F3 and `DialogSemanticsTests` pin and touches 45 files. Re-review it against the fixed `main` before merging.
- **Round 46 review (Opus) + fixes - DONE.** **A-F1** an attached routed event whose owner has no CLR event takes its
  delegate type from the static `Add{Name}Handler` (`Validation.Error` is `EventHandler<ValidationErrorEventArgs>`, was
  `RoutedEventHandler`, `CS0123`); generic delegate types are formatted `global::Ns.Name<global::Arg>`. **A-F2** an
  `x:Class` root always declares the root type as base, as in WPF (a code-behind that only lists interfaces left the
  class deriving from `object`); the same base repeated across partials is legal, a different one is `CS0263`; the tests
  that pinned the omission were rewritten. **A-F3** `{StaticResource}`/`{DynamicResource}` attributes convert in media
  graphs (assigned with `as` the property type; only plain and `{x:Type}` keys resolve, `{x:Static}` keys keep the fallback; `DynamicResource` is resolved once with a non-throwing lookup),
  and `GradientStopCollection` (keyed collection), `DrawingImage` (new, `Image` does not paint it), `PathGeometry`
  `Figures` markup (new `PathFigureCollection.Parse`; `Figures` is now settable) and a `Transform` element are built;
  constructors whose parameters are all optional count as instantiable; remaining fallbacks (`Color`,
  `CollectionViewSource`, `HierarchicalDataTemplate`) are in `docs/samples-coverage.md`. **A-F4** store-only
  `GradientBrush.MappingMode`/`SpreadMethod` (new `GradientSpreadMethod`), `TileBrush.Viewbox`/`ViewboxUnits` and
  `GeometryGroup`/`GeometryCollection` (`FillRule` is the existing `Shapes.FillRule`, honoured by the follow-up); a
  media property, property element or child the layer lacks is reported through `XamlCodeGenerator.Messages` and logged
  as a build message, not dropped silently. **A-F5** an implicit page whose derived name a hand-written class owns
  (same namespace; comments and strings ignored) is generated as `__Xaml_<path>` and the URI registered to it, instead
  of mapping the URI to the unrelated class (`RegistrationOnly` is gone). **A-F6** three over-long doc lines wrapped.
  **B-F1** the generated `InitializeComponent` wraps the body in `try`/`catch` and calls the new
  `FrameworkElement.AbandonInitialization` (`EditorBrowsable.Never`), which ends the root's init scope; a throw no longer
  leaves the thread's scope open (the generator uses a pattern match, so a non-`FrameworkElement` root still compiles).
  **B-F2** `InitScope` keeps the roots that are initialising and queues per root: `SelectionChanged` is held back only
  for a selector whose own `BeginInit` is open, or whose creating root is, and delivered at the earliest of its own or
  that root's `EndInit`; one event per change, none merged, none for an empty change; sets after a nested root ended are
  synchronous (WPF is synchronous throughout; the remaining deferral is a documented divergence). Rewrote the tests that
  pinned the global deferral. **B-F3** `InitScope.Exit` runs every queued action and rethrows the first exception.
  **B-F4** `XamlValueConverterUriTests` builds its `Frame` on the GTK thread. **B-F5** `Selector` remarks fixed; the
  internal `SelectionChangedEventArgs.OldValue`/`NewValue` are gone (three tests did use them). **B-F6** `ParseUri`
  keeps `OriginalString`; only the `pack://application:,,,` form, which .NET cannot parse, is normalised (pinned); a leading `/` stays relative (see the follow-up).
  **B-F7** `ResourceDictionary.Source` throws `ArgumentException` for null and empty. **B-F8** `OnGlibUnhandled` unwraps
  `TargetInvocationException`. **B-F9** the intermittent `Return_through_the_window_key_controller...` failure did not
  reproduce here (0 of 8 runs before Round 46, 0 of 12 on `ce09926c`, 0 of 10 consecutive runs after the fixes), so the cause is not
  proven; the code allows one: `Keyboard.FocusedElement` is process-wide and any element focused in another window (or
  left by an earlier test) made the window's Enter/Esc controller decline. Enter/Esc now consider only focus inside the
  window (`Window.FocusedHere`), with a failing-first test that leaves a focused element in another window.
  Rejected in triage: none.
  **Round 46 totals: 4743 tests** (190 WindowsBase + 3994 PresentationFramework + 525 MarkupCompiler + 34
  ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
  **Samples gate (after the fixes, Gtk4.Forms.Wpf packed from the fixed tree):** all 84 `build=pass` projects of
  `docs/samples-coverage.json` (83 portable apps plus 1 library) build through the two-build recipe, and all 76
  `run=runs` apps stay alive for 10 s (process alive; the window itself was not re-checked; the task brief said 74, the
  JSON already records 76 after startup-runtime). The first pass caught a regression: Linq died because a set before the
  items existed raised an empty `SelectionChanged`; empty changes are now dropped, and Linq stays up.
- **Round 46 follow-up (DONE).** `ParseUri` keeps a leading `/` (not `//`) relative: .NET parses it as `file:///` on
  Unix, so `Frame.Source="/Pages/P.xaml"` was absolute there (tests). Media `{StaticResource}`/`{DynamicResource}` take
  only plain and `{x:Type}` keys (an `{x:Static}` key emitted a lookup that threw at startup; it keeps the fallback
  again); `DynamicResource` uses `TryFindResource`, so a missing key is `null` as in WPF. `Window.FocusedHere` treats an
  element with no window (inside a `Popup`/`ContextMenu`) as focus here, so Enter/Esc typed in it no longer click the
  default/cancel button. `GeometryGroup` now applies its `FillRule` (the remarks wrongly said Cairo defaults to
  even-odd; it is winding) and `EllipseGeometry` starts a new sub-path, so grouped ellipses are not joined by a line.
  Spec item (c) of B-F1 (a runtime test of a generated root throwing in `InitializeComponent` while a later `ComboBox`
  raises at once) was not added: the generator tests check order and compilation, and
  `AbandonInitialization` is tested directly. Two double blank lines removed.
  **Totals: 4749 tests** (190 WindowsBase + 3997 PresentationFramework + 528 MarkupCompiler + 34 ProjectConverter),
  all pass, 0 warnings.
- **Carry-over (Round 46)**: `gf46/input-focus` is unmerged; the B-F9 flake is not reproduced, only closed by the
  window-scoped focus rule; `GeometryGroup`, `DrawingImage` and the gradient/tile members are store-only for rendering;
  `Selector` still defers events for selectors created in a bracketed root (WPF defers only a selector's own
  `BeginInit`); `SelectedIndex` set before items exist stays -1 (WPF keeps it and selects the first item when they
  arrive); `FillRule` lives in `System.Windows.Shapes` here, not `System.Windows.Media`;
  `docs/samples-coverage.md` is the `a37d3b17` measurement.
- **Round 47 pre-round commits on main.** `1b50a528` derives the implicit class name OS-independently (backslash paths
  on Linux/macOS; Linux/macOS CI), and `2c67b72c` re-measures the samples on the pinned feed: 89 of 204 portable apps
  build and 83 run (earlier unpinned runs verified to have used the local feed).
- **Round 47 track A - generator-v4 (DONE, merged).** `gf47/generator-v4` (merge `d78faffa`): `Grid.RowDefinitions`/
  `ColumnDefinitions` emitted, any other dropped property element reported as a build message, `HierarchicalDataTemplate`,
  `CollectionViewSource` and `AlternationConverter` resources built as objects, merged `ResourceDictionary` files
  compiled, registered and loaded at parse time, `Main` for a custom `Application` root. Samples evidence (gate on
  the branch, pinned feed): all 90 build, 86 of 89 apps run (was 83); DragDropEvents, StringLocalizationSample and
  AlternatingAppearanceOfItems crash to runs, CustomApplicationClass fail to runs.
- **Round 47 track B - flow-direction (DONE, merged).** `gf47/flow-direction` (merge `5f92acd0`): the `FlowDirection`
  enum, inherited `FrameworkElement.FlowDirection` applied to GTK text direction, `Inline`/`Block`/`FlowDocument`
  `FlowDirection` and `Language`, `XmlLanguage`, `NumberSubstitution` (store-only). Samples evidence: the 7
  only-flow-direction apps plus FlowContentProperty and NumbersMultipleLangauges build and run; all 90 still build.
- **Round 47 track C - input-focus (NOT merged; the Round 46 track C reworked).** `gf46/input-focus` is now 6 commits
  on `2c67b72c`, tip `a4876520` ("Rework input and focus after Round 46 review"). It was not merged: it is not rebased
  onto `main` (generator-v3/v4 and flow-direction overlap) and no review of the rework is recorded, so it still needs
  the re-review against the fixed `main` that the Round 46 note asks for.
- **Round 47 review (Opus) + fixes - DONE.** **B-F1** an `x:Class` `ResourceDictionary` is registered too (a note and no
  registration when its code-behind has no public parameterless constructor); `GtkFormsXamlRegistry` parses
  `/Asm;component/path` and `pack://application:,,,/Asm;component/path` (named assembly only, exact then leaf; with no
  assembly the given one, then every loaded assembly with registrations, exact only); generated code loads
  `Source`/merged dictionaries through the non-throwing `GtkFormsXamlRegistry.TryLoadDictionary` (a string overload,
  because `new Uri("pack://application:,,,/...")` itself throws at startup); the public `Source` setter still throws.
  No build message is emitted for an unresolvable `Source` (the generator cannot tell statically). **B-F2** implicit
  dictionary classes are named from the project-relative path (`Themes_Dark_Colors`), pages keep the stem, and
  `MarkupCompileTask` mirrors the item's relative directory under the output directory, skips duplicate items and takes a
  `ProjectDirectory` so rooted items key by relative path. **B-F3** `ObjectDataProvider` parameter lists are no longer
  reported as dropped. **B-F4** `GridLength` text accepts `px`/`in`/`cm`/`pt` (new `LengthUnits` in WindowsBase; the
  PresentationFramework length parsers keep their own, richer parsing). **B-F5** a `CollectionViewSource` used as a
  binding source (single, multi and priority bindings, from `Source`, `DataContext`, `ElementName` or `RelativeSource`)
  binds its `View`; a later `Source` change is seen on the next resolve only (the source raises no notification).
  **B-F6** Roslyn-compile tests over real DragDropEvents, StringLocalization and CustomApplicationClass XAML, and a
  real-GTK test of a window whose `InitializeComponent` the markup compiler generated at build time (the test project
  now imports the compiler targets). **B-F7** `Main` is generated only for an `ApplicationDefinition` item (new
  `GFItemType` metadata and `XamlCodeGenerator.ItemType`; `null` keeps the old behaviour) and suppressed only when the
  class itself declares `Main`. **B-F8** generator notes log at `High`. **B-F9** brace indent, `EmitResourceItems` moved
  to a partial (main file 1228 lines), `AlternationConverter` moved to `Controls/` and made WPF-exact (negative
  index wraps, `ConvertBack(null)` matches a null entry). **B-F10** `docs/samples-coverage.md` corrected. **C-F1** each
  paragraph, `Section` child and list item line carries its own directional mark (U+200E/U+200F) plus isolate, a block's
  explicit direction wins, containers no longer wrap isolates across separators. **C-F2** `FlowDirection` is applied to
  every widget a control owns (walk stops at another element's backing widget, tracked by a new widget-owner table);
  `StackPanel`/`DockPanel`/`Grid` mirror, `Canvas` does not. **C-F3** `NumberSubstitution` and `XmlLanguage` are
  unsealed, `NumberSubstitution` gains the constructors and instance properties (default `CultureSource` is `Text`, as
  reflected from the real `PresentationCore`, not `User` as the brief said) and `Equals`; `XmlLanguage.GetLanguage`
  validates RFC 3066. **C-F4** a `TextBlock` label is prefixed with a directional mark. **C-F5** `XmlLanguage` is
  converter-backed. **C-F6** `IsWidgetMaterialized` removed, bidi characters named in `BidiMarks` (the constants were still raw invisible characters until the Round 48 follow-up turned them into escapes). **CI-1**
  `{DynamicResource}` on a media property probes the enclosing parse scopes through the new `StaticResourceScope.TryFind`.
  **CI-2** the outermost `GeometryGroup` `FillRule` wins. Not reproduced as specified: CI-1's `Rectangle.Fill` scenario
  does not go through the media path; a DP-level `{DynamicResource}` still emits a one-shot `FindResource` (pinned by
  `G5_DynamicResource_keeps_FindResource`), so the test uses `GeometryDrawing.Brush`. Rejected in triage: none.
  **Round 47 totals: 4879 tests** (201 WindowsBase + 4075 PresentationFramework + 569 MarkupCompiler + 34 ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
  **Samples gate (after the fixes, Gtk4.Forms.Wpf packed from the fixed tree):** all 90 `build=pass` projects of `docs/samples-coverage.json` build through the two-build recipe and all 83 `run=runs` apps stay alive with a window for 10 s (two showed as hangs in the serial pass and run when re-run alone), plus DragDropEvents, StringLocalizationSample, AlternatingAppearanceOfItems, CustomApplicationClass and LangAttribute build and run; no regression.
- **Carry-over (Round 47)**: `gf46/input-focus` is unmerged (above); a DP-level `{DynamicResource}` on an unparented
  element still throws at `InitializeComponent` when the key lives on an ancestor in the same XAML; a
  `CollectionViewSource.Source` change is not re-resolved by existing bindings; the dictionary class name mangling can
  still collide for `A_B/C` and `A/B_C`; `docs/samples-coverage.json` is the `1b50a528` measurement (stale); the XAML-backed
  grid test pins the second row's widget at Y 34 while `RowDefinition.ActualHeight` is 40: the `Gtk.Grid` backing
  treats pixel rows as Auto (see the follow-up).
- **Round 47 follow-up.** **B-F1** an `x:Class` dictionary whose code-behind declares no constructor now gets the
  generated `public X() { InitializeComponent(); }` (it was registered but created empty); a constructor without a
  modifier (private) now counts as a declared non-public one, so the dictionary is not registered and
  `Activator.CreateInstance` cannot throw out of `TryLoadDictionary`. **B-F2** `MarkupCompileTask` skips only the
  identical item spec; two different files that map to one output path are now an error naming both. The `A_B/C` vs
  `A/B_C` class-name collision stays open. **B-F6** `AlternatingAppearanceOfItems` is a Roslyn compile test; the grid
  test now pins the real gap: the `Gtk.Grid` backing treats pixel rows as Auto, so row 1 starts at the first button's
  natural height (34), not at the 40px track (the assert flips when pixel rows are honoured). **B-F10**
  `samples-coverage.md` history row for generator-v4 is 90 / 87. **C-F1/C-F4** `BidiMarks.AppendIsolated` closes and
  reopens the isolate (with the directional mark) at every `'\n'`, so text after a `LineBreak`, in a nested
  `Section`/`List` inside a `ListItem`, or in a `Span` with its own direction keeps its direction; `TextBlock` marks
  every line of multi-line text. `ListItem.FlowDirection` was added and a list item's own direction wins over the
  list's. **C-F6** `BidiMarks` constants were meant to be `\u` escapes (done in the Round 48 follow-up).
  **Follow-up totals: 4889 tests** (201 WindowsBase + 4080 PresentationFramework + 574 MarkupCompiler + 34
  ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
  **Samples gate (Gtk4.Forms.Wpf packed from the fixed tree, pinned feed, hashes equal the local `Packages/`):** all 90
  `build=pass` projects build through the two-build recipe (12 needed a third build after a parallel restore race) and
  all 83 `run=runs` apps stay alive with a window for 10 s (three showed as hangs in the first pass and run when re-run
  alone, one was skipped by the script and run alone); no regression.
- **Round 48 track A - input-api (DONE, merged).** `gf48/input-api` (merge `ecf1d70d`): the input API surface split out
  of `gf46/input-focus`: keyboard-focus events, `MoveFocus`/`PredictFocus`, `KeyConverter`, `EditingCommands`, the full
  `Cursors` set, `ContextMenuOpening`/`Closing`, a routed `RangeBase.ValueChanged`, `Slider` commands, auto-tooltip and
  `Ticks`. Samples: CursorType, CustomRoutedCommand, EditingCommands, KeyStrokeCounter, ProgrammaticFocusControl and
  CommandSourceControlWithDispatcherTimer build and run; ContextMenuOpening, FocusVisualStyle and
  ICommandSourceImplementation build and do not run (see `samples-coverage.md`).
- **Round 48 track B - graphics-window-tail (DONE, merged).** `gf48/graphics-window-tail` (merge `5ebf5285`): `Vector`,
  `Point` arithmetic, `Matrix`, `GetLocalValueEnumerator`, `UIElement.Clip` (native bounds clip, exact in `Shape`),
  `Page.Background`, `TextBlock` attached accessors, `SystemColors` keys, `Window.DragMove`/`AllowsTransparency` and
  path-markup conversion to `Geometry`. Unblocks SystemBrushesAndColors, RestoringDefaultValues, CustomWindowUI,
  NonRectangularWindow and part of 9 apps that read `Page.Background`.
- **Round 48 track C - threading-dialogs-items (DONE, merged).** `gf48/threading-dialogs-items` (merge `f585c6c8`):
  `DispatcherObject` on `DependencyObject`, `DispatcherOperation`, frames, shutdown and per-thread queues;
  `Microsoft.Win32` `CommonDialog`/`FileDialog`/`OpenFileDialog`/`SaveFileDialog`/`OpenFolderDialog`; clipboard media
  queries; `IEditableCollectionView` on `ListCollectionView`; `BindingGroup.GetValue`/`TryGetValue`;
  `ItemsControl.ContainerFromElement`; `DependencyProperty.ReadOnly`. Samples: the four threading apps, ClipboardSpy,
  CommonDialogs and EditingCollections are the targets (EditingCollections re-checked in the review gate).
- **Round 48 review (Opus) + fixes - DONE.** **A-F1** `PredictFocus` throws `ArgumentOutOfRangeException` for
  Next/Previous/First/Last (only the four arrows predict). **A-F2** `MoveFocus` wraps only in a `Cycle` container (a
  `Window` cycles; the nearest `TabNavigation` setter wins, `Contained` stops at its ends) and sets
  `TraversalRequest.Wrapped` instead of reading it. **A-F3** `CursorType` has the WPF ordinals; `ScrollEW` is a hidden
  alias of `ScrollWE`. **A-F4** `KeyConverter` knows the WPF aliases (`Ctrl`, `Esc`, `PgUp`, ...) and `Key` carries the
  converter. **A-F5** `InputEventArgs` is a public concrete class; rebasing key, mouse and text-composition args onto it
  is a known gap (`samples-coverage.md`). **A-F6** any focusable, enabled, tab-stop `UIElement` is a focus candidate and
  `MoveFocus`/`PredictFocus` are virtual; deviation: `Focusable` defaults to true on every `UIElement` here (WPF: false
  outside controls), so a non-`Control` counts only when `Focusable` is set on it, and a `Window` is never a candidate.
  **A-F7** `CommandManager.RegisterClassCommandBinding`/`RegisterClassInputBinding`; routed lookup consults class
  bindings after the instance ones; `Slider` registers its six commands and default gestures once (the sender is the
  element that owns the class binding). **A-F8** `Style.IsSealed`/`Seal()`; a sealed style rejects setter, trigger,
  `BasedOn` and `TargetType` changes; the default focus-visual style is sealed (`SetterBaseCollection` and
  `TriggerCollection` are now `Collection<T>`, as in WPF). **B-F1** a `Clip` set before the widget exists is applied on
  creation. **B-F2** `DragMove`'s recorded press is cleared on a cancelled gesture and checked against the live pointer
  mask (`Button1Mask`) on the native path; `Stopped` was not used because GTK emits it on click-timeout while the button
  is still down. **B-F3** `TokenizerHelper` rejects empty tokens and a trailing separator. **B-F4** rendered-pixel
  test for `Shape.Clip` (fails without `ApplyClip`), `GraphicsWindowTailTests` split in two. **B-F5** `Page.Template`
  shares `Control.TemplateProperty` (`Control`'s change callback ignores non-controls). **B-F6** `Point.ToString()` uses
  the culture list separator, `Point.Equals(Point, Point)` uses `double.Equals`, `==` stays exact. **C-F1**
  `Dispatcher.Invoke(Delegate, params object[])` runs at `Send`. **C-F2** `BindingGroup.ValidateWithoutUpdate` calls the
  `(value, culture, BindingGroup)` overload. **C-F3** `ItemsControl.Items` implements `IEditableCollectionView` by
  forwarding to the default view over `ItemsSource` (inline items are not editable). **C-F4** the compiler emits
  `<X.BindingGroup>` and `ItemsControl.ItemBindingGroup`; a group enlists the expressions of its element and
  descendants (`BindingBase.BindingGroupName` empty or equal to `Name`; bindings made, or subtrees parented, later
  too); `ItemBindingGroup` copies name, flags and rule instances onto each container; ValidateItemSample and
  ValidateItemsInItemsControl build and start (the rules are not driven by the gate). **C-F5**
  `ItemsControl.ContainerFromElement` null-checks only the element and returns only real containers. **C-F6**
  `CommonItemDialog`, `OpenFolderDialog.SafeFolderName(s)`, virtual `ShowDialog()`. **C-F7** four oversized files
  split, long lines wrapped. **C-F8** a GTK test drives `Dispatcher.Invoke` through a nested GLib frame with the real
  wiring. **CI-1** constructor detection balances parentheses. **CI-2** `BidiMarks` constants are `\u` escapes (the
  Round 47 note and the first Round 48 pass said so without changing the file; done in the Round 48 follow-up). **CI-3** inline isolates no longer emit a directional mark
  after a newline. **CI-4** `MarkupCompileTask` keys duplicates by full path, compares ordinally off Windows and logs a
  low-importance message for a skipped duplicate. Rejected in triage: none.
  **Round 48 totals: 5117 tests** (255 WindowsBase + 4247 PresentationFramework + 581 MarkupCompiler (1 skipped,
  Windows) + 34 ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. `Integration.Tests` not run.
  **Samples gate (Gtk4.Forms.Wpf packed from the fixed tree, pinned feed, isolated cache):** all 90 `build=pass`
  projects build through the two-build recipe and all 83 `run=runs` apps stay up with a window for 10 s, one at a time;
  no regression (AppCompat-Quirks, LocalizationResources and DragDropThumbOps are the unchanged non-runners).
- **Round 48 follow-up - DONE.** **CI-2** `BidiMarks` now really uses `\u` escapes (the earlier notes were wrong); a
  source-level test fails on a raw invisible character. **A-F2** the `TabNavigation` lookup reads the effective value
  (style setters count) and skips `Continue`, walking to the next ancestor, so an explicit `Continue` cycles in a
  `Window`. The `Contained`-inside-a-`Window` test passes on the old code too (a local `Contained` was already
  honoured); the failing-first cases are `Continue` and the style-set mode. **A-F6** a non-`Control` is a focus
  candidate when `Focusable` has a local, style or trigger value or its type overrides the default metadata; the
  remaining gap is a non-`Control` left at the default (true here, false in WPF). **A-F7** `RaiseKeyDown` reports
  handled and `OnGtkKeyPressed` sets `RetVal`, so a `Slider` key gesture no longer also steps `Gtk.Scale`; a test emits
  `key-pressed` on the real controller (fails without the fix). **C-F4** a named binding walks up to the group of that
  name; an unnamed one joins the nearest group only when it has no `Source`/`ElementName`/`RelativeSource` and the same
  `DataContext` as the group owner (checked at creation, not re-checked when the `DataContext` changes). **A-F8**
  `SetterBase.IsSealed`; `Style.Seal` seals its setters and its triggers' setters (`Setter` and `EventSetter` reject
  changes). `SetterBaseCollection` and `TriggerCollection` moving from `List<T>` to `Collection<T>` is a deliberate
  public API break (`AddRange`, `Sort`, `Find`, `ForEach`, `RemoveAll` are gone), matching WPF. **B-F2** the live-pointer
  check now runs before the move hook and goes through a test seam; the press itself is still recorded directly,
  because `GestureClick.CurrentButton` is only valid during real event delivery. **C-F6** `AddToRecent` moved to
  `CommonItemDialog` (default true, store-only). **CI-4** duplicate-path comparison probes the output directory's case
  sensitivity instead of assuming it from the OS (macOS APFS is insensitive); the sensitive-only test skips otherwise.
  **C-F7** remaining over-long lines wrapped. **Docs** the samples-gate counts are split into re-measured and carried
  over. **Round 48 follow-up totals: 5129 tests** (255 WindowsBase + 4258 PresentationFramework + 582 MarkupCompiler
  (1 skipped, Windows) + 34 ProjectConverter), all pass; `dotnet build Gtk4.Forms.sln` 0 warnings. **Samples gate:**
  all 93 projects build and all 89 apps that run still start (see `samples-coverage.md`).
- **Carry-over (Round 48)**: `KeyEventArgs`, `MouseEventArgs` and `TextCompositionEventArgs` are not rebased onto
  `KeyboardEventArgs`/`InputEventArgs`; `Focusable` defaults to true on every `UIElement` (WPF: false outside
  controls); `Once` and `None` `TabNavigation` modes behave like `Continue`; `BindingGroup` enlistment covers simple
  `Binding`s only (not `MultiBinding`/`PriorityBinding`) and `FrameworkElement.BindingGroup` does not inherit;
  `ItemBindingGroup` shares rule instances between containers; styles are
  not sealed on first use; `samples-coverage.json` is the `1b50a528` measurement (stale).
- **Round 49 track A (lane A) - startup-build-sweep (DONE, merged).** `gf49a/startup-build-sweep` (merge `9a38e05e`):
  the startup-crash and build sweep: string converters for the layer value types, deferred `{DynamicResource}` in
  generated code, local (`clr-namespace`) element types with typed fields and event wiring, `x:TypeArguments`, the
  `CreateWidget` CS0534 fold. Samples: the apps that crashed at startup on a missing converter or an eager
  `FindResource` now build and start (see `samples-coverage.md`).
- **Round 49 track B (lane A) - runtime-xaml-navigation (DONE, merged).** `gf49a/runtime-xaml-navigation` (merge
  `8c44b817`): `XamlReader`/`XamlWriter`/`XamlParseException`, `Application.LoadComponent` and a settable
  `Application.Resources`, `NavigationEventArgs` and `PageFunction<T>`/`ReturnEventArgs<T>`, and the project
  converter's `ProjectReference` handling (a referenced WPF sibling converts with the client). Samples: the Wizard
  and navigation apps are the targets.
- **Round 49 track C (lane A) - animation-clocks (NOT merged).** `gf49a/animation-clocks` (`Clock`/`AnimationClock`,
  `BeginAnimation`, typed animation bases, `PropertyPath`/`NameScope`, `Storyboard` seek) is not on `lane/a`: it was
  not part of the integrated set when this wrap-up ran, so nothing in it is reviewed here. Its apps
  (`TranslateTransform.BeginAnimation` and friends) stay on the carry-over list.
- **Round 49 review (lane A) + fixes - DONE.** **A-F1** a XAML-attribute `{DynamicResource}` now keeps Local
  precedence as in WPF: the generator emits `XamlValueConverter.SetLocalResourceReference` (internal
  `FrameworkElement.SetLocalResourceReference`), which resolves live but stores the value in the local slot, so a style
  trigger on the same DP no longer overrides it; dictionary changes and reparenting refresh it, an explicit
  `SetValue`/`ClearValue` detaches it (`DependencyObject.OnExplicitLocalChange`), `ReadLocalValue` reports it. Plain
  `SetResourceReference` (public, style/template use) is unchanged. **A-F2 / CI-2** a `{DynamicResource}` on a hooked
  local `FrameworkElement` type emits the same local reference instead of an eager `FindResource` (which threw when
  the key sat on an ancestor); closes the Round 47 carry-over. **A-F3** `IsKnownEvent` for a hooked user type also
  accepts events of the code-behind base (`class Btn : Button` gets `Click`), falling back to
  ButtonBase/Control/FrameworkElement/UIElement. **A-F4** `OnApplyTemplate` is declared on `FrameworkElement` (still
  only `Control` calls it; a `FrameworkElement` with its own `CreateWidget` never applies a template). **A-F5** the
  converter audit walks every public type's static DPs (attached DPs on static owners); newly found `IList`,
  `CultureInfo` and `ReadOnlyObservableCollection<>` are listed, `ImageSource` and `ICommand` left the list,
  `TextDecorationCollection` is noted as the documents lane's. **A-F6** `LayoutUpdated` subscribers are weak. **A-F7**
  deferred resource references refresh on attach and on detach through their own logical-parent hook. **A-F8**
  `StartupUri` with any non-`Window` `FrameworkElement` root is hosted in a `NavigationWindow`. **A-F9 / C-F5** long
  lines wrapped, `XamlReaderTests` split (`XamlReaderErrorTests`). **C-F1** `SubscribeToResourceChain` forgets the
  dictionaries it unsubscribed, so `Application.Resources` swaps A,B,A,B keep refreshing. **C-F2** the runtime
  `XamlReader` copies the compiler's content-model table (plus `Setters` for `Style`/triggers, `Children` for
  `Storyboard`), reads a `ControlTemplate`/`DataTemplate` body as a rebuilt visual tree, resolves `Setter`/`Trigger`
  `Property` against the enclosing `TargetType`, and instantiates types whose constructor parameters are all optional
  (`RotateTransform`); a theory loads the ControlsAndLayout snippets (excluding InkCanvas, FlowDocumentPageViewer,
  `TabItem.IsSelected`; skipped when `WPF-Samples` is absent). **C-F3** `XamlWriter` declares `xmlns:x` for any
  `{x:Type}`. **C-F4** `PageFunction<T>()` is public and `PageFunctionBase.Start` is `protected` (the
  `NavigationService` calls it through `InvokeStart`). **C-F5** `XamlParseException.BaseUri` has an internal setter and
  the type is `[Serializable]`; the protected `(SerializationInfo, StreamingContext)` ctor is omitted because
  `base(info, context)` is SYSLIB0051 (an error under `TreatWarningsAsErrors`). **C-F6** the converter only converts and
  injects `Gtk4.Forms.Wpf` into siblings with `UseWPF` or `UseWindowsForms`; a plain `-windows` library is left alone
  and keeps its Windows-only target (retarget it by hand). **CI-1** dictionary class names are injective (`_` doubles,
  `/` becomes `_S_`, other characters `_xHHHH_`), so `A_B/C` and `A/B_C` differ (`Themes_S_Light_S_Colors`).
  Failing-first was observed for the local-reference, resource-swap and weak `LayoutUpdated` tests and the widened
  converter audit; the rest are new-API or reflection tests that cannot compile or pass before the change.
  **Round 49 (lane A) totals: 5474 tests** (255 WindowsBase + 4576 PresentationFramework + 600 MarkupCompiler (1
  skipped, Windows) + 43 ProjectConverter), all pass, plus 66 Wpf.Testing.Tests (5 skipped); `dotnet build
  Gtk4.Wpf.sln` 0 warnings, 0 errors. `Integration.Tests` not run.
  **Samples gate (`Gtk4.Forms.Wpf` packed from the fixed tree, lane-private pinned feed, isolated cache):**
  all 130 `build=pass` projects build through the two-build recipe and all 119 `run=runs` apps start and stay up with
  a window for 10 s, one at a time (19 first read as no-window while the other lane was running; all 19 re-checked
  alone run); no regression.
- **Round 49 (lane A) follow-up - DONE.** **A-F2** the hooked-type `{DynamicResource}` emission no longer declares a
  `__eN_fe` pattern variable (two attributes on one local element was CS0128); it casts per attribute. A hand-written
  test of the emitted `is FrameworkElement` call stood in for the generator (Round 51 CI-1 replaced it with the
  XamlBacked `HookedDynWindowTests`, which runs the generated `InitializeComponent`). **A-F1** the public
  `SetResourceReference` drops a XAML-attribute (Local) reference on the same property, as WPF's Local-precedence
  write does; `_applyingLocalDp` replaces the element-wide flag so a callback that sets another property detaches that
  property's reference. The `XamlReader`/`ProvideValue` divergence (dyn-res layer, not Local) stays open. The
  ControlsAndLayout theory now uses `SkipTestWithoutData`, so it skips when `WPF-Samples` is absent. Long
  `Generator49aTests` lines wrapped.
- **Carry-over (Round 49 lane A)**: `gf49a/animation-clocks` is unmerged; a runtime `{DynamicResource}` read by
  `XamlReader` still resolves through the dyn-res layer (not Local); template bodies read by `XamlReader` rebuild
  from the markup on every apply (no `x:Shared`/`TemplateBinding` tracking beyond the builder's own); the
  `SerializationInfo` ctor of `XamlParseException` is missing; `Style.Triggers` content-model entries other than
  `Setters` are not read from bare content; `samples-coverage.json` is still the Round 48 measurement. Round 51: a
  plain explicit local value still beats a later public `SetResourceReference` here, where WPF's Local-precedence
  write replaces it (only a XAML-local reference is replaced, in one change).
- **Round 50 (lane B) track A - imaging-codecs (DONE, merged).** `gf50b/imaging-codecs` (merge `2063d423`): the pixel
  model (`PixelFormats`, indexed formats, `BitmapPalette(s)`), `BitmapFrame`, `BitmapDecoder`/`BitmapEncoder` for BMP,
  GIF, JPEG, PNG and TIFF on gdk-pixbuf plus managed BMP/GIF codecs (WDP is a documented gap), `BitmapMetadata`,
  `BitmapCacheOption`/`BitmapCreateOptions` and `BitmapImage.Format`. Samples: the imaging apps that read
  `BitmapFrame`, `BitmapCacheOption`, `BitmapCreateOptions`, `BitmapPalettes` and `PixelFormats.Indexed1` are the
  targets.
- **Round 50 (lane B) track B - flow-document-tables (DONE, merged).** `gf50b/flow-document-tables` (merge
  `adbdfb87`): `FrameworkContentElement` as the base of flow content, `FlowDocument` column, page and typography
  properties, the `Table`/`TableRowGroup`/`TableRow`/`TableCell`/`TableColumn` family and `FlowDocumentReader`.
- **Round 50 (lane B) track C - text-ranges-typefaces (DONE, merged).** `gf50b/text-ranges-typefaces` (merge
  `d969770b`): `TextPointer`, `TextRange` `Load`/`Save` (Text, Xaml, Rtf, XamlPackage), `FlowDocument.ContentStart`/
  `ContentEnd`, `Typeface`/`GlyphTypeface`/`Fonts`/`FamilyTypeface` on Pango and the `Typography` tail.
- **Round 50 (lane B) review (Opus) + fixes - DONE.** **A-F1** `BitmapImage.EndInit` reads a stream source, and a
  local file with `BitmapCacheOption.OnLoad`, into memory at once (decode stays lazy), so the stream or file can go
  away; a plain file `UriSource` keeps the `Gtk.Picture` file path. **A-F2** multi-page TIFF: the decoder walks the
  IFD chain and hands each page to gdk-pixbuf as its own file (header pointing at that IFD, next link cleared); the
  encoder is a managed baseline writer (one little-endian IFD per frame, 8-bit RGB or RGBA, None or LZW). `Zip` is
  written as LZW because the bundled libtiff cannot read Zip strips; `SupportsMultipleFrames` is now true. **A-F3**
  `BitmapFrame`, `BitmapDecoder`, `BitmapEncoder` and `BitmapCodecInfo` have protected constructors (a derived
  `BitmapEncoder` overrides `Save`), `BitmapFrame.BaseUri` has a public setter and `ImageMetadata` is the base of
  `BitmapMetadata`; `Frame.Metadata` is typed `ImageMetadata`. **A-F4** `BitmapSource.Create` throws
  `InvalidOperationException` for an indexed format without a palette and names `pixels`/`buffer` for a short buffer.
  **A-F6** `Width`/`Height` fall back to 96 dpi. **A-F7** fixed palettes are built once; the BW and gray "Transparent"
  palettes stay opaque (WIC ignores the transparent flag for them; only the halftone ones gain an entry). **A-F8**
  exact-value tests for Bgr565, Bgr555, Rgb24, Gray16 and Cmyk32 and odd-width, padded-stride Indexed2/4 and Gray2/4
  (the converter was right); a GTK test pins the file-path route
  (`Picture.File` set; a `Gdk.MemoryTexture` check was dropped because GTK itself loads a file into a memory texture).
  **A-F9** long lines wrapped, `CodecRoundTripTests` split, the stale imaging scratch folder removed. **B-F1**
  `FlowDocumentScrollViewer` shares `ScrollViewer`'s scroll-bar properties through `AddOwner` (vertical default
  Visible, horizontal Auto); `ScrollViewer` now validates the enum like WPF. **B-F2** the four `Table*Collection`
  types are sealed `IList<T>`/`IList` over an internal parented list; the public `TextElementCollection<T>` is gone.
  **B-F3** `FrameworkContentElement.NameProperty` backs `Name` (WPF shares it with `FrameworkElement.NameProperty`
  through `AddOwner`; lane A should make the generator assign `Name` for `x:Name` on a content element). **B-F4** the
  `TextElement` DP doc is current. **B-F6** `TableCell.TextAlignment` inherits; the Block-DP gap is recorded on
  `TableCell`. **C-B50b-1** `DeleteRange` logs the measured symbol difference, so pointers after a delete that starts
  outside a paragraph land correctly. **C-B50b-2** RTF `\ucN` is tracked per group, the `\uN` fallback skips N units
  (a `\'hh` escape counts as one) and negative values wrap. **C-B50b-3** an Underline-location `TextDecorations` is
  written as RTF `\ul` and as `TextDecorations="Underline"` in XAML, and read back. **C-B50b-4** XamlPackage is an OPC
  package with `_rels/.rels` and a relationships content type; the entry part is resolved through the entry
  relationship with `Xaml/Document.xaml` as the fallback. Only the XAML part is carried (no images). The relationship
  type `http://schemas.microsoft.com/wpf/2005/10/xaml/entry` (WpfPayload `XamlRelationshipFromPackageToEntryPart`) is
  from memory and was not checked against the dotnet/wpf source. **C-B50b-5** a whole-document replace or delete that
  empties the blocks leaves one empty paragraph. **C-B50b-6** `TextPointer.Offset` clamps to the content. **C-B50b-7**
  `FontModelTests`, `TextRangeTests` and `TextRangeIOTests` split and wrapped. **CI-2** `ImageBrush` paints any
  `BitmapSource` from its decoded pixels (premultiplied into an Argb32 surface); a local PNG still uses the cairo fast
  path. Rejected in triage: **B-F5** (the three MarkupCompiler test edits follow from `TextElement` becoming a
  `DependencyObject`; a note for the lane A hand-off) and **CI-1** (`Image` already sets `CanShrink = false` for
  `Stretch.None`). Not applied: **A-F5** (see Carry-over), **B-F6**'s `TableColumn.Width` validator (`GridLength`
  already rejects negative and NaN values, so it could never fire). **Round 50 (lane B) totals: 5691 tests** (255
  WindowsBase + 4747 PresentationFramework + 584 MarkupCompiler (1 skipped, Windows) + 34 ProjectConverter + 71
  Wpf.Testing (5 skipped)), all pass; `dotnet build Gtk4.Wpf.sln` 0 warnings. `Integration.Tests` not run. **Samples
  gate:** Gtk4.Forms.Wpf and its three Gtk4.Forms dependencies packed from the fixed tree into a lane-private feed
  (isolated cache, `Gtk4.*` mapped to that feed only). All 130 `build=pass` projects still build through the two-build
  recipe (126 in two builds; CustomApplicationClass needs its sibling CustomApplicationClient converted first, so it
  was built as a pair; three were re-run after an aborted batch) and all 119 `run=runs` apps still start with a window
  for 10 s, one at a time. Thirteen showed as hangs under load in the first pass and ran when re-run alone
  (BindingDPToDP, DataBindingToStringFomat, DataTrigger, MasterDetail, SimpleBinding, SpellChecking,
  FindingElementInPanel, WidthProperties, CustomRoutedEvents, RoutedEventHandling, CustomRoutedCommand,
  HandlingEventOnCommandUsingCode, VideoViewerDemo); no regression.
- **Carry-over (Round 50 lane B)**: `FileFormatException` stays in `Gtk4.Forms.PresentationFramework`: moving it to
  `WindowsBase` is expected to make every `-windows` target ambiguous with the framework's `System.IO.Packaging`
  type (CS0433, not tried), so it likely needs a `#if` per target rather than a move; JPEG XR (WDP) has
  no codec; TIFF decoding reads no resolution tags and encodes only 8-bit RGB(A); the `Name` generator hand-off to
  lane A (B-F3) is open.
- **Round 50 (lane B) follow-up.** `BitmapPalettes` BW/gray "Transparent" palettes reverted to opaque (WIC ignores
  the flag; only the caching stays). `BitmapImage.LoadedBytes` feeds the clipboard image and makes an OnLoad `UriSource`
  image an in-memory `Image` texture. `Typeface.Equals(Typeface)` is no longer public (Learn lists only
  `Equals(object)`; the earlier "WPF declares it" claim was wrong). `ImageSource.Metadata` is virtual and
  `BitmapSource`/`BitmapFrame` override it (typed `ImageMetadata`; null for a `BitmapImage`). Table collections gain
  `AddRange`, `RemoveRange`, `Capacity`, `TrimToSize` and public `IsSynchronized`/`SyncRoot`/`CopyTo(Array,int)`.
  `FrameworkContentElement.Name` validates XAML identifiers. Long lines wrapped; the `end=9` text-range test now
  asserts the surviving text. Not applied: `TextPointer.Offset` caching (direct tree edits are not versioned, and a
  test pins the clamp) and `FileFormatException` (still open, an actual move was not attempted).
- **Round 51 (lane A) track A - animation-clocks (DONE, merged).** `gf51a/animation-clocks` (merge `4a1a9325`): lands
  `gf49a/animation-clocks` (`Clock`/`AnimationClock`/`ClockGroup`, `BeginAnimation`, typed animation bases,
  `PropertyPath`/`NameScope`), animatable geometry, drawing and transform DPs, `PointAnimation`, `Storyboard.Seek` and
  the sub-object/indexed storyboard path resolver, plus Freezable emission in the MarkupCompiler. Samples evidence is
  the Round 51 gate below.
- **Round 51 (lane A) track B - startup-run-sweep (DONE, merged).** `gf51a/startup-run-sweep` (merge `b7f4842b`):
  re-measures the lane-A target apps, fixes runtime value conversion and hosts a non-Window `StartupUri` root in a
  `NavigationWindow`; the two empty-output hangs (ContextMenuOpening, DragDropThumbOps) were diagnosed.
- **Round 51 (lane A) track C - runtime-xaml-navigation (DONE, merged).** `gf51a/runtime-xaml-navigation` (merge
  `b0846dab`): runs the Round 49 targets, Local-precedence `{DynamicResource}` in `XamlReader`, the trigger content
  model, `NavigationService.Refresh`/`StopLoading`, ordered `PageFunction` return and `XamlWriter` fidelity.
- **Round 51 (lane A) review (Opus) + fixes - DONE.** **a-F1** `Brush.Opacity` is a dependency property
  (`Brush.Opacity.cs`), so a frozen brush rejects a write, `Changed` fires and a storyboard path such as
  `(Button.Background).(SolidColorBrush.Opacity)` resolves. **a-F2** `Clone()` now copies `GradientStops` (deeply),
  mapping mode, spread method, linear and radial points/radii, the `TileBrush` state, `ImageBrush.ImageSource` and
  `VisualBrush.Visual`. **a-F3** `{DynamicResource}` on a Freezable passes the enclosing scopes to
  `XamlValueConverter.SetResourceValue(scope, target, dp, key, params scopes)`, built by the helper that also feeds
  `StaticResourceLookup`; the value is resolved once at construction and is not re-evaluated on a resource change
  (documented divergence). **a-F4** `Clock` raises time, speed, state, then completed (WPF
  `RaiseAccumulatedEvents`). **a-F5** `AnimationTimeline.GetCurrentValue` returns the destination value instead of
  calling a throwing object-typed core, and `Timeline.AllocateClock` is `protected internal virtual` (default throws
  `NotSupportedException`) and `Clock(Timeline)` is `protected internal`, so a consumer can derive from `Timeline`.
  **a-F6** long lines wrapped; the `PointAnimation` emit and `PathGeometry` clone moved
  into partials. **b-F1/b-F2** a root from `GtkFormsXamlRegistry` runs the generated `InitializeComponent` in `Host`
  (once; `CreateInstance` no longer repeats it); the test now registers a type whose factory only calls `new T()`
  and fails without the fix. **c-F1** `XamlWriter` writes a default-equal local value on an inheriting property
  (`TextElement.FontWeight` overridden back to Normal round-trips). **c-F2** a `Binding` with `StringFormat`,
  `FallbackValue`, `TargetNullValue`, validation or `BindingGroupName` is skipped, not written lossy. **c-F3**
  `ReturnTo` keeps the caller's URI, and `Refresh`/a removing return drop the replaced content from the source map.
  **c-F4** long lines wrapped (`ReferenceEqualityComparer` stays qualified: `System.Windows` defines its own).
  **CI-1** the hand-written hooked-element test became the XamlBacked `HookedDynWindowTests` (generated
  `InitializeComponent`, hooked `is FrameworkElement` path). **CI-2** one long comment wrapped. **CI-3**
  `SetResourceReference` over a XAML-local reference replaces it as a Local reference in one A -> B change (it no
  longer clears first); `ClearResourceReference` also detaches a local reference.
  **Round 51 (lane A) totals: 6007 tests** (255 WindowsBase + 5037 PresentationFramework + 606 MarkupCompiler
  (1 skipped, Windows) + 43 ProjectConverter + 66 Wpf.Testing (5 skipped)), all pass; `dotnet build Gtk4.Wpf.sln`
  0 warnings. `Integration.Tests` not run. **Samples gate (Gtk4.Forms.Wpf packed from the fixed tree, lane-private
  feed, isolated cache):** all 130 `build=pass` projects build through the two-build recipe with 0 warnings and all 119
  `run=runs` apps stay up with a window for 10 s, one at a time (10 first read as no-window while the other lane was
  running; all 10 re-checked alone run); no regression.
- **Round 51 (lane A) follow-up - DONE.** **a-F5** `Timeline.AllocateClock` is `protected internal virtual` (default  throws `NotSupportedException`) and `Clock(Timeline)` is `protected internal`, so an external `Timeline` compiles  without an override and can build a clock for itself. **c-F3** `Refresh` and a removing `ReturnTo` drop a source URI  only when no journal entry still holds the instance (`NavigationJournal.Contains`).
- **Carry-over (Round 51 lane A)**: brush state other than `Opacity` (`GradientStops`, `StartPoint`/`EndPoint`, the
  radial members, the `TileBrush` members, `ImageBrush.ImageSource`, `VisualBrush.Visual`) is still plain CLR
  properties, copied by `Clone()` but not DPs and not enforced by `IsFrozen`; `{DynamicResource}` on a Freezable is
  resolved once and not live; a plain explicit local value beats a later `SetResourceReference` (see Round 49);
  DragDropThumbOps (Round 52 item) is DONE: the MarkupCompiler registers every x:Class root in `GtkFormsXamlRegistry`
  (Round 55c track a) and the registry initialises the roots it creates;
  `Binding` has no `Delay`/`XPath`, so they are not guarded in `XamlWriter`; `samples-coverage.json` still holds the
  Round 48 measurement.
- **Round 52 (lane B) track A - annotations (DONE, merged).** `gf52b/annotations` (merge `3bc141fc`):
  `System.Windows.Annotations` object model (`Annotation`, `AnnotationResource`, `ContentLocator*`, `TextAnchor`), the
  `XmlStreamStore` in the WPF XML layout, `AnnotationService` on `FlowDocumentScrollViewer`/`FlowDocumentReader`,
  `AnnotationHelper`, `StickyNoteControl` (text notes as a GTK popover; ink notes are store-only). Samples: the
  `Documents/Annotations` apps (AnnotatedDocumentViewer, AnnotationsStyling) are the targets; neither is re-measured
  here (both stay `fail` in `samples-coverage.json`).
- **Round 52 (lane B) track B - paginated-viewers-printing (DONE, merged).** `gf52b/paginated-viewers-printing`
  (merge `e45d5695`): `DocumentPaginator` and `DynamicDocumentPaginator`, the `FlowDocument` paginator,
  `DocumentViewerBase`, `FlowDocumentPageViewer`, `DocumentViewer`, `PrintDialog` on `Gtk.PrintOperation` and
  `TextEffectResolver`. Its `ContentPosition` add/add conflict with track A's file resolved to the track A version.
  Samples: the Fixed Documents and FlowDocumentNewsClient apps are the targets (not re-measured; still `fail`).
- **Round 52 (lane B) review (Opus) + fixes - DONE.** **b-F1/b-F3** a highlight is store-only: `AnnotationService`
  no longer paints or splits runs (`TextRange.Background.cs` deleted), so a highlight cannot move a note's character
  range anchor and `Disable` cannot wipe author backgrounds; behaviour now matches WPF (the old `AnnotationHelperTests`
  asserted the painting). **b-F2** `XmlStreamStore` keeps elements that are direct content of an `anc:Resource`
  (cargo and anchor contents) even in an unknown namespace, records the namespace, and writes them back on `Flush`;
  stray structural elements are still skipped. **b-F4** `StickyNoteControl.PenWidth` defaults to 2.0031496062992127.
  **b-F6** command texts carry WPF's en-US access keys. `AttachedAnnotationChanged` is not added (it needs
  `IAttachedAnnotation`, whose members were not verified); it is listed as a gap in the `AnnotationService` doc.
  **b-F7** the Meta Data cargo writes a namespaced `anb:ZOrder="0"` instead of an unqualified `IsExpanded`; long
  lines wrapped. **b-F5** one `ContentPosition` (protected constructor, nested `MissingPosition`, `Missing`). **CI-1**
  `AnnotationService(DocumentViewerBase)` and `GetService(DocumentViewerBase)` over the host seam. **CI-2** Name
  validation follows `NameValidationHelper`: first char `_`, Lu, Ll, Lt, Lo or Nl; later chars add Mn, Mc, Lm, Nd; no
  Pc or Cf. **CI-3** `ParentedList.TrimToSize` sets `Capacity = Count`. **CI-4** doc rewraps and the unverified
  `FileFormatException` note. **CI-5** `Typeface.GetHashCode` wrapped. **CI-6** the clipboard-image test no longer
  asserts on the in-memory mirror. Rejected in triage: **CI-7** (`TextPointer.Offset` rebuilds the symbol list,
  O(N*doc) with correct results; caching needs a document change version bumped by every direct tree edit) and the
  `FileFormatException` move (per-target `#if` for the CS0433 clash). `gf52b/content-bindings-sweep` is not merged:
  it rewrites the binding engine (`Data/BindingEngine*`, `BindingTarget`), outside lane B's scope, and was not part of
  this round's reviewed tracks. **Round 52 (lane B) totals: 5850 tests** (255 WindowsBase + 4906 PresentationFramework
  + 584 MarkupCompiler (1 skipped, Windows) + 34 ProjectConverter + 71 Wpf.Testing (5 skipped)), all pass;
  `dotnet build Gtk4.Wpf.sln` 0 warnings. `Integration.Tests` not run. **Samples gate:** `Gtk4.Forms.Wpf` and its three
  Gtk4.Forms dependencies packed from the fixed tree into a lane-private feed (isolated cache, `Gtk4.*` mapped to that
  feed only). All 130 `build=pass` projects still build through the two-build recipe (CustomApplicationClass as a pair
  with CustomApplicationClient) and all 119 `run=runs` apps start with a window for 10 s, one at a time; 13 showed no
  window under load in the first pass and ran when re-run alone (BindConversion, BindingDPToDP, EditingCollections,
  UpdateSource, ValidateItemSample, SpellChecking, DragDropTextOps, Image, CaptureUnCaptureMouse,
  EventOnGainAndLooseFocus, MouseState, DefiningResources, WindowHiding); no regression.
- **Round 52 paginated-viewers review fixes - DONE** (`fix/r52b-paginated`). `DynamicDocumentPaginator`
  `IsBackgroundPaginationEnabled` is virtual (false, no-op set) and `DocumentPage.Dispose` is virtual;
  `PrintDialog.MinPage`/`MaxPage` are `uint` and 0 throws `ArgumentException`; async calls are tracked per call so a
  repeated state no longer completes as cancelled; `FitToMaxPagesAcross()` keeps `MaxPagesAcross` and both are 1..32;
  the flow paginator repaginates on block and property changes and `FlowDocumentPageViewer` refreshes its status on
  a page-count change; the DocumentViewer commands are get-only static properties, `TextEffectTarget`'s constructor is
  internal, and the `PageRangeSelection`/`TextEffect` remarks are corrected.
  Paginator follow-ups from the fix check: repaginate on nested content edits (`Run.Text`, `Paragraph.Inlines`, nested
  `Section`/`List`/`Table`) and only on layout-affecting properties; coalesce invalidation once per dispatcher turn
  (building N blocks on a shown document is O(N^2) today); add `DocumentViewer` to `CommandConverters` owners so
  `Command="DocumentViewer.FitToWidthCommand"` resolves.
- **Carry-over (Round 52 lane B)**: `AttachedAnnotationChanged`/`IAttachedAnnotation`; highlights are not painted (no
  adorner layer); `TextPointer.Offset` caching; the `FileFormatException` move; `gf52b/content-bindings-sweep` still
  to be reviewed and merged.
- **Round 19 Track B — Adorners (DONE).** New `System.Windows.Documents.Adorner` (abstract, `AdornedElement`),
  `AdornerLayer` (`Add`/`Remove`/`GetAdorners` returning null when none, static `GetAdornerLayer` walking
  `VisualParent` to the nearest `AdornerDecorator`), and `AdornerDecorator` (`Decorator`+`Child`, exposes a
  non-null `AdornerLayer`, backed by `Gtk.Overlay`). **+23 tests** (object-model + decorator walk + GTK overlay
  smoke). Object model is fully headless; overlay placement happens when both the overlay and the adorner widget
  are realized.
- **Round 19 review (Opus) + fixes — DONE.** No Critical/High/Medium. Both tracks verified correct (backend
  restore via IDisposable + no-parallelism, lazy GTK init, bidirectional format auto-convert, `GetAdornerLayer`
  null-safe VisualParent walk, `GetAdorners` null-not-empty, Remove detaches). Two **Low** fixes: **A1** added the
  missing `GtkClipboardBackend` headless coverage (degrade path); **B1** split the 294-line `AdornerTests.cs` into
  a shared helper + object-model + decorator + GTK-smoke files (all ≤200). Carried over (Low, documented):
  **A2** `DataFormats.StringFormat`→UnicodeText auto-convert; **B2** deferred adorner-overlay placement (adorners
  added before realize are tracked logically but not placed until a rendering adorner + realized overlay exist).
  **Round 19 totals: 1732 tests** (89 WindowsBase + 1424 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 20 Track A — DragDrop (DONE).** New `System.Windows.DragDropEffects`/`DragDropKeyStates` ([Flags]),
  `DragEventArgs : RoutedEventArgs` (+`QueryContinueDragEventArgs`/`GiveFeedbackEventArgs`), static `DragDrop`
  behind an internal `IDragDropBackend` seam (`InProcessDragDropBackend` default-for-tests, `GtkDragDropBackend`
  degrades to in-process when no `Gdk.Display.Default`). `UIElement.AllowDrop` DP + the four tunnel/bubble drag
  routed-event pairs (`UIElement.DragDrop.cs`); GTK `Gtk.DropTarget` wiring (`UIElement.GtkDragDrop.cs`) attached
  only when a display exists. **+46 tests** (incl. 6 pinning the production tunnel-then-bubble raisers directly).
- **Round 20 Track B — ToolTip control (DONE).** New `System.Windows.Controls.ToolTip : ContentControl`
  (`IsOpen`/`Placement`/`PlacementTarget`/`StaysOpen`). `FrameworkElement.Popups.cs` wires the tooltip to
  `Gtk.Widget.TooltipText`+`HasTooltip` (string value, or a `ToolTip` whose `Content` stringifies); headless
  stores the value and applies it on realize. **+24 tests** (DP + GTK smoke). Rich-content (UIElement) tooltip
  rendering falls back to text; `IsOpen`/`Placement` are object-model-only for now.
- **Round 20 review (Opus) + fixes — DONE.** Investigated a reported intermittent native AV
  (`0xC0000005`) in GTK smoke tests: Opus confirmed the diff is **not implicated** (GTK calls are thread-confined
  via the fixture and guarded by `HasGtkDisplay()`/`AllowDrop`; the crashing ToolTip run does no drag wiring) —
  pre-existing flakiness; the authoritative combined run was clean. Fixed **Medium** test gap: the production
  `Raise*Pair` methods were untested (tests re-implemented the sequence) — made `internal` and pinned with 6
  tests asserting preview→bubble ordering + Handled-suppression. Carried over (Low): `Gtk.DropTarget` is not
  detached on widget teardown (leak when a realized `AllowDrop` element is destroyed); `GtkDragDropBackend`'s
  synchronous `DoDragDrop` returns `None` (GTK4 drag is async — full sync needs a nested main loop).
  **Round 20 totals: 1804 tests** (89 WindowsBase + 1496 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 21 Track A — RenderTransform (DONE).** Added `SkewTransform`/`TransformGroup`/`TransformCollection`
  (`Media/Transform.Group.cs`), `UIElement.RenderTransform` (default `Transform.Identity`) + `RenderTransformOrigin`
  DPs, and a GTK application via a per-widget `Gtk.CssProvider` (`transform: matrix(...)`/`transform-origin`, a
  unique `#rt-N` selector). The matrix math + CSS mapping are extracted into testable pure functions
  (`internal RenderTransformCss`). CSS `transform` renders on GTK 4.14+; the object model (`Transform.Value`
  matrix + DP) is always correct. **+28 tests** (matrix composition order, center rotation/scale/skew, CSS
  mapping, headless).
- **Round 21 Track B — `Primitives.ScrollBar` (DONE).** New `System.Windows.Controls.Primitives.ScrollBar`
  (Orientation [default Vertical], Minimum/Maximum/Value[coerced into range, re-coerced on Min/Max change]/
  ViewportSize/SmallChange/LargeChange, `Scroll`/`ValueChanged` events, `ScrollEventArgs`/`ScrollEventType`),
  backed by `Gtk.Scrollbar`+`Gtk.Adjustment` (`Upper = Maximum + ViewportSize`) with a `_updating` reentrancy
  guard both directions. No `RangeBase` exists — owns its DPs directly like `Slider`. **+43 tests**.
- **Round 21 review (Opus) + fixes — DONE.** Both tracks correct (matrix composition order = WPF first-child-
  first, CSS element order right, coercion + no adjustment feedback loop verified). No Critical/High/Medium.
  Two **Low** fixes: **A1** the `#rt-<hashcode>` widget name could cascade a parent's rule onto a same-hash
  descendant — switched to a process-wide `Interlocked` monotonic counter; **B** split the 222-line
  `ScrollBarDpTests.cs` into defaults/roundtrips + behavior files (both ≤120).
  **Round 21 totals: 1869 tests** (89 WindowsBase + 1561 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 22 Track A — LayoutTransform (DONE).** `FrameworkElement.LayoutTransform` DP; `MeasureCore`
  expands `DesiredSize` to the transform's axis-aligned bounding box via an `internal static`
  `LayoutTransformBounds(Size, Matrix)` (identity short-circuits — no layout regression). GTK visual is
  best-effort CSS, composed with `RenderTransform` (see fix). **+17 tests** (bounds math, DP, measure).
- **Round 22 Track B — bitmap Effects (DONE).** `Media.Effects.Effect`/`BlurEffect`/`DropShadowEffect`/
  `KernelType` + `UIElement.Effect` DP applied via its OWN per-widget `Gtk.CssProvider` (`#ef-N`, priority 601)
  building `filter: blur()/drop-shadow(dx dy blur rgba())` from a pure `internal EffectCss.BuildFilter`
  (culture-invariant; `Direction` 0°=right CCW → dx=depth·cos, dy=−depth·sin). **+35 tests**.
- **Round 22 review (Opus) + fixes — DONE.** Fixed a **HIGH** defect: LayoutTransform and RenderTransform
  shared one CSS provider/rule, so the second apply (or a reset to identity) wiped the other — now both are
  **composed into a single CSS matrix** via `internal UIElement.ComposeLayoutAndRender` (layout applied first,
  then render; pinned by 5 new coexistence/offset tests). Fixed a **Medium** doc defect (the `LayoutTransformBounds`
  summary falsely claimed arrange used it). Split the 217-line `LayoutTransformTests.cs` into bounds + DP/measure
  files (**Low**). Verified clean: Effect uses a separate provider (no collision), bounds math + measure
  integration + Direction/rgba/culture formatting all correct.
  **Round 22 totals: 1916 tests** (89 WindowsBase + 1608 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 23 Track A — `ImageBrush` + `Media.Stretch` consolidation (DONE).** New `Media.Stretch` enum (the
  canonical `System.Windows.Media.Stretch`, replacing two divergent duplicates previously in
  `System.Windows.Shapes`/`System.Windows.Controls` — all consumers reference by name, no value break),
  abstract `TileBrush : Brush` (+`TileMode`/`BrushMappingMode`), `ImageBrush : TileBrush`, and
  `internal StretchMath.ComputeDestRect` (None/Fill/Uniform/UniformToFill). Painting via
  `Cairo.ImageSurface`+`SurfacePattern` (local PNG files; degrades to object-model-only otherwise). **+28 tests**.
- **Round 23 Track B — Path markup mini-language parser (DONE).** `Geometry.Parse(string)` →
  `PathGeometry`, supporting `M L H V C Q Z` absolute+relative with implicit repeats (`A`/`S`/`T` parsed but
  skipped, params consumed so the stream stays synced); added `QuadraticBezierSegment`. `Polyline` already
  existed. **+17 tests**.
- **Round 23 review (Opus) + fixes — DONE.** Both tracks correct (Stretch consolidation name-only — no value
  break; StretchMath centering; parser relative accumulation/implicit-repeat/FormatException all verified).
  Fixed **B1** (ship-blocker): split the 244-line `PathMarkupParser.cs` into parser + `PathMarkupParser.Lexer.cs`
  (partial) + `PathParseContext.cs` (all ≤160). Fixed **B2**: after `Z` the current point now returns to the
  closed figure's start (SVG contract) so a following relative command accumulates correctly. Fixed **B3**: added
  tests proving the `A`/`S`/`T` skips consume the right param counts and keep the stream synced. Carried over
  (Low): **A1** `ImageBrush.Opacity` is ignored on the image-paint path (rendering-only, unreachable headless).
  **Round 23 totals: 1965 tests** (89 WindowsBase + 1657 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 24 Track A — `ArcSegment` + `A`/`a` path command (DONE).** New `Media.SweepDirection`,
  `Media/ArcToBezier.cs` (SVG §B.2.4 endpoint→center conversion + arc→cubic split, ≤90°/segment,
  α=4/3·tan(θ/4), ≈0.027% error), `Media/ArcSegment.cs` (degenerate radius → straight line). `PathMarkupParser`
  now parses `A`/`a` into `ArcSegment`s (replacing the Round-23 skip); `PathGeometry.Bounds` expands over the
  arc end point. **+14 tests**.
- **Round 24 Track B — TextBox selection API (DONE).** New `Controls/TextBox.Selection.cs`:
  `SelectionStart`/`SelectionLength`/`SelectedText`/`CaretIndex` (CLR properties, clamped to text length),
  `Select(start,length)`/`SelectAll()`, `SelectionChanged`, and `AcceptsTab` DP. GTK sync via
  `Gtk.Entry.SelectRegion` (single-line) / `Gtk.TextView.Buffer.SelectRange` (multiline), character offsets,
  deferred until realized; managed fields authoritative headless. **+15 tests**.
- **Round 24 review (Opus) + fixes — DONE.** Both tracks verified mathematically/semantically correct
  (arc endpoint→center sign/sweep mapping, α + segment count, degenerate guards; selection clamping, text-change
  reclamp on binding, GTK char-offset sync). No Critical/High/Medium. Two **Low** fixes: corrected a broken XML
  cref in `ArcToBezier.cs` (→ `PathGeometry.Bounds`, accurately noting endpoint-only under-estimate); added the
  missing `SelectedText`-set (replace-selection) + `CaretIndex`-clamp tests.
  **Round 24 totals: 1996 tests** (89 WindowsBase + 1688 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean. (Note: the `WPF-Samples` submodule + its `Directory.Nuget.Props`/
  `Gtk4.Forms.Wpf.csproj` edits on `main` are the repo owner's concurrent commits, left untouched.)
- **Carry-over (Round 24)**: `PathGeometry.Bounds` under-estimates arcs bulging past the chord (endpoint-only).
- **Round 25 Track A — Shape stroke style (DONE).** `Media.PenLineCap`/`PenLineJoin`/`PenDashCap` enums,
  `Media.DoubleCollection` (+`Parse`), and 7 `Shape` stroke DPs (`StrokeDashArray`/`StrokeDashOffset`/
  `StrokeDashCap`/`StrokeStartLineCap`/`StrokeEndLineCap`/`StrokeLineJoin`/`StrokeMiterLimit`) applied to Cairo
  via pure `StrokeStyle` mappers (dash values × thickness, enum→`Cairo.LineCap`/`LineJoin`). **+38 tests**.
  Documented: Cairo has one line cap (WPF start/end collapse; `Triangle`→`Butt`) and no per-dash cap.
- **Round 25 Track B — Window state round-trip (DONE).** `Topmost`/`ShowInTaskbar` DPs (stored; GTK4 has no
  portable always-on-top/taskbar API — documented no-op), `Activate()`→`Present`, `Activated`/`Deactivated`/
  `StateChanged` events, and the `WindowState` **WM→DP round-trip** via GTK `notify::maximized`/`suspended`/
  `is-active`, guarded against the DP↔native echo by an instance `_updatingWindowState` flag. **+26 tests**.
- **Round 25 review (Opus) + fixes — DONE.** Track A clean (dash×thickness + null/zero guards verified). Track B
  logic correct (guard leak-free, single `StateChanged`). Fixed a **High** test-adequacy gap — the WM→DP seam
  was never actually invoked (tests bypassed it); made `SyncWindowStateFromGtk` `internal` and rewrote the tests
  to drive it (assert DP update + exactly one event + guard released + no-op when unchanged). Fixed a **Medium**:
  extracted `internal static ResolveGtkState(suspended, maximized)` applying minimized-over-maximized precedence
  (both handlers now read both flags), pinned by a 4-case theory. Added doc notes (odd-dash pattern;
  window-lifetime subscriptions).
  **Round 25 totals: 2055 tests** (89 WindowsBase + 1747 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 25)**: `Topmost`/`ShowInTaskbar` have no GTK4 native effect; `WindowState.Minimized`
  WM→DP relies on `notify::suspended` (may not fire on some Wayland compositors); Cairo single line-cap.
- **Round 26 Track A — Drawing model + DrawingBrush (DONE).** abstract `Media.Drawing` (`Bounds`+`Draw`),
  `GeometryDrawing` (Geometry/Brush/Pen → Cairo fill+stroke), `DrawingGroup` (children, `Bounds` union,
  Transform/Clip/Opacity via `PushGroup`), `DrawingBrush : TileBrush` (renders the drawing to an off-screen
  `Cairo.ImageSurface`, scaled via `StretchMath`, Stretch/TileMode honoured). Added `Rect.Empty`/`IsEmpty`/`Union`
  to WindowsBase `Primitives.cs` (NegativeInfinity sentinel — distinct from a zero rect, WPF parity). **+49 tests**.
- **Round 26 Track B — TextDecorations (DONE).** `System.Windows.TextDecoration`/`TextDecorationCollection`/
  `TextDecorationLocation` + static `TextDecorations` (Underline/Strikethrough/OverLine/Baseline); `TextDecorations`
  on both `TextBlock` (DP) and `TextElement`/`Inline` (property); rendered via Pango markup
  (`underline="single"`/`strikethrough="true"`; OverLine→`overline` Pango≥1.46; Baseline→underline fallback).
  **+26 tests**.
- **Round 26 review (Opus) — DONE, no defects.** Verified the shared `Rect` change is purely additive (no
  existing-member or zero-rect semantic shift; `Rect.Empty`≠`Rect(0,0,0,0)`), `DrawingGroup` save/restore +
  PushGroup balance correct, `DrawingBrush` zero-size-surface guarded + surfaces disposed, decoration Pango
  markup nests/escapes validly, parity on both TextBlock+Inline. Both tracks clean — no fixes required.
  **Round 26 totals: 2130 tests** (89 WindowsBase + 1822 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 26)**: `DrawingBrush`/`GeometryDrawing` render only on GTK with a Cairo surface
  (object-model always correct); `VisualBrush` still unimplemented; Pango `overline`/baseline runtime-dependent.
- **Round 27 Track A — Cursor (DONE).** `System.Windows.Input.Cursor`/`CursorType` (29 members)/`Cursors` +
  `FrameworkElement.Cursor` DP (→ `Gtk.Widget.CursorFromName` on realize) + `Mouse.OverrideCursor` (store-only —
  GTK4 has no global override API). WPF→CSS cursor-name mapping is a pure `internal CursorMap.ToGdkName`
  (Arrow→default, Hand→pointer, IBeam→text, …, None→none). **+60 tests**. `.cur`/`.ani` ctors fall back to
  `default` (unsupported, documented).
- **Round 27 Track B — cancelable `Window.Closing` (DONE).** `event CancelEventHandler? Closing`; both `Close()`
  and the GTK `CloseRequest` signal funnel through an `internal bool RequestClose()` that raises `Closing`,
  aborts (no `Closed`) on `e.Cancel`, else runs `CloseCore`; `_closingInProgress`+`_closed` guard re-entrancy and
  double-close; the WM path maps the outcome to `e.RetVal` (veto ⇒ suppress GTK destroy). **+13 tests**.
- **Round 27 review (Opus) + fixes — DONE.** Both tracks correct (cursor mapping total, realize-deferral,
  close veto/retry/re-entrancy, RetVal mapping all verified) — no high/medium. Fixed two **Low** house-rule
  breaches: split the 232-line `WindowClosingTests.cs` (→ basics + `WindowClosingGuardTests`) and the 228-line
  `CursorObjectModelTests.cs` (→ Cursor/Cursors + `CursorElementTests`), all ≤150. Pinned the GTK close-request
  return-value inversion behind an `internal static MapCloseRetVal(bool)` seam with a 2-case theory (closes the
  coverage gap where `OnCloseRequest`'s `!proceeded` was untestable headlessly).
  **Round 27 totals: 2205 tests** (89 WindowsBase + 1897 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 27)**: `.cur`/`.ani` cursor files unsupported (fall back to default); `Mouse.OverrideCursor`
  has no GTK global-override effect; the live GTK `CloseRequest` signal path is exercised only via the
  `RequestClose` seam (GTK can't raise it deterministically headless).
- **Round 28 Track A — Image.Stretch (DONE).** `Image.Stretch` (default Uniform) + `StretchDirection` (default
  Both, enum reused from `Viewbox`) DPs; backing switched `Gtk.Image`→`Gtk.Picture` (which supports sizing);
  pure `internal ImageStretchMap.ToContentFit` maps `Stretch`→`Gtk.ContentFit` (None→ScaleDown, Fill→Fill,
  Uniform→Contain, UniformToFill→Cover). **+21 tests**. `StretchDirection` has no GTK equivalent (stored only).
- **Round 28 Track B — VisualBrush (DONE).** `Media.VisualBrush : TileBrush` renders a `DrawingVisual`'s recorded
  `DrawingContext` into an off-screen `Cairo.ImageSurface` (via new `DrawingContextRenderer`: rect/ellipse/line/
  geometry; `DrawText` no-op) then tiles/scales via `StretchMath` — mirroring the Round-26 `DrawingBrush` path.
  A general (non-`DrawingVisual`) visual paints nothing (GTK can't portably snapshot a foreign/unrealized widget
  — documented). **+27 tests**.
- **Round 28 review (Opus) — DONE, no defects.** Verified the `Gtk.Image`→`Gtk.Picture` switch preserves
  `Source` semantics with no other widget-type consumer broken (only the shared smoke assertion updated),
  `Gtk.ContentFit` enum values real, Stretch/StretchDirection defaults match WPF, `DrawingContextRenderer`
  command replay + `ComputeBounds` union correct, VisualBrush mirrors the clean DrawingBrush (zero-size guard +
  disposed surface). Both tracks clean — no fixes required.
  **Round 28 totals: 2253 tests** (89 WindowsBase + 1945 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 28)**: `Image.Stretch.None`→`ScaleDown` is an approximation (WPF None = natural size);
  `VisualBrush` paints only `DrawingVisual` content (general live visuals are no-paint); `DrawText` in
  `DrawingContextRenderer` contributes no bounds/render.
- **Round 29 Track A — ItemsControl alternation (DONE).** `AlternationCount` DP (int, default 0) + read-only
  attached `AlternationIndex` (`GetAlternationIndex`); each generated container gets `index % count` (count 0/1 →
  all 0, WPF parity), reassigned on `AlternationCount` change and on every items rebuild. **+17 tests**.
- **Round 29 Track B — ScrollViewer scroll API (DONE).** Read-only `HorizontalOffset`/`VerticalOffset`/`Extent*`/
  `Viewport*`/`Scrollable*` (mapped from the `Gtk.ScrolledWindow` H/V `Adjustment`), 14 scroll commands
  (`ScrollTo*`/`Line*`/`Page*`, clamped), and a `ScrollChanged` event (`ScrollChangedEventArgs` with offsets +
  extents + viewports + deltas), with a `_scrollUpdating` reentrancy guard so a programmatic scroll raises
  `ScrollChanged` exactly once (WPF parity) while external adjustment changes also raise it. **+63 tests**.
- **Round 29 review (Opus) — SHIP, no functional defects.** Verified programmatic-scroll raises ScrollChanged
  once (matches WPF), delta snapshots correct, clamp/headless/reentrancy all sound, alternation reassignment has
  no stale-index path. Added one failing-first test pinning `ScrollToVerticalOffset` raises ScrollChanged exactly
  once (closing the last-wins test gap). Carry-over (Low): `AlternationIndex` uses `RegisterReadOnly` (no
  `RegisterAttachedReadOnly` factory exists yet — functionally equivalent in this property system);
  `ScrollToHome`/`ScrollToEnd` raise one ScrollChanged per axis (WPF coalesces to one).
  **Round 29 totals: 2323 tests** (89 WindowsBase + 2015 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean (verified against the committed `net10.0` TFMs).
  Note: the repo owner is concurrently editing `PresentationFramework.csproj`/`Gtk4.Forms.Wpf.csproj` TFMs
  (→ `netstandard2.0`/multi-target) in the working tree — left untouched; that WIP currently breaks Photino.NET
  restore (NU1202) independently of this round.
- **Carry-over**: `ImageBrush.Opacity` on the painted-image path (**A1**); `ImageBrush` only paints local PNGs
  (no HTTP/non-PNG); arc (`A`) and smooth (`S`/`T`) path commands are skipped, not rendered;
  `LayoutTransform` is honoured in measure (DesiredSize) but **not in arrange** — `RenderSize`
  stays the untransformed inner size (WPF applies it in arrange too); `RenderTransform`/`LayoutTransform`/`Effect`
  CSS render only on GTK 4.14+ (object-model always correct); detach `Gtk.DropTarget` on widget teardown
  (drag lifecycle leak); synchronous `DoDragDrop`
  result on the GTK path (needs a nested GLib main loop for async drag);
  rich-content ToolTip custom-widget rendering + `ToolTip.IsOpen`/`Placement` honoured natively;
  `DataFormats.StringFormat`↔UnicodeText auto-convert (**A2**); deferred adorner-overlay
  placement on realize (**B2**);
  compiler diagnostic/warning for a non-`EventTrigger` placed in `FrameworkElement.Triggers`
  (match WPF's compile-time rejection; runtime superset stays); positive Popup/ContextMenu open-replay GTK test
  under a realized parent window;
  `Gtk.PopoverMenu` integration for `MenuItem`; nested-path `DataTrigger` bindings;
  GTK4-native backings deferred for the Round-6 controls (`Gio.Menu`/`Gtk.PopoverMenuBar` for `Menu`,
  `Gtk.ListView`+`TreeExpander` for `TreeView`, `Gtk.ActionBar` for `ToolBar`);
  `WindowState` round-trip (reflect WM-initiated state changes back into the DP); `gtk_window_move`-free
  `WindowStartupLocation` (CenterScreen/CenterOwner currently store-only);
  wire computed arrange rects onto a `Gtk.Fixed` so managed layout drives live GTK positioning
  (replace Box/Grid native backing with `Gtk.Fixed` + SizeAllocate driven by the computed
  ArrangeRects — deferred until the managed math is verified stable).
- **Round 52b content bindings (gf52b) — reviewed & fixed.** `Run.TextProperty` (BindsTwoWayByDefault, as WPF) and
  `Binding` on `FrameworkContentElement` targets (no MultiBinding/PriorityBinding; a rejected call leaves the old
  binding intact). `Run(string, TextPointer)` splits the enclosing Run; `ListItem` is now a `FrameworkContentElement`
  and `List`/`ListItem` own their children, so a Run in a list inherits the viewer DataContext. The binding-engine
  widening to `DependencyObject` is type-only; samples gate re-run (CommonDialogs now builds and runs).
- **Round 53 (lane A) track 6 - items-templates (DONE, merged).** `gf53a/items-templates` (merge `9d6ca70d`):
  `ItemCollection` (a `CollectionView` over the inline items, or the view of `ItemsSource`), `ItemContainerGenerator`,
  `FrameworkTemplate.FindName`/`GetTemplateChild`, `ListBox.ScrollIntoView`, `Binding.UpdateSourceExceptionFilter` and
  `CollectionViewSource.Filter`. Samples evidence: BindValidation, DataBindingDemo and FindingElementsInTemplates
  build; PhotoStoreDemo gains `ScrollIntoView`.
- **Round 53 (lane A) track 1 - visual-layer-input (NOT merged).** `gf53a/visual-layer-input` (`d4e0ee3c`, 92 tests)
  stays a separate branch: it is not part of `lane/a` at this review, so none of its samples (ThicknessConverter,
  VisualBrush, ImageBrush, LayoutTransitionsDemo, CompositionTarget, DrawingVisual, Per-FrameAnimation,
  DragDropObjects, OverridingLogicalTree, BitmapEffectsGallery) are counted here. Its lane B hand-offs (the
  `DrawingContext.Context` shim, `Adorner.GetDesiredTransform`) are recorded in `docs/samples-coverage.md`.
- **Round 53 (lane A) review (Opus) + fixes - DONE.** **A6-1** `GetTemplateChild` is `protected internal`, not
  `protected virtual` (WPF's signature; nothing overrode it). **A6-2** `ItemContainerGenerator.IndexFromContainer`/
  `ItemFromContainer` throw `ArgumentNullException("container")`; a foreign container still gives -1/`UnsetValue`.
  **A6-3** `IList.IsReadOnly`/`IsFixedSize` follow `ItemsSource`. **A6-4** `ItemCollection` indexer set, `RemoveAt` and
  `Insert` address view positions (sort/filter aware, range-checked), as WPF's `InnerItemCollectionView`. **A6-5**
  two tests pin the `TemplatedParent` identity check in `FindInTemplate` (mutation-checked: both fail without it).
  **A6-6** `CreateContainerForItem` reuses `ResolveItemTemplate`/`ResolveDisplayText`;
  `IsItemItsOwnContainerOverride` moved to `ListBox.ItemPresentation.cs`. **CI-1** reflection tests pin
  `Clock(Timeline)` and `Timeline.AllocateClock` as `protected internal` (the doubles keep `protected internal
  override`: plain `protected override` is CS0507 under `InternalsVisibleTo`). **CI-2/CI-3** are lane B hand-offs in
  the samples doc, documentation only. Samples gate: all 162 building projects still build and all 152 runners start
  and stay up (7 needed a solo 20 s re-run after a loaded first pass).
  **Round 53 (lane A) totals: 6253 tests** (255 WindowsBase + 5283 PresentationFramework + 606 MarkupCompiler + 43
  ProjectConverter + 66 Wpf.Testing; 6 skipped), warning-clean.
  Carry-over: merge `gf53a/visual-layer-input` and apply the two lane B hand-offs.
- **Round 53 (lane A) follow-up.** `ItemCollection.InlineIndexOfViewItem` (RemoveAt, indexer setter) returns the view
  position directly when there is no Filter or SortDescriptions, like `Insert`: `IndexOf` is Equals-based, so unsorted
  duplicates hit the first match (`{a,b,a}.RemoveAt(2)` removed index 0). One test covers both paths. Under an active
  sort or filter, equal duplicates still resolve to the first inline match (residual, untested).

- **Round 54 (lane B) track A - Media3D (DONE, merged).** `gf54b/media3d` (merge `44cd947e`):
  `System.Windows.Media.Media3D` from nothing. The math structs (`Point3D`, `Vector3D`, `Size3D`, `Point4D`,
  `Quaternion`, `Matrix3D`, `Rect3D`) with converters, transforms and rotations, models, materials, lights, cameras,
  `MeshGeometry3D`, `Visual3D`/`ModelVisual3D` and `Viewport3D`, a managed software rasterizer on Cairo inside a
  `Gtk.DrawingArea` (painter's-algorithm depth sort, flat lighting), `DoubleAnimationUsingKeyFrames` with the four
  key-frame types, `KeyTime`, `KeySpline` and `ParallelTimeline.SlipBehavior`; `PointConverter`, `VectorConverter` and
  `MatrixConverter` in WindowsBase. Samples: Converter builds and runs; KeySplineAnimations builds but crashes at
  startup on a storyboard target (lane C); the other six build-fail on non-3D symbols (`MouseDevice`,
  `BeginStoryboard`, `Frame`, `Rect.Left`, `RenderTargetBitmap.Freeze`).
- **Round 54 (lane B) track B - documents-text-tail (DONE, merged).** `gf54b/documents-text-tail` (merge
  `767cbdec`): WPF-typed `TextElement`/`FlowDocument` typography (`FontFamily` is the `FontFamily` type; effective
  values, never null or NaN), `TextElement.ContentStart`/`ContentEnd`/`ElementStart`/`ElementEnd` over a private
  container for detached elements, `RequestBringIntoView`/`BringIntoView`, `FrameworkContentElement.ContextMenu`
  (stored), nested-edit-aware repagination and `Run(string, TextPointer)` between blocks. Samples: FontProperties and
  FlowDocumentProperties build and run; AnnotatedDocumentViewer is left on `SortDescription` (CS0200, lane A).
- **Round 54 (lane B) review (Opus) + fixes - DONE.** **A-F1** a leading Uniform key frame resolves from a virtual
  time 0 (`[U,U,U]` over 3 s is 1, 2, 3 s) and only a leading Paced frame is pinned to 0. **A-F2** `IsCumulative` adds
  the last frame value per completed iteration, not `last - origin`. **A-F3** a `Viewport3D` with a null `Camera`
  paints nothing instead of throwing. **A-F4** `DoubleAnimationUsingKeyFrames` is no longer sealed (`GetCurrentValueCore`
  is a sealed override, as in WPF). **A-F5** reading a default 3D collection (`Positions`, `Children`, ...) hands out a
  per-owner collection without `SetValue` or `Changed`; it becomes the local value on its first change;
  `SplineDoubleKeyFrame.KeySpline` defaults to a frozen `KeySpline(0,0,1,1)`. **A-F6** `FreezableList<T>` is gone: an
  internal composed `FreezableListCore<T>` backs each collection, which now derives from `Animatable`
  (`Model3DCollection`, `MaterialCollection`, `Transform3DCollection`) or `Freezable` (the rest, incl.
  `DoubleKeyFrameCollection`). **A-F7** the face normal is negated only for a back face and vertex normals use the
  inverse transpose of the world matrix. **A-F8** invalidation wiring is pinned (`Viewport3D.InvalidateCount` hook).
  **A-F9** the point, vector and matrix converters are sealed. **A-F10** `Transform3D.TransformBounds` is public; long
  files and lines split and wrapped. **B-F1** `FlowDocument.ViewChanged` is coalesced to one raise per dispatcher turn
  (50 paragraphs, `FontSize = 20`: 150 raises before, 1 now). **B-F2** deleting over a detached Paragraph, Span or Run
  prunes the old text (`Text = "bye"` gave `byehello`). **B-F3** `BringIntoView()` passes `Rect.Empty`. **B-F4** the
  `RequestBringIntoViewEventArgs` constructor is internal. **B-F5** `TextPointer.Parent` is null at the root of a
  detached element. **B-F6** viewer tests host a shown window and assert page and scroll changes; the document
  `FontFamily`/`FontSize` reach the viewer markup. **B-F7** wrapped long lines; `SortDescription` setters requested from
  lane A in `docs/samples-coverage.md`. **CI-1** a content version caches the symbol list: repeated `TextPointer.Offset`
  reads build the text tree once. **CI-2** `SystemFonts` has the `*FontFamily/Size/Style/Weight/TextDecorations` keys
  for all six font groups, resolved by `FindResource`; `FlowDocument.FlowDirectionProperty` is a real DP.
  Rejected in triage: the B-F7 build count (162 = 159 apps + 2 libraries + 1 console app, line 21 of
  `samples-coverage.md`) and CI-3 (`FontFamily` is already typed). Samples gate: all 164 projects with
  `build=pass` still build and all 154 apps with `run=runs` still run (10 of them only when re-run alone under
  load); AnnotatedDocumentViewer is down to the two `SortDescription` setters.
  **Round 54 (lane B) totals: 6344 tests** (260 WindowsBase + 5363 PresentationFramework + 607 MarkupCompiler +
  43 ProjectConverter + 71 Wpf.Testing; 6 skipped), all green; `dotnet build Gtk4.Wpf.sln` warning-clean.
- **Round 54 (lane B) follow-up.** **CI-1** the `Child` setters of `BlockUIContainer` and `InlineUIContainer` now
  invalidate the symbol cache (`ContentEnd` was one symbol short after `Child = new Button()`). **CI-2** an inherited
  right-to-left direction now reaches the markup, not just a local one. **B-F1** `ViewChanged` posts on the
  document's own dispatcher; no viewer flushes synchronously (no such request path exists, left as is). **B-F2**
  deleting over a detached `List` prunes the old text; inserting text directly inside a `List` still throws.
  **A-F7** the earlier diffuse test passed on the old code too (its normal renormalised); it now lights a slanted
  normal, which only the inverse transpose gets right. The `BuildViewMarkup` doc comment, doc line widths and the
  `samples-coverage.json` trailing newline are fixed. Samples gate: all 164
  `build=pass` projects still build and all 154 `run=runs` apps still run (6 only when re-run alone for 20 s).
  **Round 54 (lane B) follow-up totals: 6347 tests** (260 + 5366 + 607 + 43 + 71), all green.
- **Carry-over (Round 54 lane B)**: `FreezableListCore<T>` repeats ~100 lines of generated `IList` plumbing per
  collection (C# has no mixins; a source generator would remove it); `Visual3DCollection` has no nested `Enumerator`;
  typed 3D animations (`Rotation3DAnimation`, ...) and a per-pixel z-buffer remain deferred;
  `MeshGeometry3D.TextureCoordinates` edits do not repaint; the Fixed/XPS model and `DocumentMerge` are next;
  AnnotatedDocumentViewer needs `SortDescription` setters (lane A) and AnnotationsStyling needs `MenuItem.Padding`,
  `SystemParameters` and `InkCanvasEditingMode` (lanes A and C); `ScrollViewer` does not handle
  `RequestBringIntoView`; failing-first was shown against the old
  code for A-F1, A-F2, A-F3, A-F4, the normal matrix of A-F7, B-F1, B-F2 and B-F3 only (the other new tests pass
  against the new code and were not run against the old).
- **Round 55 (lane C) track a - startup and generator sweep (DONE, merged).** `gf55c/startup-generator-sweep`: the
  MarkupCompiler registers every x:Class root (not only Page/Window) in `GtkFormsXamlRegistry`, resolves root
  attached properties through their owner type, emits `VisualBrush.Visual` property elements so named Freezables are
  built and registered, resolves `{StaticResource}` in a `ResourceDictionary` root against its own keys and gives an
  `ApplicationDefinition` without x:Class a class and `Main` (`CS5001`); `sc#` colours parse in `XamlValueConverter`;
  `NavigationWindow.SourceProperty` is `Frame.SourceProperty.AddOwner`; a `GradientStop` path step is skipped in
  `Storyboard` target resolution. Samples evidence (162 building projects, two-build recipe, lane-private feed): no
  pass-to-fail and no runs-to-crash; ControlsAndLayout, CustomComboBox, GeometryDesignerDemo and DragDropThumbOps run
  (the Round 51 DragDropThumbOps carry-over is closed), Wizard and GlobalizationHomepage build and run; 157 of 161
  apps show a window.
- **Round 55 (lane C) track b - controls and media tail (DONE, merged).** `gf55c/controls-media-tail`: `RangeBase`
  with `Slider`/`ProgressBar` on it, `MediaElement`/`MediaTimeline`/`MediaClock` over `Gtk.MediaFile`, `UserControl`,
  `System.Windows.Shell` (`WindowChrome`: a caption area undecorates the window and drags it), `SystemParameters`, and
  the member tail of `MenuItem`, `Frame`, `TabItem`/`TabControl`, `Expander`, `TreeView`, `WrapPanel`, `GridSplitter`,
  `ScrollViewer`, `Popup`, `DataGrid`, `ListViewItem`. Samples evidence: all 163 build=pass projects (plus
  WindowActivationAndDeactivation) build through the packed `Gtk4.Forms.Wpf`; 153 apps smoke-run with a window, no
  regressions; WindowActivationAndDeactivation now builds and runs.
- **Round 55 (lane C) review and fixes - DONE.** Verified findings applied failing-first. Track a: the path resolver
  raises `UnanimatableTargetException` only for known layer gaps (`GradientStop`) and `InvalidOperationException`
  otherwise, and `BindClocks` traces each skipped animation; the `sc#` parser is ordinal-case, splits on commas and
  whitespace and maps NaN to 0; `GtkFormsXamlRegistry.TryCreate` runs the generated `InitializeComponent` of the
  roots it creates, so `Application.LoadComponent(Uri)` and navigation by URI return populated roots (the helper moved
  from `Application`); `NavigationWindow`'s source metadata derives from `FrameworkPropertyMetadata`; the long
  MarkupCompiler test lines are wrapped. Track b: `Slider` keeps the RangeBase `SmallChange` 0.1 and `LargeChange` 1;
  `MediaElement` control methods throw `NotSupportedException` only when neither behavior is Manual and are ignored
  while a Clock is set; `TabControl` selects a pre-marked `IsSelected` tab when it gets its items or widget;
  `WindowChrome` has the Windows defaults on a new instance (metadata default 0) with `SystemParameters`
  `WindowCaptionHeight`/`WindowNonClientFrameThickness`/`WindowCornerRadius` and an inheriting
  `IsHitTestVisibleInChrome`; a `WindowStyle` change no longer re-decorates a chrome window; `Slider` range changes
  no longer echo a clamped value back to the DP; `MediaClock` is unsealed with a `protected internal` constructor,
  `BaseUri` is an explicit `IUriContext` member, and a source-less `MediaTimeline` cannot allocate a clock;
  `MenuItem.IsSubmenuOpen` binds two-way by default and `TabItem.IsSelected` journals; `MediaElement.Gtk.cs` is split
  (`MediaElement.Stream.cs`). Also: `Storyboard` binds a `MediaTimeline` child to its `TargetName` `MediaElement`
  (`MediaClock` case in `BindClocks`), and the MarkupCompiler emits an attached property element on the root
  (`<WindowChrome.WindowChrome>`) as `WindowChrome.SetWindowChrome(this, ...)`.
- **Round 55 (lane C) totals.** `dotnet build Gtk4.Wpf.sln` 0 warnings; `PresentationFramework.Tests` 5308 passed,
  2 skipped (no media backend), 0 failed; `MarkupCompiler.Tests` 621 passed, 1 skipped, 0 failed; 21 test methods
  added, 2 replaced.
- **Round 55 (lane C) follow-up.** Verified review findings applied failing-first. MarkupCompiler: an attached property
  element with a `Binding`/`MultiBinding`/`PriorityBinding`, template or markup-extension child goes to
  `SetBinding`/template emission again (not `Owner.SetProp`), and its owner resolves through the element's own xmlns
  (user `clr-namespace` attached owners are qualified as `global::Ns.Owner` only on the Binding/MultiBinding
  property-element path; a value element on an owner the resolver cannot inspect is still skipped and reported, as
  `GeneratorAttachedElementTests` pins). `Storyboard`: `PathFigure`, `PathSegment` and `Pen` path steps are skipped
  like `GradientStop`; a `MediaTimeline` with no target binds to the containing `MediaElement`. `WindowChrome`: the
  caption drag reads only the effective (inherited) `IsHitTestVisibleInChrome` of the hit element. `sc#` parsing rejects
  an empty comma token as WPF's `TokenizerHelper` does. `RegisteredCanvas` has an `Initialize` idempotency test back;
  the stale GeometryDesignerDemo `runCause` in `samples-coverage.json` is corrected; over-long lines wrapped. Not
  verified here: `MediaClock.NaturalDuration` after `MediaOpened` and the WAV stream path (no media backend on this
  machine; the test skips).
- **Round 55 (lane C) carry-over.** Not fixed, out of lane C: `Color.FromScRgb`/`ScR*` skip the sRGB transfer
  (`Color.FromScRgb(1,0.5f,0,0).R` is 128, XAML `sc#1,0.5,0,0` gives 188, WPF gives 188 for both; fix `Color.cs` and
  have `ScRgbColor.Parse` call it); SlidePuzzleDemo (`DocumentViewerBase.IsMasterPage`, lane B; `ControlTemplate.FindName`,
  lane A); MediaGallery (`Binding.Path` typed string, lane A); WPFGallery (needs a `Microsoft.Win32.SystemEvents` package
  reference); SystemParameters `*Key` objects do not resolve through `DynamicResource`; `ScrollBar` is still on `Control`;
  the store-only members (`Balance`, `ScrubbingEnabled`, `SpeedRatio`, `StretchDirection`, `ExpandDirection`, DataGrid header
  resize, Popup animation) are deliberate GTK limits; `FontStyleConverter`/`FontWeightConverter`, `Hyperlink.TargetName`,
  `TextBox.TextAlignment` belong to other lanes; remote `http` media is unverified. If `Wpf.Testing.Tests` fails once
  (EvidenceTests, InputTests), re-run that suite alone before treating it as real.
- **Round 57 (lane B) track 5 - items-data-tail (DONE, merged).** `gf57b/items-data-tail` (merge `7237e943`):
  `SortDescription.PropertyName`/`Direction` are settable (sealed once held by a collection), `Binding.Path` is a
  `PropertyPath` (an implicit string bridge keeps generated code compiling until lane C emits
  `new PropertyPath("x")`), `PropertyPathConverter`, and `ItemCollection.RemoveAt`/indexer/`Insert` address the view
  position under a Filter or sort. Samples: AnnotatedDocumentViewer and MediaGallery are no longer blocked on those
  two APIs; the 165 runnable samples still build and run (see gate).
- **Round 57 (lane B) track 6 - documents-imaging-tail (NOT merged).** `gf57b/documents-imaging-tail` (Fixed/XPS
  documents, the CachedBitmap family, `TextDecoration` collections, `DocumentViewerBase.IsMasterPage`) is not in
  `lane/b` at `7237e943`; it stays a separate branch and this wrap-up changes none of it. Its detached-List
  `TextRange` fix is carried by the review fixes below, written against `lane/b`.
- **Round 57 (lane B) review (Opus) + fixes - DONE.** Applied failing-first. **a-F1** the duplicate-filter test
  compared with `Tag.Equals` (Name only) and passed on the old `_inline.IndexOf` code; it now asserts by reference, and
  the `CollectionChanged` test asserts the inline contents. **a-F2** the string-to-`PropertyPath` bridge maps a null
  string to a null path (WPF leaves `Path` unset), so a null never throws and the binding uses the source itself.
  **a-F3** the `Direction` setter reports `ParamName` "value", the constructor "direction". **a-F4**
  `PropertyPathConverter.ConvertFrom(null)` throws `ArgumentNullException("source")`; `ConvertTo` writes a dependency
  property as `(Owner.Name)` (no xmlns prefix, there is no serializer context), and `(Owner.Name)` resolves by name
  only when Owner is the source type or a base type (see the follow-up).
  **CI-1** `TextTreeEdit.FindOwner` searches a detached `List` root, so a cross-item delete removes the emptied
  paragraph, multi-line text no longer dereferences a null owner, and a full-range `Text` set keeps a first item with a
  paragraph. The `FlowDocument.FlowDirection` doc now says "set locally or inherited as RightToLeft". The range text
  after a full-range set keeps its trailing paragraph break (the test trims it).
  Rejected: **CI-2** and **CI-3** were already fixed by the merge (only the `ParamName` residue, a-F3); **CI-4** is lane
  C's (`XamlCodeGenerator.BindingElement.cs:80` must emit `new System.Windows.PropertyPath("x")`); **CI-5** waits on lane
  A's `DrawingContext` from `RenderOpen` (gf53a).
- **Round 57 (lane B) follow-up.** **CI-1** a cross-item delete now merges the emptied `ListItem` away
  (`ListItemCollection.Remove` added, as in WPF) and `PruneListItems` drops items it emptied, so a full-range `Text`
  set leaves one item, not an empty bullet. **a-F4** the owner of a `(Owner.Name)` path is stripped only when Owner is
  the source's type or a base type (`PropertyPath.ResolveOwner`, applied at the walk, write-back, leaf and INPC watch
  points of `BindingEngine` only; the Round 57 regression fix extended it to every `PathString` consumer).
  `(Canvas.Left)` or `(Grid.Row)` stays unresolved instead of binding an unrelated CLR `Left` or `Row`;
  attached-property paths are still not resolved.
- **Round 57 (lane B) totals.** `dotnet build Gtk4.Wpf.sln` 0 warnings. Targeted `PresentationFramework.Tests`
  (ItemCollection, SortDescription, PropertyPath, Binding, TextRange, RichTextBox, FlowDocument, TextTree, Paginator,
  Collection): 685 passed. Samples gate: 176 build=pass projects all build, 165 run=runs all start and stay up (4 needed
  a lone 20 s re-check). Carry-over: **CI-4**, **CI-5**, merging `gf57b/documents-imaging-tail`.
- **Round 57 (lane B) regression fix.** The a-F4 follow-up moved owner stripping out of `PropertyPath.ResolvedString`
  into `PropertyPath.ResolveOwner` but hooked it only into `BindingEngine`, so `Path=(TextBlock.Text)` regressed at
  seven other `PathString` consumers: the `PriorityBinding`, `MultiDataTrigger` and `DataTrigger` watches,
  `DataGridTextColumn.GetCellText`, `DataGrid` sort, `HierarchicalDataTemplate.ResolveChildren` and
  `BindingGroup.TryGetValue`. Each now applies `ResolveOwner(source, PathString)` (one failing-first test each).
  **CI-1** a delete inside an attached `FlowDocument` now finds the owning `List` at any depth (`ItemOwning`) and
  drops the emptied `ListItem`, so no empty bullet is left.


- **Round 56 (lane A) track 1 - visual layer and input surface (DONE, merged).** `gf56a/visual-layer-input`: d4e0ee3c
  was re-landed with the review findings F1-F5 fixed (FlowDocument text no longer drawn twice, the render layer keeps a
  Box's layout manager, a visual is its own ancestor/descendant, `FrameworkElement.LogicalChildren` defaults to null,
  `CompositionTarget.Rendering` handlers run one by one). 9 of the 12 target samples build and run. Brushes is blocked
  in lane C (`Margin="12pt"` reaches `PrimitiveConverters.ParseThickness` without unit parsing). Per-FrameAnimation is
  blocked in lane C's generator (it emits `.Content` for children of a `Canvas` subclass). ExpenseItDemo was blocked on
  `Mouse.PrimaryDevice`/`MouseDevice` (track 2, now merged; re-measure). Lane B can now apply CI-2.
- **Round 56 (lane A) track 2 - input devices and storyboard entry points (DONE, merged).** `gf56a/input-devices-
  storyboard`: `MouseDevice`/`KeyboardDevice` are exposed on the input event args as in WPF (`Device`, `MouseDevice`,
  `KeyboardDevice`), and `FrameworkElement.BeginStoryboard` has its three overloads.
- **Round 56 (lane A) review and fixes - DONE.** Verified findings applied failing-first. `VisualCollection` adds the
  child to its list before adopting it (undone when adoption throws), so a child added after the host is realized
  creates the render layer; `AddLogicalChild(null)` is ignored like WPF; `PathGeometry.FillRuleProperty` exists, is
  cloned and keeps the `F0`/`F1` parse; the two-argument `RoutedEventArgs` constructor assigns the fields without calling
  `OnSetSource`. Input: the public `InputEventArgs`/`KeyboardEventArgs` constructors accept a null device (WPF),
  `MouseButtonEventArgs` rejects an undefined button, the `MouseDevice.GetPosition(null)` doc matches the behaviour, and
  the GTK mouse and key paths stamp the GDK event time on the Preview and bubble args (one `Environment.TickCount` per
  programmatic dispatch). Tests added for the GTK key path, the focus args, `Compose` versus `SnapshotAndReplace`
  hand-off and device capture. Over-long lines wrapped; `PathGeometry` and `DrawingContextRenderer` split into partials.
  Rejected in triage: none.
- **Round 56 (lane A) totals.** `dotnet build Gtk4.Wpf.sln` 0 warnings; targeted `PresentationFramework.Tests` (visual
  layer, input devices, media, input, animation, geometry filters) 1000 passed, 0 failed; `WindowsBase.Tests` (routed
  events, converters) 46 passed; 21 test methods added.
- **Round 56 (lane A) carry-over.** Not fixed: a binding cannot target a `Freezable` (`BindingOperations.SetBinding`
  accepts `FrameworkElement` and `FrameworkContentElement` only, lane B Data), so `PathGeometry.FillRule` has no binding
  test; right after `BeginStoryboard(..., SnapshotAndReplace)` the property reads its base value until the first clock
  tick instead of the snapshot (one frame); `KeyboardFocusChangedEventArgs` from `Focus()` still stamps
  `Environment.TickCount`; `TextCompositionEventArgs` has no timestamp.
- **Round 58 (lane C) track 4 - startup and generator sweep II (DONE, merged).** `gf58c/startup-generator-sweep-2`: a
  keyed `ResourceDictionary` value holding a `{StaticResource}` is built on first lookup (`SetDeferred`), so an unused
  missing key no longer throws at load (ExpenseItIntro); `x:Shared="False"` builds a new value per lookup. The generator
  emits Media3D graphs (`Viewport3D.Camera`, `ModelVisual3D.Content`/`Transform`, keyed 3D resources, nested `x:Name`),
  key-frame storyboards (`DoubleAnimationUsingKeyFrames` is in the xmlns map; the `x:Name` in a geometry property
  element registers), object property elements and collection property elements, user attached owners, and
  `BasedOn="{StaticResource {x:Type T}}"` with no dictionary entry. A `{Binding}` on a non-element
  (animation, freezable, `CollectionViewSource`) goes through `NonElementBinding.TrySetBinding` and is not evaluated
  (the engine serves elements only). A compile-checked generator corpus replaces the unmerged `gf39/xaml-content-model`
  idea. Samples: Graphics/Geometry and KeySplineAnimations run; AnimationExamples builds; ExpenseItIntro and
  DataBindingDemo still fail further in (UI automation and engine causes).
- **Round 58 (lane C) track 3 - controls tail II and packaging (DONE, merged).** `gf58c/controls-tail-packaging-2`:
  `Hyperlink.TargetName`, `FontStyleConverter`/`FontWeightConverter`, an `InkCanvas` shell (store-only editing modes,
  children on an inner `Canvas`), `TextBox.TextAlignment`/`MinLines`/`MaxLines`, `FrameworkElement.Tag`, and the
  `Microsoft.Win32.SystemEvents` dependency on the `Gtk4.Forms.Wpf` package (WPFGallery's `CS1069`). The WindowChrome
  template-part caption-drag fix landed first.
- **Round 58 (lane C) review (Opus) + fixes - DONE.** Verified findings applied failing-first. **b-F1/b-F4**
  `Window.ShellChrome.EffectiveHitTestVisibleInChrome` asks `HasNonInheritedValue`, so a Style, trigger, template,
  `DynamicResource` or animation value on a template part counts (it only saw local values); tests cover a style-set
  `True` on an unflagged control, a style-set `False` on a flagged one, the in-template ancestor walk, and the caption
  drag (a press over a flagged part does not drag, plain chrome does). **b-F2** `InkCanvas` sets its inner surface's
  `LogicalParent`, so children get `DataContext`, resources, `IsLoaded`/`IsVisible` and `IsEnabled`; `VisualChildren`
  yields the surface and `FrameworkElement.Parent` of a child reports the `InkCanvas` (`ParentOfChildren`). **a-F1** an
  unkeyed nested `ResourceDictionary` in the runtime loader merges with `CopyEntriesFrom`, keeping values deferred and
  `x:Shared="False"` per-lookup. **a-F6** a merged-`Source` window test (the ExpenseItIntro shape);
  `ResourceDictionary.Remove(KeyValuePair)` realises a deferred entry before comparing. **a-F2** `TrySetBinding` writes
  a `Trace` warning naming the type, property and path, and the generator adds the message
  `binding on non-element <Type>.<Prop> is not evaluated`. **a-F3** `SetDeferred`, `CopyEntriesFrom` and
  `NonElementBinding` are `EditorBrowsable(Never)`. **b-F3** the font converters throw WPF's exception types (null:
  `NotSupportedException`; other non-string: `ArgumentException("value")`; out-of-range number:
  `ArgumentOutOfRangeException`), fall back to `base.ConvertTo`, and convert to `InstanceDescriptor`. **b-F6**
  `RequestNavigateEventArgs.Target` is `string?` and keeps a null target. **b-F5/a-F5** stale comments reworded, long
  lines wrapped. **a-F4** this entry, and the attached-owner claim below is corrected. Rejected in triage: CI-2 and
  CI-4 (already resolved: `StoryboardPathResolverGapTests` pins the gaps; `DoubleAnimationUsingKeyFrames` is mapped),
  CI-5 (`FrameworkElement.Tag.cs` registers `Tag`; only the comment was stale), CI-1 (superseded by b-F1/b-F4), CI-3
  (pinned by `GeneratorAttachedElementTests`; the doc claim is fixed), CI-6 and CI-7 (other lanes: `TextBlock`
  `ContentStart`/`ContentEnd` and `TextDecorationCollection.Freeze` are lane B; `Color.FromScRgb`,
  `SystemParameters` keys and `ScrollBar` are other lanes).
- **Round 58 (lane C) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. Targeted tests (the
  suites run in full at integrate): `PresentationFramework.Tests` touched areas 379 passed, 1 skipped, 0 failed;
  `MarkupCompiler.Tests` 641 passed, 1 skipped, 0 failed; 11 test methods added, 2 updated. Samples gate: all 176
  projects that built at `270cdb7e` build through a lane-private feed (two builds each), and all 165 apps that ran
  still run (19 apparent hangs under a crowded first pass re-checked alone: all run).
- **Round 58 (lane C) carry-over.** The binding engine still serves `FrameworkElement` targets only, so a bound
  animation `To`, a `Freezable` or a `CollectionViewSource` binding is not evaluated (lane B, `Data/**`; the follow
  test `A_bound_animation_To_follows_the_source` stays skipped as its acceptance test). ExpenseItIntro and
  DataBindingDemo need further work (`AutomationProperties.HeadingLevel`/`ItemStatus` belong to the UI-automation
  plan). `RenderOptions.*` attributes and `VisualBrush.Visual="{Binding}"` are dropped with a message.

- **Round 59 (lane A) track 3 - controls and freezable tail III (DONE, merged).** `gf59a/controls-freezable-tail-3`: the
  `DataGrid` cell model (`DataGridCell`, `DataGridCellsPresenter`, `DataGridCellInfo`, `CurrentCell`, cell and header
  styles), a routed `TextBox.SelectionChanged`, `TextBlock.ContentStart`/`ContentEnd`, typed
  `SolidColorBrush.Clone`/`CloneCurrentValue` and the WPF `FontWeight` names. It covers ExpenseItDemo's
  `DataGridCell`/`DataGridCellsPresenter` and the FontDialogDemo and SlidePuzzleDemo members that lane A owns; both
  still need lane B's documents and imaging work. The samples gate below (197 projects) is the evidence.
- **Round 59 (lane A) track 6 - the .NET 9 WPF surface for WPFGallery (DONE, merged).** `gf59a/net9-wpf-surface`: the
  `SystemColors` accent family, `Application.GetResourceStream`/`StreamResourceInfo`, `ToolTipService.ToolTipProperty`,
  `FocusManager.IsFocusScope`, `ContentPresenter.RecognizesAccessKey`, `Window.IsActive`, a routed
  `Hyperlink.RequestNavigate` with `System.Windows.Navigation` args, `ItemsControl.ItemsPanel` with
  `ItemsPanelTemplate`, and the new `MessageBoxButton`/`MessageBoxResult` values. Where GTK has no equivalent the
  object model is exact and store-only.
- **Round 59 (lane A) review (Opus) + fixes - DONE.** Verified findings applied failing-first. **a-F1** the full suite
  was red on the merge: `DataGridColumn` joined the audited `Unconverted` list (object-valued DP, no converter in
  WPF). **a-F2** `DataGridCellsPresenter` no longer hides `ItemsControl.ItemContainerGenerator`: the base property
  calls an internal virtual `CreateItemContainerGenerator()` and the presenter overrides it with the column-to-cell
  host, so `((ItemsControl)presenter).ItemContainerGenerator` maps columns to cells. **a-F3** `DataGrid.CellStyle`,
  `ColumnHeaderStyle`, `IsReadOnly` and the column's `CellStyle`/`HeaderStyle`/`IsReadOnly` now re-apply in place to
  the built cells and headers (`DataGridColumn.DataGridOwner`, `DataGrid.RefreshCellsAndHeaders`, shared with
  `Realize` and `CreateHeader`). **a-F4** `DataGridCellInfo` follows WPF: only a null column throws, a null item is
  valid, validity depends on the column, and equality uses `object.Equals` on the item (boxed `5` equals boxed `5`,
  so `CurrentCellChanged` fires once). **a-F5** pinned with a skipped test: `TextBlock.ContentStart.Parent` returns
  the internal `TextBlockHost`, not the `TextBlock`; follow-up for lane B in `Documents/TextPointer.cs` (an internal
  hook in `TextPointer.Parent` that maps a `TextBlockHost` root to its owner). **a-F6** over-long lines wrapped, stray
  blank lines removed. **b-F1** a single-instance `ItemsPanelTemplate.Content` panel backs only the first control;
  `InstallItemsPanel` leaves a second control on the default stack and writes a one-time `Trace` message (the
  `Content` XML doc says so). Lane C follow-up: the generator must emit `Factory = () => new WrapPanel{...}` for
  `ItemsPanelTemplate` so `Content` can be removed. **b-F2** `Application.GetResourceStream` throws WPF's
  `IOException("Cannot locate resource '<lower-cased relative path>'.")` instead of returning null. **b-F3**
  `SystemColors.AccentColor` is `#0078D4`, the default of dotnet/wpf `AccentColorHelper`; the Light/Dark shades are
  documented as the Windows default palette. **b-F4** `MessageBoxButton.AbortRetryIgnore`/`RetryCancel`/
  `CancelTryContinue` and `MessageBoxResult.Abort`/`Retry`/`Ignore`/`TryAgain`/`Continue` are documented as WPF .NET 10
  (dotnet/wpf main), not .NET 9, with matching ordinals; the coverage notes no longer imply WPF may lack them. **b-F5**
  `IsSealed` and `Seal()` moved from `ItemsPanelTemplate` to `FrameworkTemplate` (public, store-only, no
  modification lock). **b-F6** the test csproj comment sits above the `Page` group again. **CI-3** a pin test: the
  `KeyboardFocusChangedEventArgs.Timestamp` of a focus change is `Environment.TickCount`. Rejected in triage: CI-1
  (`FontWeight.ToString` already returns the WPF names), CI-2 (`default(FontWeight)` already equals `Normal`), CI-4
  (`TextCompositionEventArgs` already derives from `InputEventArgs` and stamps `TickCount`), CI-5 (the
  `MessageBoxButton`/`Image`/`Result` values already match WPF) and CI-6 (the dismissed-dialog mapping already matches
  Win32).
- **Round 59 (lane A) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. Full
  `PresentationFramework.Tests` on the fixed tree: 5894 passed, 4 skipped, 0 failed (10 test methods added, 3
  updated). Samples gate: all 197 projects with `build=pass` at `85493c8b` build through a lane-private feed (two
  builds each; a harness bug and a parallel-conversion race in CustomApplicationClass were fixed by patching its
  csproj and rebuilding), and all 182 apps that ran
  still run (16 apparent hangs under a crowded pass re-checked alone for 20 s: all run).
- **Round 59 (lane A) follow-up.** `DataGrid.CurrentCell` now depends only on `CurrentColumn`, so a null-item cell
  round-trips and raises `CurrentCellChanged` once. A `Columns` reset no longer disowns auto-generated columns. Stray
  blank lines removed; the totals entry now counts 10 added test methods and one gate-retry account.
- **Round 59 (lane A) carry-over.** Lane B: `TextPointer.Parent` for a `TextBlock` pointer (skipped test
  `The_pointers_Parent_is_the_TextBlock`), plus the documents and imaging members FontDialogDemo and SlidePuzzleDemo
  still need. Lane C: emit `ItemsPanelTemplate` as `Factory` so `ItemsPanelTemplate.Content` can go.
  Store-only gaps: the accent keys have no GTK source, and `FrameworkTemplate.Seal` does not lock the template.
- **Round 60 (lane B) track 2 - documents-imaging-tail (DONE, merged).** `gf60b/documents-imaging-tail`: `gf57b`
  re-applied onto `main` without `06cd79ce` (its detached-List `TextRange` fix is already on `main`). Fixed and XPS
  documents (`FixedPage` as a `FrameworkElement` with its own `Children`, `PageContent`, `Glyphs`, XPS read/write and
  copy), the freezable `ImageSource` family (`CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`,
  `TransformedBitmap`), freezable `TextDecoration` collections and `DocumentViewerBase.IsMasterPage`. Samples:
  DocumentMerge, PhotoViewerDemo, PhotoStoreDemo and ParticlesDemo are past their imaging/document walls and now stop
  on other lanes' items (see `docs/samples-coverage.md`, Round 60 hand-offs).
- **Round 60 (lane B) track 4 - binding-engine (DONE, merged).** `gf60b/binding-engine-targets`: bindings on
  non-element targets (freezables, animation To/From, `CollectionViewSource`) resolve through the inheritance context;
  ElementName bindings set before the target is in the tree resolve when the name scope becomes reachable and keep
  following their source (generated `InitializeComponent` order); prefixed owners such as `(local:Data.Label)`;
  `DataTrigger`/`MultiDataTrigger` honour RelativeSource, ElementName and converters and compare a string `Value` to a
  boxed int as WPF does. Samples: AnimationExamples gets past the binding wall (next stop: `TextEffect.Transform`).
- **Round 60 (lane B) review (Opus) + fixes - DONE.** Applied failing-first. **a-R1** `PageContent.Child` is
  write-once as in WPF: null throws `ArgumentNullException`, a second set or `AddChild` throws
  `InvalidOperationException`, and a `Source` no longer blocks it (the reload path replaces the child internally).
  **a-R2** the `DocumentViewer` test now shows the viewer in a window and asserts the page's 200x100 size and the
  child's `Gtk.Fixed` placement against an unplaced sibling (it goes red when `FixedPage.Left` is ignored).
  **b-R60b-1** context listeners are indexed per target and per ElementName; a tree change walks only the changed
  subtree (inheritance children plus freezable holders), so building a
  bound tree is linear (t(2000)/t(500) was 24.5, now under 6). **b-R60b-2** a freezable with a live binding reports
  `CanFreeze == false` and `Freeze()` throws, as does any group, brush or collection holding one; `ClearBinding`
  makes it freezable again. **b-R60b-3** a multi-step path on an element source (`ElementName=b0, Path=Tag.Rule`)
  keeps listening through the `PathWatcher` and re-subscribes downstream steps. **b-R60b-4** an unknown attached
  owner is scanned once (misses are cached and dropped on `AssemblyLoad`). **b-R60b-5** frozen freezables get no
  inheritance context and one held by two objects has none, as in WPF. **b-R60b-6** the Resources/Owner wiring and the
  NameScope notifications moved into partials, `DependencyObject.cs` is back to 204 lines, long lines wrapped and the
  now-unused `BindingTarget.IsUnder` removed.
  Rejected: **CI-1** (the null-owner `InvalidOperationException` in `TextTreeEdit.Split.cs`) is not a defect: after
  dropping `06cd79ce`, `main`'s fix covers `ListItemCollection.Remove`/`Clear` (975 passed in the merged lane) and a
  paragraph in a table cell fails earlier with "The element is not in a text container". **CI-2** and **CI-3** are
  recorded as hand-offs in `docs/samples-coverage.md`.
- **Round 60 (lane B) follow-up.** **b-R60b-1** dead ElementName registrations are pruned in `_byName` when a list
  doubles on registration (and on unregister), so rebuilt trees no longer grow it without bound; a name registered
  through an element re-resolves only the listeners below that element (a new scope owner likewise, filtered to
  ElementName users), so per-item name scopes are linear; a leaf with no listener does not allocate. A populated
  subtree walk still allocates its result array. **b-R60b-5** `ResourceDictionary` owners now go through
  `AddContextHolder`, so a freezable held by a dictionary and by another element has no context in either order.
- **Round 60 (lane B) regression fix.** ElementName listener lists no longer leak (pruned at registration);
  `ResourceDictionary.Owner` change clears every `DependencyObject` context (CollectionViewSource included); name
  notification attribution is cleared before re-resolve callbacks run.
  Targeted: 477 PresentationFramework + 10 WindowsBase passed; samples gate 197 build, 182 run.
- **Round 60 (lane B) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. 14 new or
  rewritten tests. Targeted `PresentationFramework.Tests` (Binding, DataTrigger, ElementName, NonElement, Prefixed,
  Owner, Storyboard, ResourceDictionary, Freezable, Media, Document, Fixed, Xps, Imaging, TextDecoration, Round49a,
  Style, Name, Animation, Xaml): 2585 passed, 2 skipped. `WindowsBase.Tests`: 286 passed.
  Samples gate: all 197 build=pass projects still build (0 errors) through `Gtk4.Forms.Wpf` packed from this
  worktree, and all 182 run=runs apps still start and stay up (24 needed a lone 20 s re-check, where each showed its
  window).
  Carry-over: lane C registers `Int32RectConverter`/`PixelFormatConverter` and fixes the ParticlesDemo `x:Name` in a
  `Model3D` tree and `x:Name` inside `TextEffect.Transform`; lane A takes `UIElement.TranslatePoint`,
  `AdornerLayer.GetAdornerLayer`, a typed `SolidColorBrush.Clone()` and the `TextBlock`/`TextBox` members.
- **Round 61 (lane C) track 1 - startup and generator sweep III (DONE, merged).** `gf61a/startup-generator-sweep-3`
  (`414525fe`): unit suffixes `px`/`in`/`cm`/`pt` on `LengthConverter` properties (Brushes `Margin="12pt"`), the
  base-qualified `GradientBrush.GradientStops` property element (HexSphereDemo), an explicit `Viewport3D.Children`
  (PhotoFlipperDemo, via `UnwrapExplicitCollections`), prefixed `clr-namespace` commands in `CommandConverter`
  (AnnotatedDocumentViewer), content properties of project-declared types (Per-FrameAnimation), the Grid
  `RowDefinitions`/`ColumnDefinitions` shorthand (WPFGallery), the plain-`Name` rule (`IsNamingAttribute`), and the
  `RenderOptions` attached properties. The idea in `gf39/xaml-content-model` is re-done as a compile-checked generator
  corpus test (the branch is not rebased). Measured: see the gate in the totals bullet.
- **Round 61 (lane C) track 5 - packaging: NOT merged.** `gf61b/packaging-permissions` carries no commits (it still
  points at `4fd5749d`), so there is nothing to merge: EditingExaminerDemo's `System.Security.Permissions` dependency
  and the WPFGallery `Microsoft.Win32.SystemEvents` leftovers stay open. The package's dependency list is unchanged.
- **Round 61 (lane C) review + fixes - DONE.** Verified findings applied failing-first. **a-F1** `Timeline` carries
  `[RuntimeNameProperty("Name")]` (new `Timeline.RuntimeName.cs`), so `<Storyboard Name="sb">` and named animations get
  their field and `RegisterName` again (sweep III's plain-`Name` rule had dropped them); tests include a code-behind
  that calls `sb.Begin(this)`. **a-F2** `Command="local:MainWindow.ExitCommand"` on a project type resolves at compile
  time: the code-behind is scanned for a static member (then `Member + "Command"`) and emits `global::<owner>.<member>`,
  else the runtime converter stays the fallback. **a-F3** the keyed-resource fallback no longer stores element text as
  a string: only `String`/primitive/decimal value elements keep it; a `Color` of `R`/`G`/`B`/`A` or `ScA`/`ScR`/`ScG`/
  `ScB` literals becomes `Color.FromArgb`/`FromScRgb` (A defaults to 0 as `default(Color)`), a text-only
  converter-backed struct goes through `ConvertByType`, an attribute-less instantiable object is `new`ed, and anything
  else gets no entry plus `Skipped resource <Name> (line N)`. **a-F4** `RenderOptions` cache-invalidation thresholds
  default to 0.707 and 1.414 as WPF. **a-F5** `UnwrapExplicitCollections` detaches the nodes before re-parenting, so
  their line info survives. **a-F6** the unit-suffix test used a plain `double` (`ArcSegment.RotationAngle`), which WPF's
  `DoubleConverter` rejects; it is now `Infinity` routed through the converter. **a-F7** the Grid shorthand applies to
  Grid subclasses, including project types through their declared base. Rejected in triage: CI-1 (already fixed:
  `IsNamingAttribute` gates `NameScopes` and the generator; the `Timeline` regression it caused is a-F1), CI-2 (already
  fixed: keyed elements with attributes are built, `Keyed_element_with_attributes_in_a_dictionary_is_built_not_stringified`;
  the string fallback is a-F3), CI-3 (already fixed: `RenderOptions` exists and the generator emits `Set*`; the default
  mismatch is a-F4; `VisualBrush.Visual={Binding}` is lane B), CI-4 (already fixed: the `CommandConverters` remark points
  to the resolver-taking overload).
- **Round 61 (lane C) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. Targeted tests (the
  suites run in full at integrate): `MarkupCompiler.Tests` 716 (715 passed, 1 skipped, 0 failed);
  `PresentationFramework.Tests` `RenderOptions`/`Timeline`/`Storyboard` 88 passed; 13 test methods added, 7 updated
  (the four that locked in the string fallback now assert no entry). Samples gate: all 197 projects that built at
  `85493c8b` build through a lane-private feed (two builds each; CustomApplicationClass needs its sibling client built
  first), and all 182 apps that ran still run (172 in a crowded first pass; the 10 without a window handle re-checked
  alone for 20 s: all alive with a window). The gate caught one regression (XmlnsBind: `XmlDataProvider` with an
  unbuildable `XmlNamespaceManager`, used by a `{StaticResource}`, lost its entry and threw at load); that case keeps a
  placeholder entry and a message, pinned by a test.
- **Round 61 (lane C) follow-up.** The keyed-resource placeholder rule now counts `{StaticResource}` references across
  every XAML file of the project, not only the file being generated: `MarkupCompileTask` pre-scans the XAML items into
  `ProjectStaticResourceKeys` (attribute and `<StaticResource ResourceKey=.../>` element forms), so an unbuildable
  resource in `App.xaml` or a shared dictionary keeps its entry for other files (WPFGallery `WrapPanelTemplate` and
  `GitHubIconGeometry`, DocumentSerialization `ZoomIconDrawingImage`). The scan is by key name, so a same-named key in
  another file over-keeps a placeholder, never drops one; keys written as `{x:Type ...}` are not matched. Without the
  task (direct generator use) the old same-document search applies, now also by key text only. The carry-over wording
  above ("not referenced by a `{StaticResource}`") means "by no XAML file of the project". Grid
  `RowDefinitions`/`ColumnDefinitions` shorthand tests the property name before `IsGrid`, so plain attributes no
  longer scan the code-behind sources.
- **Round 61 (lane C) carry-over.** Packaging (EditingExaminerDemo, WPFGallery `SystemEvents`) is unstarted. Keyed
  elements that are unbuildable and not referenced by a `{StaticResource}` get no `Resources` entry (a later
  `FindResource` for them throws as in WPF for a missing key); `XmlDataProvider.XmlNamespaceManager` is still not built.

- **Round 62 (lane A) track 1 - automation attached properties (DONE, merged).** `gf62a/automation-attached-properties`:
  `AutomationProperties.HeadingLevel` (`AutomationHeadingLevel`), `ItemStatus` and `AcceleratorKey` with WPF's
  registration and validation, read by `UIElementAutomationPeer`'s `GetHeadingLevelCore`, `GetItemStatusCore` and
  `GetAcceleratorKeyCore`; a menu item falls back to `InputGestureText`; the accelerator key is pushed to GTK
  `KeyShortcuts`. Samples: DataBindingDemo fail→runs, ExpenseItDemo's automation wall gone, WPFGallery's
  `HeadingLevel` wall gone (it now builds and hits the run-time `ControlCornerRadius` wall).
- **Round 62 (lane A) track 3 - window adorner layer (DONE, merged).** `gf62b/window-adorner-layer`: `Window` owns an
  implicit adorner layer hosted in a `Gtk.Overlay` around the content, so `AdornerLayer.GetAdornerLayer` finds a layer
  for any element in a window. Samples: PhotoStoreDemo crash→runs.
- **Round 62 (lane A) track 4 - controls tail IV (DONE, merged).** `gf62c/datagrid-length`: `DataGridLength`,
  `DataGridLengthUnitType`, `DataGridLengthConverter`, `DataGrid.ColumnWidth` and a `DataGridLength`-typed
  `DataGridColumn.Width`. Samples: ExpenseItDemo fail→runs (with track 1).
- **Round 62 (lane A) review + fixes - DONE.** Verified findings applied failing-first. **a-F1**
  `MenuItemAutomationPeer.AffectsAccessible` includes `InputGestureText`, so a later change re-pushes `KeyShortcuts`
  (an explicit `AcceleratorKey` still wins). **a-F2** the automation tests assert the peer defaults, null rejection for
  both string properties, and observe pushes through a new internal `PeerAccessible.Spy` seam (fails if
  `SyncKeyShortcuts` is removed). **b-F1** `Reposition` uses `ComputeBounds(overlay)` (border box in overlay
  coordinates) instead of `ComputePoint(main)`; the placement tests assert bounds equality, including an element inside
  a padded, bordered `Border`. **b-F2** a placed adorner widget is `CanTarget=false` (restored on unplace), so picks
  over the adorned element reach it (PhotoStoreDemo's image `MouseDown` runs). **b-F3** an adorned element that is
  unmapped, collapsed or widget-less hides the adorner and leaves its geometry alone; it returns, respecting the
  adorner's own `Visibility`. **b-F4** the "does not grow the content" test can now fail (a 5000x5000 adorner widget;
  it fails with `SetMeasureOverlay(widget, true)`). **b-F5** `Window.OnFrameLayout` calls
  `ImplicitAdornerLayer.RepositionAll()`; the test moves the adorned element by a GTK-only size request on a sibling,
  because a direct `Window.SizeAllocate` raises no frame-clock layout (nothing runs to observe it). **b-F6**
  `WindowAdornerLayerTests` split, tree pins in `WindowAdornerLayerTreeTests`. **c-F1** `DataGridLength.ToString()` is
  culture-invariant. **c-F2** `_unitValue` is the value except for Auto (1.0), as dotnet/wpf; the statics are
  constructed as `(AutoValue, unit, 0d, 0d)` as in the source (so `new DataGridLength(1, SizeToHeader) !=
  DataGridLength.SizeToHeader`). Spec mismatch flagged: the lane brief said the content-sized units normalise to 1.0;
  dotnet/wpf normalises only Auto. **c-F3** an infinite number converts with `NotSupportedException`. **c-F4** the
  `ApplyColumnWidths` comment now says a Pixel width is a header-only minimum size request; a test pins that a long
  header widens a Pixel(10) column. **c-F6** the column-width test pumps and asserts data-row `Hexpand`, also after a
  `ColumnWidth` change (fails without the cell loop).
- **Round 62 (lane A) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. Targeted
  `PresentationFramework.Tests` (`Adorner`, `Automation`, `DataGrid`, `Window`, `MenuItem`): 683 tests, 0 failed; the
  suites run in full at integrate. Samples gate: all
  207 projects build through a lane-private feed and all 191 apps that ran at `17c64167` still run (see
  `samples-coverage.md`, "Round 62 (lane A) gate"). Checks: ExpenseItDemo's `CreateExpenseReportDialogBox` opens and
  `ColumnWidth="*"` reaches the grid; a native click on PhotoStoreDemo's image reaches `OnMouseDown`.
- **Round 62 (lane A) follow-up.** Reviewer findings verified; fixes failing-first. **b-F3** `Reposition` now takes
  Visible from `VisibilityGtkMap` (Hidden stays mapped at opacity 0), so Hidden -> Visible shows the adorner again.
  **b-F2** a `UIElement.SuppressCanTarget` flag makes `ApplyCanTarget` keep a placed adorner untargetable through
  Visibility/IsHitTestVisible/IsEnabled changes; `Unplace` re-applies the current state instead of a stale snapshot.
  **b-F5** `AdornerDecorator` layers now reposition on the overlay's frame-clock layout (`AdornerLayer.Frame.cs`), so an
  adorner hidden while its element was unmapped returns. **c-F2** `DataGridLengthConverter` builds fresh
  `new DataGridLength(1.0, unit)` values (NaN desired/display, equal to WPF's), not the statics. **c-F4** the Pixel
  header test now shows the grid, pumps and asserts the header's allocated width. **Capture check (b-F2)**: with a
  placed (CanTarget=false) adorner, `CaptureMouse()` succeeds and `RaiseMouseMove`/`RaiseMouseButton` on the adorned
  element are re-targeted to the adorner (`Mouse.GetInputTarget` is managed, independent of the adorner's pick state);
  tested, not driven by a native drag. **b-F5 test gap kept**: the window-width (centred horizontal move) case stays
  uncovered; `Window.SizeAllocate` raises no frame layout. Full `PresentationFramework.Tests` at `328b893e`: 6156
  passed, 0 failed, 3 skipped (6159). Targeted after the fixes (`Adorner`, `DataGrid`, `Window`, `MouseCapture`,
  `Visibility`): 529 passed, 0 failed.
- **Round 62 (lane A) carry-over.**
  - `AutomationElementIdentifiers.HeadingLevelProperty` (WPF 4.8+), `AutomationElement.HeadingLevelProperty` and a
    `PeerProvider`/`PropertyCatalog` case are not implemented.
  - Hand-off to lane B: `XamlValueConverter` has no `[TypeConverter]` fallback, so `DataGridLength` registers itself in
    its static constructor. That is safe on dependency-property paths (the DP field initialiser touches
    `DataGridLength` first), but type-only paths miss it: `ConvertByType(typeof(DataGridLength), "*")` before first
    use returns the raw string, and so do `XamlObjectBuilder.Members` for non-DP members,
    `XamlValueConverter.TextConversion` and `XamlObjectWriter.Values`. Lane B should register `DataGridLength` in
    `RegisterBuiltIns` and add `System.Windows.Controls.DataGridLength` to `ConverterBackedStructs` (the drift test
    mirrors it), or add a general `TypeDescriptor` fallback. `[TypeConverter]` types in the `ThemeMode` style have the
    same gap.
  - The generator does not emit `DataGrid.Columns` (a property element), so ExpenseItDemo's dialog grid has no columns
    (lane B).
  - Window's implicit adorner layer is store-only with respect to the visual tree: `VisualTreeHelper.GetChild(window,
    0)` is still the content, where WPF's template has `Border > AdornerDecorator`. Popups and
    `ScrollContentPresenter` have no implicit adorner layer, where WPF gives each one.
  - Adorner-drawn interactive content is not pickable (placed adorner widgets are `CanTarget=false`). Whether
    `Mouse.Capture`/`CaptureMouse` on an adorner routes events through the window-level controller (as
    `RubberbandAdorner` does) is checked at the managed level (see the follow-up); no native drag was driven.
  - Explicit `AdornerDecorator` layers reposition on the overlay frame layout (follow-up); they are not
    driven by the window hook.
  - A Pixel `DataGridColumn.Width` is a minimum size request (WPF clips); star weights other than equal are store-only.
- **Round 63 (lane B) track 2 - startup and generator sweep IV (DONE, merged).** `gf63b/startup-generator-sweep-4`
  (`2ca1bdb8`): an object-typed attribute literal stays a string (SlidePuzzleDemo `Tag="1"`), `TextBlock.TextEffects`
  with names registered inside `TextEffect.Transform` (AnimationExamples), `CommandBindings`, inline styles,
  `UIElement.OpacityMask`, `Int32RectConverter`/`PixelFormatConverter` (mirrored in `ConverterBackedStructs`), and the
  most common 'Skipped property element' cases. The `gf39/xaml-content-model` idea stays the compile-checked corpus
  test of Round 61 (the branch is not rebased).
- **Round 63 (lane B) track 5 - packaging: NOT merged.** `gf63b/packaging-permissions` delivered nothing (Round 61's
  attempt was BLOCKed before any commit, and this one did not ship either), so there is nothing to merge:
  EditingExaminerDemo's `System.Security.Permissions` dependency (verify the net10.0 reference assembly,
  SYSLIB0003 staying a warning, pin it, nuspec test) and the WPFGallery `SystemEvents` leftovers stay open lane work,
  not a defect in the merged code.
- **Round 63 (lane B) track 6 - rendering hygiene (DONE, merged).** `gf63b/rendering-hygiene` (`a0e4d8cb`): the
  DrawFunc `Cairo.Context` is disposed deterministically on the GTK thread (the 'Cairo.Context is leaking' finalizer
  warnings of Per-FrameAnimation and ParticlesDemo), and textures scaled below `MinTextureScale` are not applied (the
  Gsk 'out of memory' frame failures of ParticlesDemo).
- **Round 63 (lane B) review + fixes - DONE.** Applied failing-first. **a-F1** the TextEffects generator emits
  `if (x.TextEffects == null) x.TextEffects = new TextEffectCollection();` once per property element before the first
  `Add`: `TextElement.TextEffects` and `FlowDocument.TextEffects` default to null, so `<Run.TextEffects>` threw
  `NullReferenceException` in `InitializeComponent`. **a-F4** `TextBlock.TextEffects` is a plain
  `GetValue` (nullable): the old first-read `SetValue` made a local value that shadowed a Style setter and invalidated
  the visual. Remaining deviation: the default is null, not WPF's empty frozen collection. **a-F2** unsupported
  attributes on a `CommandBinding` child (`PreviewExecuted`, `PreviewCanExecute`) are reported as `Skipped attribute
  <name> on CommandBinding: not available in the layer`; the supported handlers stay wired. **a-F3** run-time tests:
  the generated code is compiled and run, so `Tag="1"` is a string and `FindName("TextEffectRotateTransform")` returns
  the `RotateTransform`; the object key frame test is text-only plus AssertCompiles (the layer has no
  `ObjectAnimationUsingKeyFrames`/`DiscreteObjectKeyFrame`, so the generator drops that animation and the literal rule
  cannot reach it, before and after the sweep);
  mutation-checked (reverting the object-literal rule turns the first red). **a-F6** `Int32Rect` 'Empty' is compared
  ordinally as in WPF ('empty' is a `FormatException`); `FormatException` for token counts and unknown pixel formats is
  this layer's convention (WPF throws `InvalidOperationException` / `ArgumentException`, the latter from memory).
  **c-N2** a degenerate texture on a triangle with at least 1 px² of screen area falls back to the solid colour instead
  of painting nothing. **c-N1** the sub-pixel replay theory is renamed `_smoke`: the cairo image replay always reports
  Success. **c-N3** test leaks fixed (`ImageSurface` disposed; `Graphene.Rect.Alloc` instead of a never-freed
  `AllocHGlobal`). **c-N4** the `MinTextureScale` remark is corrected and says the 1e-8 threshold is empirical.
  **CI-2** `Packed_nuspec_declares_SystemEvents_10x_in_every_dependency_group`. Rejected in triage: a-F5 (not a
  defect: the `ComboBox` `CreateWidget` guard prevents a stack overflow and `UIElement.OpacityMask` matches WPF; both
  sit outside Track 2's ownership, so integrate should check them for conflicts), CI-1 (already done: the imaging
  converters are registered and mirrored), CI-3 (outside the lane: `Data/**`, `XmlDataProvider`,
  `FlowDocument.TextEffectsProperty` identity, `SystemEvents` off Windows; the a-F1 guard removes the crash), CI-4
  (`gf39/xaml-content-model` is superseded by `GeneratorContentModelCorpusTests`; only integrate may delete it).
- **Round 63 (lane B) totals.** `dotnet build Gtk4.Wpf.sln -nodeReuse:false` 0 warnings, 0 errors. Targeted tests (the
  suites run in full at integrate): `MarkupCompiler.Tests` `GeneratorSweep4*` 42 passed; `PresentationFramework.Tests`
  `TextBlockTextEffects`/`Viewport3DDegenerate`/`CairoDrawFunc`/`XamlValueConverterImaging`/`Round49aConverter` 56
  passed; `ProjectConverter.Tests` `PackedNuspec` 2 passed.
  Samples gate: all 204 projects that built at `17c64167` build through a lane-private pinned feed (two builds each),
  and all 191 apps that ran still run and stay up for 10 s (20 with no window handle at 10 s re-checked alone for 20 s:
  all alive with a window; OpacityMasking needed 35 s and shows its window). A first pass packed a stale MarkupCompiler
  (a mutation-check build left in `bin`) and showed a false 2DTransforms failure (`Text = 0`); re-packed from a clean
  solution build it passes.
- **Round 63 (lane B) follow-up.** The Run/Paragraph TextEffects guard is now checked by running generated  `InitializeComponent` (the hand-written null-guard test is removed); the object key frame test is documented as  text-only; the Viewport3D degenerate-texture test is renamed to match the corrected MinTextureScale wording; two  over-long test lines are wrapped. The MarkupCompiler run-time tests load the layer assemblies in the default load  context; the Linux and macOS CI legs still have to confirm they need no display.
- **Round 63 (lane B) carry-over.** Packaging (EditingExaminerDemo `System.Security.Permissions`, WPFGallery
  `SystemEvents`) is still unstarted. `TextElement.TextEffects`, `FlowDocument.TextEffects` and `TextBlock.TextEffects`
  default to null (not an empty frozen collection). `CommandBinding` still has only `Executed` and `CanExecute`.
- **Round 70 (lane J) review fixes (VisualBrush / TileBrush).** Measured causes per sample tab: Painting with Visuals
  (black tiles: the pre-3fb62afe fill painted Cairo's default black, then an empty detached source and a 1-pixel
  nested tile), Magnifying Glass (the source was never laid out or mapped, and a Canvas/StackPanel painted no tile
  background), Reflection (DrawingGroup children had no inheritance context; a late `Width`/`Height` binding never
  reached the widget). Review fixes: **J70-1** detached sources no longer get their own toplevel (it took activation):
  they sit in a non-targetable, non-focusable overlay layer far outside the app window, no `Present`. **J70-2**
  `OpacityMask` of a child of a Backdrop panel is pushed around that child's whole render (CSS background and border
  included). **J70-3** a brush painted inside its own source drops the echo invalidation once two re-renders in a row are
  pixel-identical (bounded `Changed`); the source link is made at first paint, since an App.xaml resource sets `Visual`
  before GTK is initialised (GeometryDesignerDemo crashed in the gate). **J70-4** weak leases: the layer goes with the last brush. **J70-5** a detached source is re-measured and
  re-arranged on every render. **J70-6..11** pixel tests for RelativeTransform, Transform order, Flip modes, grid and
  reflection flip; Graphene rect owned and freed; device-resolution tile uses the transformed cell axes and caps total
  pixels; the late-size push only clears a request this code set; lines wrapped. Cross-lane hooks: lane G (Control
  `Background`) and lane K (`TextBlock.Foreground`) call `BrushPaint.Fill` from a Backdrop snapshot (BrushCss emits
  nothing for tile brushes); lane I (`Shape.OpacityMask`) is covered under a Backdrop panel and needs a snapshot hook
only for a Shape that is the window content; lane H `RenderingPainter` reads `VisualBrush.TryGetLiveSurface`.
- **Round 70 (lane J) review fixes, second pass.** The independent review blocked three regressions. **Host choice:**
  a detached source is hosted in the toplevel of the window that paints the brush (`BrushPaint.PaintingRoot`, set by
  every Backdrop snapshot; the active window otherwise), and every render re-checks that the layer is still mapped and
  re-hosts the source if not; with no window at all (`Shape.PaintTo`, offscreen rendering) the source is mapped by hand
  under a never-mapped `Fixed` and snapshotted through it, so it still renders. The layer is created from an idle
  callback, not inside the snapshot (no "snapshot GtkFixed without a current allocation" warning). **Liveness:** the echo
  is recognised by its cause, not by timing: invalidations during the 100 ms after the brush's own `Changed` are held,
  then one re-render decides whether the source really changed; a dirty brush always raises `Changed` eventually.
  **Transforms:** a masked child's mask is filled in the child's own space (`ComputeTransform`), so it turns with a
  `RenderTransform`. Lane J's gaps unchanged: no `Present`, no extra toplevel.

## Phased roadmap (after Phase 0 proves the spine)

1. **Core controls, layout, binding.** `StackPanel`/`DockPanel`/`Canvas`/`WrapPanel` with WPF
   `Measure`/`Arrange` (Tier 2), `TextBox`/`CheckBox`/`RadioButton`/`ComboBox`/`Image`/`Border`,
   two-way binding + converters, `ResourceDictionary` + `StaticResource`, `Style`+`Setter`.
2. **Templating & triggers.** `ControlTemplate`/`DataTemplate`, `ItemsControl`/`ListBox`/`ListView`,
   `Trigger`/`DataTrigger`/`EventTrigger`, implicit styles.
3. **Navigation, windows, dialogs, commands.** `NavigationWindow`/`Page`/`Frame`, `Window` chrome +
   `ShowDialog`, `ICommand`/`RoutedCommand`, `MenuItem`/`ToolBar`.
4. **Media & shapes & text.** `System.Windows.Shapes`, `Brush`/`Geometry`/`Transform`, `FlowDocument`
   subset, `RichTextBox` (hard).
5. **Long tail to 1:1.** The rest of the control set and the WPF features catalogued as app demand
   surfaces; `MultiBinding`, `VisualStateManager`, animation/storyboards, 3D (likely never).

Each phase ships a growing **WPF-sample gallery** (ported real WPF XAML, unmodified) that is the
regression suite.

## Verification

- **Compat gallery**: a project of *unmodified* WPF `.xaml` pages, one per feature, built with the
  markup compiler and run on Windows + Xvfb Linux + macOS, asserted via the GTK renderer-test harness
  (`Gtk4.Forms.Platform.Gtk.UnitTests` is the model).
- **Property-system unit tests** (pure): `DependencyProperty` precedence, inheritance, coercion,
  routed-event tunnel/bubble — fail-before/pass-after, no GTK needed.
- **Markup-compiler unit tests**: given WPF XAML in, assert the generated `InitializeComponent`/IL
  (mirror how `Gtk4.Forms.Xaml.UnitTests` drives `XamlCTask` in-process via `MockCompiler`).
- **Drop-in e2e**: a stock WPF repo + `Gtk4.Forms.Wpf` → cross-platform exe, no source edits.

## Risks

| Risk | Note / mitigation |
|---|---|
| **Scope — this is Avalonia-scale** | Phase 0 de-risks the architecture cheaply; everything after is incremental and demand-driven. Be explicit that "1:1" is a direction, not a date. |
| `DependencyProperty`/visual-tree semantics are subtle | Faithful property system is Phase-0, tested in isolation before anything renders. |
| WPF markup extensions + `System.Xaml` divergence | Compiler is `System.Xaml`-free (Cecil + `XmlReader`), so no Windows-only `System.Xaml` dependency; extension set grows per phase. |
| Tier-1/Tier-2 seam leaks | Keep `UIElement`→`Gtk.Widget` the single boundary; a control can move Tier 1→2 without API change. |
| `ControlTemplate`/triggers/`RichTextBox` | The genuinely hard long tail — scheduled late, flagged as "redesign, not 1:1" if a feature proves intractable on GTK. |
| Fights the active packaging refactor | New `Gtk4.Forms.*` projects are additive (own dirs); touch shared `Directory.Build.*` minimally. |

## Recommendation

Approve **Phase 0 only** first: the `WindowsBase` property-system subset + the forked markup compiler
(minimal) + four controls on Tier 1 + one binding + `Gtk4.Forms.Wpf`, proving **B1–B4**. That slice
is small, fully testable, and settles every architectural unknown (xmlns binding, the compiler fork,
the hybrid bridge, the drop-in). Only after it runs a real one-window WPF app unmodified on two OSes
is it worth committing to the multi-phase build toward 1:1.
