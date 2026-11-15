#!/usr/bin/env bash
# CI (Linux): build the GTK 4 backend and run all three unit-test suites headlessly.
#
# Unlike Windows (gvsbuild) the Linux GTK 4 runtime comes from the distro: Ubuntu 24.04 ships
# libgtk-4-1. The renderer tests are real GTK 4 widgets, so they run under Xvfb with GDK_BACKEND=x11
# (the GTK backend's pixbuf/screenshot paths are unsupported on Wayland); the screen must be large
# enough for the 800x600 test windows, hence 1280x1024 rather than Xvfb's 640x480 default.
#
# GtkSharp4 is restored from the committed vendored feed (build/gtksharp4-packages, see NuGet.config).
# Assumes .NET from actions/setup-dotnet.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1

echo "== install the GTK 4 runtime, a headless X server and fonts =="
sudo apt-get update -qq
# fonts-dejavu-core: a runner with no fonts makes Pango measure bold and regular identically and the
# font-attribute tests fail. dbus-x11: the GTK 4 accessibility stack wants a session bus.
sudo apt-get install -y -qq --no-install-recommends \
  libgtk-4-1 \
  libgl1-mesa-dri libglx-mesa0 \
  xvfb dbus-x11 fonts-dejavu-core

echo "== report native GTK version =="
dpkg -s libgtk-4-1 2>/dev/null | grep -i '^Version' || true

echo "== build (Debug; GeneratePackageOnBuild off - CI tests, it does not pack) =="
dotnet build Xamarin.Forms.Gtk4.sln -c Debug -p:GeneratePackageOnBuild=false

echo "== Core unit tests =="
dotnet test Xamarin.Forms.Core.UnitTests -c Debug --no-build --logger trx

# Category!=LongRunning drops the MSBuildTests (build-integration tests that restore generated
# projects from an OS-temp dir, where the relative "Local" feed cannot resolve - a pre-existing
# NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
echo "== XAML unit tests (excluding LongRunning MSBuild integration tests) =="
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'

# XF_GTK_REQUIRE_NATIVE turns "no display / no GTK runtime" from a skip into a failure: both are
# installed above, so an unusable environment means the apt or Xvfb step silently broke.
echo "== GTK renderer tests (real GTK 4 widgets, headless under Xvfb) =="
GDK_BACKEND=x11 XF_GTK_REQUIRE_NATIVE=1 GSETTINGS_BACKEND=memory \
  xvfb-run -a --server-args="-screen 0 1280x1024x24" \
    dotnet test Xamarin.Forms.Platform.GTK.UnitTests -c Debug --no-build --logger trx
