// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Produces one artifact of the knowledge base for one release.
    /// </summary>
    /// <remarks>
    /// Registered as a collection, so "regenerate everything for a tag" is a loop over
    /// <see cref="Order"/> rather than a list of calls each caller has to keep in step. The tests and
    /// the CLI (#81) run the same loop.
    /// </remarks>
    public interface IKnowledgeGenerator
    {
        /// <summary>What this generator produces, e.g. <c>metamodel</c>. Used when reporting progress.</summary>
        string Artifact { get; }

        /// <summary>
        /// Where this generator sits in a full run, lowest first.
        /// </summary>
        /// <remarks>
        /// Ordering is a number rather than a declared dependency graph because there is only one real
        /// constraint: the cross-references read the metamodel index and the generated examples, so
        /// they must run last. Values are spaced so an artifact can be added between two others.
        /// </remarks>
        int Order { get; }

        /// <summary>
        /// Generates the artifact for <paramref name="tag"/>.
        /// </summary>
        /// <returns>
        /// What was written, or why nothing was. Inputs that are simply absent are a
        /// <see cref="GenerationOutcome.Skipped"/> result; inputs that are present but would yield
        /// incorrect output throw.
        /// </returns>
        Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default);
    }
}
