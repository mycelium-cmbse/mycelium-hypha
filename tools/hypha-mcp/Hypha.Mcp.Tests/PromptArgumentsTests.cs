// ------------------------------------------------------------------------------------------------
// <copyright file="PromptArgumentsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System.Collections.Generic;
    using System.Text.Json;

    using Hypha.Mcp.Prompts;

    using ModelContextProtocol.Protocol;

    [TestFixture]
    public class PromptArgumentsTests
    {
        private static readonly Prompt Prompt = new()
        {
            Name = "two_required",
            Arguments =
            [
                new PromptArgument { Name = "a", Required = true },
                new PromptArgument { Name = "b", Required = true },
                new PromptArgument { Name = "c", Required = false },
            ],
        };

        [TestCase("""{"b": "  "}""")]
        [TestCase("""{"b": null}""")]
        [TestCase("""{"c": "given"}""")]
        [TestCase("""{"a": "", "b": "\t"}""")]
        public void Missing_lists_the_required_arguments_left_out_or_blank_in_declared_order(string arguments)
        {
            Assert.That(PromptArguments.Missing(Prompt, Arguments(arguments)), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void Missing_is_empty_when_every_required_argument_is_given_and_an_optional_one_is_not()
        {
            Assert.That(PromptArguments.Missing(Prompt, Arguments("""{"a": "x", "b": "y"}""")), Is.Empty);
        }

        [Test]
        public void Without_arguments_every_required_one_is_missing()
        {
            Assert.That(PromptArguments.Missing(Prompt, null), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void A_prompt_that_declares_no_arguments_misses_none()
        {
            Assert.That(PromptArguments.Missing(new Prompt { Name = "overview" }, null), Is.Empty);
        }

        [Test]
        public void The_refusal_names_the_prompt_and_each_argument_in_the_plural_when_there_are_several()
        {
            Assert.Multiple(() =>
            {
                Assert.That(PromptArguments.Refusal("p", ["a"]), Is.EqualTo("The prompt 'p' needs the argument 'a'."));
                Assert.That(PromptArguments.Refusal("p", ["a", "b"]), Is.EqualTo("The prompt 'p' needs the arguments 'a', 'b'."));
            });
        }

        private static Dictionary<string, JsonElement> Arguments(string json) =>
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
    }
}
