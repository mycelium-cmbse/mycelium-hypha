// ------------------------------------------------------------------------------------------------
// <copyright file="McpJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Hypha.Knowledge.Lookup;

    /// <summary>
    /// The source-generated contract of the tool answers <see cref="Hypha.Knowledge.Query.QueryJsonContext"/>
    /// does not hold: a lookup page, and what a release change did.
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(LookupResult))]
    [JsonSerializable(typeof(InstallAnswer))]
    [JsonSerializable(typeof(UseAnswer))]
    [JsonSerializable(typeof(RemoveAnswer))]
    internal sealed partial class McpJsonContext : JsonSerializerContext
    {
        /// <summary>
        /// Gets the context with the same options as <see cref="Hypha.Knowledge.Query.QueryJsonContext.Readable"/>:
        /// camel case, and text left unescaped, since the reader is a model, never a browser.
        /// </summary>
        public static McpJsonContext Readable { get; } = new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }
}
