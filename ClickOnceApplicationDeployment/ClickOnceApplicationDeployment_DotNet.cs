#if !NETFRAMEWORK

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using PureManApplicationDeployment;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RFBApplicationDeployment
{

    public partial class ClickOnceApplicationDeployment
    {

#region Properties
        
        /// <summary>
        /// Access to the underlying provider this library uses to read the ClickOnce manifests in .Net
        /// </summary>
        public PureManClickOnce CurrentDeployment_DotNet {get; private set;}

        /// <inheritdoc cref="PureManClickOnce.IsNetworkDeployment"/>
        private bool IsNetworkDeployedValue => CurrentDeployment_DotNet.IsNetworkDeployment;

        /// <summary>
        /// Library currently does not provide support for this functionality. 
        /// </summary>
        /// <returns>False</returns>
        private bool IsFirstRunValue => false;

        /// <inheritdoc cref="PureManClickOnce.CurrentVersion"/>
        private Version CurrentVersionValue => CurrentDeployment_DotNet.CurrentVersion;

        /// <inheritdoc cref="PureManClickOnce.DataDir"/>
        private string DataDirectoryValue => IsNetworkDeployed ? CurrentDeployment_DotNet.DataDir : AssemblyInfo.GetAssembly().Location;

        /// <summary>
        /// Last time the application had checked for an update - Resets to DateTime.Now when the application first starts up, as no way to store this information currently.
        /// <br/> This value is updated when <see cref="CheckForDetailedUpdateAsync(CancellationToken?)"/> runs successfully.
        /// </summary>
        private DateTime TimeOfLastUpdateCheckValue => CurrentDeployment_DotNet.TimeOfLastUpdateCheckValue;

#endregion

#region Methods 

        /// <inheritdoc cref="PureManClickOnce.UpdateAsync(CancellationToken?)"/>
        private Task<bool> UpdateAsyncMethod(CancellationToken? token) => CurrentDeployment_DotNet.UpdateAsync(token);

        /// <summary>
        /// Try to check the server version, then return the results of the attempt.
        /// </summary>
        /// <returns>New <see cref="UpdateCheckResults"/> object</returns>
        /// <inheritdoc cref="PureManClickOnce.RefreshServerVersion(CancellationToken?)"/>
        private async Task<UpdateCheckResults> CheckForDetailedUpdateAsyncMethod(CancellationToken? token = default)
        {
            Exception e = null;
            Task<Version> t = null;
            Version v = null;
            bool success = false;
            try
            {
                t = CurrentDeployment_DotNet.RefreshServerVersion(token);
                await t;
                success = t.IsCompletedSuccessfully;
                if (success)
                    v = t.Result;
                else
                    e = t.Exception;
            }
            catch (Exception err)
            {
                e = err;
                v = null;
            }
            return new UpdateCheckResults(v, !success, e);
        }

        /// <inheritdoc cref="PureManClickOnce.CheckUpdateAvailableAsync(CancellationToken?)"/>
        private Task<bool> CheckUpdateAvailableAsyncMethod(CancellationToken? token)
        {
            return this.CurrentDeployment_DotNet.CheckUpdateAvailableAsync(token);
        }

    }

#endregion

}


#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif
