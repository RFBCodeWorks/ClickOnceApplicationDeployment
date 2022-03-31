#if NETFRAMEWORK

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
//using System.Deployment;
using System.Deployment.Application;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RFBApplicationDeployment
{
    public partial class ClickOnceApplicationDeployment
    {

        /// <summary>
        /// Access to the underlying provider this library uses to read the ClickOnce manifests in .NetFramework
        /// </summary>
        public ApplicationDeployment CurrentDeployment => ApplicationDeployment.CurrentDeployment;

        private AssemblyName ThisAssemblyName;

        #region Properties 

        /// <inheritdoc cref="ApplicationDeployment.IsNetworkDeployed"/>
        private bool IsNetworkDeployedValue => ApplicationDeployment.IsNetworkDeployed;

        /// <inheritdoc cref="ApplicationDeployment.IsFirstRun"/>
        private bool IsFirstRunValue => IsNetworkDeployed && CurrentDeployment.IsFirstRun;

        /// <inheritdoc cref="ApplicationDeployment.TimeOfLastUpdateCheck"/>
        private DateTime TimeOfLastUpdateCheckValue => IsNetworkDeployed ? CurrentDeployment.TimeOfLastUpdateCheck : DateTime.Now;

        /// <inheritdoc cref="ApplicationDeployment.CurrentVersion"/>
        private Version CurrentVersionValue => IsNetworkDeployed ? CurrentDeployment.CurrentVersion : ThisAssemblyName.Version;

        /// <inheritdoc cref="ApplicationDeployment.DataDirectory"/>
        private string DataDirectoryValue => IsNetworkDeployed ? CurrentDeployment.DataDirectory : AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        #endregion

        #region Methods         

        /// <param name="token"><see cref="CancellationToken"/> used to cancel the checking for update process</param>
        /// <returns>New <see cref="UpdateCheckResults"/> object</returns>
        /// <inheritdoc cref="ApplicationDeployment.CheckForUpdate()"  path="/summary"/>
        private async Task<UpdateCheckResults> CheckForDetailedUpdateAsyncMethod(CancellationToken? token = default)
        {
            if (!IsNetworkDeployed)
            {
                return new UpdateCheckResults(null,false, new Exception("Application is not network deployed!"));
            }
            else
            {
                TaskCompletionSource<CheckForUpdateCompletedEventArgs> tc = new TaskCompletionSource<CheckForUpdateCompletedEventArgs>();
                void handler(object _, CheckForUpdateCompletedEventArgs e) { tc.TrySetResult(e); }
                CurrentDeployment.CheckForUpdateCompleted += (CheckForUpdateCompletedEventHandler)handler;
                bool Result = await Extensions.RunCancellableTask(CurrentDeployment.CheckForUpdateAsync, CurrentDeployment.CheckForUpdateAsyncCancel, tc, token ?? CancellationToken.None);
                CurrentDeployment.CheckForUpdateCompleted -= handler;
                var args = tc.Task.Result;
                tc.Task.Dispose();
                bool cancelled = args?.Cancelled ?? true || args.Error != null;
                return new UpdateCheckResults(args?.AvailableVersion , cancelled, args?.Error);
            }
        }

        /// <returns>TRUE if the update was completed, otherwise false.</returns>
        /// <param name="token"><see cref="CancellationToken"/> used to cancel the update process</param>
        /// <inheritdoc cref="ApplicationDeployment.UpdateAsync"/>
        /// <exception cref="OperationCanceledException"/> - Occurs if Token.IsCancellationRequested
        private async Task<bool> UpdateAsyncMethod(CancellationToken? token = default)
        {
            if (!IsNetworkDeployed)
            {
                return false;
            }
            else
            {
                TaskCompletionSource<bool> tc = new TaskCompletionSource<bool>();
                void handler(object _, CheckForUpdateCompletedEventArgs e) { tc.TrySetResult(e.UpdateAvailable); }
                CurrentDeployment.CheckForUpdateCompleted += (CheckForUpdateCompletedEventHandler)handler;
                bool Result = await Extensions.RunCancellableTask(CurrentDeployment.UpdateAsync, CurrentDeployment.UpdateAsyncCancel, tc, token ?? CancellationToken.None);
                CurrentDeployment.CheckForUpdateCompleted -= handler;
                tc.Task.Dispose();
                return Result;
            }

            #endregion
        }

        /// <inheritdoc cref="ApplicationDeployment.CheckForUpdate()"/>
        private async Task<bool> CheckUpdateAvailableAsyncMethod(CancellationToken? token)
        {
            var res = await CheckForDetailedUpdateAsyncMethod(token);
            if (!res.Cancelled)
                return res.AvailableVersion > this.CurrentVersion;
            else
                return false;
        }
    }

}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#endif
