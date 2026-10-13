// ------------------------------------------------------------------------------------------------
// <copyright file="IOpenApiReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>
    /// Reads the REST/HTTP PSM out of an OpenAPI document.
    /// </summary>
    public interface IOpenApiReader
    {
        /// <summary>Reads the schemas and endpoints of <paramref name="json"/>.</summary>
        /// <exception cref="System.InvalidOperationException">
        /// The document is not an OpenAPI document hypha can read: no <c>paths</c> or no
        /// <c>components/schemas</c>.
        /// </exception>
        OpenApiModel Read(string json);
    }
}
