// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiReaderTests.cs" company="Starion Group S.A.">
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
    /// Tests for <see cref="OpenApiReader"/> on a small, invented OpenAPI document - no OMG file.
    /// </summary>
    [TestFixture]
    public class OpenApiReaderTests
    {
        /// <summary>
        /// An invented OpenAPI 3.1 document. The schemas, their members and the paths are deliberately
        /// declared out of name order, and one schema carries a <c>Request</c> suffix without being
        /// used by any endpoint.
        /// </summary>
        internal const string Document = """
            {
              "openapi": "3.1.0",
              "info": { "title": "Toy Widget API", "version": "9.9" },
              "tags": [ { "name": "Zeta" }, { "name": "Alpha" } ],
              "paths": {
                "/widgets": {
                  "get": {
                    "operationId": "getWidgets",
                    "summary": "List the widgets",
                    "tags": [ "Alpha" ],
                    "parameters": [
                      { "$ref": "#/components/parameters/widgetId" },
                      {
                        "name": "filter",
                        "in": "query",
                        "description": "name | colour",
                        "schema": { "type": [ "string", "null" ] }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "Ok",
                        "content": {
                          "application/json": {
                            "schema": { "type": "array", "items": { "$ref": "#/components/schemas/Widget" } },
                            "examples": { "one": { "value": { "$ref": "#/components/schemas/OnlyInExample" } } }
                          }
                        }
                      },
                      "404": { "$ref": "#/components/responses/NotFound" },
                      "default": { "description": "Oops" }
                    }
                  },
                  "post": {
                    "operationId": "postWidget",
                    "tags": [ "Zeta" ],
                    "requestBody": {
                      "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WidgetDraft" } } }
                    },
                    "responses": {
                      "201": {
                        "description": "Created",
                        "content": { "application/json": { "schema": { "$ref": "#/components/schemas/Widget" } } }
                      }
                    }
                  }
                },
                "/aardvarks": {
                  "delete": {
                    "operationId": "deleteAardvarks",
                    "responses": { "204": { "description": "Gone" } }
                  }
                }
              },
              "components": {
                "parameters": {
                  "widgetId": {
                    "name": "widgetId",
                    "in": "path",
                    "required": true,
                    "description": "first line\r\nsecond line",
                    "schema": { "type": "string", "format": "uuid" }
                  }
                },
                "responses": {
                  "NotFound": {
                    "description": "No such widget",
                    "content": { "application/json": { "schema": { "$ref": "#/components/schemas/Problem" } } }
                  }
                },
                "schemas": {
                  "WidgetRequest": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/WidgetRequest",
                    "type": "object",
                    "properties": { "name": { "type": "string" } }
                  },
                  "Widget": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Widget",
                    "type": "object",
                    "additionalProperties": false,
                    "required": [ "@id", "@type" ],
                    "properties": {
                      "successor": {
                        "oneOf": [
                          {
                            "$ref": "#/components/schemas/Identified",
                            "$comment": "https://example.org/spec/SystemsModelingAPI/20990101/Widget"
                          },
                          { "type": "null" }
                        ]
                      },
                      "parts": { "type": "array", "items": { "$ref": "#/components/schemas/Part" } },
                      "mixed": {
                        "type": "array",
                        "items": {
                          "oneOf": [
                            { "$ref": "#/components/schemas/Part" },
                            { "$ref": "#/components/schemas/Gadget" }
                          ]
                        }
                      },
                      "owner": {
                        "$ref": "#/components/schemas/Identified",
                        "$comment": "https://example.org/spec/SystemsModelingAPI/20990101/Owner"
                      },
                      "label": { "oneOf": [ { "type": "string" }, { "type": "null" } ] },
                      "note": { "type": [ "string", "null" ] },
                      "state": { "enum": [ "open", "closed" ] },
                      "element": { "$ref": "#/components/schemas/Element" },
                      "@type": { "const": "Widget" },
                      "@id": { "type": "string", "format": "uuid" }
                    }
                  },
                  "WidgetDraft": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/WidgetDraft",
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "@type": { "const": "Widget" },
                      "tag": { "$ref": "#/components/schemas/Tagging" }
                    }
                  },
                  "Tagging": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Tagging",
                    "type": "object",
                    "properties": { "shared": { "$ref": "#/components/schemas/Shared" } }
                  },
                  "Part": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Part",
                    "type": "object",
                    "properties": { "shared": { "$ref": "#/components/schemas/Shared" } }
                  },
                  "Shared": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Shared",
                    "type": "object",
                    "properties": { "value": { "type": "integer" } }
                  },
                  "Gadget": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Gadget",
                    "type": "object",
                    "properties": { "size": { "type": "number" } }
                  },
                  "Identified": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Identified",
                    "type": "object",
                    "required": [ "@id" ],
                    "properties": { "@id": { "type": "string", "format": "uuid" } }
                  },
                  "Problem": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Problem",
                    "type": "object",
                    "properties": { "description": { "type": "string" } }
                  },
                  "OnlyInExample": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/OnlyInExample",
                    "type": "object",
                    "properties": { "x": { "type": "string" } }
                  },
                  "Choice": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Choice",
                    "oneOf": [
                      { "$ref": "#/components/schemas/Part" },
                      { "$ref": "#/components/schemas/Gadget" }
                    ]
                  },
                  "Colour": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Colour",
                    "type": "string",
                    "enum": [ "red", "green" ]
                  },
                  "Token": {
                    "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Token",
                    "type": "string",
                    "format": "uuid"
                  },
                  "Element": {
                    "$id": "https://example.org/spec/KerML/20990101/Element",
                    "type": "object",
                    "properties": { "@id": { "type": "string", "format": "uuid" } }
                  },
                  "Anonymous": {
                    "type": "object",
                    "properties": { "x": { "type": "string" } }
                  }
                }
              }
            }
            """;

        private OpenApiModel model = null!;

        [SetUp]
        public void SetUp() => this.model = new OpenApiReader().Read(Document);

        [Test]
        public void Reads_the_document_title_versions_and_tags()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.model.Title, Is.EqualTo("Toy Widget API"));
                Assert.That(this.model.Version, Is.EqualTo("9.9"));
                Assert.That(this.model.OpenApiVersion, Is.EqualTo("3.1.0"));
                Assert.That(this.model.Tags, Is.EqualTo(new[] { "Zeta", "Alpha" }));
            });
        }

        [Test]
        public void The_namespace_comes_from_the_schema_id()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Widget").Namespace, Is.EqualTo(OpenApiNames.ApiNamespace));
                Assert.That(
                    this.Schema("Widget").Id,
                    Is.EqualTo("https://example.org/spec/SystemsModelingAPI/20990101/Widget"));
                Assert.That(this.Schema("Element").Namespace, Is.EqualTo(OpenApiNames.MetamodelNamespace));

                // No $id at all says nothing about belonging to the API, so it is not claimed for it.
                Assert.That(this.Schema("Anonymous").Id, Is.Empty);
                Assert.That(this.Schema("Anonymous").Namespace, Is.EqualTo(OpenApiNames.MetamodelNamespace));
            });
        }

        [Test]
        public void The_kind_is_object_union_enumeration_or_the_plain_type()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Widget").Kind, Is.EqualTo(OpenApiNames.ObjectKind));
                Assert.That(this.Schema("Choice").Kind, Is.EqualTo(OpenApiNames.UnionKind));
                Assert.That(this.Schema("Colour").Kind, Is.EqualTo(OpenApiNames.EnumerationKind));
                Assert.That(this.Schema("Token").Kind, Is.EqualTo("string (uuid)"));
            });
        }

        [Test]
        public void A_union_lists_its_variants_and_an_enumeration_its_values_in_document_order()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Choice").Variants, Is.EqualTo(new[] { "Part", "Gadget" }));
                Assert.That(this.Schema("Choice").Properties, Is.Empty);
                Assert.That(this.Schema("Colour").Values, Is.EqualTo(new[] { "red", "green" }));
                Assert.That(this.Schema("Widget").Variants, Is.Empty);
                Assert.That(this.Schema("Widget").Values, Is.Empty);
            });
        }

        [Test]
        public void Additional_properties_are_read_only_when_stated()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Widget").AdditionalProperties, Is.False);
                Assert.That(this.Schema("Part").AdditionalProperties, Is.Null);
            });
        }

        [Test]
        public void A_member_is_required_when_the_schema_lists_it()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Property("Widget", "@id").Required, Is.True);
                Assert.That(this.Property("Widget", "@type").Required, Is.True);
                Assert.That(this.Property("Widget", "label").Required, Is.False);
                Assert.That(this.Property("WidgetDraft", "@type").Required, Is.False);
            });
        }

        [Test]
        public void A_member_is_nullable_through_a_null_alternative()
        {
            var label = this.Property("Widget", "label");

            Assert.Multiple(() =>
            {
                Assert.That(label.Type, Is.EqualTo("string"));
                Assert.That(label.Nullable, Is.True);
            });
        }

        [Test]
        public void A_member_is_nullable_through_a_type_array_containing_null()
        {
            var note = this.Property("Widget", "note");

            Assert.Multiple(() =>
            {
                Assert.That(note.Type, Is.EqualTo("string"));
                Assert.That(note.Nullable, Is.True);
            });
        }

        [Test]
        public void A_member_without_a_null_alternative_is_not_nullable()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Property("Widget", "@id").Nullable, Is.False);
                Assert.That(this.Property("Widget", "owner").Nullable, Is.False);
                Assert.That(this.Property("Widget", "parts").Nullable, Is.False);
            });
        }

        [TestCase("@id", "string (uuid)")]
        [TestCase("parts", "Part[]")]
        [TestCase("mixed", "(Part | Gadget)[]")]
        [TestCase("@type", "const \"Widget\"")]
        [TestCase("state", "enum (open, closed)")]
        [TestCase("element", "Element")]
        [TestCase("owner", "Identified")]
        [TestCase("successor", "Identified")]
        public void The_type_is_written_in_the_compact_notation(string member, string expected)
        {
            Assert.That(this.Property("Widget", member).Type, Is.EqualTo(expected));
        }

        [Test]
        public void A_reference_by_identifier_names_its_target_from_the_comment()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Property("Widget", "owner").RefersTo, Is.EqualTo("Owner"));

                // Also through the nullable wrapper around the reference.
                Assert.That(this.Property("Widget", "successor").RefersTo, Is.EqualTo("Widget"));
                Assert.That(this.Property("Widget", "successor").Nullable, Is.True);

                Assert.That(this.Property("Widget", "element").RefersTo, Is.Null);
                Assert.That(this.Property("Widget", "parts").RefersTo, Is.Null);
            });
        }

        [Test]
        public void A_schema_an_endpoint_refers_to_directly_takes_that_role()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Widget").Role, Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(this.Schema("WidgetDraft").Role, Is.EqualTo(OpenApiNames.RequestRole));
            });
        }

        [Test]
        public void A_role_reaches_the_schemas_referred_to_transitively()
        {
            Assert.Multiple(() =>
            {
                // WidgetDraft -> Tagging -> Shared, and Widget -> Part -> Shared.
                Assert.That(this.Schema("Tagging").Role, Is.EqualTo(OpenApiNames.RequestRole));
                Assert.That(this.Schema("Part").Role, Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(this.Schema("Gadget").Role, Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(this.Schema("Identified").Role, Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(this.Schema("Shared").Role, Is.EqualTo(OpenApiNames.BothRoles));
            });
        }

        [Test]
        public void A_role_is_followed_through_a_shared_response_component()
        {
            Assert.That(this.Schema("Problem").Role, Is.EqualTo(OpenApiNames.ResponseRole));
        }

        [Test]
        public void A_request_suffix_in_the_name_does_not_make_a_schema_a_request_model()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("WidgetRequest").Role, Is.EqualTo(OpenApiNames.NoRole));

                // ... and a request model needs no such suffix.
                Assert.That(this.Schema("WidgetDraft").Role, Is.EqualTo(OpenApiNames.RequestRole));
            });
        }

        [Test]
        public void A_schema_no_endpoint_reaches_is_unused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Schema("Colour").Role, Is.EqualTo(OpenApiNames.NoRole));

                // Choice refers to Part, which is used - that does not make Choice itself used.
                Assert.That(this.Schema("Choice").Role, Is.EqualTo(OpenApiNames.NoRole));
            });
        }

        [Test]
        public void An_example_does_not_count_as_a_use_of_a_schema()
        {
            Assert.That(this.Schema("OnlyInExample").Role, Is.EqualTo(OpenApiNames.NoRole));
        }

        [Test]
        public void Schemas_are_ordered_by_name()
        {
            var names = this.model.Schemas.Select(schema => schema.Name).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Count.EqualTo(15));
                Assert.That(names, Is.Ordered.Using((System.Collections.IComparer)StringComparer.Ordinal));
                Assert.That(names[0], Is.EqualTo("Anonymous"));
                Assert.That(names[^1], Is.EqualTo("WidgetRequest"));
            });
        }

        [Test]
        public void Members_are_ordered_by_name()
        {
            Assert.That(
                this.Schema("Widget").Properties.Select(property => property.Name),
                Is.EqualTo(new[]
                {
                    "@id", "@type", "element", "label", "mixed", "note", "owner", "parts", "state", "successor",
                }));
        }

        [Test]
        public void Endpoints_are_kept_in_document_order()
        {
            Assert.That(
                this.model.Endpoints.Select(endpoint => $"{endpoint.Method} {endpoint.Path}"),
                Is.EqualTo(new[] { "GET /widgets", "POST /widgets", "DELETE /aardvarks" }));
        }

        [Test]
        public void An_endpoint_carries_its_identifier_summary_and_tags()
        {
            var get = this.model.Endpoints[0];
            var delete = this.model.Endpoints[2];

            Assert.Multiple(() =>
            {
                Assert.That(get.OperationId, Is.EqualTo("getWidgets"));
                Assert.That(get.Summary, Is.EqualTo("List the widgets"));
                Assert.That(get.Tags, Is.EqualTo(new[] { "Alpha" }));
                Assert.That(delete.Summary, Is.Empty);
                Assert.That(delete.Tags, Is.Empty);
                Assert.That(delete.Parameters, Is.Empty);
            });
        }

        [Test]
        public void A_parameter_is_resolved_through_the_shared_components()
        {
            var parameters = this.model.Endpoints[0].Parameters;

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Count.EqualTo(2));
                Assert.That(
                    parameters[0],
                    Is.EqualTo(new OpenApiParameter(
                        "widgetId", "path", true, "string (uuid)", "first line\r\nsecond line")));
                Assert.That(
                    parameters[1],
                    Is.EqualTo(new OpenApiParameter("filter", "query", false, "string", "name | colour")));
            });
        }

        [Test]
        public void A_response_is_resolved_through_the_shared_components()
        {
            Assert.That(
                this.model.Endpoints[0].Responses,
                Is.EqualTo(new[]
                {
                    new OpenApiResponse("200", "Ok", "Widget[]"),
                    new OpenApiResponse("404", "No such widget", "Problem"),
                    new OpenApiResponse("default", "Oops", null),
                }));
        }

        [Test]
        public void The_request_body_type_is_read_when_there_is_one()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.model.Endpoints[0].Request, Is.Null);
                Assert.That(this.model.Endpoints[1].Request, Is.EqualTo("WidgetDraft"));
            });
        }

        [TestCase("""{ "openapi": "3.1.0", "components": { "schemas": {} } }""")]
        [TestCase("""{ "openapi": "3.1.0", "paths": {} }""")]
        [TestCase("""{ "openapi": "3.1.0", "paths": {}, "components": {} }""")]
        public void A_document_without_paths_or_schemas_is_rejected(string json)
        {
            Assert.That(() => new OpenApiReader().Read(json), Throws.InvalidOperationException);
        }

        [Test]
        public void A_document_with_neither_tags_nor_info_still_reads()
        {
            var read = new OpenApiReader().Read("""{ "paths": {}, "components": { "schemas": {} } }""");

            Assert.Multiple(() =>
            {
                Assert.That(read.Title, Is.Empty);
                Assert.That(read.Tags, Is.Empty);
                Assert.That(read.Schemas, Is.Empty);
                Assert.That(read.Endpoints, Is.Empty);
            });
        }

        private OpenApiSchema Schema(string name) => this.model.Schemas.Single(schema => schema.Name == name);

        private OpenApiProperty Property(string schema, string name) =>
            this.Schema(schema).Properties.Single(property => property.Name == name);
    }
}
