# CI (Windows): build the GTK 4 backend and run all three unit-test suites.
#
# The native GTK 4 runtime is fetched by GtkSharp4's own GtkSharp.targets (a gvsbuild download into
# %LOCALAPPDATA%\Gtk\4.22.4), which auto-imports for package consumers - Platform.GTK / Maps.GTK are
# built by the solution below, so the install runs as part of the build. No display server is
# needed: the renderer tests draw through the GDK Win32 backend, and GtkFixture initialises GTK on a
# dedicated STA thread (GTK 4's Win32 backend calls OleInitialize, which requires STA).
#
# GtkSharp4 itself is restored from the committed vendored feed (build/gtksharp4-packages, see
# NuGet.config); Net4x.NuGetUtility and everything else come from nuget.org. Assumes .NET 10 (and 8,
# for Net4x.NuGetUtility's Windows-only MSBuild tasks) from actions/setup-dotnet.
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

Write-Host '== build (Debug; installs the gvsbuild GTK 4 runtime via GtkSharp4.targets) =='
# GeneratePackageOnBuild is disabled: CI builds to test, not to pack, and packing trips the
# pre-existing Controls.GTK NU5039 (missing readme) that has nothing to do with the tests.
# Release cannot build the XAML suite (InternalsVisibleTo is #if DEBUG upstream), so Debug is forced.
dotnet build Xamarin.Forms.Gtk4.sln -c Debug -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# XF_GTK_REQUIRE_NATIVE turns "no display / no GTK runtime" from a harness skip into a failure, so a
# broken install goes red rather than green-by-absence.
$env:XF_GTK_REQUIRE_NATIVE = '1'

Write-Host '== Core unit tests =='
dotnet test Xamarin.Forms.Core.UnitTests -c Debug --no-build --logger trx
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# Category!=LongRunning drops the MSBuildTests: build-integration tests that restore generated
# projects from an OS-temp directory, where the repo's relative "Local" feed cannot resolve (a
# pre-existing NU1301 unrelated to the GTK backend). Every XAML loader / XamlC compiler test runs.
Write-Host '== XAML unit tests (excluding LongRunning MSBuild integration tests) =='
dotnet test Xamarin.Forms.Xaml.UnitTests -c Debug --no-build --logger trx --filter 'Category!=LongRunning'
if ($LASTEXITCODE) { exit $LASTEXITCODE }

Write-Host '== GTK renderer tests (real GTK 4 widgets, STA GTK thread) =='
dotnet test Xamarin.Forms.Platform.GTK.UnitTests -c Debug --no-build --logger trx
exit $LASTEXITCODE
