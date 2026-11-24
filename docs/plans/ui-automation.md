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
| Out-of-process exposure | Linux: GTK 4 exports its accessible tree over AT-SPI. Windows: the bundled `gtk-4-1.dll` (4.22.4) has AccessKit **disabled at build time**, so UIA sees no GTK tree (Phase 0). macOS: unverified. |
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
  `ITableProvider`, `ITableItemProvider`, `IWindowProvider`, `ITransformProvider`; `ITextProvider` (Phase 4, flat character model).
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
| Edit (TextBox) | `TextBox` (`SearchBox` for search); `MultiLine`, `ReadOnly`, `Required` (`Placeholder` later). A multi-line box pushes to its inner `TextView`, not the `ScrolledWindow` |
| ComboBox / List / ListItem | `ComboBox` / `List` / `ListItem`; `Selected`, `Expanded` |
| Tab / TabItem | `TabList` / `Tab` |
| Tree / TreeItem | `Tree` / `TreeItem`; `Level`, `Expanded` |
| Menu / MenuItem | `Menu` (or `MenuBar`) / `MenuItem` (`MenuItemCheckbox`/`Radio`) |
| Slider / ProgressBar / ScrollBar | `Slider` / `ProgressBar` / `Scrollbar`; value via `InitValue`/`ValueNow` |
| DataGrid / row / cell | `Grid` / `Row` / `GridCell` |
| Window | `Window` (`Dialog` when modal); `Modal` |
| `AutomationProperties.Name`, `HelpText`, `LabeledBy` | `Label`, `Description`, `AccessibleRelation.LabelledBy` |
| `AutomationProperties.LiveSetting`, `LiveRegionChanged`, `Notification` events | `gtk_accessible_announce` (GTK 4.14+), high priority for `Assertive` and `ImportantAll`/`ImportantMostRecent`, medium otherwise; no persistent GTK property |
| `IsEnabled=false`, `IsOffscreen` | `AccessibleState.Disabled`, `Hidden`: GTK derives both from sensitivity and visibility, so the peer does not push them (verified for `Disabled`) |

GtkSharp4 rules found in Phase 1: use the `Update*` array overloads (the single-value ones leave the value unchanged), and pass states as `int` values (tristate and boolean states ignore a boolean `GLib.Value`). Phase 0 found that the role can only be set before the widget is realized, so the role is applied at widget creation (`AccessibleRole` on the widget the peer targets). Only explicit names (`AutomationProperties.Name`, then the labelling element) are pushed; a name GTK derives from content stays with GTK, and clearing an explicit value calls `gtk_accessible_reset_property` (GtkSharp4 does not bind it, so `PeerAccessible.Native.cs` P/Invokes it). `Label` (a `Gtk.Box`) and the editable `ComboBox` host push to their inner widget through `Target()`, so the AT tree has no nested duplicates.

### Test driver (`Gtk4.Forms.Wpf.Testing`, new project and package)

- **`WpfAppHost`**: starts the app under test **in the test process** on the GTK thread (the
  `WpfGtkFixture` pattern, reused and made public), either from its `Application` type or from a
  `Window` factory. It exposes `MainWindow` as an `AutomationElement`; `Dispose` closes the windows.
- **Locators**: `app.Find(By.AutomationId("Ok"))`, `By.Name`, `By.ControlType`, combined conditions and
  scoping (`element.Find(...)` searches below that element). All of them are sugar over `FindFirst`/`FindAll`, with clear "not found" messages
  that dump the nearby tree.
- **Waits**: `WaitUntil(condition, timeout)`, implemented by pumping the GTK loop on its own thread, never
  `Thread.Sleep`. `WaitForIdle()` drains the dispatcher and GTK idle.
- **Actions**: semantic by default (`Click()` uses `InvokePattern`, `Type(text)` uses `ValuePattern`,
  `Check()` uses `TogglePattern`). `Input.Keyboard`/`Input.Mouse` provide routed input, and
  `NativeInput` (opt-in, `[Trait("UI","Native")]`) provides OS injection.
- **Evidence**: `Screenshot(element)` through the GTK snapshot (`RenderTargetBitmap` renders only a `DrawingVisual`). On failure the test gets
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
		App.WaitUntil(() => App.MainWindow.TryFind(By.Name("Welcome, ada")) != null);
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

## Phase 0 findings

Spike run on Windows 11, GtkSharp4 `4.22.4.26325`, runtime `%LOCALAPPDATA%\Gtk\4.22.4` (gvsbuild). The
scratch projects were deleted after the spike.

**1. Roles can be set until the AT context is realized, not after.**
- Reflection over `GtkSharp.dll`: `Gtk.Widget.AccessibleRole { get; set; }` is the generated wrapper of the
  GObject property `accessible-role` (`[GLib.Property("accessible-role")]`, not `[Obsolete]`). The same
  setter exists on `IAccessible` and `ATContext`. There is no other role API.
- Real GTK probe: `new Gtk.Button()` has role `Button`. Setting `Checkbox` before the window is presented
  works: the widget, its `ATContext` and `gtk_accessible_get_accessible_role` all report `Checkbox`. After
  `Present()` the context is `Realized == true`. Setting a new role then logs
  `Gtk-CRITICAL: Widget of type "GtkButton" already has an accessible role of type
  "GTK_ACCESSIBLE_ROLE_CHECKBOX"` and the role stays unchanged.
