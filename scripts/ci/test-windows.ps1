# CI (Windows): build the WPF-on-GTK 4 backend and run its unit-test suites.
#
# The native GTK 4 runtime is fetched by GtkSharp4's own GtkSharp.targets (a gvsbuild download into
# %LOCALAPPDATA%\Gtk\4.22.4), which auto-imports for package consumers - PresentationFramework is
# built by the solution below, so the install runs as part of the build. No display server is
# needed: the renderer tests draw through the GDK Win32 backend, and WpfGtkFixture initialises GTK on
# a dedicated STA thread (GTK 4's Win32 backend calls OleInitialize, which requires STA).
#
# GtkSharp4 itself is restored from the committed vendored feed (build/gtksharp4-packages, see
# NuGet.config); Net4x.NuGetUtility and everything else come from nuget.org. Assumes .NET 10 (and 8,
# for Net4x.NuGetUtility's Windows-only MSBuild tasks) from actions/setup-dotnet.
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

# Warm up the GTK 4 runtime install SERIALLY first. GtkSharp4's InstallGtk target downloads the
# gvsbuild zip into %LOCALAPPDATA%\Gtk and is guarded only by "gtk-4-1.dll not present" - with no
# lock, a parallel first build has every GtkSharp4-referencing project race on the same gtk.zip and
# fails with MSB3923 ("the process cannot access the file ... because it is being used by another
# process"). Building one consumer with -maxcpucount:1 installs it once; the solution build below
# then finds it present and skips the download, so there is nothing to race on.
Write-Host '== warm up the gvsbuild GTK 4 runtime (serial, avoids the parallel download race) =='
dotnet build Gtk4.PresentationFramework/Gtk4.PresentationFramework.csproj -c Debug -maxcpucount:1 -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE) { exit $LASTEXITCODE }

Write-Host '== build (Debug; installs the gvsbuild GTK 4 runtime via GtkSharp4.targets) =='
# GeneratePackageOnBuild is disabled: CI builds to test, not to pack. Debug is the tested config.
dotnet build Gtk4.Wpf.sln -c Debug -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# XF_GTK_REQUIRE_NATIVE turns "no display / no GTK runtime" from a harness skip into a failure, so a
# broken install goes red rather than green-by-absence.
$env:XF_GTK_REQUIRE_NATIVE = '1'

# The logic suites (WindowsBase DP/dispatcher, MarkupCompiler XAML->code, Wpf.ProjectConverter) need
# no GTK runtime or display.
Write-Host '== WindowsBase unit tests =='
dotnet test Gtk4.WindowsBase.Tests -c Debug --no-build --logger trx
if ($LASTEXITCODE) { exit $LASTEXITCODE }

Write-Host '== MarkupCompiler unit tests =='
dotnet test Gtk4.MarkupCompiler.Tests -c Debug --no-build --logger trx
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# PackedNuspecTests inspects the real package; the build above runs with GeneratePackageOnBuild off.
Write-Host '== pack Gtk4.Wpf =='
dotnet pack Gtk4.Wpf/Gtk4.Wpf.csproj -c Debug --no-build
if ($LASTEXITCODE) { exit $LASTEXITCODE }

Write-Host '== Wpf.ProjectConverter unit tests =='
dotnet test Gtk4.Wpf.ProjectConverter.Tests -c Debug --no-build --logger trx
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# PresentationFramework hosts the real-GTK-4 renderer smoke tests; WpfGtkFixture runs them on an STA
# GTK thread (see header).
Write-Host '== PresentationFramework tests (real GTK 4 widgets, STA GTK thread) =='
dotnet test Gtk4.PresentationFramework.Tests -c Debug --no-build --logger trx
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# The UI test driver and the sample UI tests: the same GTK thread setup, driven through UI Automation.
Write-Host '== Wpf.Testing tests (UI driver over real GTK 4 widgets and official samples) =='
dotnet test Gtk4.Wpf.Testing.Tests -c Debug --no-build --logger trx
exit $LASTEXITCODE
