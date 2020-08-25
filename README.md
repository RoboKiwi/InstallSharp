# InstallSharp

InstallSharp is a library for easily making your .NET application self-installing and self-updating.

The goal is to be a fast and simple installer for small, self-contained applications that don't need the complexity of an MSI installer.

## Features

* Supports Desktop and Start Menu shortcuts in Windows
* Portable exe support (include InstallSharp as embedded code instead of separate assembly)
* Allows self-updating from GitHub releases

## Planned Features

* Include easy support to add automatic versioning of your releases that can plug into Azure DevOps, GitHub Actions etc
* Scripts included in package with commands for publishing, versioning etc
* Multiple platform support (MacOSX, Linux)

## TODO

[x] Start Menu shortcut
[x] Desktop shortcut
[x] Hosting releases on GitHub
[x] Semantic version support
[ ] Hosting releases on CDN
[ ] Portable app (self contained exe)
[ ] Archive support for releases (multiple files for distribution instead of portable exe)
[ ] NuGet package
[ ] MacOSX support
[ ] Linux support
[ ] Code signing (Authenticode)
[ ] Custom setup actions (install, uninstall, update etc)
[ ] Automatic versioning script
[ ] NuGet scripts
[ ] Migration from ClickOnce
[ ] Migration from Squirrel