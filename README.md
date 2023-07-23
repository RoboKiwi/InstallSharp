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

## Goals

* Simplicity (avoid complexity; complex installation needs can be addressed by MSI i.e. WiX)
* Convention over configuration (most configuration can be inferred by the deployed application's .exe)
* Unobtrusive

## TODO

- [x] Start Menu shortcut
- [x] Desktop shortcut
- [x] Hosting releases on GitHub
- [x] Semantic version support
- [x] Add / Remove Programs details
- [x] Progress callback to allow displaying download / setup progress
- [ ] Automatically unblock downloaded updates?
- [ ] Maybe use Mutex to handle locks?
- [ ] Allow UAC elevation prompts for installs that require administrative privileges
- [ ] Hosting releases on CDN
- [ ] App packaging types
  - [ ] Portable app (single self contained exe)
  - [ ] Zipped app (portable, 1 or more files)
  - [ ] MSI / MSIX
- [ ] NuGet package
- [ ] Multiple platform support
  - [x] Windows
  - [ ] Mac OSX
  - [ ] Linux support
- [ ] Code signing (Authenticode)
- [ ] Custom setup actions (install, uninstall, update etc)
- [ ] Automatic versioning script
- [ ] NuGet scripts
- [ ] Migration from ClickOnce
- [ ] Migration from Squirrel
- [ ] Supporting pinning / pinned shortcuts

# Docs

## Get Started

* Add a reference to the InstallSharp NuGet package for your project
* Add code to create a global `ApplicationUpdater`
* Add an assembly attribute to specify your releases URL
* Execute InstallSharp before the rest of your program, so that updates can be applied if necessary

```csharp

    <assembly: >
    class Program
    {
        internal static readonly ApplicationUpdater Updater = new ApplicationUpdater();

        static void Main(string[] args)
        {
            var result = Updater.Execute();
            if( result.RestartPending ) return;
                        
            // Process your program as normal
            Console.WriteLine("Hello World!");
        }
    }
```

## Checking for updates


```csharp




```

## Downloading an update


## Applying an update


## Restarting after an update


## Automatically downloading updates in the background


## Automatically applying updates in the background


## Running custom code during setup actions

# Command line

Setup operations are invoked by calling the application with a `setup` argument e.g. `<app.exe> setup`

| Command         | Operation | Re-launch |
|-----------------|-----------|-----------|
| `app.exe setup <install>` | Default install | yes |
| `app.exe setup update` | Downloads latest version side-by-side, applies the update and exits | no |
| `app.exe setup uninstall` | Default uninstall, without removing the application file(s) | no |
| `app.exe setup remove` | Full uninstall, removes application file(s) | no |

# Renaming your application

If you choose to rename your application, if you have been using a guid then you won't have to do anything.

This is why you're encouraged to specify an installation guid either in the main application's `[assembly: Guid("guid here")]` or by specifying the guid in the arguments to `[assembly: InstallSharpAttribute]` or when creating your `ApplicationUpdater` object.

## Advanced

If you wish to change the default setup argument from `setup` to something else, specify the option when calling `ApplicationUpdater.ParseCommandLine`.

# Installation Packages

There are three ways to package and deliver your app:

* Self-contained (portable) executable
* Zip file containing 1 or more files, including the main .exe
* MSI/MSIX installer package

# Migration

## Moving from MSI to InstallSharp

If you already have a system that handles deployments and updates MSI, you should integrate your application with InstallSharp, then make a final MSI release containing the application that now has InstallSharp support.

You could continue to release your application in both MSI and EXE forms to give users that are still on the MSI installation ample time to eventually update to the latest version, before you finally phase out MSI support.

# Internals

## How InstallSharp knows when an update is being applied

InstallSharp determines that the program is running as an update by the environment and context, which can differ slightly from how the application is being packaged and delivered.

### Single exe

When a single executable is updated, the update is downloaded and executed next to the .exe, but the file extension is ".update.exe". When the update is launched, InstallSharp detects that it is running as ".update.exe" and execute as an update.

### Zip file

When a zip file update is downloaded, the zip is downloaded and extracted next to the current exe into a folder called ".update". When the update is applied, InstallSharp will check if it's running from the ".update" directory and execute as an update.

### MSI / MSIX

When an installation package update is downloaded, the installer will be invoked as normal, and the installer is responsible for re-launching the application as necessary. InstallSharp is only responsible for launching the installer.

## Installation Context

When the executable runs, InstallSharp will check the directory, parent directory, user environment and registry to determine whether the application is "installed", registered in Add/Remove Programs, has shortcuts, and is running as an update.

| Scenario | Example Location | IsUpdate | IsDeployed | Package Type | Update / Install Target |
|----------|----------|----------|-------------|--------------|-------------------------|
| 1 | C:\Users\David\Downloads\MyApp.exe | `false` | `false` | exe | |
| 2 | C:\Users\David\Downloads\MyApp.update.exe | `true` | `false` | exe | C:\Users\David\Downloads\MyApp.exe |
| 3 | C:\Users\David\Downloads\MyApp\MyApp.exe | `false` | `true` | archive | |
| 4 | C:\Users\David\Downloads\MyApp\\.update\MyApp.exe | `true` | `true` | archive | C:\Users\David\Downloads\MyApp |
| 5 | C:\Users\David\AppData\Local\Temp\MyApp.exe | `false` | `false` | exe | C:\Users\David\AppData\Local\Programs\MyApp\MyApp.exe |
| 6 | C:\Users\David\AppData\Local\Temp\ZipTmp\MyApp.exe | `false` | `false` | archive | C:\Users\David\AppData\Local\Programs\MyApp\MyApp.exe |

### Scenario 1

The application is self-contained and has been downloaded and run. It's not considered "deployed", but it can still be self-updated (Scenario 2), and even registered and shortcutted.

### Scenario 2

This is the update scenario for Scenario 1

### Scenario 3

The application was an archive deploy, and apparently simply downloaded and extracted in place in the Downloads folder. This is loosely considered "deployed" in that it is isolated somewhat in its own folder, even if it exists under Downloads.

### Scenario 4

This is the update scenario for Scenario 3

### Scenario 5

The application is self-contained and executing from the %TEMP% folder. It can't be considered deployed, and really needs to be "deployed" somewhere. Deployment in this scenario would default to %LocalAppData%\Programs unless the user specified somewhere else (or copied the .exe manually).

### Scenario 6

The application is an archive package, and would have been "opened" and temporarily extracted to the %TEMP% folder by an archive program such as the default Windows Explorer zip support, or something like 7-zip. The app is not considered deployed.