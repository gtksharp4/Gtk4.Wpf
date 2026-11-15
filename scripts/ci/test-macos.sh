#!/usr/bin/env bash
# CI (macOS): build and run all three unit-test suites.
#
# Built PER PROJECT, not via `dotnet build Xamarin.Forms.Gtk4.sln`. On the GitHub macOS-arm64 runner
# the SOLUTION build evaluates most netstandard2.0 projects without their NETStandard.Library
# reference and the compiler gets no references (CS0518 "Predefined type 'System.Object' is not
# defined") - the very same projects build correctly one at a time. Each csproj carries an explicit,
# macOS-only NETStandard.Library PackageReference (the implicit NETStandard.Library.Ref is not
# resolved on this runner); running each test project's own build picks it up where the solution
# build drops it.
#
# The real-GTK-4 renderer suite runs through a thread-0 runner (GTK 4's Quartz backend requires
# thread 0, which `dotnet test` owns): the test assembly is executed directly and its custom entry
# point (Program.cs) owns thread 0. GtkSharp4 is restored from the committed vendored feed; the
# native GTK 4 runtime comes from Homebrew.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1

echo "== Core unit tests (builds its own project graph) =="
dotnet test Xamarin.Forms.Core.UnitTests -c Debug -p:GeneratePackageOnBuild=false --logger trx

# Category!=LongRunning drops the MSBuildTests (build-integration tests that restore generated
# projects from an OS-temp dir, where the relative "Local" feed cannot resolve - a pre-existing
# NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug -p:GeneratePackageOnBuild=false --logger trx --filter 'Category!=LongRunning'

# Install the native GTK 4 runtime after the managed builds (see the main macOS comment on
# DYLD_FALLBACK_LIBRARY_PATH; it is set inline for the renderer command only).
echo "== install the GTK 4 runtime via Homebrew =="
brew install gtk4 >/dev/null
GTK_PREFIX="$(brew --prefix)"
pkg-config --modversion gtk4 2>/dev/null || true

echo "== build the GTK renderer test assembly (its own project graph) =="
dotnet build Xamarin.Forms.Platform.GTK.UnitTests -c Debug -p:GeneratePackageOnBuild=false

echo "== GTK renderer tests (real GTK 4 widgets, thread-0 runner over Quartz) =="
GTK_DLL="Xamarin.Forms.Platform.GTK.UnitTests/bin/Debug/net10.0/Xamarin.Forms.Platform.GTK.UnitTests.dll"
DYLD_FALLBACK_LIBRARY_PATH="$GTK_PREFIX/lib:/usr/local/lib:/usr/lib:/lib" \
	XF_GTK_MAC_RUNNER=1 XF_GTK_REQUIRE_NATIVE=1 \
	dotnet exec "$GTK_DLL"
