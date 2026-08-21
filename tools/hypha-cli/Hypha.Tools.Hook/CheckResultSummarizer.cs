// ------------------------------------------------------------------------------------------------
// <copyright file="CheckResultSummarizer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Turns <c>hypha check</c>'s local-vs-online comparison into one line of
    /// <c>additionalContext</c>, or nothing.
    /// </summary>
    /// <remarks>
    /// This is also where the plugin used to run a second, Python <c>SessionStart</c> hook
    /// (<c>hooks/check-spec-pdfs.py</c>) purely to say "the OMG PDFs are missing for the default
    /// release" - folded in here so the plugin needs neither a second hook nor Python present just to
    /// report that. Silent by default, matching that hook's tone: only phases the user can act on, or
    /// would otherwise mistake for nothing happening, get a line.
    /// </remarks>
    public static class CheckResultSummarizer
    {
        /// <summary>
        /// The OMG specification PDFs, and the upstream path each is published at per release tag.
        /// </summary>
        private static readonly (string FileName, string PathFormat)[] SpecificationPdfs =
        [
            ("1-Kernel_Modeling_Language.pdf",
                "https://github.com/Systems-Modeling/SysML-v2-Release/blob/{0}/doc/1-Kernel_Modeling_Language.pdf"),
            ("2a-OMG_Systems_Modeling_Language.pdf",
                "https://github.com/Systems-Modeling/SysML-v2-Release/blob/{0}/doc/2a-OMG_Systems_Modeling_Language.pdf"),
            ("3-Systems_Modeling_API_and_Services.pdf",
                "https://github.com/Systems-Modeling/SysML-v2-Release/blob/{0}/doc/3-Systems_Modeling_API_and_Services.pdf"),
        ];

        /// <summary>
        /// Summarizes <paramref name="result"/>, or returns <c>null</c> when there is nothing worth
        /// saying.
        /// </summary>
        /// <param name="result">What <c>hypha check --json</c> reported, or <c>null</c> if it failed.</param>
        /// <param name="pluginRoot">Where to look for already-downloaded specification PDFs.</param>
        public static string? Summarize(HookCheckResult? result, DirectoryInfo pluginRoot)
        {
            if (result is null)
            {
                return null;
            }

            var installed = result.InstalledTags ?? [];
            var online = result.AvailableOnline ?? [];
            var newest = online.Count > 0 ? online[0] : null;

            if (installed.Count == 0)
            {
                return newest is null
                    ? null
                    : "Hypha: no SysML v2/KerML knowledge is installed locally yet. Available "
                      + $"releases online (newest first): {string.Join(", ", online)}. Ask which "
                      + "release to fetch, then run 'hypha fetch --tag <tag>' followed by "
                      + "'hypha generate --tag <tag>'.";
            }

            if (newest is not null && !installed.Contains(newest, StringComparer.Ordinal))
            {
                return $"Hypha: installed locally: {string.Join(", ", installed)} "
                    + $"(default: {result.DefaultTag}). A newer release is available: {newest}. Ask "
                    + $"whether to fetch it too - 'hypha fetch --tag {newest}' followed by "
                    + $"'hypha generate --tag {newest}'.";
            }

            return MissingSpecificationsMessage(result.DefaultTag, pluginRoot);
        }

        private static string? MissingSpecificationsMessage(string? defaultTag, DirectoryInfo pluginRoot)
        {
            if (defaultTag is null)
            {
                return null;
            }

            var specsDirectory = Path.Combine(pluginRoot.FullName, "sources", defaultTag, "specs");

            var missing = SpecificationPdfs
                .Where(pdf => !File.Exists(Path.Combine(specsDirectory, pdf.FileName)))
                .ToList();

            if (missing.Count == 0)
            {
                return null;
            }

            var names = string.Join(", ", missing.Select(pdf => pdf.FileName));
            var urls = string.Join(
                "\n", missing.Select(pdf => "  - " + string.Format(pdf.PathFormat, defaultTag)));

            return "Hypha: the OMG specification PDFs for release " + defaultTag + " are not present "
                + "locally, so spec-citation cannot quote normative text - metamodel-lookup and "
                + "sysml-validation still work fully, and spec-citation can still name the governing "
                + $"clause from the generated cross-references.\n\nMissing: {names}.\n\nDownload into "
                + $"sources/{defaultTag}/specs/:\n{urls}\n\nQuoting the text itself needs Python and a "
                + "maintainer source checkout (tools/spec-extract) - not available from an installed "
                + "plugin.";
        }
    }
}
