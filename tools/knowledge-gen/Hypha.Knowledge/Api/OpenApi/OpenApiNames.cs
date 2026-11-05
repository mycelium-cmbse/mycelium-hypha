// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiNames.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>
    /// The fixed words the OpenAPI knowledge uses, so the reader, the renderer and the
    /// cross-references cannot drift apart on a spelling.
    /// </summary>
    public static class OpenApiNames
    {
        /// <summary>A schema of the API itself: its <c>$id</c> is in the SystemsModelingAPI namespace.</summary>
        public const string ApiNamespace = "api";

        /// <summary>A schema that mirrors a KerML / SysML metamodel element.</summary>
        public const string MetamodelNamespace = "metamodel";

        /// <summary>Used in a request body only.</summary>
        public const string RequestRole = "request";

        /// <summary>Used in a response only.</summary>
        public const string ResponseRole = "response";

        /// <summary>Used in both a request body and a response.</summary>
        public const string BothRoles = "request and response";

        /// <summary>Not reachable from any endpoint.</summary>
        public const string NoRole = "unused";

        /// <summary>An object schema.</summary>
        public const string ObjectKind = "object";

        /// <summary>A schema that is one of several alternatives.</summary>
        public const string UnionKind = "union";

        /// <summary>A schema that is one of a fixed set of literals.</summary>
        public const string EnumerationKind = "enumeration";

        /// <summary>What marks a schema's <c>$id</c> as belonging to the API specification.</summary>
        internal const string ApiIdMarker = "/SystemsModelingAPI/";
    }
}
