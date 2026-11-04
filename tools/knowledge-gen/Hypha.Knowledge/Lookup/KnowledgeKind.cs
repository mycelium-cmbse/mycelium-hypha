// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeKind.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    using System.Text.Json.Serialization;

    /// <summary>Which generated index a lookup resolves a name against.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<KnowledgeKind>))]
    public enum KnowledgeKind
    {
        /// <summary><c>knowledge/&lt;tag&gt;/metamodel/index.json</c>: metaclasses, enumerations and primitives.</summary>
        [JsonStringEnumMemberName("metamodel")]
        Metamodel,

        /// <summary><c>knowledge/&lt;tag&gt;/model-library/index.json</c>: standard-library declarations.</summary>
        [JsonStringEnumMemberName("model-library")]
        ModelLibrary,
    }
}
