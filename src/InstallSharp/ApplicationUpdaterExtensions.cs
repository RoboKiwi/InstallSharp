using System.Threading.Tasks;

namespace InstallSharp
{
    public static class ApplicationUpdaterExtensions
    {
        public static UpdateInfo CheckForUpdate(this ApplicationUpdater updater, UpdateCheckArgs args = null)
        {
            return updater.CheckForUpdateAsync(args).GetAwaiter().GetResult();
        }

        public static bool Update(this ApplicationUpdater updater, UpdateCheckArgs args = null)
        {
            return updater.UpdateAsync(args).GetAwaiter().GetResult();
        }

        public static async Task<bool> EnsureUpdatedAsync(this ApplicationUpdater updater, UpdateCheckArgs args = null, bool register = true)
        {
            
            
            // Ensure the application is registered if necessary
            if (register) await updater.InstallAsync().ConfigureAwait(false);

            // Check for 
            if (await updater.UpdateAsync(args).ConfigureAwait(false)) return true;
            return await updater.UpdateAsync(args).ConfigureAwait(false);
        }

        public static bool EnsureUpdated(this ApplicationUpdater updater, UpdateCheckArgs args = null, bool register = true)
        {
            return EnsureUpdatedAsync(updater, args, register).GetAwaiter().GetResult();
        }
    }
}