- While `AtContext.Realized` is false, the role can still be changed: on a fresh widget, on a widget that
  was added to a shown window after it was shown, on a hidden child and on a widget that was removed from
  its parent. When realization happens depends on the backend.
- **Decision:** the peer applies the role in `ApplyAutomationOnCreated`, right after `CreateWidget` and
  before the widget is parented, from `GetAutomationControlType()`. Name, states and value can be updated
  at any time with `UpdateProperty/UpdateState`. If a role changes after realization, the push is skipped
  (`AtContext.Realized` guard) and that is a documented gap. Built-in WPF control types are fixed per class,
  so only custom peers whose control type changes at runtime are affected.

**2. Windows: GTK does not reach UI Automation.** The bundled GTK is built without AccessKit.
- `GTK_A11Y=help` prints `accesskit - Disabled during GTK build`, `atspi - Not available on this
  platform`, plus `test` and `none`. Every widget's context is a `GtkTestATContext`. The DLL has no
  `accesskit_*` and no `UiaReturnRawElementProvider` strings: the AccessKit name in it is only the backend
  table entry.
- UIA probe (Windows PowerShell `UIAutomationClient`) against a shown GTK window with a labelled button,
  a check button and an entry. The window appears as `Name='<title>'`, `ControlType.Window`, class
  `gdkSurfaceToplevel`, framework `Win32`, `WindowPattern` only. That comes from the HWND, not from GTK.
  Descendants: **0** (raw view and `FindAll`). A desktop-wide search by the button's accessible name:
  not found.
- Exposing the tree to UIA would need a GTK built with `-Daccesskit=enabled`. gvsbuild support for that
  is unverified.

**3. macOS and Linux could not be tested on this machine.**
- Linux: the AT-SPI backend is GTK 4's platform backend. Indirect evidence: the Windows build calls it
  "not available on this platform", and CI already installs `dbus-x11` because the GTK 4 a11y stack
  wants a session bus. A tree dump under Xvfb (`at-spi2-core`, `python3-pyatspi`, `dbus-run-session`)
  has not been run yet. The local WSL has no GTK 4.
- macOS: the Homebrew `gtk4` used in CI is a different build. Whether its AccessKit is enabled is
  unknown. Checking it means running `GTK_A11Y=help` and looking at a window in Accessibility Inspector
  on the runner.

**4. Prototype (scratch real-GTK test, passed on Windows).** The path was a WPF `Button` with
`AutomationProperties.Name`, then a minimal peer, then a push before parenting: `AccessibleRole = Button`
and `UpdateProperty(Label)`. Next, `Window.Show()` and a pump until `AtContext.Realized`. Finally the
test walked `FirstAccessibleChild/NextAccessibleSibling` from the `Gtk.Window` to the button node.
- `gtk_test_accessible_has_role(Button)` is true. `gtk_test_accessible_check_property(Label, "Accept")`
  matches, and a wrong value is rejected.
- Invoke through `Gtk.Widget.Activate()` raised `Button.Click` **asynchronously**: it needs a pump, and
  arrives about one GTK activate timeout later. That fits WPF, where `ButtonAutomationPeer.Invoke` also
  dispatches asynchronously. Client calls must wait for the dispatcher and GTK to be idle (Decision 2).
- A second push after realization reported the role as frozen.
- The in-process GTK tree can be asserted in tests through GTK's public test API
  (`gtk_test_accessible_has_role/has_property/check_property/check_state`, exported by `gtk-4-1.dll`).
  GtkSharp4 does not bind it, so Phase 1 needs a small P/Invoke helper in the test project. That it
  behaves the same on the Linux AT-SPI backend is unverified (`GTK_A11Y=test` is the fallback).

**Phase 5 go/no-go**

| OS | Verdict | Why |
|---|---|---|
| Windows | **No-go** with the current runtime | AccessKit is disabled in the bundled GTK. UIA sees only the top-level HWND, with 0 children. It becomes a go only with a GTK runtime built with AccessKit, which is unverified. |
| Linux | **Conditional go** | AT-SPI is GTK's native backend. The first Phase 5 task is an AT-SPI tree dump in CI under Xvfb and `dbus-run-session`. If the dump does not show the role and name, Linux becomes a no-go. |
| macOS | **No-go until verified** | It depends on the Homebrew GTK build (AccessKit), which is untested. One CI check (`GTK_A11Y=help`) decides. |

Phases 1–4 (in-process) are unaffected on all three OSes. The in-process GTK tree, test API included,
works on Windows.

## Progress

### Phase 1 — done

Landed in `Gtk4.Forms.PresentationFramework` (no new project):

- Shared types in `System.Windows.Automation`: `AutomationIdentifier`/`AutomationProperty`/`AutomationEvent`/
  `AutomationPattern`, `ControlType` (all 39), `AutomationElementIdentifiers` and the `*PatternIdentifiers`
  classes with the real UIA ids, the enums (`ToggleState`, `ExpandCollapseState`, `ScrollAmount`,
  `RowOrColumnMajor`, `WindowVisualState`...), `ElementNotEnabledException`, `ElementNotAvailableException`.
