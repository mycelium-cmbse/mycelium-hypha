// ------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using Hypha.Knowledge.Layout;

    /// <summary>
    /// The repository this test run is executing inside, or <c>null</c> when it is not inside one.
    /// </summary>
    /// <remarks>
    /// Deliberately thin. It holds no path knowledge of its own - that is
    /// <see cref="KnowledgeLayout"/>'s job now - and exists only so the generation fixtures share one
    /// discovery instead of each doing their own.
    /// </remarks>
    internal static class Repository
    {
        /// <summary>The layout, or <c>null</c> when the tests run outside a checkout.</summary>
        public static IKnowledgeLayout? Layout { get; } = KnowledgeLayout.Discover();
    }
}
