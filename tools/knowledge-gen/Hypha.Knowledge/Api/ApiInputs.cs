// ------------------------------------------------------------------------------------------------
// <copyright file="ApiInputs.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    /// <summary>
    /// The names the API inputs are stored under, below <c>sources/api/&lt;tag&gt;/</c>.
    /// </summary>
    public static class ApiInputs
    {
        /// <summary>The OpenAPI document, under <c>openapi/</c>.</summary>
        public const string OpenApi = "openapi.json";

        /// <summary>The PIM model, under <c>xmi/</c>.</summary>
        public const string PimModel = "Systems-Modeling-API.xmi";

        /// <summary>The KerML abstract syntax the PIM refers to, beside it under <c>xmi/</c>.</summary>
        public const string KermlXmi = "KerML_only_xmi.uml";

        /// <summary>The specification PDF, under <c>specs/</c>. OMG-copyrighted.</summary>
        public const string Specification = "3-Systems_Modeling_API_and_Services.pdf";
    }
}
