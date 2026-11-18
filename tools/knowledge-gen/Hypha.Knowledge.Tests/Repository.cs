// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Tests
{
    using Hypha.Knowledge.Layout;

    internal static class Repository
    {
        public static IKnowledgeLayout? Layout { get; } = KnowledgeLayout.Discover();
    }
}
