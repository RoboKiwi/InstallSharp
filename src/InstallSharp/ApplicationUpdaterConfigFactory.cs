using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace InstallSharp
{
    public static class ApplicationUpdaterConfigFactory
    {
        public static ApplicationUpdaterConfig Create(ApplicationUpdaterConfig config = null, FileVersionInfo info = null, Assembly assembly = null)
        {
            var Args = config ?? new ApplicationUpdaterConfig();

            if (info == null)
            {
                var module = Process.GetCurrentProcess().MainModule;
                if (module == null) throw new NotSupportedException("Can't infer application updater args as the main module for the current process can't be found");
                info = module.FileVersionInfo;
            }

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }
            
            // Infer any arguments from the running executable
            Args.FullFileName ??= info.FileName;
            Args.FileName ??= Path.GetFileName(Args.FullFileName);
            Args.Name ??= Path.GetFileNameWithoutExtension(Args.FileName);

            Args.Version ??= info.FileVersion;
            Args.AssetName ??= Args.FileName;
            Args.CompanyName ??= info.CompanyName;
            Args.ProductName ??= info.ProductName;
            Args.Progress ??= new Progress<ProgressModel>();

            if (Args.Name == null) throw new ArgumentNullException(nameof(Args.Name), "Short filename is null");
            Args.InstallPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", Args.Name);
            
            Args.UpdateSuffix ??= ApplicationUpdaterConfig.DefaultUpdateSuffix;
            
            // Get the InstallSharp attribute, which has custom configuration that might not be inferred or passed in manual configuration
            var attribute = assembly?.GetCustomAttribute<InstallSharpAttribute>();

            // Find the application url if not passed
            if (Args.UpdateUrl == null)
            {
                if (!string.IsNullOrWhiteSpace(attribute?.Url))
                {
                    // The application url is specified in the InstallSharpAttribute
                    Args.UpdateUrl = attribute.Url;
                }
                else
                {
                    // Try to automatically infer a GitHub url from the application name and company
                    Args.UpdateUrl = "github.com/" + Args.CompanyName.Replace(" ", "") + "/" + Args.Name.Replace(" ", "");
                }
            }

            // Get a GUID for the program. In order of precendance, it can be specified:
            //
            // 1. In the ApplicationUpdateConfig passed to ApplicationUpdater
            // 2. In the InstallSharpAttribute on the application assembly
            // 3. In the GuidAttribute on the application assembly

            // 1. In the ApplicationUpdateConfig passed to ApplicationUpdater
            if (Args.Guid != Guid.Empty) return Args;

            // 2. In the InstallSharpAttribute on the application assembly
            if (!string.IsNullOrWhiteSpace(attribute?.Guid))
            {
                if (!Guid.TryParse(attribute.Guid, out var guid)) throw new FormatException($"Couldn't parse GUID from InstallSharpAttribute: '{attribute.Guid}'");
                Args.Guid = guid;
                return Args;
            }

            // 3. In the GuidAttribute on the application assembly
            var guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            if (!string.IsNullOrWhiteSpace(guidAttribute?.Value))
            {
                if (!Guid.TryParse(guidAttribute.Value, out var guid)) throw new FormatException($"Couldn't parse GUID from assembly attribute: '{guidAttribute.Value}'");
                Args.Guid = guid;
                return Args;
            }

            return Args;
        }
    }
}