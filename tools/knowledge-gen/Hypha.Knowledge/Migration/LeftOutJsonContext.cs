// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Migration
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for <see cref="LeftOutFile.FileName"/>, so writing and reading it
    /// needs no reflection-based serialization.
    /// </summary>
    [JsonSerializable(typeof(LeftOutRecord))]
    internal sealed partial class LeftOutJsonContext : JsonSerializerContext
    {
    }
}