- `System.Windows.Automation.Provider`: `IRawElementProviderSimple` and the ten pattern interfaces.
- `System.Windows.Automation.Peers`: `AutomationPeer` (full template-method surface, notification and
  property-changed events with internal listener registry for Phase 2), `UIElementAutomationPeer` (`FromElement`
  does not create unless the widget exists, see below; `CreatePeerForElement` creates and caches), `FrameworkElementAutomationPeer`
  (`AutomationProperties` then labelling element for the name; `x:Name` for the id), `ItemsControlAutomationPeer`
  and `ItemAutomationPeer`, and peers for Button, ToggleButton, CheckBox, RadioButton, TextBox, PasswordBox,
  TextBlock, Label, ComboBox (+ items), ListBox (+ items), Slider and Window.
- `UIElement.OnCreateAutomationPeer` (protected virtual, `UIElement.Automation.cs`) with WPF's caching (a null
  result is not cached).
- GTK push: the role is set at widget creation, before parenting. Name, required, checked/pressed, selected,
  expanded, value range and read-only/multi-line follow property changes.

Deviations and gaps (all documented in code):

- Provider interfaces are implemented explicitly, as in WPF; overridable behaviour goes through internal hooks
  (`CanSelectMultipleCore`). `ItemsControlAutomationPeer`, `SelectorAutomationPeer`, `ItemAutomationPeer` and
  `SelectorItemAutomationPeer` are abstract with protected constructors. `ComboBoxItemAutomationPeer` derives from
  `SelectorItemAutomationPeer` and `ListBoxItemWrapperAutomationPeer` stays internal, as in WPF.
- Peers are created eagerly when the widget is created (the role must be set before realization), unlike WPF, which
  creates them when a client asks. So `FromElement` returns a peer for any element with a widget. A throwing
  `OnCreateAutomationPeer` is logged through `Trace` and skipped; it never breaks widget creation.
- Names: content-bearing peers use the content's plain text with access-key markers removed (`_OK` is `OK`),
  then a string `ToolTip`; item peers use their container's peer name.
- Gaps against WPF: `GetControlledPeersCore`/`GetHeadingLevelCore` only return their defaults, `ResetChildrenCache` is a
  no-op (nothing is cached), `IsHwndHost` is internal and always false, and `ButtonBase.OnClick` does not raise
  `InvokePatternOnInvoked` yet (events arrive in Phase 4). `ComboBox.IsDropDownOpen` is store-only on GTK, so
  expand/collapse is checked on the property, not on a popup.
- `GetChildren` is recomputed on every call (no cache), so the peer tree always mirrors the visual tree; elements
  without a peer are transparent, as in WPF. Item peers are cached per item.
- No `RangeBase` in the layer, so `SliderAutomationPeer` derives from `FrameworkElementAutomationPeer`.
  `ScrollItem` and `ContentElementAutomationPeer` are not implemented. `ItemsControl`, `TabControl`,
  `TreeView`, `Menu` and the rest get no peer yet (Phase 4).
- Bounds are relative to the root window: GTK 4 has no screen position for a toplevel.
- `Button.Invoke` goes through `Dispatcher.BeginInvoke` like WPF, so with the GTK bridge installed it needs a
  pump; without it the call is synchronous.
- `IsWidgetCreated` is overridden by content containers to mean "has a content box" and is false for `Button`;
  automation code uses `HasBackingWidget`. This also fixed `AutomationProperties.Name` changes after creation,
  which were never pushed, and the single-value `UpdateProperty` overload, which never changed anything.
- A role changed after realization is not applied (Phase 0 finding). Custom peers whose control type changes at
  runtime are the only affected case.
- GTK value checks use the variadic `gtk_test_accessible_check_*` API through a P/Invoke probe in the test project.
  They skip on macOS ARM64, whose varargs ABI cannot be called that way, and the double-valued checks run on
  Windows only (a non-variadic delegate leaves AL unset on the SysV ABI); role and `has_*` checks still run.

