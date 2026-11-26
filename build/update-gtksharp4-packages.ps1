# Refreshes the committed GtkSharp4 wrapper packages under build/gtksharp4-packages/ (the vendored
# local feed CI and fresh clones restore the GTK 4 binding from - see NuGet.config).
#
# GtkSharp4 is a hard fork that is not published to nuget.org, so its 8 core wrapper packages are
# committed here. Run this after rebuilding the GtkSharp4 fork to pull the new nupkgs in, then commit
# the result. NuGet.config floats the reference at 4.22.4.* so a new build is picked up automatically.
#
#   pwsh build/update-gtksharp4-packages.ps1 -Source C:\Temp\GtkSharp\BuildOutput\NugetPackages
param(
    [string]$Source = (Join-Path $env:TEMP '..\GtkSharp\BuildOutput\NugetPackages')
)
$ErrorActionPreference = 'Stop'

# The closure Gtk4.Wpf actually restores: GtkSharp4 and its transitive wrapper dependencies.
# The workload, Ref/Runtime/Sdk/Template/Manifest packages and the optional bindings (Adwaita,
# WebkitGtk, GtkSource, JavaScriptCore) are deliberately excluded - the backend does not reference
# them.
$packages = @(
    'GtkSharp4', 'GdkSharp4', 'GskSharp4', 'GLibSharp4',
    'GioSharp4', 'CairoSharp4', 'PangoSharp4', 'GrapheneSharp4'
)

$dest = Join-Path $PSScriptRoot 'gtksharp4-packages'
if (-not (Test-Path $Source)) { throw "GtkSharp build output not found: $Source" }
New-Item -ItemType Directory -Force -Path $dest | Out-Null

# Replace the whole set so a version bump does not leave stale nupkgs behind (the float would then
# resolve to the wrong one).
Get-ChildItem $dest -Filter '*.nupkg' | Remove-Item -Force

foreach ($id in $packages) {
    $latest = Get-ChildItem $Source -Filter "$id.*.nupkg" |
        Where-Object { $_.Name -notmatch 'snupkg' } |
        Sort-Object Name | Select-Object -Last 1
    if (-not $latest) { throw "no nupkg for $id under $Source" }
    Copy-Item $latest.FullName $dest
    Write-Host "vendored $($latest.Name)"
}
Write-Host "Done. Review and commit build/gtksharp4-packages/."
