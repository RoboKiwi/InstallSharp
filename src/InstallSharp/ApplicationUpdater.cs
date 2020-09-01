using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InstallSharp.Native;
using Microsoft.Win32;

namespace InstallSharp
{
    public class ApplicationUpdater
    {
        /// <summary>
        /// The default filename suffix for downloaded self-updating exes.
        /// </summary>
        
        static readonly HttpClient client;

        internal readonly ApplicationUpdaterConfig config;
        readonly CancellationToken cancellationToken;

        static ApplicationUpdater()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression =  DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            
            client = new HttpClient(handler);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationUpdater"/>, inferring all defaults from
        /// the running executable, including the updater URL e.g. if the application is called MyApplication.exe,
        /// and the Company is MyCompany, InstallSharp will check github.com/MyCompany/MyApplication for releases.
        /// </summary>
        public ApplicationUpdater() : this((ApplicationUpdaterConfig) null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationUpdater"/>, using the specified <see cref="updateUri"/> and inferring
        /// all other defaults for <see cref="ApplicationUpdaterConfig"/> from the running executable.
        /// </summary>
        /// <param name="updateUri">The URL for <see cref="ApplicationUpdaterConfig.UpdateUrl"/>.</param>
        /// <param name="cancellationToken"></param>
        public ApplicationUpdater(string updateUri, CancellationToken cancellationToken = default) : this(new ApplicationUpdaterConfig(updateUri), cancellationToken)
        {
        }

        public ApplicationUpdater(ApplicationUpdaterConfig config, CancellationToken cancellationToken = default)
        {
            this.cancellationToken = cancellationToken;
            this.config = ApplicationUpdaterConfigFactory.Create(config);
        }

        public Task UninstallAsync(UninstallArgs args = null)
        {
            var productName = args?.ProductName ?? config.ProductName;

            // Remove the Add / Remove Programs information
            Registry.CurrentUser.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + GetUninstallKeyName(), false);

            // Start Menu shortcut
            var programsFolder = new DirectoryInfo( Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.Programs)));
            var shortcutPath = new FileInfo(Path.Combine(programsFolder.FullName, productName + ".lnk"));
            if(shortcutPath.Exists) shortcutPath.Delete();

            // Desktop shortcut
            var desktopFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
            var desktopShortcut = new FileInfo(Path.Combine(desktopFolder.FullName, productName + ".lnk"));
            if( desktopShortcut.Exists ) desktopShortcut.Delete();

            if (args == null || !args.Silent)
            {
                config.Progress.Report(new ProgressModel(ProgressState.Done, "Uninstalled successfully", -1));
            }

