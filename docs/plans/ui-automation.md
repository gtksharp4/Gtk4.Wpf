# Plan — UI Automation for `Gtk4.Forms` (WPF-on-GTK), testable from code

**Goal.** Make every element of an unmodified WPF app running on `Gtk4.Forms` discoverable and drivable
**from test code**, using WPF's own UI Automation API, on Windows, Linux and macOS. A UI test should look
like a normal WPF UI Automation test (find by `AutomationId`, read properties, invoke patterns, assert),
run in `dotnet test`, and be deterministic in CI.

**Branch.** `plan/ui-automation` (worktree `.claude/wt/ui-automation-plan`), branched from `main` at
`70ba8a47`. Implementation happens on `ua/*` branches off `main` (see "Coordination").

## Where we are (verified on `main` at `70ba8a47`)

| Area | State |
|---|---|
| `System.Windows.Automation.AutomationProperties` | Exists (Round 34): `Name`, `HelpText`, `AutomationId`, `LabeledBy`, `IsRequiredForForm`, `LiveSetting`. Only `Name` and `HelpText` reach GTK (`AutomationAccessiblePush.cs`). |
| `System.Windows.Automation.Peers` | **Missing entirely.** No `AutomationPeer` and no `OnCreateAutomationPeer`. Blocks 4 official samples, including `WPFGallery` (`docs/samples-coverage.md`). |
| `System.Windows.Automation.Provider` (pattern interfaces) | Missing. |
| UIA client API (`AutomationElement`, `TreeWalker`, `Condition`, patterns) | Missing. On real WPF it lives in `UIAutomationClient`/`UIAutomationTypes`. |
| GTK accessibility | GtkSharp4 exposes the full GTK 4 model: `Gtk.AccessibleRole` (85 roles), `AccessibleState` (9), `AccessibleProperty` (20), and `AccessibleExtensions.UpdateProperty/UpdateState/UpdateRelation/InitValue`. |
| Out-of-process exposure | Linux: GTK 4 exports its accessible tree over AT-SPI. Windows/macOS: the bundled `gtk-4-1.dll` (4.22.4) contains `accesskit`, which suggests an AccessKit backend, but that is **unverified**. It would surface the tree to UIA and NSAccessibility. |
| Test harness | `WpfGtkFixture`/`WpfGtkTestBase` run tests on the GTK thread (STA on Windows, thread 0 on macOS); `GtkPump` refuses off-thread use. `Integration.Tests` builds and runs a converted sample but detects its window only on Windows (`NativeWindowProbe`). |
| Input in tests | Tests emit signals on the layer's own event controllers. Round 46/47 reviews showed this **skips native GTK handling** (e.g. `Gtk.Text` consuming Enter), so it cannot prove end-to-end behaviour. |

## Decisions

1. **The test API is WPF's UI Automation API, not a new one.** Peers (`System.Windows.Automation.Peers`),
   provider interfaces (`System.Windows.Automation.Provider`) and the client API
   (`System.Windows.Automation.AutomationElement` and the rest) are reimplemented in their real namespaces,
   like every other WPF type in this repo. Test code written for WPF UIA compiles and runs unchanged, and
   apps that define custom peers (`OnCreateAutomationPeer` overrides) compile.
2. **In-process first.** The client API is backed **directly by the peer tree in the same process**. That
   works identically on all three OSes, needs no OS accessibility service and is deterministic. Real WPF
   test code that runs in-process (e.g. `UIElementAutomationPeer.CreatePeerForElement(button)`, then
   `GetPattern(PatternInterface.Invoke)`) gets exactly that behaviour.
3. **The peer tree is the single source of truth**, and it is **pushed into GTK's accessible tree**
   (role, name, states, relations, value). Screen readers then see the same tree the tests see, and
   out-of-process automation (Phase 5) needs no second model.
