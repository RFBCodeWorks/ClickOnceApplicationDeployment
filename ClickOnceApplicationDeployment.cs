using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

#if NET472_OR_GREATER

//using System.Deployment;
using System.Deployment.Application;

#else

using PureManApplicationDeployment;

#endif

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RFBApplicationDeployment
{

    /// <summary>
    /// Returns information about the current Executing Assembly
    /// <br/> DLL meant to bridge the ClickOnce gap from .NetFramework to .Net5 to simplify code base for multi-target callers
    /// <br/> .NetFramework compatibility provided by: System.Deployment.dll
    /// <br/> .Net compatibility provided by: <see href="https://github.com/derskythe/WpfSettings"/>
    /// </summary>
    public class ClickOnceApplicationDeployment
    {
        /// <summary>
        /// Static object that contains provides information about the to the entry assembly.
        /// </summary>
        public static ClickOnceApplicationDeployment EntryApplication { get; } = new ClickOnceApplicationDeployment();

        #region < Constructors & private fields >

        /// <summary>
        /// Get information about the current application.
        /// </summary>
        /// <remarks>
        /// FilePath provided by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.Location
        /// </remarks>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public ClickOnceApplicationDeployment()
        {
            FilePath = AssemblyInfo.GetAssembly().Location;
            Refresh();
        }

        /// <summary>
        /// Get information about the application at the specified <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Path to the executable</param>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public ClickOnceApplicationDeployment(string filePath)
        {
            FilePath = filePath;
            Refresh();
        }

        
        /// <summary>
        /// Refresh the Data About this application
        /// </summary>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public void Refresh()
        {
            Assembly = AssemblyInfo.GetAssembly(FilePath);
            AssemblyVersionInfo = AssemblyInfo.GetVersionInfo(FilePath);
#if NET472_OR_GREATER
#else
            PureMan = new PureManClickOnce(FilePath);
#endif
        }

        private Assembly Assembly;

        private FileVersionInfo AssemblyVersionInfo;
        
        #endregion

        #region < Public Properties & Method >

        /// <summary>
        /// Path to the exectuable / manifest.
        /// </summary>
        public string FilePath { get; }

        /// <inheritdoc cref="FileVersionInfo.ProductName"/>
        public virtual string ProductName => AssemblyVersionInfo.ProductName;

        /// <inheritdoc cref="FileVersionInfo.CompanyName"/>
        public virtual string CompanyName => AssemblyVersionInfo.CompanyName;

        /// <inheritdoc cref="FileVersionInfo.Comments"/>
        public virtual string Comments => AssemblyVersionInfo.Comments;

        /// <summary>
        /// Gets a value indicating whether this instance is network deployment.
        /// </summary>
        /// <value><c>true</c> if this instance is network deployment; otherwise, <c>false</c>.</value>
        public bool IsNetworkDeployed => IsNetworkDeployedValue;

        /// <inheritdoc cref="IsFirstRunValue"/>
        public virtual bool IsFirstRun => IsFirstRunValue;

        /// <inheritdoc cref="CurrentVersionValue"/>
        public virtual Version CurrentVersion => CurrentVersionValue;

        /// <inheritdoc cref="GetUpdatedVersionInfoMethod"/>
        public virtual Version GetUpdatedVersionInfo(CancellationToken? token) => GetUpdatedVersionInfoMethod(token);

        /// <inheritdoc cref="DataDirectoryValue"/>
        public virtual string DataDirectory => DataDirectoryValue;

        /// <inheritdoc cref="TimeOfLastUpdateCheckValue"/>
        public virtual DateTime TimeOfLastUpdateCheck => TimeOfLastUpdateCheckValue;

        /// <summary>
        /// Asynchronously check if an update is available. 
        /// </summary>
        /// <returns>new Task{bool}/returns>
        public virtual Task<bool> IsUpdateAvailable(CancellationToken token) => IsUpdateAvailableMethod(token);

        /// <inheritdoc cref="UpdateAsyncMethod"/>
        public virtual Task<bool> UpdateAsync(CancellationToken token) => UpdateAsyncMethod(token);

        #endregion


#if NET472_OR_GREATER

        /// <inheritdoc cref="ApplicationDeployment.IsNetworkDeployed"/>
        private bool IsNetworkDeployedValue => ApplicationDeployment.IsNetworkDeployed;

        private ApplicationDeployment CurrentDeployment => ApplicationDeployment.CurrentDeployment;

        /// <inheritdoc cref="ApplicationDeployment.IsFirstRun"/>
        private bool IsFirstRunValue => IsNetworkDeployed ? CurrentDeployment.IsFirstRun : false;

        /// <inheritdoc cref="ApplicationDeployment.CurrentVersion"/>
        private Version CurrentVersionValue => IsNetworkDeployed ? CurrentDeployment.CurrentVersion : AssemblyInfo.GetAssemblyVersion();

        /// <param name="token">This has no effect when using .NetFramework</param>
        /// <inheritdoc cref="ApplicationDeployment.UpdatedVersion"/>
        private Version GetUpdatedVersionInfoMethod(CancellationToken? token) => IsNetworkDeployed ? CurrentDeployment.UpdatedVersion : CurrentVersion;

        /// <inheritdoc cref="ApplicationDeployment.UpdatedApplicationFullName"/>
        private string UpdatedApplicationFullNameValue => IsNetworkDeployed ? CurrentDeployment.UpdatedApplicationFullName : AssemblyInfo.GetAssemblyName().Name;

        /// <inheritdoc cref="ApplicationDeployment.DataDirectory"/>
        private string DataDirectoryValue => IsNetworkDeployed ? CurrentDeployment.DataDirectory : AssemblyInfo.GetAssembly().Location;

        /// <inheritdoc cref="ApplicationDeployment.TimeOfLastUpdateCheck"/>
        private DateTime TimeOfLastUpdateCheckValue => IsNetworkDeployed ? CurrentDeployment.TimeOfLastUpdateCheck : DateTime.Now;

        /// <param name="token"><see cref="CancellationToken"/> used to cancel the checking for update process</param>
        /// <inheritdoc cref="ApplicationDeployment.CheckForUpdate()"/>
        private async Task<bool> IsUpdateAvailableMethod(CancellationToken? token = default)
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
                bool Result = await Extensions.RunCancellableTask(CurrentDeployment.CheckForUpdateAsync, CurrentDeployment.CheckForUpdateAsyncCancel, tc, token ?? CancellationToken.None);
                CurrentDeployment.CheckForUpdateCompleted -= handler;
                tc.Task.Dispose();
                return Result;
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
        }

#else

        private PureManClickOnce PureMan;

        /// <inheritdoc cref="PureManClickOnce.IsNetworkDeployment"/>
        private bool IsNetworkDeployedValue => PureMan.IsNetworkDeployment;

        /// <summary>
        /// Library currently does not provide support for this functionality. 
        /// </summary>
        /// <returns>False</returns>
        private bool IsFirstRunValue => false;

        /// <inheritdoc cref="PureManClickOnce.CurrentVersion"/>
        private Version CurrentVersionValue => IsNetworkDeployed ? PureMan.CurrentVersion().Result : AssemblyInfo.GetAssemblyVersion();

        /// <summary>
        /// Check the network for the latest version -- Requires network access!
        /// </summary>
        /// <inheritdoc cref="PureManClickOnce.ServerVersion(CancellationToken?)"/>
        private Version GetUpdatedVersionInfoMethod(CancellationToken? token) => IsNetworkDeployed ? PureMan.ServerVersion(token).Result : CurrentVersion;

        /// <inheritdoc cref="PureManClickOnce.DataDir"/>
        private string DataDirectoryValue => IsNetworkDeployed ? PureMan.DataDir : AssemblyInfo.GetAssembly().Location;

        private DateTime lastUpdate = DateTime.Now;

        /// <summary>
        /// Last time the application had checked for an update - Resets to DateTime.Now when the application first starts up, as no way to store this information currently.
        /// </summary>
        private DateTime TimeOfLastUpdateCheckValue => IsNetworkDeployed ? lastUpdate : DateTime.Now;

        /// <inheritdoc cref="PureManClickOnce.UpdateAvailable(CancellationToken?)"/>
        private async Task<bool> IsUpdateAvailableMethod(CancellationToken token)
        {
            if (!IsNetworkDeployed) return false;
            try
            {
                bool available = await PureMan.UpdateAvailable(token);
                lastUpdate = DateTime.Now;
                return available;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="PureManClickOnce.Update(CancellationToken?)"/>
        private Task<bool> UpdateAsyncMethod(CancellationToken? token) => PureMan.Update(token);


#endif
    }

}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member