// ------------------------------------------------------------------------------------------------
// <copyright file="IApiKnowledgeGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using Hypha.Knowledge.Generation;

    /// <summary>
    /// Produces one artifact of the API knowledge base for one <b>API version</b>.
    /// </summary>
    /// <remarks>
    /// The same contract as <see cref="IKnowledgeGenerator"/>, but a separate collection: the tag it
    /// is given is an API version, not a language release, so a run over the language releases must
    /// not pick these up. They are registered under this type only.
    /// </remarks>
    public interface IApiKnowledgeGenerator : IKnowledgeGenerator
    {
    }
}
