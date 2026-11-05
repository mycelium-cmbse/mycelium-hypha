// ------------------------------------------------------------------------------------------------
// <copyright file="IApiLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Where the Systems Modeling API inputs and knowledge live: <c>sources/api/&lt;tag&gt;/</c> and
    /// <c>knowledge/api/&lt;tag&gt;/</c>, beside the language releases rather than inside one.
    /// </summary>
    /// <remarks>
    /// The API is versioned on its own axis, so it has its own manifest and its own default. As with
    /// <see cref="Layout.IKnowledgeLayout"/>, what generation <b>reads</b> resolves against the
    /// repository root and what it <b>writes</b> against the output root.
    /// </remarks>
    public interface IApiLayout
    {
        /// <summary>The installed API versions, newest first. Empty when there is no manifest.</summary>
        IReadOnlyList<string> InstalledTags { get; }

        /// <summary>The API version answered from by default, or <c>null</c> when there is no manifest.</summary>
        string? DefaultTag { get; }

        /// <summary><c>knowledge/api/versions.json</c>.</summary>
        FileInfo VersionManifest { get; }

        /// <summary><c>sources/PrimitiveTypes.xmi</c>, shared with the language releases.</summary>
        FileInfo SharedPrimitiveTypes { get; }

        /// <summary><c>sources/api/&lt;tag&gt;</c> - everything fetched for one API version.</summary>
        DirectoryInfo Sources(string tag);

        /// <summary><c>sources/api/&lt;tag&gt;/openapi/openapi.json</c>.</summary>
        FileInfo OpenApi(string tag);

        /// <summary><c>sources/api/&lt;tag&gt;/xmi</c> - the PIM model and the KerML model it refers to.</summary>
        DirectoryInfo Xmi(string tag);

        /// <summary><c>sources/api/&lt;tag&gt;/xmi/Systems-Modeling-API.xmi</c>.</summary>
        FileInfo PimModel(string tag);

        /// <summary><c>sources/api/&lt;tag&gt;/specs/</c> - OMG-copyrighted.</summary>
        DirectoryInfo Specifications(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;</c>.</summary>
        DirectoryInfo Knowledge(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/pim</c>.</summary>
        DirectoryInfo Pim(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/pim/metamodel.json</c>.</summary>
        FileInfo PimGraph(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/openapi</c>.</summary>
        DirectoryInfo OpenApiKnowledge(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/openapi/index.json</c>.</summary>
        FileInfo OpenApiIndex(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/spec</c> - verbatim OMG text.</summary>
        DirectoryInfo SpecificationText(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/spec/index.json</c>.</summary>
        FileInfo SpecificationCatalog(string tag);

        /// <summary><c>knowledge/api/&lt;tag&gt;/cross-references.json</c>.</summary>
        FileInfo CrossReferences(string tag);
    }
}
