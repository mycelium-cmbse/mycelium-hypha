// ------------------------------------------------------------------------------------------------
// <copyright file="HyphaHome.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System;
    using System.IO;

    /// <summary>
    /// The one per-user folder every hypha client shares: its <c>sources/</c> and <c>knowledge/</c>, the
    /// release-operation lock and the managed toolchain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is the <see cref="EnvironmentVariable"/> folder when that is set, and otherwise
    /// <c>hypha</c> under the per-user local application data folder: <c>%LOCALAPPDATA%</c> on
    /// Windows, <c>$XDG_DATA_HOME</c> or <c>~/.local/share</c> on Linux, and
    /// <c>~/Library/Application Support</c> on macOS.
    /// </para>
    /// <para>
    /// A maintainer working in a source checkout keeps using the checkout instead. The order is:
    /// an explicit root; the CLI's checkout above the working directory; this folder when it is set
    /// explicitly (<see cref="EnvironmentVariable"/> or <see cref="HyphaKnowledgeOptions.Home"/>);
    /// the checkout above the running assembly; and this folder's default location.
    /// </para>
    /// </remarks>
    public static class HyphaHome
    {
        /// <summary>The environment variable that moves the folder.</summary>
        public const string EnvironmentVariable = "HYPHA_HOME";

        /// <summary>The folder's name under the per-user local application data folder.</summary>
        public const string FolderName = "hypha";

        /// <summary>The managed toolchain's folder (uv, its package cache and its Python) in the home folder.</summary>
        public const string ToolchainFolder = "toolchain";

        /// <summary>
        /// Resolves the home folder.
        /// </summary>
        /// <param name="getEnvironmentVariable">
        /// Reads an environment variable; <c>null</c> uses <see cref="Environment.GetEnvironmentVariable(string)"/>.
        /// A test passes its own so it never depends on the machine it runs on.
        /// </param>
        /// <param name="getFolderPath">
        /// Resolves a special folder to its path; <c>null</c> uses
        /// <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/>.
        /// </param>
        /// <returns>
        /// The <see cref="EnvironmentVariable"/> folder as a full path when it is set and not blank;
        /// otherwise <see cref="FolderName"/> under the per-user local application data folder. It may
        /// not exist yet.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="EnvironmentVariable"/> is not set and the system has no per-user local application
        /// data folder.
        /// </exception>
        public static DirectoryInfo Resolve(
            Func<string, string?>? getEnvironmentVariable = null,
            Func<Environment.SpecialFolder, string>? getFolderPath = null)
        {
            getFolderPath ??= Environment.GetFolderPath;

            if (Configured(getEnvironmentVariable) is { } configured)
            {
                return configured;
            }

            var data = getFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(data))
            {
                throw new InvalidOperationException(
                    $"this system has no per-user application data folder; set {EnvironmentVariable} to the "
                    + "folder hypha should keep its knowledge in");
            }

            return new DirectoryInfo(Path.Combine(data, FolderName));
        }

        /// <summary>
        /// The home folder when <see cref="EnvironmentVariable"/> names it, so that a folder the user
        /// chose can win over a checkout found only because of where the binary sits.
        /// </summary>
        /// <param name="getEnvironmentVariable">
        /// Reads an environment variable; <c>null</c> uses <see cref="Environment.GetEnvironmentVariable(string)"/>.
        /// </param>
        /// <returns>
        /// The <see cref="EnvironmentVariable"/> folder as a full path, or <c>null</c> when it is not set or blank.
        /// </returns>
        public static DirectoryInfo? Configured(Func<string, string?>? getEnvironmentVariable = null)
        {
            getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

            var configured = getEnvironmentVariable(EnvironmentVariable);

            return string.IsNullOrWhiteSpace(configured)
                ? null
                : new DirectoryInfo(Path.GetFullPath(configured.Trim()));
        }

        /// <summary>The managed toolchain's folder in <paramref name="home"/>.</summary>
        /// <param name="home">The home folder.</param>
        /// <returns><c>toolchain/</c> under <paramref name="home"/>; it may not exist yet.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="home"/> is <c>null</c>.</exception>
        public static DirectoryInfo Toolchain(DirectoryInfo home)
        {
            ArgumentNullException.ThrowIfNull(home);

            return new DirectoryInfo(Path.Combine(home.FullName, ToolchainFolder));
        }
    }
}
