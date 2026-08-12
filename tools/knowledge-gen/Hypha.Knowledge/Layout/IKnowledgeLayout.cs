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
    /// One place that knows the layout, so a generator, a test and the CLI cannot disagree about it.
    /// Every member is a path, not a promise that the path exists - ask the returned
    /// <see cref="FileSystemInfo"/>.
    /// <para>
    /// Members are split by direction: what generation <b>reads</b> resolves against <see cref="Root"/>,
    /// what it <b>writes</b> against <see cref="OutputRoot"/>. They are the same folder in a normal run;
    /// separating them is what lets the CLI write elsewhere and the golden tests regenerate into a
    /// scratch folder instead of over the committed knowledge base.
    /// </para>
    /// </remarks>
    public interface IKnowledgeLayout
    {
        /// <summary>The repository root, the folder holding <c>sources/</c> and <c>knowledge/</c>.</summary>
        DirectoryInfo Root { get; }

        /// <summary>
        /// Where generated artifacts are written. The same as <see cref="Root"/> unless a caller
        /// redirected it.
        /// </summary>
        DirectoryInfo OutputRoot { get; }

        /// <summary>The installed release tags, newest first. Empty when there is no manifest.</summary>
        IReadOnlyList<string> InstalledTags { get; }

        /// <summary>The tag answered from by default, or <c>null</c> when there is no manifest.</summary>
        string? DefaultTag { get; }

        /// <summary><c>knowledge/versions.json</c>.</summary>
        /// <remarks>
        /// Resolved against <see cref="Root"/>: it records which releases have been fetched into
        /// <c>sources/</c>, so it belongs with them rather than with whatever a run happens to
        /// generate. Redirecting the output therefore does not produce a manifest alongside it.
        /// </remarks>
        FileInfo VersionManifest { get; }

        /// <summary>
        /// <c>sources/PrimitiveTypes.xmi</c> - the OMG UML primitives library, shared by every release
        /// rather than fetched per tag.
        /// </summary>
        FileInfo SharedPrimitiveTypes { get; }

        /// <summary><c>knowledge/cross-references.schema.json</c>, which is not per release.</summary>
        FileInfo CrossReferenceSchema { get; }

        /// <summary><c>knowledge/model-library.schema.json</c>, which is not per release.</summary>
        FileInfo ModelLibrarySchema { get; }

        /// <summary><c>sources/</c> - the folder every release's inputs are fetched into.</summary>
        DirectoryInfo Sources { get; }

        /// <summary><c>sources/&lt;tag&gt;</c> - everything fetched for one release.</summary>
        DirectoryInfo ReleaseSources(string tag);

        /// <summary><c>sources/&lt;tag&gt;/xmi</c> - the metamodel XMI.</summary>
        DirectoryInfo Xmi(string tag);

        /// <summary><c>sources/&lt;tag&gt;/textual</c> - the grammar and the models.</summary>
        DirectoryInfo TextualSources(string tag);

        /// <summary><c>sources/&lt;tag&gt;/textual/bnf</c>.</summary>
        DirectoryInfo Bnf(string tag);

        /// <summary>
        /// <c>sources/&lt;tag&gt;/textual/sysml.library</c> - the normative standard libraries.
        /// </summary>
        DirectoryInfo ModelLibrarySources(string tag);

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

        /// <summary><c>knowledge/&lt;tag&gt;/model-library</c>.</summary>
        DirectoryInfo ModelLibrary(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/model-library/packages</c>.</summary>
        DirectoryInfo ModelLibraryPackages(string tag);

        /// <summary><c>knowledge/&lt;tag&gt;/model-library/index.json</c>.</summary>
        FileInfo ModelLibraryIndex(string tag);

        /// <summary>
        /// <c>knowledge/&lt;tag&gt;/spec/&lt;document&gt;/index.json</c> - git-ignored, so it is
        /// routinely absent.
        /// </summary>
        /// <remarks>
        /// Resolved against <see cref="Root"/> even though it sits under <c>knowledge/</c>: the spec
        /// catalog is written by the Python PDF chain and is an <b>input</b> to the cross-references,
        /// not something this toolchain generates.
        /// </remarks>
        FileInfo SpecificationCatalog(string tag, string document);

        /// <summary><c>knowledge/&lt;tag&gt;/cross-references.json</c> - where a run writes it.</summary>
        FileInfo CrossReferences(string tag);

        /// <summary>
        /// <c>knowledge/&lt;tag&gt;/cross-references.json</c> as a previous run committed it.
        /// </summary>
        /// <remarks>
        /// The same file as <see cref="CrossReferences"/> in a normal run, and a different one as soon
        /// as the output is redirected: this is the carry-forward source the cross-references read when
        /// the specifications are absent, so it has to keep pointing at the committed document rather
        /// than following the output to an empty folder.
        /// </remarks>
        FileInfo CommittedCrossReferences(string tag);
    }
}
