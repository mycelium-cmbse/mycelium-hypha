// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Where everything lives: the per-release inputs under <c>sources/</c> and the generated
    /// knowledge under <c>knowledge/</c>.
    /// </summary>
    /// <remarks>
    /// One place that knows the layout, so a generator, a test and the CLI (#81) cannot disagree about
    /// it. Every member is a path, not a promise that the path exists - ask the returned
    /// <see cref="FileSystemInfo"/>.
    /// </remarks>
    public interface IKnowledgeLayout
    {
        /// <summary>The repository root, the folder holding <c>sources/</c> and <c>knowledge/</c>.</summary>
        DirectoryInfo Root { get; }

        /// <summary>The installed release tags, newest first. Empty when there is no manifest.</summary>
        IReadOnlyList<string> InstalledTags { get; }

        /// <summary>The tag answered from by default, or <c>null</c> when there is no manifest.</summary>
        string? DefaultTag { get; }

        /// <summary><c>knowledge/versions.json</c>.</summary>
        FileInfo VersionManifest { get; }

        /// <summary>
        /// <c>sources/PrimitiveTypes.xmi</c> - the OMG UML primitives library, shared by every release
        /// rather than fetched per tag.
        /// </summary>
        FileInfo SharedPrimitiveTypes { get; }

        /// <summary><c>knowledge/cross-references.schema.json</c>, which is not per release.</summary>
        FileInfo CrossReferenceSchema { get; }

        /// <summary><c>sources/&lt;tag&gt;/xmi</c> - the metamodel XMI.</summary>
        DirectoryInfo Xmi(string tag);

        /// <summary><c>sources/&lt;tag&gt;/textual</c> - the grammar and the models.</summary>
        DirectoryInfo TextualSources(string tag);

        /// <summary><c>sources/&lt;tag&gt;/textual/bnf</c>.</summary>
        DirectoryInfo Bnf(string tag);

        /// <summary>One of that release's grammars, e.g. <c>KerML-textual-bnf.kebnf</c>.</summary>
        FileInfo Grammar(string tag, string fileName);

        /// <summary><c>sources/&lt;tag&gt;/specs</c> - OMG-copyrighted, git-ignored.</summary>
        DirectoryInfo Specifications(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;</c>.</summary>
        DirectoryInfo Knowledge(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/metamodel</c>.</summary>
        DirectoryInfo Metamodel(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/metamodel/index.json</c>.</summary>
        FileInfo MetamodelIndex(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/textual-notation</c>.</summary>
        DirectoryInfo TextualNotation(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/textual-notation/examples</c>.</summary>
        DirectoryInfo Examples(string tag);

        /// <summary>
        /// <c>knowledge/&lt;tag&gt;/spec/&lt;document&gt;/index.json</c> - git-ignored, so it is
        /// routinely absent.
        /// </summary>
        FileInfo SpecificationCatalog(string tag, string document);

        /// <summary><c>knowledge/&lt;tag&gt;/cross-references.json</c>.</summary>
        FileInfo CrossReferences(string tag);
    }
}
