// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for the two documents the metamodel generator writes,
    /// <c>metamodel.json</c> and <c>index.json</c>, so neither writing them nor reading them back
    /// needs reflection.
    /// </summary>
    /// <remarks>
    /// The contract lives in the knowledge library, beside the types, so reading these documents does
    /// not need the generator or uml4net; the generator that writes them resolves the same metadata
    /// through it.
    /// <para>
    /// The camel-case naming policy is part of the file format, so it is declared here rather than
    /// only in the generator's options: a read through <see cref="JsonSerializerContext"/>'s
    /// <c>Default</c> instance binds every member.
    /// </para>
    /// </remarks>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(MetamodelDocument))]
    [JsonSerializable(typeof(MetamodelLookup))]
    public sealed partial class MetamodelJsonContext : JsonSerializerContext
    {
    }
}
