// ------------------------------------------------------------------------------------------------
// <copyright file="LauncherJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System.Text.Json.Serialization;

    /// <summary>The source-generated contract of the launcher's own documents, so that reading them stays NativeAOT-safe.</summary>
    [JsonSerializable(typeof(LaunchManifest))]
    internal sealed partial class LauncherJsonContext : JsonSerializerContext
    {
    }
}
