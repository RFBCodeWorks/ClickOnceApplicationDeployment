using System;
using System.Collections.Generic;
using System.Text;

namespace RFBApplicationDeployment
{
    /// <summary>
    /// Represents the results of an UpdateCheck that was performed by <see cref="ClickOnceApplicationDeployment.CheckForDetailedUpdateAsync(System.Threading.CancellationToken?)"/>
    /// </summary>
    public class UpdateCheckResults
    {
        internal UpdateCheckResults(Version version, bool cancelled, Exception exception)
        {
            AvailableVersion = version;
            Cancelled = cancelled;
            Error = exception;
        }

        /// <summary>
        /// Available version on the server
        /// </summary>
        /// <returns>
        /// Version Object if it was able to be read successfully, othernull null.
        /// </returns>
        public Version AvailableVersion { get; }

        /// <summary>
        /// Value indicating if the request to read the server version was cancelled or faulted.
        /// </summary>
        public bool Cancelled { get; }

        /// <summary>
        /// Exception thrown when attempting to check the server version 
        /// </summary>
        public Exception Error { get; }
    }
}
