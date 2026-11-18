// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Mcp.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;

    using Hypha.Knowledge.Lookup;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;

    using NUnit.Framework;

    [TestFixture]
    public class ToolRefusalsTests
    {
        [Test]
        public void A_refusal_is_answered_as_a_tool_error_holding_its_message()
        {
            var result = ToolRefusals.Refusal("Release 2098-01 is not installed.");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.True);
                Assert.That(result.Content.OfType<TextContentBlock>().Single().Text, Is.EqualTo("Release 2098-01 is not installed."));
            });
        }

        [Test]
        public void A_file_that_cannot_be_read_or_an_upstream_that_cannot_be_reached_is_a_failure()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new IOException())), Is.True);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new UnauthorizedAccessException())), Is.True);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new JsonException())), Is.True);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new HttpRequestException())), Is.True);
            });
        }

        [Test]
        public void An_answer_that_says_what_is_wrong_and_how_to_fix_it_is_a_refusal()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ToolRefusals.IsFailure(new McpException("x")), Is.False);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new KnowledgeLookupException("y"))), Is.False);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new ArgumentException())), Is.False);
                Assert.That(ToolRefusals.IsFailure(new McpException("x", new InvalidOperationException())), Is.False);
            });
        }

        [Test]
        public void The_filter_needs_the_handler_it_wraps()
        {
            Assert.That(() => ToolRefusals.AsToolErrors(null!), Throws.ArgumentNullException);
        }
    }
}
