#!/usr/bin/env bash
# CI (macOS): build the GTK 4 backend and run all three unit-test suites.
#
# The GTK 4 runtime comes from Homebrew (preinstalled on GitHub macOS runners). GtkSharp4 is restored
# from the committed vendored feed (build/gtksharp4-packages, see NuGet.config).
#
# WARNING - the GTK renderer tests are expected to be fragile here, and this job gates on them by
# request. Homebrew's gtk4 builds only the Quartz GDK backend, which requires GTK (NSWindow creation,
# main-loop polling) to run on THREAD 0. GtkFixture runs GTK on a dedicated background thread (and
# only marks it STA on Windows), so gtk_init / the main-loop pump can fail or block under headless
# Quartz - the same limitation the GtkSharp fork documents, where macOS is informational and uses a
# thread-0 MacRunner. Core and XAML are platform-independent and unaffected. If the renderer step
# hangs, the job timeout catches it; making it reliably green needs a thread-0 test runner.
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

echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'

echo "== GTK renderer tests (real GTK 4 widgets via Quartz; see the thread-0 warning above) =="
XF_GTK_REQUIRE_NATIVE=1 \
  dotnet test Xamarin.Forms.Platform.GTK.UnitTests -c Debug --no-build --logger trx
