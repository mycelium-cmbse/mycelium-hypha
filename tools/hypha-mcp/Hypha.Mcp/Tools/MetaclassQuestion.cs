// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassQuestion.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// What <c>hypha_metaclass</c> answers about a metaclass. Each value is the name of the
    /// <c>hypha query</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MetaclassQuestion>))]
    public enum MetaclassQuestion
    {
        /// <summary>Its entry in the metamodel index: kind, package, supertypes and page.</summary>
        [JsonStringEnumMemberName("element")]
        Element,

        /// <summary>Every supertype.</summary>
        [JsonStringEnumMemberName("ancestors")]
        Ancestors,

        /// <summary>The metaclasses that specialize it, directly or not.</summary>
        [JsonStringEnumMemberName("subclasses")]
        Subclasses,

        /// <summary>Whether it specializes another metaclass, and through which supertypes.</summary>
        [JsonStringEnumMemberName("specializes")]
        Specializes,

        /// <summary>The metaclasses its features are typed by, and the features typed by it.</summary>
        [JsonStringEnumMemberName("neighbours")]
        Neighbours,

        /// <summary>The OCL constraints that apply to it, each naming where it is declared.</summary>
        [JsonStringEnumMemberName("constraints")]
        Constraints,

        /// <summary>The operations that apply to it, each naming where it is declared.</summary>
        [JsonStringEnumMemberName("operations")]
        Operations,

        /// <summary>The literals of an enumeration.</summary>
        [JsonStringEnumMemberName("literals")]
        Literals,

        /// <summary>Its owned and inherited features, each naming where it comes from.</summary>
        [JsonStringEnumMemberName("features")]
        Features,

        /// <summary>Its features without the inherited ones it redefines.</summary>
        [JsonStringEnumMemberName("effective-features")]
        EffectiveFeatures,
    }
}
