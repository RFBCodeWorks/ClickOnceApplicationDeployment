using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace RFBApplicationDeployment
{
    internal static class AssemblyInfo
    {
        #region < Assembly >

        /// <inheritdoc cref="Assembly.GetEntryAssembly()" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static Assembly GetAssembly() => Assembly.GetEntryAssembly();

        /// <inheritdoc cref="Assembly.Load(AssemblyName)" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static Assembly GetAssembly(string filePath) => Assembly.LoadFile(filePath);

        #endregion

        #region < AssemblyName >

        /// <summary>Gets an <see cref="AssemblyName"/> for the currently executing Assembly.</summary>
        /// <inheritdoc cref="Assembly.GetName()" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static AssemblyName GetAssemblyName() => GetAssembly().GetName();


        /// <summary>Gets the <see cref="Version"/> for the Assembly at the specified path.</summary>
        /// <inheritdoc cref="Assembly.GetName()" />
        /// <inheritdoc cref="Assembly.Load(AssemblyName)" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static AssemblyName GetAssemblyName(string filePath) => GetAssembly(filePath).GetName();

        #endregion

        #region < Version >

        /// <summary>Gets the <see cref="Version"/> for the currently executing Assembly.</summary>
        /// <inheritdoc cref="AssemblyName.Version" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static Version GetAssemblyVersion() => GetAssemblyName().Version;

        /// <summary>Gets the <see cref="Version"/> for the currently executing Assembly.</summary>
        /// <inheritdoc cref="GetAssemblyName(string)" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static Version GetAssemblyVersion(string filePath) => GetAssemblyName(filePath).Version;

        #endregion

        #region < FileVersionInfo >

        /// <summary>Gets the <see cref="Version"/> for the currently executing Assembly.</summary>
        /// <inheritdoc cref="FileVersionInfo.GetVersionInfo" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static FileVersionInfo GetVersionInfo() => FileVersionInfo.GetVersionInfo(GetAssembly().Location);

        /// <summary>Gets the <see cref="Version"/> for the currently File at the specified path</summary>
        /// <inheritdoc cref="FileVersionInfo.GetVersionInfo" />
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static FileVersionInfo GetVersionInfo(string filePath) => FileVersionInfo.GetVersionInfo(filePath);

        #endregion

    }
}
