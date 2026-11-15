#!/usr/bin/env bash
# CI (macOS): build the GTK 4 backend and run the two platform-independent suites (Core, XAML).
#
# The real-GTK-4 renderer suite is NOT run on macOS. Homebrew's gtk4 builds only the Quartz GDK
# backend, which requires GTK (NSWindow creation, main-loop polling) to run on THREAD 0. The dotnet
# test host owns thread 0, and GtkFixture runs GTK on a dedicated background thread (STA only on
# Windows), so gtk_init / the main-loop pump cannot work under headless Quartz - the same limitation
# the GtkSharp fork documents, where macOS uses a thread-0 MacRunner. Running the suite here would
# hang or fail; enabling it needs an equivalent thread-0 runner for Xamarin.Forms, which is a
# separate piece of work. Core and XAML are platform-independent and fully exercised here. The
# renderer suite still gates on Windows and Linux.
#
# No native GTK 4 runtime is installed: building Platform.GTK compiles against the managed GtkSharp4
# bindings (vendored, see NuGet.config) and needs no native library, and no renderer test runs.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1

echo "== build (Debug; GeneratePackageOnBuild off - CI tests, it does not pack) =="
dotnet build Xamarin.Forms.Gtk4.sln -c Debug -p:GeneratePackageOnBuild=false

echo "== Core unit tests =="
dotnet test Xamarin.Forms.Core.UnitTests -c Debug --no-build --logger trx

# Category!=LongRunning drops the MSBuildTests (build-integration tests that restore generated
# projects from an OS-temp dir, where the relative "Local" feed cannot resolve - a pre-existing
# NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'
