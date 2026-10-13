// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands.Api
{
    using System.CommandLine;

    /// <summary>
    /// <c>hypha api</c> - the Systems Modeling API and Services knowledge, which is versioned apart
    /// from the language releases and so has its own set of verbs.
    /// </summary>
    /// <remarks>
    /// The verbs mirror the top-level ones (<c>discover</c>, <c>fetch</c>, <c>generate</c>,
    /// <c>list</c>, <c>use</c>, <c>remove</c>) and act on <c>sources/api/</c> and
    /// <c>knowledge/api/</c>. Nothing here touches an installed language release.
    /// </remarks>
    public sealed class ApiCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCommand"/> class.
        /// </summary>
        public ApiCommand()
            : base("api", "Fetch and generate the Systems Modeling API & Services knowledge")
        {
        }
    }
}
