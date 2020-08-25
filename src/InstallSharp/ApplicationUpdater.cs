using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
        public const string DefaultUpdateSuffix = ".update.exe";
        
        static readonly HttpClient client;

        internal readonly ApplicationUpdaterArgs Args;
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
        /// Creates a new instance of <see cref="ApplicationUpdater"/>, using the specified <see cref="updateUri"/> and inferring
        /// all other defaults for <see cref="ApplicationUpdaterArgs"/> from the running executable.
        /// </summary>
        /// <param name="updateUri">The URL for <see cref="ApplicationUpdaterArgs.UpdateUrl"/>.</param>
        /// <param name="cancellationToken"></param>
        public ApplicationUpdater(string updateUri, CancellationToken cancellationToken = default) : this(new ApplicationUpdaterArgs(updateUri), cancellationToken)
        {
        }

        public ApplicationUpdater(ApplicationUpdaterArgs args, CancellationToken cancellationToken = default)
        {
            this.Args = args;
            this.cancellationToken = cancellationToken;
            
            var module = Process.GetCurrentProcess().MainModule;
            if (module == null) throw new NotSupportedException("Can't infer application updater args as the main module for the current process can't be found");
            var assembly = Assembly.GetEntryAssembly();
            
            var info = module.FileVersionInfo;

            // Infer any arguments from the running executable
            args.FullFileName = info.FileName;
            args.FileName ??= Path.GetFileName(args.FullFileName);
            args.Name ??= Path.GetFileNameWithoutExtension(args.FileName);

            args.Version = info.FileVersion;
            args.AssetName ??= args.FileName;
            args.CompanyName ??= info.CompanyName;
            args.ProductName ??= info.ProductName;
            args.Progress ??= new Progress<ProgressModel>();

            if (args.Name == null) throw new ArgumentNullException(nameof(args.Name), "Short filename is null");
            args.InstallPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", args.Name);

            // Try get the program GUID to help uniquely identify it even across renames
            if (args.Guid != Guid.Empty) return;
            if (assembly == null) throw new NotSupportedException("Couldn't get the entry assembly to determine program guid");
            var guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            if (guidAttribute == null) return;
            if(!Guid.TryParse(guidAttribute.Value, out var guid)) throw new FormatException($"Couldn't parse GUID from assembly attribute: '{guidAttribute.Value}'");
            args.Guid = guid;
        }

        public Task UninstallAsync(UninstallArgs args = null)
        {
            var productName = args?.ProductName ?? Args.ProductName;

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
                Args.Progress.Report(new ProgressModel(ProgressState.Done, "Uninstalled successfully", -1));
            }

            return Task.CompletedTask;
        }

        public string GetUninstallKeyName()
        {
            var value = Args.Guid == Guid.Empty ? Args.Name : Args.Guid.ToString();
            if( string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("The detected uninstall key name is null or empty");
            return value;
        }

        public async Task InstallAsync(InstallArgs args = null)
        {
            var name = args?.Name ?? Args.Name;
            var company = args?.Company ?? Args.CompanyName;
            var path = Environment.ExpandEnvironmentVariables( args?.Path ?? Args.InstallPath );
            var filename = Path.Combine(path, Args.FileName);
            var productName = args?.ProductName ?? Args.ProductName;
            
            // Copy the file to the destination if it doesn't already exist
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists) fileInfo.Directory.Create();
            File.Copy(Args.FullFileName, fileInfo.FullName, true);

            // https://docs.microsoft.com/en-nz/windows/win32/msi/uninstall-registry-key
            var keyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + GetUninstallKeyName();
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + GetUninstallKeyName()))
            {
                if( key == null) throw new InvalidOperationException("Couldn't create / open registry key: " + keyName);

                var stringsToWrite = new[] {
                    new { Key = "DisplayIcon", Value = $"{filename},0" },
                    new { Key = "DisplayName", Value = args?.ProductName ?? Args.ProductName },
                    new { Key = "DisplayVersion", Value = Args.Version },
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
            CreateShortcut(shortcutPath.FullName, fileVersionInfo.FileName);
            
            // Desktop shortcut
            var desktopFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
            var desktopShortcut = new FileInfo(Path.Combine(desktopFolder.FullName, productName + ".lnk"));
            CreateShortcut(desktopShortcut.FullName, fileVersionInfo.FileName);
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
                commandLine.Append(fileVersionInfo.FileName);
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

            if (ProcessManager.CreateProcess(null, commandLine, processSecurity, threadSecurity, false, normalPriorityClass, IntPtr.Zero, null, startupInfo, processInformation))
            {
                // Process was created successfully
                return;
            }

            // We couldn't create the process, so raise an exception with the details.
            throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        public async Task UpdateAsync(UpdateCheckArgs args)
        {
            // Check for available update
            var update = await CheckForUpdateAsync(args);
            if (update != null)
            {
                // Download the update side by side
                var file = await DownloadAsync(update, progress, cancellationToken);

                // Launch the downloaded update
                Launch(new LaunchArgs(file.Filename));
            }
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
                var fileInfo = new FileInfo( Path.ChangeExtension(fileVersionInfo.FileName, updateSuffix));
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
            var assetName = args?.AssetName ?? Path.GetFileName(fileVersionInfo.FileName);
            var allowPreRelease = args != null && args.AllowPreRelease;
            var ignoreTags = args?.IgnoreTags ?? new string[] { };
            var uri = args?.Uri ?? defaultUri;
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assetName, fileVersionInfo.FileVersion));

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
                    CurrentVersion = new SemanticVersion(fileVersionInfo.ProductVersion)
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
            var source = fileVersionInfo.FileName;
            var destination = args.Target;

            // If no destination was specified, default to overwrite current path, trimmming ".update.exe" to ".exe"
            if (string.IsNullOrWhiteSpace(destination))
            {
                destination = fileVersionInfo.FileName;
                if (destination.EndsWith(updateSuffix))
                {
                    destination = destination.Substring(0, destination.Length - updateSuffix.Length) + ".exe";
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
    }
}