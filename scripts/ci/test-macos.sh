#!/usr/bin/env bash
# CI (macOS): build the GTK 4 backend and run all three unit-test suites.
#
# The real-GTK-4 renderer suite runs through a THREAD-0 runner, not `dotnet test`. Homebrew's gtk4
# builds only the Quartz GDK backend, which requires GTK (NSWindow creation, main-loop polling) to
# run on thread 0 - the process's first thread. The dotnet test host owns thread 0, so the suite is
# run by executing the test assembly directly: its custom entry point (Program.cs) owns thread 0,
# runs xUnit on a background thread, and drains the GTK work queue GtkFixture marshals onto (see
# GtkThreadBridge). Core and XAML are platform-independent and keep using `dotnet test`.
#
# GtkSharp4 is restored from the committed vendored feed (build/gtksharp4-packages, see NuGet.config);
# the native GTK 4 runtime comes from Homebrew.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1

echo "== install the GTK 4 runtime via Homebrew =="
brew install gtk4 >/dev/null
GTK_PREFIX="$(brew --prefix)"
# GLibrary resolves libgtk-4 by dlopen; point the dynamic loader at Homebrew's lib dir so the
# vendored binding finds it (arm64 runners use /opt/homebrew, x86 /usr/local).
export DYLD_FALLBACK_LIBRARY_PATH="$GTK_PREFIX/lib:${DYLD_FALLBACK_LIBRARY_PATH:-}"
pkg-config --modversion gtk4 2>/dev/null || true

echo "== build (Debug; GeneratePackageOnBuild off - CI tests, it does not pack) =="
dotnet build Xamarin.Forms.Gtk4.sln -c Debug -p:GeneratePackageOnBuild=false

echo "== Core unit tests =="
dotnet test Xamarin.Forms.Core.UnitTests -c Debug --no-build --logger trx

# Category!=LongRunning drops the MSBuildTests (build-integration tests that restore generated
# projects from an OS-temp dir, where the relative "Local" feed cannot resolve - a pre-existing
# NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'

# Run the renderer suite through the thread-0 runner. OperatingSystem.IsMacOS() selects the thread-0
# path in Program.cs; XF_GTK_MAC_RUNNER makes that explicit and robust if the OS check returns false
# under the runner. XF_GTK_REQUIRE_NATIVE turns "no GTK runtime" from a skip into a failure.
echo "== GTK renderer tests (real GTK 4 widgets, thread-0 runner over Quartz) =="
GTK_DLL="Xamarin.Forms.Platform.GTK.UnitTests/bin/Debug/net10.0/Xamarin.Forms.Platform.GTK.UnitTests.dll"
XF_GTK_MAC_RUNNER=1 XF_GTK_REQUIRE_NATIVE=1 dotnet exec "$GTK_DLL"