4. **Three levels of interaction, chosen per test:**
   - **Semantic** (default): automation patterns (`InvokePattern.Invoke()`, `ValuePattern.SetValue()`,
     `TogglePattern.Toggle()`...). Deterministic; this is how most WPF UIA tests drive apps.
   - **Routed-input**: synthesise WPF input through the layer's input path (event controllers → routed
     events) for tests of `KeyDown`, `MouseDown` and focus.
   - **Native-input** (opt-in): real OS input injection (Win32 `SendInput`, X11 XTest, macOS `CGEvent`) for
     true end-to-end checks such as native widget key handling. These are known to be flakier in CI, so they
     are marked with a trait and gate only where they can run reliably.
5. **A test-driver package on top**, `Gtk4.Forms.Wpf.Testing`: app hosting, locators, waits and
   screenshots for xUnit. It is a thin, opinionated layer over the standard API, never a replacement for it.

## Architecture

```
 test code (xUnit)                      app under test (unmodified WPF, in-process)
 ─────────────────                      ─────────────────────────────────────────────
 Gtk4.Forms.Wpf.Testing  ──uses──►  System.Windows.Automation (client API, in-process)
  AppHost / Locator / Wait /              AutomationElement, TreeWalker, Condition,
  Screenshot                              InvokePattern, ValuePattern, ...
                                                │ backed by
                                                ▼
                                    System.Windows.Automation.Peers
                                      AutomationPeer ← UIElementAutomationPeer ←
                                      FrameworkElementAutomationPeer ← ButtonAutomationPeer, ...
                                      implements System.Windows.Automation.Provider.I*Provider
                                                │ pushes role/name/state/value
                                                ▼
                                    Gtk.Accessible (GTK 4 a11y tree)
                                                │ exported by GTK
                                                ▼
                                    AT-SPI (Linux) │ AccessKit → UIA / NSAccessibility (Win/macOS, if verified)
                                    = Phase 5 out-of-process
```

### Provider side (`Gtk4.Forms.PresentationFramework`, namespace `System.Windows.Automation.Peers`)

- `AutomationPeer` with WPF's full template-method surface: `GetChildrenCore`, `GetNameCore`,
  `GetAutomationIdCore`, `GetAutomationControlTypeCore`, `GetClassNameCore`, `GetBoundingRectangleCore`,
  `IsEnabledCore`, `IsOffscreenCore`, `HasKeyboardFocusCore`, `SetFocusCore`, `GetPattern`,
  `RaiseAutomationEvent`, `RaisePropertyChangedEvent`, `InvalidatePeer`, and the public non-`Core`
  wrappers.
- `UIElementAutomationPeer` (`CreatePeerForElement`, `FromElement`), `FrameworkElementAutomationPeer`
  (honours `AutomationProperties.*`, `x:Name` fallback as WPF does), `ContentElementAutomationPeer`,
  `ItemsControlAutomationPeer` with `ItemAutomationPeer`.
- `UIElement.OnCreateAutomationPeer()` (protected virtual) and the internal cache, exactly as WPF does.
- Control peers, in priority order taken from what the samples and typical tests use: `Button`,
  `ToggleButton`, `CheckBox`, `RadioButton`, `TextBox`, `PasswordBox`, `TextBlock`, `Label`, `ComboBox` (+
  items), `ListBox` (+ items), `ListView`, `TabControl` (+ items), `TreeView` (+ items), `Menu`, `MenuItem`,
  `Slider`, `ScrollViewer`, `ProgressBar`, `Image`, `Window`, `DataGrid` (+ rows and cells), `Expander`,
  `GroupBox`, `StatusBar`, `ToolBar`, `Hyperlink`.
- Provider interfaces (`System.Windows.Automation.Provider`): `IInvokeProvider`, `IValueProvider`,
  `IToggleProvider`, `ISelectionProvider`, `ISelectionItemProvider`, `IExpandCollapseProvider`,
  `IRangeValueProvider`, `IScrollProvider`, `IScrollItemProvider`, `IGridProvider`, `IGridItemProvider`,
  `ITableProvider`, `ITableItemProvider`, `IWindowProvider`, `ITransformProvider`; `ITextProvider` later.
