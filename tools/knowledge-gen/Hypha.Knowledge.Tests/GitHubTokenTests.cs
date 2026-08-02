// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubTokenTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for resolving the optional GitHub token, against an injected environment so the real
    /// one is never read or mutated.
    /// </summary>
    [TestFixture]
    public class GitHubTokenTests
    {
        [Test]
        public void Prefers_github_token()
        {
            var token = GitHubToken.FromEnvironment(
                Read(new() { ["GITHUB_TOKEN"] = "primary", ["GH_TOKEN"] = "secondary" }));

            Assert.That(token, Is.EqualTo("primary"));
        }

        [Test]
        public void Falls_back_to_gh_token()
        {
            var token = GitHubToken.FromEnvironment(Read(new() { ["GH_TOKEN"] = "secondary" }));

            Assert.That(token, Is.EqualTo("secondary"));
        }

        [Test]
        public void Is_null_when_neither_is_set()
        {
            Assert.That(GitHubToken.FromEnvironment(Read([])), Is.Null);
        }

        [Test]
        public void Treats_an_empty_variable_as_unset()
        {
            // An exported-but-empty variable is a common shell accident; it must not become "Bearer ".
            var token = GitHubToken.FromEnvironment(
                Read(new() { ["GITHUB_TOKEN"] = "  ", ["GH_TOKEN"] = "secondary" }));

            Assert.That(token, Is.EqualTo("secondary"));
        }

        private static Func<string, string?> Read(Dictionary<string, string> environment) =>
            name => environment.TryGetValue(name, out var value) ? value : null;
    }
}
