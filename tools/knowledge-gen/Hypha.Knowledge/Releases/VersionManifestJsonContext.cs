// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Releases
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for <c>knowledge/versions.json</c>, so writing and reading it
    /// needs no reflection-based serialization.
    /// </summary>
    [JsonSerializable(typeof(VersionManifest))]
    internal sealed partial class VersionManifestJsonContext : JsonSerializerContext
    {
    }
}
