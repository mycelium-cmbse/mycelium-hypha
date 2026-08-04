// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInputs.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Which upstream files make up one release, and which are deliberately left out.
    /// </summary>
    /// <remarks>
    /// Selection is a pure function of a repository listing so it can be tested without the network.
    /// </remarks>
    public static class ReleaseInputs
    {
        /// <summary>Upstream path to the name it is stored under in <c>sources/&lt;tag&gt;/xmi/</c>.</summary>
        public static IReadOnlyDictionary<string, string> XmiFiles { get; } =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["org.omg.sysml/model/KerML_only_xmi.uml"] = "KerML_only_xmi.uml",
                ["org.omg.sysml/model/SysML_only_xmi.uml"] = "SysML_only_xmi.uml",
            });

        /// <summary>
        /// The OMG UML primitives library. Shared by every release rather than fetched per tag: it is
        /// published by neither upstream and is identical for all of them.
        /// </summary>
        public const string SharedXmi = "PrimitiveTypes.xmi";

        /// <summary>The specification PDFs. OMG-copyrighted - written only to a git-ignored folder.</summary>
        public static IReadOnlyList<string> SpecificationPdfs { get; } =
        [
            "doc/1-Kernel_Modeling_Language.pdf",
            "doc/2a-OMG_Systems_Modeling_Language.pdf",
            "doc/3-Systems_Modeling_API_and_Services.pdf",
        ];

        private static readonly string[] GrammarSuffixes = [".kebnf", ".kgbnf"];
        private static readonly string[] ModelRoots = ["kerml/", "sysml/"];
        private static readonly string[] ModelSuffixes = [".kerml", ".sysml"];

        /// <summary>
        /// The grammar and model files hypha needs, from a full Release-repository listing.
        /// </summary>
        /// <remarks>
        /// Grammar first, then the models, each ordered - so the result is stable regardless of the
        /// order the listing arrives in. The grammar's HTML rendering, CSS and SVGs are excluded, and
        /// so is <c>sysml.library/</c>: ingesting the standard libraries is a separate decision (#80),
        /// and a loose prefix match would quietly pull in 24 MB of it.
        /// </remarks>
        public static IReadOnlyList<string> SelectTextual(IEnumerable<string> treePaths)
        {
            ArgumentNullException.ThrowIfNull(treePaths);

            var paths = treePaths as IReadOnlyCollection<string> ?? treePaths.ToList();

            var grammar = paths
                .Where(path => path.StartsWith("bnf/", StringComparison.Ordinal))
                .Where(path => GrammarSuffixes.Any(suffix => path.EndsWith(suffix, StringComparison.Ordinal)))
                .OrderBy(path => path, StringComparer.Ordinal);

            var models = paths
                .Where(path => ModelRoots.Any(root => path.StartsWith(root, StringComparison.Ordinal)))
                .Where(path => ModelSuffixes.Any(suffix => path.EndsWith(suffix, StringComparison.Ordinal)))
                .OrderBy(path => path, StringComparer.Ordinal);

            return grammar.Concat(models).ToList();
        }
    }
}