Tests: 98 in `PresentationFramework.Tests/Automation` (75 at the first landing, 23 added by the review fixes) (identifiers, peer core and tree, every pattern through
direct provider calls against real GTK widgets, GTK role/name/state through GTK's test API). Observed on Windows:
`WindowsBase.Tests` 201, `MarkupCompiler.Tests` 574, `Wpf.ProjectConverter.Tests` 34, `PresentationFramework.Tests` 4150,
all passing; `dotnet build Gtk4.Forms.sln` warning-clean. Linux and macOS were not run here.

Samples gate (pinned recipe, package 1.0.0.26325 packed from this branch): all 90 samples that had `build=pass` still
build. Of the 83 that had `run=runs`, 75 ran in the 10 s smoke run and the other 8 (CollectionBinding,
CollectionViewSource, BindingToStringFomat, MultiBinding, XmlnsBind, SearchingForElement, UsingElements,
WindowSizingOrder) ran when re-run alone with a 30 s limit (the same load noise earlier reports recorded): 83 of 83.
The 4 `Automation.Peers` samples (ExpenseItIntro, CustomComboBox, DataBindingDemo, WPFGallery) no longer report any
peer or automation error; they still fail on other layer gaps (`DataGrid` parts, `Popup`/`MenuItem` members,
generator output, `FilterEventArgs`, `UserControl`, `System.Windows.Shell`).


Review-fix gate (same recipe, package 1.0.0.26325 repacked from the fixed tree): 90 of 90 samples with `build=pass` still build, and 83 of 83 with `run=runs` ran in the smoke run (80 in the first 10 s pass; AddingEventHandler, ProcessingCommandLineArguments and IntroToStylingAndTemplating ran when re-run alone with a 30 s limit).

### Phase 2 — done

Landed in `Gtk4.Forms.PresentationFramework/Automation/Client` (no new project; the client API lives next to the
peers it wraps, so there is nothing to add to the solution or the CI scripts):

- `AutomationElement` (`RootElement`, `FocusedElement`, `FromPoint`, `FromLocalProvider`, `Current`/`Cached` views,
  `FindFirst`/`FindAll`, `GetCurrentPattern`/`TryGetCurrentPattern`, `GetCurrentPropertyValue`, `SetFocus`,
  `GetClickablePoint`, `GetSupportedPatterns`/`Properties`, `GetRuntimeId`, equality; `FromHandle` throws
  `PlatformNotSupportedException`), `Condition` and its `Property`/`And`/`Or`/`Not`/`True`/`False` forms (with
  `IgnoreCase` and WPF's argument validation), `TreeWalker` with the three view walkers, `CacheRequest`
  (properties, patterns, `Children`/`Parent` scope), and `Automation` (view conditions, `Compare`, the
  `Add*/Remove*EventHandler` methods).
- Patterns: `InvokePattern`, `ValuePattern`, `TogglePattern`, `SelectionPattern`, `SelectionItemPattern`,
  `ExpandCollapsePattern`, `RangeValuePattern`, `ScrollPattern`, `WindowPattern`, each wrapping its Phase 1 provider,
  with WPF's exceptions: `ElementNotEnabledException`, `ElementNotAvailableException` (the element's window closed),
  `InvalidOperationException` for an unsupported pattern, `NoClickablePointException`.
- Every client call goes through `AutomationThread`: it runs on the GTK thread through the `Dispatcher` (exceptions
  are carried back to the caller) and returns after the dispatcher is idle (a bounded GLib pump on the GTK thread,
  an `ApplicationIdle` barrier from another thread). Nested calls and event handlers run inline.
- Events: `Automation.Add*EventHandler` subscribe to the peers' static events with `TreeScope` filtering. Handlers
  run synchronously on the GTK thread. `Button.OnClick` and `RepeatButton.OnClick` (with a new
  `RepeatButtonAutomationPeer`) raise `InvokePatternOnInvoked`, as in WPF; toggle buttons do not. `Keyboard.SetFocused`
  raises `AutomationFocusChanged`. `AsyncContentLoaded` reaches the client as `AsyncContentLoadedEventArgs`.

Deviations from the plan and WPF, all documented in code:

- The root element is an in-process pane (`DesktopAutomationPeer`, "Desktop") whose children are the visible windows of
  `Application.Current`. A window with no `Application` is reachable only from its own element.
- `FromPoint` and `BoundingRectangle` use window coordinates (GTK 4 has no screen position), so with several windows
  the topmost one containing the point wins.
- Tree walkers with a condition prune non-matching elements and promote their children. A match whose ancestors all
  fail the condition is a child of the root for sibling purposes.
- `CacheRequest.TreeScope` takes any non-empty mix of `Element`, `Children` and `Descendants` and rejects `Parent` and
  `Ancestors`, as WPF does; `Descendants` is accepted but not cached. `AutomationElementMode` is stored but always
  acts as `Full`. Cached children carry the request's properties and patterns and have the requesting element as
  `CachedParent`.
- An unsupported pattern property reads as the WPF schema default (`IsReadOnly` true, `ToggleState` Indeterminate,
  `ExpandCollapseState` LeafNode, `Selection` empty); other properties read as the type default (unverified against
  WPF for the scroll percent and view-size properties).
- Property conditions and `GetCurrentPropertyValue` cover the element properties and the properties of the nine
  patterns above. Other properties throw `ArgumentException`.
- `WindowOpenedEvent`/`WindowClosedEvent` and the other events with no peer source can be subscribed to but are never
  raised. `ScrollItem`, `Grid`, `Table` and the rest still have no client pattern class.
- Structure-changed events are only raised by `InvalidatePeer`, always as `ChildrenInvalidated` (Phase 4 adds the
  fine-grained change types).
- `FindFirst`, the `TreeWalker` methods, `FocusedElement` and `CachedParent` return `null` through a non-nullable
  signature, like the oblivious real API, so WPF test code compiles warning-free.

Tests: 57 in `PresentationFramework.Tests/AutomationClient`, written in the shape of WPF UIA tests over shown GTK
windows (find by `AutomationId`, `GetCurrentPattern`, invoke, set value, toggle, select, range, window, events, cache,
tree walking), plus off-thread tests with the main-loop bridge installed (calls marshal, exceptions cross the thread,
events arrive on the GTK thread). Observed on Windows: `WindowsBase.Tests` 201, `MarkupCompiler.Tests` 574,
`Wpf.ProjectConverter.Tests` 34, `PresentationFramework.Tests` 4207, all passing; `dotnet build Gtk4.Forms.sln`
warning-clean. Linux and macOS were not run here. The samples gate was not re-run for this phase (no sample code changed).

### Phase 3 — done

Landed as two new projects, both in `Gtk4.Wpf.sln`, `Gtk4.Wpf.CrossPlatform.slnf` and the three CI scripts
(`scripts/ci/test-{windows.ps1,linux.sh,macos.sh}`, after the `PresentationFramework` step):

- `Gtk4.Forms.Wpf.Testing` (net10.0, package `Gtk4.Forms.Wpf.Testing`, references PresentationFramework and
  `xunit.v3.extensibility.core`):
  - `GtkThread` owns the GTK thread (STA on Windows). Work reaches it through the GLib main context (`PostToMainLoop`
    at `Send` priority), so `Dispatcher` calls from the test thread and `Invoke` share one path. On macOS
    `RunOnCurrentThread` adopts thread 0 and moves the test runner to a worker.
  - `WpfAppHost`: `Start<TApp>()`/`Start(Type)` (creates the app, calls the generated `InitializeComponent`, runs
    startup so `StartupUri` shows the window) and `Start(Func<Window>)`. `MainWindow` is an `AutomationElement`;
    `Invoke` runs code on the GTK thread; `Dispose` closes windows and drops the automation subscriptions.
  - Locators: `By.AutomationId/Name/ControlType/ClassName/AllOf/AnyOf/And`; `Find` (throws `ElementNotFoundException`
    with the locator, the scope and the tree under it), `TryFind`, `FindAll` on any element (scoping) and on the host.
  - Waits: `WaitUntil` (condition runs on the GTK thread, pumps the loop, throws `WaitTimeoutException` with the tree),
    `WaitFor(By)`, `WaitForIdle()` (an idle-priority marker). No sleeps.
  - Semantic actions: `Click` (Invoke, or Select for a list item), `Type` (Value), `Check`/`Uncheck` (Toggle, up to
    three steps for a tri-state), `Select`, `Expand`, `Collapse`, `GetText`, `AsInvoke`/`AsValue`/...
  - Routed input (`App.Input`): `Keyboard.Press/Down/Up` and `Mouse.MoveTo/Leave/Click/DoubleClick`, through
    `RaiseKeyDown`, `RaiseMouseButton` and the pointer seams the GTK controllers call (PresentationFramework exposes
    them with `InternalsVisibleTo`). Native OS injection is out of scope for this phase.
  - Evidence: `Screenshot`/`SaveScreenshot` render a widget with GTK's own snapshot path (widget paintable, render node,
    renderer texture, `gdk_texture_save_to_png`, called through P/Invoke because GtkSharp4 binds none of it).
    `SaveEvidence` writes `<test>.tree.txt` and `<test>.png` to `WPF_UI_EVIDENCE_DIR` (default
    `<bin>/ui-evidence`). `RenderTargetBitmap` is not used: the layer renders only a `DrawingVisual`.
  - xUnit: `[WpfUiFact]` (skips without GTK, fails instead when `XF_GTK_REQUIRE_NATIVE` is set), `WpfUiTest`,
    `WpfUiTest<TApp>`, `WpfUiWindowTest<TWindow>` (start the app per test; on failure save the evidence) and the
    `WpfUiCollection` collection definition. Parallelism is disabled by an assembly attribute in the test project.
