// ------------------------------------------------------------------------------------------------
// <copyright file="ApiJsonTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;

    // Covers what the source-generated contract behind ApiJson guarantees: a document type nobody
    // declared is refused here rather than reflected over.
    [TestFixture]
    public class ApiJsonTests
    {
        private sealed record Undeclared(string Name);

        [Test]
        public void A_document_type_the_contract_does_not_declare_is_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    () => ApiJson.Render(new Undeclared("x")),
                    Throws.InstanceOf<NotSupportedException>().With.Message.Contains(nameof(Undeclared)));
                Assert.That(
                    () => ApiJson.Read<Undeclared>("{\"name\":\"x\"}"),
                    Throws.InstanceOf<NotSupportedException>().With.Message.Contains(nameof(Undeclared)));
            });
        }

        [Test]
        public void A_declared_document_type_round_trips()
        {
            var graph = new PimGraph([new PimClass("Project", null, null, null)], null, null, null);

            var rendered = ApiJson.Render(graph);

            Assert.Multiple(() =>
            {
                Assert.That(rendered, Does.EndWith("\n"));
                Assert.That(rendered, Does.Not.Contain("\r\n"), "LF endings, independent of the host");
                Assert.That(rendered, Does.Not.Contain("interfaces"), "absent rather than null");
                Assert.That(ApiJson.Read<PimGraph>(rendered).Classes![0].Name, Is.EqualTo("Project"));
            });
        }

        [Test]
        public void A_file_holding_the_json_literal_null_is_not_a_readable_document() =>
            Assert.That(
                () => ApiJson.Read<PimGraph>("null"),
                Throws.InstanceOf<InvalidOperationException>().With.Message.Contains(nameof(PimGraph)));

        [Test]
        public void A_blank_payload_and_a_null_document_are_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => ApiJson.Read<PimGraph>("   "), Throws.ArgumentException);
                Assert.That(() => ApiJson.Render<PimGraph>(null!), Throws.ArgumentNullException);
            });
        }
    }
}
