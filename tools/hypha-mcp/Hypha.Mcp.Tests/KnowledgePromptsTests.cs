// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgePromptsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using Hypha.Mcp.Prompts;

    using ModelContextProtocol;

    [TestFixture]
    public class KnowledgePromptsTests
    {
        [Test]
        public void Each_prompt_carries_what_it_was_given()
        {
            Assert.Multiple(() =>
            {
                Assert.That(KnowledgePrompts.ExplainMetaclass(" PartUsage "), Does.Contain("metaclass PartUsage.").And.Contain("hypha_metaclass"));
                Assert.That(KnowledgePrompts.CiteSpecification("redefinition"), Does.Contain("about redefinition?").And.Contain("hypha_search_spec"));
                Assert.That(KnowledgePrompts.ValidateNotation("part def Vehicle;"), Does.Contain("part def Vehicle;").And.Contain("hypha_syntax"));
                Assert.That(KnowledgePrompts.ApiLookup("Which operation creates a project?"), Does.Contain("Which operation creates a project?").And.Contain("hypha_api"));
            });
        }

        [Test]
        public void Every_prompt_starts_from_what_is_installed_and_never_from_memory()
        {
            var prompts = new[]
            {
                KnowledgePrompts.Overview(),
                KnowledgePrompts.ExplainMetaclass("PartUsage"),
                KnowledgePrompts.CiteSpecification("redefinition"),
                KnowledgePrompts.ValidateNotation("part def Vehicle;"),
                KnowledgePrompts.ApiLookup("Which operation creates a project?"),
            };

            Assert.That(prompts, Is.All.Contains("hypha_release_status").And.All.Contains("memory"));
        }

        [Test]
        public void A_citation_is_quoted_from_the_clause_not_the_search_excerpt()
        {
            Assert.That(KnowledgePrompts.CiteSpecification("redefinition"), Does.Contain("hypha_spec 'clause'").And.Contain("never quote an excerpt"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void A_blank_argument_is_refused_naming_it(string? value)
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => KnowledgePrompts.ExplainMetaclass(value!), Throws.TypeOf<McpException>().With.Message.Contains("'name'"));
                Assert.That(() => KnowledgePrompts.CiteSpecification(value!), Throws.TypeOf<McpException>().With.Message.Contains("'topic'"));
                Assert.That(() => KnowledgePrompts.ValidateNotation(value!), Throws.TypeOf<McpException>().With.Message.Contains("'notation'"));
                Assert.That(() => KnowledgePrompts.ApiLookup(value!), Throws.TypeOf<McpException>().With.Message.Contains("'question'"));
            });
        }
    }
}