- `Gtk4.Forms.Wpf.Testing.Tests` (net10.0): `ByTests`, the driver tests over a code-built form (`HostTests`,
  `InputTests`, `EvidenceTests`), and UI tests of `Gtk4.Forms.WpfSample` (started from its `App` type) and of five
  official samples compiled in process from `WPF-Samples` (`MainWindow.xaml` as a `Page`, the code-behind and data
  classes as `Compile` items; `App.xaml` is left out): `SimpleBinding`, `MasterDetail`, `DirectionalBinding`,
  `HandlingEventOnCommandUsingCode` (commands, KeyBinding and a `MessageBox` through a recording presenter) and
  `MousePointer` (cursor enter/leave).

Tests: 56 in `Gtk4.Forms.Wpf.Testing.Tests` (55 run, 1 skipped layer-gap test). Observed on Windows:
`WindowsBase.Tests` 201, `MarkupCompiler.Tests` 575, `Wpf.ProjectConverter.Tests` 34, `PresentationFramework.Tests`
4220, `Wpf.Testing.Tests` 55 passed + 1 skipped, all passing; `dotnet build Gtk4.Forms.sln` warning-clean. A deliberately failing test
showed the evidence files (tree and PNG) written from `Dispose`. Linux and macOS were not run here, and the samples
gate was not re-run (no sample or layer behaviour changed).

Deviations from the plan and WPF, all documented in code or here:

