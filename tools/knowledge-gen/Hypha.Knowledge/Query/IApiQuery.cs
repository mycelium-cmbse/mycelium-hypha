// ------------------------------------------------------------------------------------------------
// <copyright file="IApiQuery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Metamodel;

    /// <summary>
    /// Questions about a Systems Modeling API version, answered from its PIM <c>metamodel.json</c> and
    /// its <c>cross-references.json</c>.
    /// </summary>
    /// <remarks>
    /// The API is a version axis of its own, parallel to the language releases, so its queries take an
    /// API version rather than a release tag, and answer from the default API version without one.
    /// Names match exactly (ordinal). Read-only: it never fetches or generates.
    /// </remarks>
    public interface IApiQuery
    {
        /// <summary>A PIM class with its owned and inherited attributes and its operations.</summary>
        /// <param name="name">The class name, such as <c>Project</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>The class, or <c>null</c> when the PIM has no such class.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<ClassNode> PimClass(string name, string? version = null);

        /// <summary>
        /// A PIM type's cross-references: the specification clauses that cover it, its REST/HTTP schema
        /// and its members.
        /// </summary>
        /// <param name="name">The type name, such as <c>Commit</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>The entry, or <c>null</c> when the cross-references have no such type.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<ApiTypeEntry> Type(string name, string? version = null);

        /// <summary>A PIM service operation's cross-references: its clauses and REST/HTTP endpoints.</summary>
        /// <param name="name">The operation, qualified by its service, such as <c>ProjectService.createProject</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>The entry, or <c>null</c> when the cross-references have no such operation.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<ApiOperationEntry> Operation(string name, string? version = null);
    }
}
