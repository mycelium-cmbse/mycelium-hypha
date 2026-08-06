// ------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using Hypha.Knowledge.Layout;

    /// <summary>
    /// The repository this test run is executing inside, or <c>null</c> when it is not inside one.
    /// </summary>
    /// <remarks>
    /// Deliberately thin, and deliberately the same shape as its counterpart in
    /// <c>Hypha.Knowledge.Tests</c>. The path knowledge itself lives once, in
    /// <see cref="KnowledgeLayout"/>; this project used to carry its own copy of it, which is exactly
    /// the duplication #92 exists to remove.
    /// </remarks>
    internal static class Repository
    {
        /// <summary>The layout, or <c>null</c> when the tests run outside a checkout.</summary>
        public static IKnowledgeLayout? Layout { get; } = KnowledgeLayout.Discover();
    }
}
