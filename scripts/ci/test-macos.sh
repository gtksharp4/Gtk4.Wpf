#!/usr/bin/env bash
# CI (macOS): build the GTK 4 backend and run all three unit-test suites.
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

export DOTNET_CLI_TELEMETRY_OPTOUT=1

# The macOS runner's git cannot resolve the branch for GitInfo, so the assembly version falls back to
# $(VersionPrefix) in Version.targets - see the comment there. No git configuration is needed here.
echo "== build (Debug; GeneratePackageOnBuild off) =="
dotnet build Gtk4.Forms.sln -c Debug -p:GeneratePackageOnBuild=false

echo "== Core unit tests =="
dotnet test Gtk4.Forms.Core.UnitTests -c Debug --no-build --logger trx

# Category!=LongRunning drops the MSBuildTests (build-integration tests that restore generated
# projects from an OS-temp dir, where the relative "Local" feed cannot resolve - a pre-existing
# NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Gtk4.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'

echo "== install the GTK 4 runtime via Homebrew =="
brew install gtk4 >/dev/null
GTK_PREFIX="$(brew --prefix)"
pkg-config --modversion gtk4 2>/dev/null || true

# DYLD_FALLBACK_LIBRARY_PATH is set INLINE for the renderer command only (with the default dirs
# preserved): overriding it before the build drops the dirs the SDK compiler needs. XF_GTK_MAC_RUNNER
# selects the thread-0 path in Program.cs; XF_GTK_REQUIRE_NATIVE turns "no GTK runtime" into a failure.
echo "== GTK renderer tests (real GTK 4 widgets, thread-0 runner over Quartz) =="
GTK_DLL="Gtk4.Forms.Platform.Gtk.UnitTests/bin/Debug/net10.0/Gtk4.Forms.Platform.GTK.UnitTests.dll"
# -verbose prints STARTING/FINISHED per test, so a native Quartz crash names the test it died on.
DYLD_FALLBACK_LIBRARY_PATH="$GTK_PREFIX/lib:/usr/local/lib:/usr/lib:/lib" \
	XF_GTK_MAC_RUNNER=1 XF_GTK_REQUIRE_NATIVE=1 \
	dotnet exec "$GTK_DLL" -verbose
