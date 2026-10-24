// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiRendererTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Linq;

    using Hypha.Knowledge.Api.OpenApi;

    /// <summary>
    /// Tests for <see cref="OpenApiRenderer"/>, on the invented document of
    /// <see cref="OpenApiReaderTests"/>.
    /// </summary>
    [TestFixture]
    public class OpenApiRendererTests
    {
        private OpenApiModel model = null!;
        private OpenApiRenderer renderer = null!;

        [SetUp]
        public void SetUp()
        {
            this.model = new OpenApiReader().Read(OpenApiReaderTests.Document);
            this.renderer = new OpenApiRenderer();
        }

        [Test]
        public void A_schema_page_is_named_after_the_schema()
        {
            Assert.That(this.renderer.SchemaFileName("Widget"), Is.EqualTo("Widget.md"));
        }

        [Test]
        public void A_schema_page_opens_with_its_front_matter_and_heading()
        {
            Assert.That(
                this.Page("Widget"),
                Does.StartWith(
                    "---\n"
                    + "name: Widget\n"
                    + "id: https://example.org/spec/SystemsModelingAPI/20990101/Widget\n"
                    + "kind: object\n"
                    + "role: response\n"
                    + "additionalProperties: false\n"
                    + "---\n\n"
                    + "# Widget\n\n"
                    + "REST/HTTP PSM schema · object · used in responses\n"));
        }

        [Test]
        public void The_front_matter_leaves_out_additional_properties_when_the_schema_does_not_state_them()
        {
            Assert.That(this.Page("Part"), Does.Not.Contain("additionalProperties"));
        }

        [TestCase("WidgetDraft", "used in request bodies")]
        [TestCase("Shared", "used in request bodies and in responses")]
        [TestCase("Colour", "not used by any endpoint")]
        public void The_role_is_spelled_out_under_the_heading(string schema, string sentence)
        {
            Assert.That(this.Page(schema), Does.Contain(" · " + sentence + "\n"));
        }

        [Test]
        public void The_properties_table_gives_type_required_nullable_and_target()
        {
            Assert.That(
                this.Page("Widget"),
                Does.Contain(
                    "\n## Properties\n\n"
                    + "| Property | Type | Required | Nullable | Refers to |\n"
                    + "| --- | --- | --- | --- | --- |\n"
                    + "| @id | string (uuid) | yes | no |  |\n"
                    + "| @type | const \"Widget\" | yes | no |  |\n"
                    + "| element | Element | no | no |  |\n"
                    + "| label | string | no | yes |  |\n"
                    + "| mixed | ([Part](Part.md) \\| [Gadget](Gadget.md))[] | no | no |  |\n"
                    + "| note | string | no | yes |  |\n"
                    + "| owner | [Identified](Identified.md) | no | no | Owner |\n"
                    + "| parts | [Part](Part.md)[] | no | no |  |\n"
                    + "| state | enum (open, closed) | no | no |  |\n"
                    + "| successor | [Identified](Identified.md) | no | yes | [Widget](Widget.md) |\n"));
        }

        [Test]
        public void A_quoted_literal_is_never_turned_into_a_link()
        {
            // Widget has a page, and "Widget" is also the constant its @type is fixed to: the type
            // notation links, the literal must not.
            var page = this.Page("WidgetDraft");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("| @type | const \"Widget\" | no | no |  |\n"));
                Assert.That(page, Does.Not.Contain("[Widget]"));
                Assert.That(this.Page("Widget"), Does.Contain("| parts | [Part](Part.md)[] |"));
            });
        }

        [Test]
        public void Only_a_schema_that_has_a_page_is_linked()
        {
            var page = this.Page("Widget");

            Assert.Multiple(() =>
            {
                // Element mirrors a metamodel element and Owner is not a schema at all.
                Assert.That(page, Does.Not.Contain("[Element]"));
                Assert.That(page, Does.Not.Contain("[Owner]"));
                Assert.That(page, Does.Contain("[Identified](Identified.md)"));
            });
        }

        [Test]
        public void A_union_page_lists_its_variants()
        {
            var page = this.Page("Choice");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("\n## Variants\n\n- [Part](Part.md)\n- [Gadget](Gadget.md)\n"));
                Assert.That(page, Does.Not.Contain("## Properties"));
            });
        }

        [Test]
        public void An_enumeration_page_lists_its_values()
        {
            Assert.That(this.Page("Colour"), Does.Contain("\n## Values\n\n- `red`\n- `green`\n"));
        }

        [Test]
        public void A_schema_page_lists_the_endpoints_that_use_it()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.Page("Widget"),
                    Does.EndWith(
                        "\n## Used by\n\n"
                        + "- `GET /widgets` – response 200\n"
                        + "- `POST /widgets` – response 201\n"));
                Assert.That(
                    this.Page("WidgetDraft"), Does.EndWith("\n## Used by\n\n- `POST /widgets` – request body\n"));

                // Reached through Widget only, so no endpoint names it.
                Assert.That(this.Page("Part"), Does.Not.Contain("## Used by"));
            });
        }

        [Test]
        public void The_endpoints_page_opens_with_the_title_and_the_totals()
        {
            var page = this.renderer.RenderEndpoints(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.StartWith("# Toy Widget API – endpoints\n\n"));
                Assert.That(page, Does.Contain("\n3 operations on 2 paths.\n"));
            });
        }

        [Test]
        public void The_endpoints_page_groups_by_the_tag_order_of_the_document()
        {
            var page = this.renderer.RenderEndpoints(this.model);

            var zeta = page.IndexOf("\n## Zeta\n", StringComparison.Ordinal);
            var alpha = page.IndexOf("\n## Alpha\n", StringComparison.Ordinal);
            var other = page.IndexOf("\n## Other\n", StringComparison.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(zeta, Is.GreaterThan(0));
                Assert.That(alpha, Is.GreaterThan(zeta), "the document declares Zeta before Alpha");
                Assert.That(other, Is.GreaterThan(alpha), "untagged operations follow the declared groups");
                Assert.That(
                    page.IndexOf("### POST /widgets", StringComparison.Ordinal), Is.InRange(zeta, alpha));
                Assert.That(page.IndexOf("### GET /widgets", StringComparison.Ordinal), Is.InRange(alpha, other));
                Assert.That(
                    page.IndexOf("### DELETE /aardvarks", StringComparison.Ordinal), Is.GreaterThan(other));
            });
        }

        [Test]
        public void A_tag_the_document_does_not_declare_follows_the_declared_ones()
        {
            var undeclared = this.model with
            {
                Endpoints =
                [
                    this.model.Endpoints[0] with { Tags = ["Beta"] },
                    this.model.Endpoints[1],
                ],
            };

            var page = this.renderer.RenderEndpoints(undeclared);

            Assert.Multiple(() =>
            {
                Assert.That(
                    page.IndexOf("\n## Beta\n", StringComparison.Ordinal),
                    Is.GreaterThan(page.IndexOf("\n## Zeta\n", StringComparison.Ordinal)));

                // A declared group without operations gets no heading.
                Assert.That(page, Does.Not.Contain("## Alpha"));
                Assert.That(page, Does.Not.Contain("## Other"));
            });
        }

        [Test]
        public void An_endpoint_is_rendered_with_its_parameters_request_and_responses()
        {
            var page = this.renderer.RenderEndpoints(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(
                    page,
                    Does.Contain(
                        "\n### GET /widgets\n\n"
                        + "`getWidgets` – List the widgets\n"
                        + "\n| Parameter | In | Required | Type | Description |\n"
                        + "| --- | --- | --- | --- | --- |\n"
                        + "| widgetId | path | yes | string (uuid) | first line second line |\n"
                        + "| filter | query | no | string | name \\| colour |\n"
                        + "\n| Status | Description | Body |\n"
                        + "| --- | --- | --- |\n"
                        + "| 200 | Ok | [Widget](schemas/Widget.md)[] |\n"
                        + "| 404 | No such widget | [Problem](schemas/Problem.md) |\n"
                        + "| default | Oops |  |\n"));
                Assert.That(
                    page,
                    Does.Contain(
                        "\n### POST /widgets\n\n"
                        + "`postWidget`\n"
                        + "\nRequest body: [WidgetDraft](schemas/WidgetDraft.md)\n"));
            });
        }

        [Test]
        public void A_pipe_in_a_description_is_escaped_in_a_table_cell()
        {
            var page = this.renderer.RenderEndpoints(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("name \\| colour"));
                Assert.That(page, Does.Not.Contain("name | colour"));
            });
        }

        [Test]
        public void The_index_links_the_api_schemas_and_only_names_the_metamodel_ones()
        {
            var page = this.renderer.RenderIndex("1.0", this.model);

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.StartWith("# Toy Widget API – REST/HTTP PSM (API version 1.0)\n\n"));
                Assert.That(
                    page,
                    Does.Contain(
                        "OpenAPI 3.1.0, API version 9.9. 15 schemas (13 of the API itself, "
                        + "2 mirroring KerML / SysML elements) and 3 operations - see [endpoints](endpoints.md).\n"));
                Assert.That(page, Does.Contain("| [Widget](schemas/Widget.md) | object | response |\n"));
                Assert.That(page, Does.Contain("| [Choice](schemas/Choice.md) | union | unused |\n"));
                Assert.That(page, Does.Not.Contain("schemas/Element.md"));
                Assert.That(page, Does.EndWith("\n`Anonymous`, `Element`\n"));
            });
        }

        [Test]
        public void The_index_has_no_metamodel_section_when_every_schema_belongs_to_the_api()
        {
            var apiOnly = this.model with
            {
                Schemas = this.model.Schemas
                    .Where(schema => schema.Namespace == OpenApiNames.ApiNamespace)
                    .ToList(),
            };

            Assert.That(this.renderer.RenderIndex("1.0", apiOnly), Does.Not.Contain("## Metamodel element schemas"));
        }

        [Test]
        public void Every_page_uses_line_feeds_only()
        {
            // The document carries a description with a CR LF in it, which is where one would leak.
            Assert.That(this.model.Endpoints[0].Parameters[0].Description, Does.Contain("\r\n"));

            Assert.Multiple(() =>
            {
                foreach (var schema in this.model.Schemas)
                {
                    Assert.That(this.renderer.RenderSchema(this.model, schema), Does.Not.Contain("\r"), schema.Name);
                }

                Assert.That(this.renderer.RenderEndpoints(this.model), Does.Not.Contain("\r"));
                Assert.That(this.renderer.RenderIndex("1.0", this.model), Does.Not.Contain("\r"));
            });
        }

        private string Page(string schema) =>
            this.renderer.RenderSchema(this.model, this.model.Schemas.Single(candidate => candidate.Name == schema));
    }
}
