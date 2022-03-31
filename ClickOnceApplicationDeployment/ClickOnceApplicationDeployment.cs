using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

#if NETFRAMEWORK

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
    public partial class ClickOnceApplicationDeployment
    {
        /// <summary>
        /// Static object that contains provides information about the entry assembly.
        /// </summary>
        /// <remarks>
        /// For .Net targets, the Publish-Path must be set using <see cref="SetupEntryApplication(string)"/>
        /// </remarks>
        public static ClickOnceApplicationDeployment EntryApplication { get; private set; } = new ClickOnceApplicationDeployment();

        /// <summary>
        /// Update the <see cref="EntryApplication"/> object with to point at the supplied <paramref name="entryApp"/> instead of using the parameterless constructor.
        /// </summary>
        /// <param name="entryApp"></param>
        public static void SetupEntryApplication(ClickOnceApplicationDeployment entryApp)
        {
            if (!(entryApp is null)) EntryApplication = entryApp;
        }

        /// <summary>
        /// Update the <see cref="EntryApplication"/> object with to point at the supplied <paramref name="publishPath"/> instead of using the parameterless constructor.
        /// </summary>
        /// <inheritdoc cref="ClickOnceApplicationDeployment(string)" path="*"/>
        public static void SetupEntryApplication(string publishPath)
        {
            if (!string.IsNullOrWhiteSpace(publishPath))
            {
                EntryApplication = new ClickOnceApplicationDeployment(publishPath);
            }
        }

        #region < Constructors & private fields >

        /// <summary>
        /// Get information about the current application. <br/>
        /// This will work fine for .NetFramework, while .Net applications should use <see cref="ClickOnceApplicationDeployment(string)"/>
        /// </summary>
        /// <remarks>
        /// FilePath provided by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.Location
        /// </remarks>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public ClickOnceApplicationDeployment()
        {
            ExecutablePath = AssemblyInfo.GetAssembly().Location;
            Refresh();
        }

        /// <summary>
        /// Get information about the application at the specified <paramref name="publishPath"/>
        /// </summary>
        /// <param name="publishPath">
        /// This is the URL the application will search for updates at.
        /// <br/> This is a required value for .NetCore and .Net5+ due to this library being unable to retrieve this value reliably.
        /// <para/> For .NetFramework, this has no effect due to usage of System.Deployment.dll, but should still be specified for use in multi-target projects.
        /// </param>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public ClickOnceApplicationDeployment(string publishPath)
        {
#if !NETFRAMEWORK
            CurrentDeployment_DotNet = new PureManClickOnce(publishPath);
#endif
            ExecutablePath = AssemblyInfo.GetAssembly().Location;
            Refresh();
        }

        /// <summary>
        /// Refresh the Data About this application
        /// </summary>
        /// <inheritdoc cref="Assembly.LoadFile(string)" path="/exception"/>
        public void Refresh()
        {
            AssemblyVersionInfo = AssemblyInfo.GetVersionInfo(ExecutablePath);
#if NETFRAMEWORK
            ThisAssemblyName = AssemblyInfo.GetAssemblyName();
#endif

        }

        private FileVersionInfo AssemblyVersionInfo;
        
#endregion

#region < Public Properties >

        /// <summary>
        /// Path to the exectuable / manifest.
        /// </summary>
        public string ExecutablePath { get; }

        /// <remarks>value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/> </remarks>
        /// <inheritdoc cref="FileVersionInfo.ProductName"/>
        public virtual string ProductName => AssemblyVersionInfo.ProductName;

        /// <remarks>value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/> </remarks>
        /// <inheritdoc cref="FileVersionInfo.CompanyName"/>
        public virtual string CompanyName => AssemblyVersionInfo.CompanyName;

        /// <remarks>value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/> </remarks>
        /// <inheritdoc cref="FileVersionInfo.Comments"/>
        public virtual string Comments => AssemblyVersionInfo.Comments;

        /// <summary>
        /// Gets a value indicating whether this instance is network deployment.
        /// </summary>
        /// <value><c>true</c> if this instance is network deployment; otherwise, <c>false</c>.</value>
        public bool IsNetworkDeployed => IsNetworkDeployedValue;

        /// <inheritdoc cref="IsFirstRunValue"/>
        public virtual bool IsFirstRun => IsFirstRunValue;

        /// <inheritdoc cref="DataDirectoryValue"/>
        public virtual string DataDirectory => DataDirectoryValue;

        /// <inheritdoc cref="TimeOfLastUpdateCheckValue"/>
        public virtual DateTime TimeOfLastUpdateCheck => TimeOfLastUpdateCheckValue;

        /// <inheritdoc cref="CurrentVersionValue"/>
        public virtual Version CurrentVersion => CurrentVersionValue;

        #endregion

        #region < Public Methods >

        /// <summary>
        /// Asynchronously check if an update is available. 
        /// </summary>
        /// <returns><see cref="Task"/>&lt;<see cref="UpdateCheckResults"/>&gt;</returns>
        public virtual Task<UpdateCheckResults> CheckForDetailedUpdateAsync(CancellationToken? token = default) => CheckForDetailedUpdateAsyncMethod(token);

        /// <inheritdoc cref="UpdateAsyncMethod"/>
        public virtual Task<bool> UpdateAsync(CancellationToken? token = default) => UpdateAsyncMethod(token);


        /// <returns>
        /// <see cref = "Task" />&lt;<see cref="bool"/>&gt; <br/>
        /// returns FALSE when <see cref="IsNetworkDeployed"/> == false.
        /// </returns>
        /// <inheritdoc cref="CheckUpdateAvailableAsyncMethod"/>
        public virtual Task<bool> CheckUpdateAvailableAsync(CancellationToken? token = default)
        {
            if (!IsNetworkDeployed)
            {
                return Task.FromResult(false);
            }
            else return CheckUpdateAvailableAsyncMethod(token);
        }
#endregion


    }

}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member