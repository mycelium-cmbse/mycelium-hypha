// ------------------------------------------------------------------------------------------------
// <copyright file="HomeFolder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System;
    using System.IO;

    /// <summary>
    /// Resolves <c>HYPHA_HOME</c> exactly as <c>Hypha.Knowledge.Layout.HyphaHome</c> does, which the
    /// launcher cannot reference without giving up NativeAOT.
    /// </summary>
    /// <remarks>
    /// The <c>HYPHA_HOME</c> environment variable when it is set, which must then be an absolute path;
    /// otherwise <c>hypha</c> under the per-user local application data folder. A test checks that both
    /// resolve the same folder.
    /// </remarks>
    internal static class HomeFolder
    {
        /// <summary>The environment variable that moves the folder.</summary>
        public const string EnvironmentVariable = "HYPHA_HOME";

        /// <summary>Resolves the folder.</summary>
        /// <param name="getEnvironmentVariable">Reads an environment variable.</param>
        /// <param name="getFolderPath">Resolves a special folder to its path.</param>
        /// <returns>The folder, which may not exist yet.</returns>
        /// <exception cref="InvalidOperationException">
        /// <c>HYPHA_HOME</c> is relative or not a valid path, or it is not set and the system has no per-user
        /// local application data folder.
        /// </exception>
        public static DirectoryInfo Resolve(Func<string, string?> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath)
        {
            ArgumentNullException.ThrowIfNull(getEnvironmentVariable);
            ArgumentNullException.ThrowIfNull(getFolderPath);

            var configured = getEnvironmentVariable(EnvironmentVariable)?.Trim();

            if (!string.IsNullOrEmpty(configured))
            {
                if (!Path.IsPathFullyQualified(configured))
                {
                    throw new InvalidOperationException($"{EnvironmentVariable} must be an absolute path, but is '{configured}'");
                }

                try
                {
                    return new DirectoryInfo(Path.GetFullPath(configured));
                }
                catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
                {
                    throw new InvalidOperationException($"{EnvironmentVariable} is not a valid folder: {exception.Message}", exception);
                }
            }

            var data = getFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(data))
            {
                throw new InvalidOperationException(
                    $"this system has no per-user application data folder; set {EnvironmentVariable} to the folder hypha should keep its knowledge in");
            }

            return new DirectoryInfo(Path.Combine(data, "hypha"));
        }
    }
}