            return Task.CompletedTask;
        }

        public string GetUninstallKeyName()
        {
            var value = config.Guid == Guid.Empty ? config.Name : config.Guid.ToString();
            if( string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("The detected uninstall key name is null or empty");
            return value;
        }

        public async Task InstallAsync(InstallArgs args = null)
        {
            var name = args?.Name ?? config.Name;
            var company = args?.Company ?? config.CompanyName;
            var path = Environment.ExpandEnvironmentVariables( args?.Path ?? config.InstallPath );
            var filename = Path.Combine(path, config.FileName);
            var productName = args?.ProductName ?? config.ProductName;
            
            // Copy the file to the destination if it doesn't already exist
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists) fileInfo.Directory.Create();
            File.Copy(config.FullFileName, fileInfo.FullName, true);

            // https://docs.microsoft.com/en-nz/windows/win32/msi/uninstall-registry-key
            var keyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + GetUninstallKeyName();
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + GetUninstallKeyName()))
            {
                if( key == null) throw new InvalidOperationException("Couldn't create / open registry key: " + keyName);

                var stringsToWrite = new[] {
                    new { Key = "DisplayIcon", Value = $"{filename},0" },
                    new { Key = "DisplayName", Value = args?.ProductName ?? config.ProductName },
                    new { Key = "DisplayVersion", Value = config.Version },
                    new { Key = "InstallDate", Value = DateTime.Now.ToString("yyyyMMdd") },
                    new { Key = "InstallLocation", Value = path },
                    new { Key = "Publisher", Value = company },
                    new { Key = "QuietUninstallString", Value = $"\"{filename}\" setup uninstall /silent"},
                    new { Key = "UninstallString", Value = $"\"{filename}\" setup uninstall"},
                    new { Key = "HelpLink", Value = args?.Url ?? "" },
                    new { Key = "Comments", Value = "Comments" }
                };

                var dwordsToWrite = new[] {
                    new { Key = "EstimatedSize", Value = (int)(new FileInfo(filename).Length / 1024) },
                    new { Key = "NoModify", Value = 1 },
                    new { Key = "NoRepair", Value = 1 },
                    new { Key = "Language", Value = 0x0409 },
                };

                foreach (var kvp in stringsToWrite)
                {
                    key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.String);
                }
                foreach (var kvp in dwordsToWrite)
                {
                    key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
                }
            }

            // Start Menu shortcut
            var programsFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs)));
            var shortcutPath = new FileInfo(Path.Combine(programsFolder.FullName, productName + ".lnk"));
            CreateShortcut(shortcutPath.FullName, config.FullFileName);
            
            // Desktop shortcut
            var desktopFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
            var desktopShortcut = new FileInfo(Path.Combine(desktopFolder.FullName, productName + ".lnk"));
            CreateShortcut(desktopShortcut.FullName, config.FullFileName);
        }

        void CreateShortcut(string path, string target)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            var shortcut = (IShellLinkW)new CShellLink();
            
            try
            {
                shortcut.SetWorkingDirectory(Path.GetDirectoryName(target));
                shortcut.SetPath(target);
                ((IPersistFile)shortcut).Save(path, true);
            }
            finally
            {
                Marshal.ReleaseComObject(shortcut);
            }

            // ReSharper restore SuspiciousTypeConversion.Global
        }

        public void Launch(LaunchArgs args = null)
        {
            var commandLine = new StringBuilder();

            if (args?.Target != null)
            {
                commandLine.Append(args.Target);
            }
            else
            {
                commandLine.Append(config.FullFileName);
            }

            if (args?.Args != null)
            {
                commandLine.Append(" ").Append(args.Args);
            }

            var normalPriorityClass = 0x0020;
            var processInformation = new ProcessInformation();
            var startupInfo = new StartupInfo();
            var processSecurity = new SecurityAttributes();
            var threadSecurity = new SecurityAttributes();

            processSecurity.nLength = Marshal.SizeOf(processSecurity);
            threadSecurity.nLength = Marshal.SizeOf(threadSecurity);

            // TODO: Add support for *nix

            if (ProcessManager.CreateProcess(null, commandLine, processSecurity, threadSecurity, false, normalPriorityClass, IntPtr.Zero, null, startupInfo, processInformation))
            {
                // Process was created successfully
                return;
            }

            // We couldn't create the process, so raise an exception with the details.
            throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        /// <summary>
        /// Downloads and launches any available update, otherwise returns <c>false</c>. You should exit immediately
        /// if <c>true</c> is returned, as the update process will be waiting for processes to exit and release locks.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(UpdateCheckArgs args = null)
        {
            // Check for available update
            var update = await CheckForUpdateAsync(args);
            if (update != null && update.IsUpgrade())
            {
                // Download the update side by side
                var file = await DownloadAsync(update, config.Progress, cancellationToken);

                // Launch the downloaded update
                Launch(new LaunchArgs(file.Filename));

                return true;
            }

            return false;
        }

        public async Task<TempFile> DownloadAsync(UpdateInfo info, IProgress<ProgressModel> progress, CancellationToken cancellationToken = default)
        {
            using (var response = await client.GetAsync(info.Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    progress.Report(new ProgressModel(ProgressState.Ready, "Update cancelled. Ready.", 100));
                    return null;
                }

                var length = response.Content.Headers.ContentLength;
                double lengthInMb = !length.HasValue ? -1 : (double)length.Value / 1024 / 1024;
                double bytesDownloaded = 0;
                
                // Download next to the executing .exe but with the extension .update.exe
                var fileInfo = new FileInfo( Path.ChangeExtension(config.FullFileName, config.UpdateSuffix));
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var file = File.Open(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var buffer = new byte[65535 * 4];

                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    while (bytesRead != 0)
                    {
                        await file.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        bytesDownloaded += bytesRead;

                        if (length.HasValue)
                        {
                            double downloadedMegs = bytesDownloaded / 1024 / 1024;
                            var percent = (int)Math.Floor((bytesDownloaded / length.Value) * 100);
                            var status = string.Format(CultureInfo.CurrentUICulture, "Downloaded {0:F2} MB of {1:F2} MB", downloadedMegs, lengthInMb);
                            progress.Report(new ProgressModel(ProgressState.Updating, status, percent));
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            progress.Report(new ProgressModel(ProgressState.Ready, "Update cancelled. Ready.", 100));
                            return null;
                        }

                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    }
                }

                var model = new TempFile
                {
                    Asset = info.Name,
                    Filename = fileInfo.FullName
                };

                return model;
            }

        }

        public async Task<UpdateInfo> CheckForUpdateAsync(UpdateCheckArgs args = null)
        {
            var assetName = args?.AssetName ?? config.AssetName;
            var allowPreRelease = args != null && args.AllowPreRelease;
            var ignoreTags = args?.IgnoreTags ?? new string[] { };
            var uri = args?.Uri ?? config.UpdateUrl;
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assetName, config.Version));

            // Get the latest releases from GitHub
            using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var results = JsonSerializer.Deserialize<List<GitHubRelease>>(content);

                var latest = results.FirstOrDefault(x => (allowPreRelease || !x.prerelease) &&
                                                         !ignoreTags.Any(tag => tag.Equals(x.tag_name, StringComparison.OrdinalIgnoreCase) ));
                if (latest == null)
                {
                    Trace.TraceWarning("Couldn't find a release from the list returned by GitHub");
                    return null;
                }

                var asset = latest.assets.FirstOrDefault(x => x.name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
                if (asset == null)
                {
                    Trace.TraceWarning($"Couldn't find '${assetName}' in the release assets for " + latest.name);
                    return null;
                }

                var info = new UpdateInfo
                {
                    Version = new SemanticVersion(latest.tag_name),
                    Uri = asset.browser_download_url.ToString(),
                    IsPreRelease = latest.prerelease,
                    Name = asset.name,
                    CurrentVersion = new SemanticVersion(config.Version)
                };

                return info;
            }
        }

        /// <summary>
        /// Waits until all other instances of this .exe have finished running,
        /// or by a specified .exe name.
        /// </summary>
        /// <param name="processName">The name of the processes to wait for (default of <c>null</c> to use the current process name)</param>
        /// <param name="millisecondsToWait">The number of milliseconds to wait for each process to exit (default <c>2000</c>)</param>
        void WaitForOtherProcesses(string processName = null, int millisecondsToWait = 2000)
        {
            // Make sure we don't wait for ourselves
            var currentProcess = Process.GetCurrentProcess();

            processName ??= currentProcess.ProcessName;

            var processes = Process.GetProcessesByName(processName).Where(x => x.Id != currentProcess.Id).ToList();

            foreach (var process in processes)
            {
                if (process.HasExited) continue;
                process.WaitForExit(millisecondsToWait);
            }
        }

        public async Task ApplyUpdateAsync(ApplyUpdateArgs args)
        {
            var source = config.FullFileName;
            var destination = args.Target;

            // If no destination was specified, default to overwrite current path, trimmming ".update.exe" to ".exe"
            if (string.IsNullOrWhiteSpace(destination))
            {
                destination = source;
                if (destination.EndsWith(config.UpdateSuffix))
                {
                    destination = destination.Substring(0, destination.Length - config.UpdateSuffix.Length) + ".exe";
                }
            }

            // Make sure the destination exe doesn't have any running processes that would prevent an overwrite
            WaitForOtherProcesses( Path.GetFileNameWithoutExtension(destination) );
            
            File.Copy(source, destination, true);
            
            // Launch the updated app
            if (args.Launch)
            {
                // Add arguments to delete the temporary exe now that the update is applied
                Launch(new LaunchArgs { Target = destination });
            }
        }
        
        class GitHubRelease
        {
            public string name { get; set; }

            public string tag_name { get; set; }

            public ICollection<GitHubAsset> assets { get; set; }

            public bool prerelease { get; set; }
        }

        class GitHubAsset
        {
            public long id { get; set; }

            public string name { get; set; }

            public Uri browser_download_url { get; set; }

            public string content_type { get; set; }

            public long size { get; set; }

            public DateTimeOffset created_at { get; set; }

            public DateTimeOffset updated_at { get; set; }
        }

        public async Task CleanupAsync(CleanupArgs args)
        {
            if (args?.Target == null) return;
            WaitForOtherProcesses();
            File.Delete(args.Target);
        }
        
        public async Task<bool> ExecuteAsync()
        {
            // Use the application updater configuration and the command line to figure
            // out what we're going to be doing
            var commandline = CommandLineArgumentsFactory.Parse(config);

            // We didn't find any valid InstallSharp command, so return early
            if (commandline.Command == Command.None) return false;

            switch (commandline.Command)
            {
                case Command.Install:
                    break;

                case Command.Update:
                    throw new NotImplementedException();
                
                case Command.ApplyUpdate:
                    
                    var args = new ApplyUpdateArgs
                    {
                        Elevate = commandline.Elevate,
                        Launch = commandline.Launch,
                        Target = commandline.Target
                    };

                    await ApplyUpdateAsync(args).ConfigureAwait(false);
                    break;

                case Command.Uninstall:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public UpdateContext GetUpdateContext()
        {
            var updateContext = new UpdateContext();

            var directoryName = Path.GetDirectoryName(config.FullFileName);
            if (directoryName == null) throw new InvalidOperationException("Couldn't get directory name that the app is running from");
            var directory = new DirectoryInfo(directoryName);

            // If the application is executing from the TEMP folder, then it's probably been executed from a .zip that has been
            // opened and temporarily extracted by Windows Explorer or 7-zip etc
            var isTempFolder = directoryName.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase);

            // If the application is executing from the special Downloads folder, we can't really count it as "installed"
            // and it should probably be copied somewhere to be considered as installed and allowed to have shortcuts etc
            var downloadsFolder = Win32Api.GetKnownFolderPath(Win32Api.KnownFolders.Downloads, Win32Api.SpecialFolderOption.DoNotVerify);
            var isDownloadFolder = directoryName.StartsWith(downloadsFolder, StringComparison.OrdinalIgnoreCase);

            var filename = this.config.FileName;

            // We won't count as "installed" if we're running from temp or downloads
            updateContext.IsDeployed = !isTempFolder && !isDownloadFolder;

            // Check for in-place update of exe (.update.exe)
            var extensionless = Path.GetFileNameWithoutExtension(filename);
            if (extensionless.EndsWith(config.UpdateSuffix, StringComparison.OrdinalIgnoreCase))
            {
                
                updateContext.IsUpdate = true;
                updateContext.PackageType = PackageType.Executable;
                updateContext.UpdateSource = new FileInfo(config.FullFileName);
                var destinationName = extensionless.Substring(0, extensionless.Length - config.UpdateSuffix.Length);
                updateContext.UpdateDestination = new FileInfo( Path.Combine(Path.GetDirectoryName(config.FullFileName), destinationName + ".exe" ) );
                return updateContext;
            }

            // Check for archive update (update parent folder from extracted sub folder containing updates)
            if (Path.GetExtension(directoryName).Equals(config.UpdateSuffix))
            {
                updateContext.IsUpdate = true;
                updateContext.PackageType = PackageType.Archive;
                updateContext.UpdateSource = directory;
                updateContext.UpdateDestination = directory.Parent;

                return updateContext;
            }

            // Check if the app is executing from the TEMP folder, in which case it's been extracted
            // and executed from a zip file
            if (isTempFolder)
            {
                updateContext.IsUpdate = false;
                updateContext.IsDeployed = false;
                updateContext.PackageType = PackageType.Archive;
                updateContext.UpdateSource = directory;
                updateContext.UpdateDestination = new FileInfo(config.InstallPath);
                return updateContext;
            }

            // Are we running from the Downloads folder?
            if (isDownloadFolder)
            {
                // If we're in a sub directory under Downloads then we can consider this an archive package (one or more files, in own folder)
                if (directory.Parent != null && directory.Parent.FullName.Equals(downloadsFolder, StringComparison.OrdinalIgnoreCase))
                {
                    updateContext.IsUpdate = true;
                    updateContext.PackageType = PackageType.Archive;
                    updateContext.UpdateSource = directory;
                    updateContext.UpdateDestination = directory.Parent;
                }
                else
                {
                    updateContext.IsUpdate = false;
                    updateContext.PackageType = PackageType.Executable;
                    updateContext.UpdateSource = new FileInfo(config.FullFileName);
                    updateContext.UpdateDestination = new FileInfo(config.InstallPath);
                }

                return updateContext;
            }

            return null;
        }
    }

    /// <summary>
    /// Contains information on where the application is executing or updating from.
    /// </summary>
    public class UpdateContext
    {
        public PackageType PackageType { get; set; }

        /// <summary>
        /// <c>true</c> if the application is currently running as an update
        /// </summary>
        public bool IsUpdate { get; set; }

        public FileSystemInfo UpdateSource { get; set; }

        public FileSystemInfo UpdateDestination { get; set; }

        public bool IsDeployed { get; set; }
    }

    public enum DeploymentDestination
    {
        None = 0,

        /// <summary>
        /// The application will get deployed where the user first placed and ran the exe
        /// </summary>
        InPlace = 1,

        /// <summary>
        /// The application will install to LocalAppData\Programs
        /// </summary>
        UserPrograms = 2,

        /// <summary>
        /// The application will install to ProgramData\Programs
        /// </summary>
        MachinePrograms = 3
    }

    public enum DeploymentSource
    {
        None = 0,

        /// <summary>
        /// Releases are on GitHub
        /// </summary>
        GitHub = 1,

    }

    public enum PackageType
    {
        None = 0,
        Executable = 1,
        Archive = 2,
        Msi = 3,
        Msix
    }
}