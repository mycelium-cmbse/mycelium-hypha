// ------------------------------------------------------------------------------------------------
// <copyright file="QueryJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// The source-generated contract for the answers of <see cref="IKnowledgeQuery"/> and
    /// <see cref="IApiQuery"/>: every <see cref="QueryResult{T}"/> they return, with its grounding, and
    /// the <see cref="ReleaseInventory"/>. The <c>hypha query --json</c> output and, later, the MCP tools'
    /// results are written through it, so both give the same document.
    /// </summary>
    /// <remarks>Names are camel case, as in the generated knowledge the answers are read from.</remarks>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ReleaseInventory))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<string>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<FeatureInfo>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<Redefinition>>))]
    [JsonSerializable(typeof(QueryResult<Specialization>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<DeclaredMember<OperationNode>>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<EnumerationLiteralNode>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<Subsetting>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<TypedFeature>>))]
    [JsonSerializable(typeof(QueryResult<Neighbourhood>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<PackageContents>>))]
    [JsonSerializable(typeof(QueryResult<OppositeEnd>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<Contribution>>))]
    [JsonSerializable(typeof(QueryResult<ReleaseDiff>))]
    [JsonSerializable(typeof(QueryResult<LookupEntry>))]
    [JsonSerializable(typeof(QueryResult<CrossReferenceEntry>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<FeatureAssignment>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<ProductionUse>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<KeywordUse>>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<ClauseUse>>))]
    [JsonSerializable(typeof(QueryResult<ModelLibraryDeclarationEntry>))]
    [JsonSerializable(typeof(QueryResult<SpecificationClause>))]
    [JsonSerializable(typeof(QueryResult<IReadOnlyList<SearchHit>>))]
    [JsonSerializable(typeof(QueryResult<ResolvedAddress>))]
    [JsonSerializable(typeof(QueryResult<ClassNode>))]
    [JsonSerializable(typeof(QueryResult<ApiTypeEntry>))]
    [JsonSerializable(typeof(QueryResult<ApiOperationEntry>))]
    public sealed partial class QueryJsonContext : JsonSerializerContext
    {
        /// <summary>
        /// Gets a context for output a person or a model reads: the same contract, with operators such as
        /// <c>:&gt;&gt;</c> and <c>=&gt;</c> written as they are rather than escaped.
        /// </summary>
        public static QueryJsonContext Readable { get; } = new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }
}
