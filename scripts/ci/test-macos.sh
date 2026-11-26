#!/usr/bin/env bash
# CI (macOS): build the WPF-on-GTK 4 backend and run its unit-test suites.
#
# The netstandard2.0 CS0518 failures unique to the GitHub macOS-arm64 runner (its netstandard2.0
# targeting pack resolves to nothing) are handled in the library csproj files: on macOS they disable
# the implicit framework reference and reference netstandard.dll explicitly from the NETStandard.Library
# package. The build command here is therefore identical to Windows/Linux - no extra restore flags.
#
# The real-GTK-4 renderer suite runs through a thread-0 runner (GTK 4's Quartz backend requires thread
# 0, which `dotnet test` owns): the test assembly is executed directly and its custom entry point
# (Program.cs) owns thread 0. GtkSharp4 is restored from the committed vendored feed; the native GTK 4
# runtime comes from Homebrew.
set -euo pipefail

# shellcheck source=annotate-failures.sh
source "$(dirname "$0")/annotate-failures.sh"

export DOTNET_CLI_TELEMETRY_OPTOUT=1

# The macOS runner's git cannot resolve the branch for GitInfo, so the assembly version falls back to
# $(VersionPrefix) in Version.targets - see the comment there. No git configuration is needed here.
echo "== build (Debug; GeneratePackageOnBuild off) =="
# The filter leaves out the WPF HelloWorld sample (net10.0-windows): it needs the Windows SDK.
dotnet build Gtk4.Wpf.CrossPlatform.slnf -c Debug -p:GeneratePackageOnBuild=false

# The logic suites (WindowsBase DP/dispatcher, MarkupCompiler XAML->code, Wpf.ProjectConverter) need
# no GTK runtime.
echo "== WindowsBase unit tests =="
run_annotated "macOS WindowsBase.Tests" dotnet test Gtk4.WindowsBase.Tests -c Debug --no-build --logger trx

echo "== MarkupCompiler unit tests =="
run_annotated "macOS MarkupCompiler.Tests" dotnet test Gtk4.MarkupCompiler.Tests -c Debug --no-build --logger trx

# PackedNuspecTests inspects the real package; the build above runs with GeneratePackageOnBuild off.
echo "== pack Gtk4.Wpf =="
dotnet pack Gtk4.Wpf/Gtk4.Wpf.csproj -c Debug --no-build

echo "== Wpf.ProjectConverter unit tests =="
run_annotated "macOS ProjectConverter.Tests" \
	dotnet test Gtk4.Wpf.ProjectConverter.Tests -c Debug --no-build --logger trx

# The test process is never frontmost on the runner, so App Nap may throttle its timers: single main-loop
# iterations blocked for up to 33 s, even in a test with no window (GtkPump STALL lines). Opt out for all apps.
echo "== disable App Nap =="
defaults write NSGlobalDomain NSAppSleepDisabled -bool YES

echo "== install the GTK 4 runtime via Homebrew =="
brew install gtk4 >/dev/null
GTK_PREFIX="$(brew --prefix)"
pkg-config --modversion gtk4 2>/dev/null || true

# DYLD_FALLBACK_LIBRARY_PATH is set INLINE for the renderer command only (with the default dirs
# preserved): overriding it before the build drops the dirs the SDK compiler needs. XF_GTK_MAC_RUNNER
# selects the thread-0 path in Program.cs; XF_GTK_REQUIRE_NATIVE turns "no GTK runtime" into a failure.
echo "== PresentationFramework tests (real GTK 4 widgets, thread-0 runner over Quartz) =="
GTK_DLL="Gtk4.PresentationFramework.Tests/bin/Debug/net10.0/Gtk4.PresentationFramework.Tests.dll"
# -verbose prints STARTING/FINISHED per test, so a native Quartz crash names the test it died on.
# A function, not `env VAR=... cmd`: SIP strips DYLD_* when a protected binary such as /usr/bin/env
# starts, so the variable would never reach dotnet.
run_gtk_tests() {
	DYLD_FALLBACK_LIBRARY_PATH="$GTK_PREFIX/lib:/usr/local/lib:/usr/lib:/lib" \
		XF_GTK_MAC_RUNNER=1 XF_GTK_REQUIRE_NATIVE=1 \
		dotnet exec "$GTK_DLL" -verbose
}
run_annotated "macOS PresentationFramework.Tests" run_gtk_tests

# Same thread-0 runner for the UI driver tests: Program.cs hands thread 0 to GtkThread.RunOnCurrentThread.
echo "== Wpf.Testing tests (UI driver, thread-0 runner over Quartz) =="
UI_DLL="Gtk4.Wpf.Testing.Tests/bin/Debug/net10.0/Gtk4.Wpf.Testing.Tests.dll"
run_ui_tests() {
	DYLD_FALLBACK_LIBRARY_PATH="$GTK_PREFIX/lib:/usr/local/lib:/usr/lib:/lib" \
		XF_GTK_MAC_RUNNER=1 XF_GTK_REQUIRE_NATIVE=1 \
		dotnet exec "$UI_DLL" -verbose
}
run_annotated "macOS Wpf.Testing.Tests" run_ui_tests
