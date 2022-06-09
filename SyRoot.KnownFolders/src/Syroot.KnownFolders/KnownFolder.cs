using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Syroot.Windows.IO
{
    /// <summary>
    /// Represents a special Windows directory and provides methods to retrieve information about it.
    /// </summary>
    public sealed class KnownFolder
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="KnownFolder"/> class for the folder of the given type. It
        /// provides the values for the current user.
        /// </summary>
        /// <param name="type">The <see cref="KnownFolderType"/> of the known folder to represent.</param>
        public KnownFolder(KnownFolderType type)
            : this(type, WindowsIdentity.GetCurrent())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnownFolder"/> class for the folder of the given type. It
        /// provides the values for the given impersonated user.
        /// </summary>
        /// <param name="type">The <see cref="KnownFolderType"/> of the known folder to represent.</param>
        /// <param name="identity">The <see cref="WindowsIdentity"/> of the impersonated user whose properties will be
        /// provided.</param>
        public KnownFolder(KnownFolderType type, WindowsIdentity identity)
        {
            Type = type;
            Identity = identity;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the type of the known folder which is represented.
        /// </summary>
        public KnownFolderType Type { get; }

        /// <summary>
        /// Gets the <see cref="WindowsIdentity"/> of the user whose folder properties are provided.
        /// </summary>
        public WindowsIdentity Identity { get; }

        /// <summary>
        /// Gets the default path of the folder. This does not require the folder to be existent.
        /// </summary>
        /// <exception cref="ExternalException">The path could not be retrieved.</exception>
        public string DefaultPath
        {
            get => GetPath(KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY | KNOWN_FOLDER_FLAG.KF_FLAG_DEFAULT_PATH);
        }

        /// <summary>
        /// Gets the user friendly name of the folder as displayed in the File Explorer.
        /// </summary>
        /// <exception cref="ExternalException">The name could not be retrieved.</exception>
        public unsafe string DisplayName
        {
            get
            {
                // Retrieve IShellItem instance.
                Guid guid = Type.GetGuid();
                Guid riid = typeof(IShellItem).GUID;
                void* pv;
                HRESULT hr = PInvoke.SHGetKnownFolderItem(&guid, KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY,
                    Identity.AccessToken, &riid, &pv);

                PWSTR pszName = default;
                try
                {
                    hr.ThrowOnFailure();
                    IShellItem shellItem = (IShellItem)Marshal.GetObjectForIUnknown(new(pv));

                    // Get display name from IShellItem instance.
                    shellItem.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, &pszName);
                    return pszName.ToString();
                }
                catch (Exception ex)
                {
                    throw new ExternalException("Could not get display name. Check inner exception for details.", ex);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(new(pszName.Value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the path as currently configured. This does not require the folder to be existent.
        /// </summary>
        /// <exception cref="ExternalException">The folder could not be retrieved or set.</exception>
        public string Path
        {
            get => GetPath(KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY);
            set => SetPath(KNOWN_FOLDER_FLAG.KF_FLAG_DEFAULT, value);
        }

        /// <summary>
        /// Gets or sets the path as currently configured, with all environment variables expanded.
        /// This does not require the folder to be existent.
        /// </summary>
        /// <exception cref="ExternalException">The path could not be retrieved or set.</exception>
        public string ExpandedPath
        {
            get => GetPath(KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY | KNOWN_FOLDER_FLAG.KF_FLAG_NO_ALIAS);
            set => SetPath(KNOWN_FOLDER_FLAG.KF_FLAG_DONT_UNEXPAND, value);
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the folder using its Desktop.ini settings.
        /// </summary>
        /// <exception cref="ExternalException">The folder could not be created.</exception>
        public void Create()
        {
            GetPath(KNOWN_FOLDER_FLAG.KF_FLAG_INIT | KNOWN_FOLDER_FLAG.KF_FLAG_CREATE);
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private unsafe string GetPath(KNOWN_FOLDER_FLAG flags)
        {
            Guid guid = Type.GetGuid();
            PWSTR pszPath;
            HRESULT hr = PInvoke.SHGetKnownFolderPath(&guid, (uint)flags, Identity.AccessToken, &pszPath);
            try
            {
                hr.ThrowOnFailure();
                return pszPath.ToString();
            }
            catch (Exception ex)
            {
                throw new ExternalException("Could not retrieve path. Check inner exception for details.", ex);
            }
            finally
            {
                Marshal.FreeCoTaskMem(new(pszPath.Value));
            }
        }

        private unsafe void SetPath(KNOWN_FOLDER_FLAG flags, string path)
        {
            Guid guid = Type.GetGuid();
            HRESULT hr = PInvoke.SHSetKnownFolderPath(&guid, (uint)flags, Identity.AccessToken, path);
            try
            {
                hr.ThrowOnFailure();
            }
            catch (Exception ex)
            {
                throw new ExternalException("Cannot set path. Check inner exception for details.", ex);
            }
        }
    }
}
