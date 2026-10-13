// ------------------------------------------------------------------------------------------------
// <copyright file="IApiCrossReferenceBuilder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Api.OpenApi;

    /// <summary>
    /// Lines up the PIM, the specification clauses and the REST/HTTP PSM.
    /// </summary>
    public interface IApiCrossReferenceBuilder
    {
        /// <summary>Builds the cross-references for one API version.</summary>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="pim">The PIM types.</param>
        /// <param name="openApi">The REST/HTTP PSM index; <c>null</c> when it was not generated.</param>
        /// <param name="clauses">The specification clauses; empty when the text was not generated.</param>
        ApiCrossReferenceDocument Build(
            string apiVersion, PimGraph pim, OpenApiIndexDocument? openApi, IReadOnlyList<ApiClause> clauses);
    }
}
