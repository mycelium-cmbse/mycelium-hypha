// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInstallation.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// What one release install fetched, and what it recorded.
    /// </summary>
    /// <param name="Tag">The release that was installed.</param>
    /// <param name="Metamodel">The metamodel XMI files written under <c>sources/&lt;tag&gt;/xmi/</c>.</param>
    /// <param name="Textual">
    /// The grammar and model files written under <c>sources/&lt;tag&gt;/textual/</c>.
    /// </param>
    /// <param name="Specifications">
    /// The specification PDFs written under the git-ignored <c>sources/&lt;tag&gt;/specs/</c>; empty
    /// unless they were asked for.
    /// </param>
    /// <param name="Version">The commits the tag resolved to in both upstreams.</param>
    /// <param name="Manifest">The manifest that now records the release.</param>
    public sealed record ReleaseInstallation(
        string Tag,
        IReadOnlyList<FileInfo> Metamodel,
        IReadOnlyList<FileInfo> Textual,
        IReadOnlyList<FileInfo> Specifications,
        InstalledVersion Version,
        FileInfo Manifest)
    {
        /// <summary>The number of files fetched, across every kind of input.</summary>
        public int FileCount => this.Metamodel.Count + this.Textual.Count + this.Specifications.Count;
    }
}
