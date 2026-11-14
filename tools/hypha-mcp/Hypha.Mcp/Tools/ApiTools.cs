// ------------------------------------------------------------------------------------------------
// <copyright file="ApiTools.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.ComponentModel;

    using Hypha.Knowledge.Query;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tool that answers questions about a Systems Modeling API version, through <see cref="IApiQuery"/>.
    /// </summary>
    [McpServerToolType]
    public sealed class ApiTools
    {
        /// <summary>Answers the questions.</summary>
        private readonly IApiQuery api;

        /// <summary>Initializes a new instance of the <see cref="ApiTools"/> class.</summary>
        /// <param name="api">Answers the questions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="api"/> is <c>null</c>.</exception>
        public ApiTools(IApiQuery api)
        {
            ArgumentNullException.ThrowIfNull(api);

            this.api = api;
        }

        /// <summary>Answers a question about the Systems Modeling API.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="name">The PIM class, the type, the operation or the schema.</param>
        /// <param name="clause">The clause number, for <c>clause</c>.</param>
        /// <param name="version">The API version; the default API version when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_api", Title = "Systems Modeling API", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Answers a question about the OMG Systems Modeling API and Services: 'pim-class' (a class of its "
            + "platform-independent model), 'type' (an API type's cross-references: its PIM class, schema and "
            + "operations), 'operation' (an API operation's cross-references), 'schema' (a schema of the REST/HTTP "
            + "API as its OpenAPI document states it: each property's name, type, whether it is required or "
            + "nullable and what it refers to, and a union's variants) or 'clause' (a clause of the API "
            + "specification, with its verbatim text).")]
        public string Api(
            [Description("What to answer.")] ApiQuestion question,
            [Description(
                "For 'pim-class': the class, such as Project; for 'type': the type, such as Commit; for 'operation': the "
                + "operation qualified by its service, such as ProjectService.createProject; for 'schema': the schema, "
                + "such as Commit or CommitRequest, which 'type' names for a type.")]
            string? name = null,
            [Description("For 'clause': the clause number, such as 7.1.4.")] string? clause = null,
            [Description("The installed API version to answer from, such as 2026-04. The default API version when omitted.")] string? version = null) =>
            question switch
            {
                ApiQuestion.PimClass => Answer.Of(() => this.api.PimClass(Answer.Required(name, "name", "pim-class"), version)),
                ApiQuestion.Type => Answer.Of(() => this.api.Type(Answer.Required(name, "name", "type"), version)),
                ApiQuestion.Operation => Answer.Of(() => this.api.Operation(Answer.Required(name, "name", "operation"), version)),
                ApiQuestion.Schema => Answer.Of(() => this.api.Schema(Answer.Required(name, "name", "schema"), version)),
                ApiQuestion.Clause => Answer.Of(() => this.api.Clause(Answer.Required(clause, "clause", "clause"), version)),
                _ => throw new McpException($"'{question}' is not a question hypha_api answers."),
            };
    }
}
