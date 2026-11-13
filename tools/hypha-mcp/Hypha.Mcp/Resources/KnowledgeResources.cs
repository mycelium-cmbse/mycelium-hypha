// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeResources.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Resources
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The knowledge base as MCP resources: every <c>hypha://{tag}/{kind}/{name}</c> address is a page a
    /// client can read, as Markdown.
    /// </summary>
    /// <remarks>
    /// One resource template, not a list: a release has thousands of pages, which no client wants listed.
    /// The addresses come from <c>hypha_search</c> and <c>hypha_search_spec</c>, and everything here can
    /// also be read through a tool, since not every client reads resources.
    /// </remarks>
    [McpServerResourceType]
    public sealed class KnowledgeResources
    {
        /// <summary>The template every page's address fills in.</summary>
        public const string UriTemplate = "hypha://{tag}/{kind}/{name}";

        /// <summary>Resolves an address to its page.</summary>
        private readonly IKnowledgeQuery query;

        /// <summary>Finds a model-library declaration within its file.</summary>
        private readonly IKnowledgeLookup lookup;

        /// <summary>Where the generated pages are.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Reads a page.</summary>
        private readonly IDocumentCache documents;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeResources"/> class.</summary>
        /// <param name="query">Resolves an address to its page.</param>
        /// <param name="lookup">Finds a model-library declaration within its file.</param>
        /// <param name="layout">Where the generated pages are.</param>
        /// <param name="documents">Reads a page.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public KnowledgeResources(IKnowledgeQuery query, IKnowledgeLookup lookup, IKnowledgeLayout layout, IDocumentCache documents)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(lookup);
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(documents);

            this.query = query;
            this.lookup = lookup;
            this.layout = layout;
            this.documents = documents;
        }

        /// <summary>Reads the page an address names.</summary>
        /// <param name="tag">The release.</param>
        /// <param name="kind">What the page is: one of <see cref="HyphaAddress.Kinds"/>.</param>
        /// <param name="name">The metaclass, the declaration's qualified name or the clause number.</param>
        /// <returns>The page as Markdown.</returns>
        /// <exception cref="McpException">
        /// The address is not one, names nothing in the release, or its page was never generated; the message
        /// says which.
        /// </exception>
        [McpServerResource(UriTemplate = UriTemplate, Name = "hypha_page", Title = "hypha knowledge page", MimeType = "text/markdown")]
        [Description(
            "A page of the SysML v2 / KerML knowledge base: a metaclass (metamodel), a standard-library declaration "
            + "(model-library, by qualified name), or a KerML or SysML specification clause (kerml, sysml2, by number).")]
        public ResourceContents Page(
            [Description("The release, such as 2026-05.")] string tag,
            [Description("What the page is.")][AllowedValues(HyphaAddress.Metamodel, HyphaAddress.ModelLibrary, HyphaAddress.Kerml, HyphaAddress.Sysml)] string kind,
            [Description("The metaclass, the declaration's qualified name, or the clause number.")] string name) =>
            Answer.Ask(() =>
            {
                var address = HyphaAddress.Parse($"{HyphaAddress.Scheme}://{tag}/{kind}/{name}");

                return new TextResourceContents
                {
                    Uri = address.Uri,
                    MimeType = "text/markdown",
                    Text = address.Kind == HyphaAddress.ModelLibrary ? this.Declaration(address) : this.Whole(address),
                };
            });

        /// <summary>A metaclass or clause page, whole.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The page's text.</returns>
        /// <exception cref="McpException">The address names nothing, or its page was never generated.</exception>
        private string Whole(HyphaAddress address)
        {
            var resolved = this.query.Resolve(address);

            if (resolved.Unavailable is { } unavailable)
            {
                throw new McpException(unavailable);
            }

            var page = resolved.Value
                ?? throw new McpException($"Release {address.Tag} has no {address.Kind} page '{address.Name}'. Find the address with hypha_search.");

            return this.documents.ReadText(new FileInfo(Path.Combine(this.layout.OutputRoot.FullName, page.Page)));
        }

        /// <summary>
        /// A model-library declaration, as the lookup pages it: from the declaration on, rather than the whole
        /// library file, which can run to thousands of lines.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The declaration's text.</returns>
        /// <exception cref="McpException">The release declares no such name, or several.</exception>
        private string Declaration(HyphaAddress address)
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, address.Name, address.Tag, null, LookupRequest.DefaultMaxLines));

            return result.Status == LookupStatus.Found && result.Content is { } content
                ? content
                : throw new McpException(
                    $"Release {address.Tag} has no single model-library declaration '{address.Name}'"
                    + (result.Matches.Count > 0 ? $"; did you mean {string.Join(", ", result.Matches)}?" : ". Find it with hypha_search."));
        }
    }
}