- The plan example used `Find(...) != null` inside `WaitUntil`; `Find` throws, so the example now uses `TryFind`.
- `InitGtk` in `Application` became `internal` so the host installs the same dispatcher bridges as `Application.Run`.
- Mouse enter/leave are raised on the target only (the layer's pointer seam), so a `Border` handler needs the
  `UIElement` overloads, because a `Border` has no peer.
- `HandlingEventOnCommand` (the `Window.CommandBindings`/`InputBindings` XAML variant) was replaced by
  `HandlingEventOnCommandUsingCode`: the markup compiler drops those two property elements, so the sample builds and
  runs with its command wiring missing.
- Review fixes after Phase 3: no GTK runtime now returns from the loop thread in every case (`GtkThread.RunLoop`);
  `WaitForIdle` is bounded by count and time and throws `WaitTimeoutException` when the idle marker never fires
  (a zero-interval timer), and `Dispose` tolerates that; `Mouse`/`Keyboard` input to a disabled element throws
  `ElementNotEnabledException` (a real device delivers nothing); `x:Name`/`Name` now also sets
  `FrameworkElement.Name` in the markup compiler, so `By.AutomationId` finds XAML-named controls; `TreeDump` escapes
  line breaks; `Launch` throws when the window never maps; the package pins its PresentationFramework and
  WindowsBase dependencies to the exact build version (`[x]`), because the driver uses their internals.
- Not fixed (layer gap): nothing calls `CommandManager.InvalidateRequerySuggested` after `Loaded` adds a binding, so
  a command button stays disabled; tracked in `samples-coverage.md`, with a skipped test in `CommandSampleTests`.
- Unverified: the GTK snapshot path and the `Wpf.Testing.Tests` suite on Linux (Xvfb) and macOS; only Windows ran.

### Phase 4 — done

Landed in `Gtk4.Forms.PresentationFramework/Automation` (no new project):

- Peers: `ListView` (+ `ListViewItem`), `TabControl` (+ `TabItem`; children are the tabs, then the selected content),
  `TreeView` and `TreeViewItem` (expand/collapse with `LeafNode`, selection item, GTK `Level`/`Expanded`/`Selected`),
  `Menu` (`MenuBar`) and `MenuItem` (Invoke without sub-items, ExpandCollapse with them, Toggle when checkable),
  `ScrollViewer` (scroll pattern with WPF's validation), `ProgressBar` (read-only range, none when indeterminate),
  `Image`, `Expander`, `GroupBox`, `StatusBar`, `ToolBar`, `Hyperlink` (child of its `TextBlock`'s peer), `RichTextBox`
  (`Document`), and `DataGrid` with column headers, rows and cells (`IGridProvider`, `ITableProvider`,
  `ISelectionProvider`; rows `ISelectionItemProvider`; cells `IGridItemProvider`, `ITableItemProvider`, read-only
  `IValueProvider`).
- Providers and client: `IGridProvider`, `IGridItemProvider`, `ITableProvider`, `ITableItemProvider`, `ITextProvider`,
  `ITextRangeProvider`, `TextUnit`, `TextPatternRangeEndpoint`; client `GridPattern`, `GridItemPattern`,
  `TablePattern`, `TableItemPattern`, `TextPattern`, `TextPatternRange`, with the identifier classes and the text
  events in the client event map. The text model is flat character offsets: `Line` and `Paragraph` split at line
  feeds, `Format`, `Page` and `Document` are the whole text, visible ranges are the whole text, a point maps to
  offset 0, `ScrollIntoView` is a no-op, text attributes are not modelled (`GetAttributeValue` returns
  `NotSupported`, `FindAttribute` returns null and the client has no `AutomationTextAttribute`).
- Events, all raised from the peers: selection events follow WPF's `Selector` rules (one `ElementSelected` for a
  single new selection, added/removed events otherwise, one `Invalidated` above 20 changes); structure events carry
  `ChildAdded`, `ChildRemoved`, `ChildrenBulkAdded`, `ChildrenBulkRemoved`, `ChildrenReordered` or
  `ChildrenInvalidated`, from a children diff on every items change (a lone added child is the event's sender, a
  removed one is reported by the parent with the child's runtime id; a sort raises `ChildrenReordered`); `Invoked`
  also for `MenuItem`, checkable ones included (after their toggle); text selection and text changed events from `TextBox` and `RichTextBox`; property events for expand state,
  progress and scroll position.
- GTK: `LabeledBy` is pushed as `AccessibleRelation.LabelledBy` through `AccessibleExtensions.UpdateRelation` (the
  `IAccessible[]` overload, which writes the value) and reset with `Widget.ResetRelation`. It is pushed again from the
  label's widget creation when the labelled element's widget came first. `LiveRegionChanged` on a peer whose `LiveSetting`
  is not `Off`, and every `Notification` event, call `Widget.Announce`. Tests read the relation target back through
  `gtk_test_accessible_check_relation`.
- `AutomationPeer.NotifyChildrenChanged` is the one hook `ItemsControl` and `DataGrid` call when their items change.
- Snapshot tests of the automation tree for the five Phase 3 samples (`Wpf.Testing.Tests/Samples/
  AccessibilityTreeTests.cs`).

Deviations from the plan and WPF:

- `DataGrid` is not an `ItemsControl` here, so its rows, cells and headers derive from a new public
  `GridPartAutomationPeer` rather than `ItemAutomationPeer`, and rows are cached per item reference (a repeated item
  shares one peer). Cell values are read-only; the layer cannot edit cells. `GridPartAutomationPeer` is not in WPF: it
  is public only because public peers derive from it, with an internal constructor and `EditorBrowsable(Never)`.
  `DataGridItemAutomationPeer`, `DataGridCellItemAutomationPeer` and `DataGridColumnHeaderAutomationPeer` have
  internal constructors (WPF's are public, with other signatures), and the header peer does not derive from
  `ButtonBaseAutomationPeer`.
- Text ranges hold character offsets that are clamped to the current text on every use, instead of tracking text
  positions through edits as WPF does.
- `Hyperlink` is not an element (no widget), so `HyperlinkAutomationPeer` derives from `AutomationPeer`; there is no
  `ContentElementAutomationPeer`.
- `TreeView.SelectedItem` is a `TreeViewItem` in this layer, so the tree's selection holds node peers.
- `ListView` has no `GridView`, so the grid pattern WPF adds for one is absent. `ContextMenu` and `StatusBarItem`
  have no peers. `PropertyCondition` and `GetCurrentPropertyValue` still cover only the nine Phase 2 patterns.
- `TabItem` pushes no role: its widget is the page box, and GTK's notebook owns the tab role.
- Items added to a `ListBox` through `Items` after the widget exists still do not reach GTK (a layer gap that
  predates this phase); the peers report them anyway.

Tests: 44 new in `PresentationFramework.Tests` (37 at the first landing, 7 from the review fixes) (peer patterns, `DataGrid`, text provider ranges, events in order,
GTK relation, role and announcements, and the client grid and text patterns) and 5 snapshot tests in
`Wpf.Testing.Tests`. Observed on Windows: `WindowsBase.Tests` 201, `MarkupCompiler.Tests` 575,
`Wpf.ProjectConverter.Tests` 34, `PresentationFramework.Tests` 4264, `Wpf.Testing.Tests` 60 passed + 1 skipped, all
passing; `dotnet build Gtk4.Forms.sln` warning-clean. Linux and macOS were not run.

Samples gate (pinned recipe, package 1.0.0.26325 packed from this branch): 90 of 90 samples with `build=pass` still
build, and 83 of 83 with `run=runs` ran (79 in the first 10 s pass; FindingSourceElement, SimpleLayout, OpacityMasking
and EventTriggers ran when re-run alone with a 30 s limit, the same load noise earlier phases recorded).

Review-fix gate (same recipe, package 1.0.0.26325 repacked from the fixed tree): 90 of 90 samples with `build=pass` still build, and 83 of 83 with `run=runs` ran (81 in the 10 s pass; OpacityMasking and MousePointer ran when re-run alone with a 30 s limit).

### Phase 5 — done

Phase 0 decided the scope: Windows is no-go (the bundled GTK has AccessKit disabled, UIA sees an empty window),
Linux and macOS are unverifiable on this machine. So no out-of-process UIA client was written; nothing in it could be
proved here. What landed, in `Gtk4.Forms.Wpf.Testing`:

- `NativeInput` (`App.Native`): opt-in operating-system input through `SendInput` on Windows. `MoveTo`, `Click`
  (left or right, optional point in the element's coordinates) and `TypeText` (Unicode keystrokes). It finds the GTK
  top-level by process, class `gdkSurfaceToplevel` and title, and waits for the app to be idle after each action. On
  other platforms it throws `PlatformNotSupportedException`; `NativeInput.IsSupported` says which.
- `WpfNativeFactAttribute`: the test is skipped unless the OS is Windows and `XF_NATIVE_INPUT=1`, so it never gates
  CI. Test classes also carry `[Trait("UI", "Native")]` for selection by trait.
- Measured, and handled in `NativeInput.Target`: GDK's Win32 window is larger than the GTK surface (428x529 around a
  400x500 window at 96 dpi, an invisible frame of 14 px per side, 15 at the top), so a click at the plain client
  origin missed by about 15 px. The frame is derived from the client size minus the GTK window size. Unverified: other
  DPI scales and multi-monitor setups.
- Safety checks (review fixes): before any click or keystroke the window must be confirmed foreground  (`GetForegroundWindow`, retried briefly), else `InvalidOperationException`; a click also checks that the main  window is the root under the target point. `App.Native` targets elements of the main window only: elements in  another window, dialog or popup throw `NotSupportedException` (their bounds are relative to their own root), and  the window is found by title, so two top-levels with the same title are ambiguous. `INPUT` is laid out as a type  plus a union, so x86 hosts get the 28-byte record. Input is delivered asynchronously: wait for the expected  effect. Scaling uses GTK's integer scale factor, so fractional Windows scales stay unverified.
- Run with `XF_NATIVE_INPUT=1 dotnet test Gtk4.Forms.Wpf.Testing.Tests --filter "UI=Native"`. The cursor moves, so
  keep the desktop unlocked and do not touch the mouse meanwhile.

Follow-up steps for the platforms not delivered (each needs the named check first):

- **Linux (AT-SPI).** In CI under Xvfb: `dbus-run-session -- xvfb-run ... ` with `at-spi2-core` and `python3-pyatspi`,
  start the sample, dump the registry tree with `pyatspi` and check that the Button node has role `push button` and
  name `Accept`. Only on success add a client over `org.a11y.atspi` (Tmds.DBus or the `at-spi2` C library) behind the
  existing `AutomationElement` shape; peers map to AT-SPI nodes by role and name, with `AutomationId` unavailable
  because GTK 4 exposes no such property (match on name and role, or on a unique accessible description).
- **macOS.** On the runner: `GTK_A11Y=help` must list `accesskit` as available or the macOS backend as enabled;
  then inspect a window with Accessibility Inspector. If available, drive it with `AXUIElement` (P/Invoke to
  ApplicationServices) and `CGEvent` for native input. Otherwise stay no-go.
- **Windows UIA.** Becomes possible only with a GTK runtime built with AccessKit. Check with
  `GTK_A11Y=help` on that runtime, then repeat the Phase 0 UIAutomationClient query.
- **Any OS without exposure.** Option 1 of the open questions below (an in-app automation server over a named pipe)
  stays the portable alternative; it was not started.

Tests: `Wpf.Testing.Tests` `NativeInputTests`: four native tests (click a button, click a check box, type into a text box, refuse an element of another window; run and passed with `XF_NATIVE_INPUT=1`, skipped otherwise), the opt-in rule for `null`, empty, `0`, `true` and `1`, and the `INPUT` size.


Observed on Windows: `WindowsBase.Tests` 201, `MarkupCompiler.Tests` 575, `Wpf.ProjectConverter.Tests` 34,
`PresentationFramework.Tests` 4264 passed; `Wpf.Testing.Tests` 66 passed + 5 skipped (the 4 native tests and the
earlier skip), and with `XF_NATIVE_INPUT=1 --filter "UI=Native"` 10 passed; `dotnet build Gtk4.Forms.sln`
warning-clean. Linux and macOS were not run. The samples gate was not re-run: no product code changed.

1. Should the driver package also host the app **out-of-process** (launch the converted app and attach)
   before Phase 5, using an in-app automation server over a named pipe? That gives process isolation on
   every OS without AccessKit, at the cost of a small custom protocol. The recommendation is to decide
   after Phase 3.
2. Package naming: `Gtk4.Forms.Wpf.Testing`, or ship the client API in its own package
   (`Gtk4.Forms.UIAutomationClient`) so non-test tools can reference it without the driver.

## Open issues after the phase checks

Left open by the per-phase Opus confirm checks when `ua/automation` merged into `main` (`f977d105`). None of them
regresses existing behaviour. Fix them before new automation scope.

Peers and names:
- `ComboBoxItemAutomationPeer` derives from `SelectorItemAutomationPeer`; WPF uses `ListBoxItemAutomationPeer`.
  `ListBoxItemWrapperAutomationPeer` is internal and sealed; WPF has it public, deriving from
  `FrameworkElementAutomationPeer`. The plan's "Deviations" bullet wrongly calls both "as in WPF".
- ~~ListBox item names lose underscores~~ Fixed in Round 64 (lane D): `PeerText.FromContentLiteral` for ListBoxItem,
  ComboBoxItem, TreeViewItem and Expander; the button, label, menu-item, TabItem and GroupBox peers strip access keys
  (`PeerText.FromContent`, as WPF's `GetNameCore` does).
- ~~`PeerText.RemoveAccessKey` drops every `_`~~ Fixed in Round 64 (lane D): `AccessKeyParser.RemoveAccessKeyMarker` follows WPF (`"a_b_c"` →
  `"ab_c"`, `"trailing_"` stays); `PeerNameTests` asserts the WPF result.
- The GTK label taken from `LabeledBy` is not re-pushed when the labelling element's text changes.
- Peers are still created eagerly in `ApplyAutomationOnCreated` (documented deviation). The test
  `CreatePeerForElement_caches_and_FromElement_does_not_create` passes only because of call order.
- `RepeatButtonAutomationPeer.Invoke` defers the click; WPF clicks synchronously (only `Button` defers).
- `RaiseAsyncContentLoadedEvent` resets `PendingAsyncContentLoaded` to null instead of restoring it on re-entry.
- `PeerAccessible.Announce` calls `gtk_accessible_announce` unguarded; it exists only since GTK 4.14.
- `TextRangeProvider.Move` breaks the contract for degenerate ranges: a caret at the end moves backward on a
  forward move, a mid-word `Move(Word, -1)` returns 0 but moves, and a caret never reaches the document end.

Client:
- `ElementCache.Take` ignores `TreeScope.Descendants` (it never recurses) and caches properties and patterns on the
  root even when the scope excludes `Element`.
- `CachedParent` on the root of a cached subtree throws; check WPF, which likely returns null there.
- `ScrollPattern.HorizontalViewSize`/`VerticalViewSize` default to 0; the UIA schema default is 100.

Driver:
- `WpfAppHost.Launch` waits for idle with no tolerance, so an app with an always-ready source cannot be launched.
- `UiInput.Keyboard.Up` can throw before clearing `Keyboard.Modifiers`, leaking held modifiers into later tests.
- `x:Name` sets `Name` only for types found in reference assemblies, not for the app's own types.
- Native input: `RequireMainWindow` accepts popover content (ContextMenu, Popup, submenus), whose clicks land in the
  wrong place. A second window with the main window's title makes `Handle()` pick the wrong HWND. `MoveTo` moves the
  cursor before the cover check. Foreground and cover are not re-checked before each `SendInput`. Fractional
  Windows scales (125%, 150%) are not detected.

Tests that would pass without their fix:
- `WaitForIdle_is_bounded_when_a_zero_interval_timer_is_always_ready`: the timer is clamped to 1 ms, so it is never
  always-ready, and the catch accepts either outcome.
- `Disabled_state_follows_IsEnabled` and `Selected_state_follows_the_selection` check states GTK sets by itself.
- `DataGrid_clearing_the_selection_and_sorting_raise_their_events` accepts any `StructureChanged`, not
  `ChildrenReordered`.
- `Input_record_is_the_size_of_the_platform_INPUT_structure` cannot fail on x64; the foreground and cover checks
  have no tests.

Housekeeping: `GridPatterns.cs` exceeds ~200 lines; some lines in this plan exceed ~120 columns.
