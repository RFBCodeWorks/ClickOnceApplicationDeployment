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

        /// <summary>
        /// Gets the Web site or file share from which this application updates itself.
        /// </summary>
        /// <returns>
        /// The update path, expressed as an HTTP, HTTPS, or file URL; or as a Windows network file path (UNC) <para/>
        /// For .Net applications, this is the publishPath passed into the constructor.
        /// </returns>
        /// <para/>Value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/>
        /// </summary>
        /// <inheritdoc cref="FileVersionInfo.ProductName"/>
        public virtual string ProductName => AssemblyVersionInfo.ProductName;

        /// <summary>
        /// <inheritdoc cref="FileVersionInfo.CompanyName" path="/summary"/>
        /// <para/>Value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/>
        /// </summary>
        /// <inheritdoc cref="FileVersionInfo.CompanyName"/>
        public virtual string CompanyName => AssemblyVersionInfo.CompanyName;

        /// <summary>
        /// <inheritdoc cref="FileVersionInfo.Comments" path="/summary"/>
        /// <para/>Value retrieved via: <see cref="FileVersionInfo.GetVersionInfo"/>
        /// </summary>
        /// <inheritdoc cref="FileVersionInfo.Comments"/>
        public virtual string Comments => AssemblyVersionInfo.Comments;

        /// <summary>
        /// Gets a value indicating whether this instance is network deployment.
        /// </summary>
        /// <value><c>true</c> if this instance is network deployment; otherwise, <c>false</c>.</value>
        public bool IsNetworkDeployed => IsNetworkDeployedValue;

        /// <summary>
        /// Gets a value indicating whether this is the first time this applicaiton has run on the client computer.
        /// </summary>
        /// <returns>
        /// true if this version of the application has never run on the client computer before; otherwise false;
        /// </returns>
        /// <remarks>
        /// - .Net3.1 and .Net5 currently do not provide a way to test for this, so this will always return false.
        /// <br/> - .NetFramework will report this value properly via System.Deployment.dll
        /// </remarks>
        public virtual bool IsFirstRun => IsFirstRunValue; //ToDo: Try to find a way to get this working on .Net5

        /// <summary>
        /// Gets the path to the ClickOnce data directory
        /// </summary>
        /// <returns>
        /// A string containing the path to the application's data directory on the local disk.
        /// </returns>
        public virtual string DataDirectory => DataDirectoryValue;

        /// <summary>
        /// Last time the application has checked for an update. 
        /// </summary>
        /// <remarks>
        /// - Resets to DateTime.Now when the application first starts up, as no way to store this information currently.
        /// <br/> - This value is updated when <see cref="CheckForDetailedUpdateAsync(CancellationToken?)"/> runs successfully.
        /// </remarks>
        public virtual DateTime TimeOfLastUpdateCheck => TimeOfLastUpdateCheckValue;

        /// <summary>
        /// Gets the <see cref="Version"/> of the current running instance of the application
        /// </summary>
        public virtual Version CurrentVersion => CurrentVersionValue;

        #endregion

        #region < Public Methods >

        /// <summary>
        /// Asynchronously check if an update is available. 
        /// </summary>
        /// <returns>
        /// <see cref="Task"/>&lt;<see cref="UpdateCheckResults"/>&gt;
        /// which contains the results update check, as well as if any errors that occurred when attempting to reach the update server.
        /// </returns>
        public virtual Task<UpdateCheckResults> CheckForDetailedUpdateAsync(CancellationToken? token = default) => CheckForDetailedUpdateAsyncMethod(token);


        /// <summary>
        /// Starts an asynchronous download and isntallation of the latest version of this application.
        /// </summary>
        /// <returns>TRUE if the update was completed, otherwise false.</returns>
        /// <param name="token"><see cref="CancellationToken"/> used to cancel the update process</param>
        /// <exception cref="OperationCanceledException"/> - Occurs if Token.IsCancellationRequested
        public virtual Task<bool> UpdateAsync(CancellationToken? token = default) => UpdateAsyncMethod(token);


        /// <summary>
        /// Checks the <see cref="UpdateLocation"/> to determine whether a new update is available.
        /// </summary>
        /// <returns>
        /// <see cref = "Task" />&lt;<see cref="bool"/>&gt;whose value is TRUE if an update is available, otherwise false.
        /// </returns>
        /// <exception cref="OperationCanceledException"/>
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