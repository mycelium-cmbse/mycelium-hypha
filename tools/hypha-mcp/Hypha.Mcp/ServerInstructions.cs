// ------------------------------------------------------------------------------------------------
// <copyright file="ServerInstructions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp
{
    /// <summary>
    /// The <c>instructions</c> the server sends a client when it connects.
    /// </summary>
    /// <remarks>
    /// Several clients (Claude Desktop, Zed, Windsurf) load no skills, so this text is all the guidance
    /// they get: what the knowledge covers, how far each answer can be trusted, and which tool answers
    /// which question. It names every tool, and a test checks that it names exactly the registered ones.
    /// </remarks>
    public static class ServerInstructions
    {
        /// <summary>The instructions.</summary>
        public const string Text =
            """
            hypha answers questions about the OMG SysML v2 and KerML standards (not UML) and the Systems
            Modeling API, from knowledge generated on this machine from the official OMG sources: the
            metamodel, the textual-notation grammar, the standard model library, the specification text
            and the cross-references between them. Answer from these tools rather than from memory, and
            say which release answered.

            Grounding. Every result carries a grounding with a tier, the release and the source file:
            - NORMATIVE: verbatim specification text. Quote it, with the clause number.
            - MODEL: read from the metamodel. State it as what the metamodel says.
            - DERIVED: computed from the metamodel or matched across documents (closures, cross-references).
              Present it as derived, and check it against NORMATIVE text when the answer depends on it.
            A result with "unavailable" set means that part of the knowledge was never generated; say so,
            and do not fill the gap from memory. A tool error names what is missing, such as a release that
            is not installed, and how to fix it.

            Which tool:
            - hypha_release_status: the installed releases and API versions, the defaults, and what each has
              generated. Call it first when unsure what is installed.
            - hypha_check_releases: whether a newer release or API version is offered upstream; it asks
              GitHub. When a newer release or API version is, tell the user and offer hypha_install_release
              (with axis 'api' for an API version).
            - hypha_metaclass: one metaclass: its index entry, supertypes, subclasses, whether it specializes
              another, neighbours, constraints, operations, enumeration literals, and features.
            - hypha_feature: one feature: what redefines or subsets it, its opposite end, the constraints
              that name it, and every feature that contributes to it.
            - hypha_typed_by: the features typed by a type, the features that can hold a value of it, and the
              composite features that can own an element.
            - hypha_package: a metamodel package's metaclasses and nested packages.
            - hypha_syntax: the textual notation: an element's cross-references (clauses, productions,
              examples), which syntax fills in a feature, what a production builds, and where a keyword
              or operator is used.
            - hypha_spec: the verbatim text of a KerML or SysML clause, or the elements a clause treats.
            - hypha_search: find metaclasses, library declarations, clauses and worked examples by name,
              qualified name, clause number or title; each hit has a hypha:// address.
            - hypha_search_spec: find the specification clauses whose text says a word or phrase. Read a
              clause with hypha_spec before quoting it; an excerpt is not a quotation.
            - hypha_resolve: the generated page a hypha:// address names.
            - hypha_lookup: an element page, a model-library declaration or a worked example model the
              release ships, a page at a time.
            - hypha_diff: what changed between two installed releases.
            - hypha_api: the Systems Modeling API: a PIM class, an API type or operation, a REST schema's
              properties, or a clause of the API specification.

            Changing releases. Only when the user asks for it, since an install downloads from GitHub and
            takes minutes:
            - hypha_install_release: fetch a release and generate its knowledge, the newest one when no tag
              is given. It reports progress while it runs, and says which artifacts were generated.
            - hypha_use_release: make an installed release the default.
            - hypha_remove_release: delete an installed release that is not the default.
            Each takes axis 'api' to do the same for a Systems Modeling API version, which hypha_api answers from.
            Another client may be changing releases at the same time; these tools wait their turn.

            Every hypha:// address is also a resource, hypha://{tag}/{kind}/{name}, whose contents are the
            page as Markdown, for clients that read resources.

            Every tool answers from the default release unless given a tag (for hypha_api, a version).
            Names are metamodel names, such as PartUsage; qualified names use ::, such as ISQBase::mass.
            """;
    }
}
