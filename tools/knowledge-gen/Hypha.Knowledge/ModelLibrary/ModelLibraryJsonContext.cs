// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for <c>knowledge/&lt;tag&gt;/model-library/index.json</c>, so
    /// writing and reading it needs no reflection-based serialization.
    /// </summary>
    [JsonSerializable(typeof(ModelLibraryDocument))]
    internal sealed partial class ModelLibraryJsonContext : JsonSerializerContext
    {
    }
}
