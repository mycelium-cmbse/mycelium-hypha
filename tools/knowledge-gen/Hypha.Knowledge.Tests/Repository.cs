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

    internal static class Repository
    {
        public static IKnowledgeLayout? Layout { get; } = KnowledgeLayout.Discover();
    }
}
