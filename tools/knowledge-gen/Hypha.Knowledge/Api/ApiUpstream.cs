// ------------------------------------------------------------------------------------------------
// <copyright file="ApiUpstream.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Where the Systems Modeling API and Services inputs live.
    /// </summary>
    /// <remarks>
    /// An API version is one <b>tag of the API-Services repository</b> that is also a language release,
    /// so its specification PDF and the KerML abstract syntax the PIM refers to can be read at the same
    /// tag. The PIM model itself is the exception: no Systems-Modeling repository carries it, so it is
    /// read from the OMG document server, where it is the same file for every API version.
    /// </remarks>
    public static class ApiUpstream
    {
        /// <summary>The REST/HTTP PSM: the OpenAPI document.</summary>
        public const string ServicesRepository = "Systems-Modeling/SysML-v2-API-Services";

        /// <summary>The OpenAPI document, in the API-Services repository.</summary>
        public const string OpenApiPath = "public/docs/openapi.json";

        /// <summary>The API specification PDF, in the Release repository. OMG-copyrighted.</summary>
        public const string SpecificationPath = "doc/3-Systems_Modeling_API_and_Services.pdf";

        /// <summary>The KerML abstract syntax the PIM refers to, in the Pilot-Implementation repository.</summary>
        public const string KermlXmiPath = "org.omg.sysml/model/KerML_only_xmi.uml";

        /// <summary>The OMG document identifier of the PIM model.</summary>
        public const string PimDocument = "ptc/25-02-29";

        /// <summary>
        /// Where the PIM model is published. Overridable so a test can serve it from a stub.
        /// </summary>
        [SuppressMessage(
            "Major Code Smell",
            "S1075:URIs should not be hardcoded",
            Justification = "The published location of an OMG document; overridable through the options.")]
        public static Uri DefaultPimAddress { get; } = new(
            "https://www.omg.org/spec/SystemsModelingAPI/20250201/Systems-Modeling-API.xmi", UriKind.Absolute);
    }
}