- Types shared with the client (`System.Windows.Automation`, `UIAutomationTypes` on real WPF):
  `ControlType`, `AutomationProperty`, `AutomationEvent`, `AutomationPattern`, `PatternInterface`,
  `AutomationControlType`, `ToggleState`, `ExpandCollapseState`, `RowOrColumnMajor`, `ScrollAmount`,
  `WindowVisualState` and the rest.

### Client side, in-process (namespace `System.Windows.Automation`)

- `AutomationElement`: `RootElement` (the app's windows), `FocusedElement`, `FromPoint`, `Current` and
  `Cached` property views, `FindFirst`/`FindAll(TreeScope, Condition)`, `GetCurrentPattern`,
  `TryGetCurrentPattern`, `GetCurrentPropertyValue`, `SetFocus`, `GetClickablePoint`.
  `FromHandle(IntPtr)` throws `PlatformNotSupportedException`, a documented gap.
- `Condition`, `PropertyCondition` (including `PropertyConditionFlags.IgnoreCase`), `AndCondition`,
  `OrCondition`, `NotCondition`, `Condition.TrueCondition`/`FalseCondition`.
- `TreeWalker` (`RawViewWalker`, `ControlViewWalker`, `ContentViewWalker`), `TreeScope`.
- Pattern classes (`InvokePattern`, `ValuePattern`, `TogglePattern`, `SelectionPattern`,
  `SelectionItemPattern`, `ExpandCollapsePattern`, `RangeValuePattern`, `ScrollPattern`, `GridPattern`,
  `TablePattern`, `WindowPattern`...) wrapping the providers, with WPF's exceptions
  (`ElementNotEnabledException`, `ElementNotAvailableException`, `InvalidOperationException` for an
  unsupported pattern).
- `Automation.AddAutomationEventHandler`, `AddAutomationPropertyChangedEventHandler`,
  `AddStructureChangedEventHandler`, `AddAutomationFocusChangedEventHandler`, raised from the peers'
  events.
- Calls are marshalled to the GTK thread through the `Dispatcher`, so test code can call from the test
  thread as it would with real UIA. Each call returns after the dispatcher is idle, so property reads
  after an action see the result.

### GTK accessibility bridge

The peer pushes into `Gtk.Accessible` on realise and on every relevant change:

| WPF (`AutomationControlType` / property) | GTK 4 |
|---|---|
| Button | `AccessibleRole.Button` |
| CheckBox / RadioButton | `Checkbox` / `Radio`; `AccessibleState.Checked` (mixed for an indeterminate value) |
| Edit (TextBox) | `TextBox` (`SearchBox` for search); `MultiLine`, `ReadOnly`, `Placeholder` |
| ComboBox / List / ListItem | `ComboBox` / `List` / `ListItem`; `Selected`, `Expanded` |
| Tab / TabItem | `TabList` / `Tab` |
| Tree / TreeItem | `Tree` / `TreeItem`; `Level`, `Expanded` |
| Menu / MenuItem | `Menu` (or `MenuBar`) / `MenuItem` (`MenuItemCheckbox`/`Radio`) |
| Slider / ProgressBar / ScrollBar | `Slider` / `ProgressBar` / `Scrollbar`; value via `InitValue`/`ValueNow` |
| DataGrid / row / cell | `Grid` / `Row` / `GridCell` |
| Window | `Window` (`Dialog` when modal); `Modal` |
| `AutomationProperties.Name`, `HelpText`, `LabeledBy` | `Label`, `Description`, `AccessibleRelation.LabelledBy` |
| `IsEnabled=false`, `IsOffscreen` | `AccessibleState.Disabled`, `Hidden` |

Phase 0 decides from GtkSharp4 reflection whether the role can be set after widget creation. If it can't,
the role is applied at widget creation through the widget type's `accessible-role` property.

### Test driver (`Gtk4.Forms.Wpf.Testing`, new project and package)

- **`WpfAppHost`**: starts the app under test **in the test process** on the GTK thread (the
  `WpfGtkFixture` pattern, reused and made public), either from its `Application` type or from a
  `Window` factory. It exposes `MainWindow` as an `AutomationElement`, plus `Shutdown`.
- **Locators**: `app.Find(By.AutomationId("Ok"))`, `By.Name`, `By.ControlType`, combined conditions and
  scoping (`within(...)`). All of them are sugar over `FindFirst`/`FindAll`, with clear "not found" messages
  that dump the nearby tree.
- **Waits**: `WaitUntil(condition, timeout)`, implemented by pumping the GTK loop on its own thread, never
  `Thread.Sleep`. `WaitForIdle()` drains the dispatcher and GTK idle.
- **Actions**: semantic by default (`Click()` uses `InvokePattern`, `Type(text)` uses `ValuePattern`,
  `Check()` uses `TogglePattern`). `Input.Keyboard`/`Input.Mouse` provide routed input, and
  `NativeInput` (opt-in, `[Trait("UI","Native")]`) provides OS injection.
- **Evidence**: `Screenshot(element)` through `RenderTargetBitmap`/GTK snapshot. On failure the test gets
  the automation tree dump and a PNG.
- **xUnit integration**: `[WpfUiFact]`/collection fixture, so a suite shares one GTK thread, with parallel
  execution disabled as the harness requires.

```csharp
public class LoginTests : WpfUiTest<App>        // starts App in-process on the GTK thread
{
	[WpfUiFact]
	public void Login_enables_after_both_fields_are_filled()
	{
		var login = App.MainWindow.Find(By.AutomationId("Login"));
		Assert.False(login.Current.IsEnabled);

		App.MainWindow.Find(By.AutomationId("User")).AsValue().SetValue("ada");
		App.MainWindow.Find(By.AutomationId("Password")).AsValue().SetValue("secret");

		App.WaitUntil(() => login.Current.IsEnabled);
		login.AsInvoke().Invoke();
		App.WaitUntil(() => App.MainWindow.Find(By.Name("Welcome, ada")) != null);
	}
}
```

The same test written with the plain WPF API (`AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
new PropertyCondition(AutomationElement.AutomationIdProperty, "Login"))`, then
`((InvokePattern)e.GetCurrentPattern(InvokePattern.Pattern)).Invoke()`) must also work. That is the
compatibility check for Decision 1.

## Phases

Each phase merges on its own, with the existing rules: test-first, warning-clean on every TFM, Opus review,
and the pinned samples gate (no currently building or running sample may regress).

| Phase | Deliverable | Acceptance |
|---|---|---|
| **0. Spike (1–2 days)** | Verify: can GtkSharp4 set and update roles after creation; does GTK 4.22 on Windows/macOS expose the tree (AccessKit) to UIA / NSAccessibility (check with Accessibility Insights / Accessibility Inspector); AT-SPI exposure under Xvfb in CI (`accerciser` or the `pyatspi` dump). Prototype one `ButtonAutomationPeer` end to end. | A short findings section appended to this file; go/no-go for Phase 5 per OS. |
| **1. Peer core** | `AutomationPeer`/`UIElementAutomationPeer`/`FrameworkElementAutomationPeer`, `OnCreateAutomationPeer`, shared automation types, provider interfaces, peers for the first 12 controls (Button → Window), GTK push for role/name/state. | Peer tree mirrors the visual tree; patterns work by direct provider calls; `WPFGallery` and the other 3 `Automation.Peers` samples get past their peer errors; GTK roles visible in a real-GTK test. |
| **2. In-process client API** | `AutomationElement`, conditions, `TreeWalker`, pattern classes, exceptions, thread marshalling. | A suite of WPF-style UIA tests (written against the real API, compiled unchanged against real WPF as a cross-check where possible) passes on all 3 OSes in CI. |
| **3. Test driver** | `Gtk4.Forms.Wpf.Testing` package: `WpfAppHost`, locators, waits, semantic actions, routed input, screenshots, failure dumps. | UI tests for 5 official samples (e.g. `SimpleBinding`, `MasterDetail`, `EditingCommands`, `CursorType`, a dialog sample) run green in CI on Windows, Linux and macOS. |
| **4. Breadth** | Remaining control peers (ItemsControl family, `DataGrid` grid/table patterns, `TreeView`, `Menu`), automation events (property/structure/focus), `ITextProvider`/`TextPattern` (basic ranges), `AutomationProperties` fully pushed (`LabeledBy`, `LiveSetting` → GTK announcements). | Peer coverage for every control the layer implements; event tests; `AccessibilityTreeTests` snapshot per sample. |
| **5. Out-of-process (per OS, if Phase 0 says go)** | Linux: drive the app over AT-SPI from a separate test process. Windows/macOS: the same through AccessKit to UIA / NSAccessibility. Optional native-input tests (`SendInput`/XTest/`CGEvent`). | The same 5 sample tests pass out-of-process where supported; documented gaps where not. |

## Testing the automation layer itself

- **Unit (no GTK)**: peer property computation, condition evaluation, tree walking over a stubbed peer
  tree, pattern argument validation and exceptions.
- **Real-GTK** (`PresentationFramework.Tests`, inside `Run(() => ...)`): peers over real widgets,
  patterns change real widget state (`Invoke` clicks the `Gtk.Button`, `SetValue` sets the `Gtk.Entry`
  text), the GTK accessible role/state/name match the table above, and events fire once in the right
  order.
- **Driver tests**: `Gtk4.Forms.Wpf.Testing` drives `Gtk4.Forms.WpfSample` and the 5 sample apps.
- **Parity cross-check**: where a test can also run against real WPF on Windows, a separate CI job runs it
  there, so any behaviour difference is visible rather than assumed.

## Risks

| Risk | Mitigation |
|---|---|
| Scope: WPF UIA is large (peers for ~60 controls, 20+ patterns, the client API) | Phase by demand. The samples and the five reference test suites decide the order; anything else is store-only with a documented gap. |
| GTK roles may be fixed at widget construction | Phase 0 checks. The fallback sets the role at creation from the peer's control type. |
| AccessKit on Windows/macOS may be missing or partial | In-process automation (Phases 1–4) does not depend on it. Phase 5 is per-OS and optional. |
| Semantic actions bypass native input handling | Routed-input and native-input levels exist for the cases that need them, and the two levels are not mixed silently. |
| Thread affinity (GTK thread; thread 0 on macOS) | All client calls marshal through the `Dispatcher`; the driver owns the GTK thread exactly like `WpfGtkFixture`. |
| Flaky UI tests | No sleeps; waits pump the loop; serial execution; failure dumps (tree + PNG). Native-input tests carry a trait and do not gate where unreliable. |

## Coordination with the parity rounds

- The parity rounds are running in `.claude/wt/main`. Implementation of this plan uses `ua/*` branches off
  `main` in their own worktrees and merges between rounds, never during one.
- **File ownership.** This work owns `Gtk4.Forms.PresentationFramework/Automation/**` (new
  `Automation/Peers`, `Automation/Provider` and client files), the new `Gtk4.Forms.Wpf.Testing` project and
  its tests. The only edits to shared types are additive partial files (`UIElement.Automation.cs` for
  `OnCreateAutomationPeer` and the peer cache), plus one-line `OnCreateAutomationPeer` overrides in control
  partials.
- The parity scout must not pick `System.Windows.Automation.Peers` (4 samples) while this plan is active.
  Record that in `docs/samples-coverage.md` "Next rounds" when Phase 1 starts.
- A new project must be added to both `Gtk4.Wpf.sln` and `Gtk4.Wpf.CrossPlatform.slnf`.

## Open questions

1. Should the driver package also host the app **out-of-process** (launch the converted app and attach)
   before Phase 5, using an in-app automation server over a named pipe? That gives process isolation on
   every OS without AccessKit, at the cost of a small custom protocol. The recommendation is to decide
   after Phase 3.
2. Package naming: `Gtk4.Forms.Wpf.Testing`, or ship the client API in its own package
   (`Gtk4.Forms.UIAutomationClient`) so non-test tools can reference it without the driver.
