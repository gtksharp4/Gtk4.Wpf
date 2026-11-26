#!/usr/bin/env bash
# CI (Linux): build the WPF-on-GTK 4 backend and run its unit-test suites headlessly.
#
# Unlike Windows (gvsbuild) the Linux GTK 4 runtime comes from the distro: Ubuntu 24.04 ships
# libgtk-4-1. The renderer tests are real GTK 4 widgets, so they run under Xvfb with GDK_BACKEND=x11
# (the GTK backend's pixbuf/screenshot paths are unsupported on Wayland); the screen must be large
# enough for the 800x600 test windows, hence 1280x1024 rather than Xvfb's 640x480 default.
#
# GtkSharp4 is restored from the committed vendored feed (build/gtksharp4-packages, see NuGet.config).
# Assumes .NET from actions/setup-dotnet.
set -euo pipefail

# shellcheck source=annotate-failures.sh
source "$(dirname "$0")/annotate-failures.sh"

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
# The filter leaves out the WPF HelloWorld sample (net10.0-windows): it needs the Windows SDK.
dotnet build Gtk4.Wpf.CrossPlatform.slnf -c Debug -p:GeneratePackageOnBuild=false

# The logic suites (WindowsBase DP/dispatcher, MarkupCompiler XAML->code, Wpf.ProjectConverter) need
# no GTK runtime or display.
echo "== WindowsBase unit tests =="
run_annotated "Linux WindowsBase.Tests" dotnet test Gtk4.WindowsBase.Tests -c Debug --no-build --logger trx

echo "== MarkupCompiler unit tests =="
run_annotated "Linux MarkupCompiler.Tests" dotnet test Gtk4.MarkupCompiler.Tests -c Debug --no-build --logger trx

# PackedNuspecTests inspects the real package; the build above runs with GeneratePackageOnBuild off.
echo "== pack Gtk4.Wpf =="
dotnet pack Gtk4.Wpf/Gtk4.Wpf.csproj -c Debug --no-build

echo "== Wpf.ProjectConverter unit tests =="
run_annotated "Linux ProjectConverter.Tests" \
	dotnet test Gtk4.Wpf.ProjectConverter.Tests -c Debug --no-build --logger trx

# XF_GTK_REQUIRE_NATIVE turns "no display / no GTK runtime" from a skip into a failure: both are
# installed above, so an unusable environment means the apt or Xvfb step silently broke.
echo "== PresentationFramework tests (real GTK 4 widgets, headless under Xvfb) =="
run_gtk_tests() {
  GDK_BACKEND=x11 XF_GTK_REQUIRE_NATIVE=1 GSETTINGS_BACKEND=memory \
    xvfb-run -a --server-args="-screen 0 1280x1024x24" \
      dotnet test Gtk4.PresentationFramework.Tests -c Debug --no-build --logger trx
}
run_annotated "Linux PresentationFramework.Tests" run_gtk_tests

# The UI test driver and the sample UI tests share the GTK setup above; they also run under Xvfb.
echo "== Wpf.Testing tests (UI driver over real GTK 4 widgets and official samples, headless under Xvfb) =="
run_ui_tests() {
  GDK_BACKEND=x11 XF_GTK_REQUIRE_NATIVE=1 GSETTINGS_BACKEND=memory \
    xvfb-run -a --server-args="-screen 0 1280x1024x24" \
      dotnet test Gtk4.Wpf.Testing.Tests -c Debug --no-build --logger trx
}
run_annotated "Linux Wpf.Testing.Tests" run_ui_tests
