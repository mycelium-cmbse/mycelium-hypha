// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceBuilderTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;

    /// <summary>
    /// Tests for the API cross-reference join, on synthetic data - an invented PIM, an invented
    /// REST/HTTP PSM index and invented clauses. No knowledge base and no PDF.
    /// </summary>
    [TestFixture]
    public class ApiCrossReferenceBuilderTests
    {
        internal static readonly PimGraph Pim = new(
            [
                new PimClass("Record", [Owned("id")], [], []),
                new PimClass(
                    "Project",
                    [Owned("name"), new PimAttribute("commits", 0, -1, true, null)],
                    [new PimAttribute("id", 1, 1, false, "Record")],
                    []),
                new PimClass("Commit", [Owned("message")], null, null),
                new PimClass("Branch", [Owned("name")], [], []),
                new PimClass("DataIdentity", [], [], []),
                new PimClass("MergeResult", [Owned("conflict")], [], []),
                new PimClass(
                    "ProjectService",
                    [],
                    [],
                    [new("getProjects"), new("getById"), new("createProject"), new("mergeProject")]),
                new PimClass("ElementService", [], [], [new("getById"), new("getElements")]),
            ],
            [new("Named")],
            [new("ChangeKind")],
            [new("Stamp")]);

        internal static readonly ApiClause ProjectClause = new(
            "7.1", "Project", "A wombat sentence that defines nothing in bold.");

        internal static readonly ApiClause DataModelClause = new(
            "7.2",
            "Zanzibar Overview",
            """
            **Data Identity** - a quokka sentence invented for this test.

            **Project** - another xylophone sentence that sets **Commit** in bold.

            **Figure 3. Branch and friends**
            """);

        internal static readonly ApiClause ServiceClause = new(
            "7.10",
            "ProjectService",
            """
            The **ProjectService** hands out a **Project** on request, said the narwhal.

            getProjects(): Project[0..*]
            createProject(name: String): Project
            """);

        internal static readonly ApiClause ModelMappingClause = new(
            "8.1",
            "Yonder Mapping",
            """
            An axolotl introduces the table.

            **PIM Concept REST/HTTP PSM Model**
            Project ProjectModel
            Commit Commit
            MergeResult.conflict Conflict
            Branch Branch [0..*]

            Stray Afterwards
            **PIM Concept OSLC PSM Resource**
            Project OslcProject
            Commit OslcCommit
            """);

        internal static readonly ApiClause EndpointMappingClause = new(
            "8.2",
            "Hither Mapping",
            """
            A capybara introduces the table.

            **PIM Service REST / HTTP PSM Endpoint**
            **ProjectService**
            getProjects GET /projects
            getById GET/projects/{projectId}
            createProject POST /projects
            mergeProject POST
            /projects/{projectId}/merge
            **ElementService**
            getById GET /elements/{elementId}
            getProjects GET /wrong
            """);

        internal static readonly OpenApiIndexDocument OpenApi = new(
            OpenApiIndexDocument.CurrentSchemaVersion,
            "1.0",
            "Toy API",
            "9.9",
            "3.1.0",
            new OpenApiIndexCounts(8, 7, 3),
            new SortedDictionary<string, OpenApiIndexSchema>(StringComparer.Ordinal)
            {
                ["ProjectModel"] = ApiSchema(
                    OpenApiNames.ResponseRole,
                    Member("@id"), Member("@type", "const \"Project\""), Member("extra"), Member("id"), Member("name")),
                ["ProjectDraft"] = ApiSchema(
                    OpenApiNames.RequestRole, Member("@type", "const \"Project\""), Member("name")),
                ["Commit"] = ApiSchema(OpenApiNames.ResponseRole, Member("@id"), Member("message")),

                // Named like a request model for Commit, but nothing in it says it is one.
                ["CommitRequest"] = ApiSchema(OpenApiNames.RequestRole, Member("message")),

                // Fixed to Commit, but never used in a request body.
                ["CommitView"] = ApiSchema(OpenApiNames.ResponseRole, Member("@type", "const \"Commit\"")),
                ["Branch"] = ApiSchema(OpenApiNames.BothRoles, Member("@type", "const \"Branch\""), Member("name")),
                ["Conflict"] = ApiSchema(OpenApiNames.ResponseRole, Member("reason")),
                ["DataIdentity"] = new OpenApiIndexSchema(
                    "https://example.org/spec/KerML/20990101/DataIdentity",
                    OpenApiNames.MetamodelNamespace,
                    OpenApiNames.ObjectKind,
                    OpenApiNames.ResponseRole,
                    null,
                    null,
                    null),
            },
            [
                Endpoint("GET", "/projects", "listProjects"),
                Endpoint("GET", "/projects/{projectId}", "readProject"),
                Endpoint("GET", "/elements/{elementId}", "readElement"),
            ]);

        private static readonly ApiClause[] AllClauses =
        [
            ProjectClause, DataModelClause, ServiceClause, ModelMappingClause, EndpointMappingClause,
        ];

        private IApiCrossReferenceBuilder builder = null!;

        [SetUp]
        public void SetUp() => this.builder = new ApiCrossReferenceBuilder();

        [Test]
        public void Records_the_version_and_the_shape_it_is_written_in()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, AllClauses);

            Assert.Multiple(() =>
            {
                Assert.That(document.ApiVersion, Is.EqualTo("1.0"));
                Assert.That(document.SchemaVersion, Is.EqualTo(ApiCrossReferenceDocument.CurrentSchemaVersion));
                Assert.That(document.Sources, Is.EqualTo(new ApiCrossReferenceSources(true, true, true)));
            });
        }

        [Test]
        public void Every_pim_type_is_listed_with_its_kind_and_page()
        {
            var document = this.builder.Build("1.0", Pim, null, []);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Types.Keys,
                    Is.EqualTo(new[]
                    {
                        "Branch", "ChangeKind", "Commit", "DataIdentity", "ElementService", "MergeResult", "Named",
                        "Project", "ProjectService", "Record", "Stamp",
                    }));
                Assert.That(document.Types["Project"].Kind, Is.EqualTo("class"));
                Assert.That(document.Types["Named"].Kind, Is.EqualTo("interface"));
                Assert.That(document.Types["ChangeKind"].Kind, Is.EqualTo("enumeration"));
                Assert.That(document.Types["Stamp"].Kind, Is.EqualTo("dataType"));
                Assert.That(document.Types["Project"].Pim, Is.EqualTo("pim/elements/Project.md"));
            });
        }

        [Test]
        public void A_clause_titled_after_the_type_is_linked_by_its_title()
        {
            var document = this.builder.Build("1.0", Pim, null, [ProjectClause]);

            Assert.That(
                document.Types["Project"].Clauses,
                Is.EqualTo(new[] { new ApiClauseEdge("7.1", ApiCrossReferenceDocument.TitleMethod) }));
        }

        [Test]
        public void A_paragraph_that_opens_by_defining_the_type_links_it_even_when_spelled_in_two_words()
        {
            var document = this.builder.Build("1.0", Pim, null, [DataModelClause]);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Types["DataIdentity"].Clauses,
                    Is.EqualTo(new[] { new ApiClauseEdge("7.2", ApiCrossReferenceDocument.DefinitionMethod) }));
                Assert.That(
                    document.Types["Project"].Clauses,
                    Is.EqualTo(new[] { new ApiClauseEdge("7.2", ApiCrossReferenceDocument.DefinitionMethod) }));
            });
        }

        [Test]
        public void A_name_set_in_bold_is_linked_as_a_mention()
        {
            var document = this.builder.Build("1.0", Pim, null, [DataModelClause]);

            Assert.Multiple(() =>
            {
                // In the middle of a sentence, and as one word of a bold figure caption.
                Assert.That(
                    document.Types["Commit"].Clauses,
                    Is.EqualTo(new[] { new ApiClauseEdge("7.2", ApiCrossReferenceDocument.BoldMentionMethod) }));
                Assert.That(
                    document.Types["Branch"].Clauses,
                    Is.EqualTo(new[] { new ApiClauseEdge("7.2", ApiCrossReferenceDocument.BoldMentionMethod) }));
            });
        }

        [Test]
        public void A_name_that_is_not_in_bold_is_not_linked()
        {
            var document = this.builder.Build(
                "1.0", Pim, null, [new ApiClause("3", "Terms", "A Commit and a Branch walk past a MergeResult.")]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["Commit"].Clauses, Is.Empty);
                Assert.That(document.Types["Branch"].Clauses, Is.Empty);
                Assert.That(document.Types["MergeResult"].Clauses, Is.Empty);
            });
        }

        [Test]
        public void The_strongest_link_wins_and_a_clause_is_listed_once()
        {
            const string text = "**Commit** - an ocelot definition.\n\nLater, **Commit** again.";

            var document = this.builder.Build(
                "1.0",
                Pim,
                null,
                [new ApiClause("5", "Commit", text), new ApiClause("6", "Elsewhere", text)]);

            Assert.That(
                document.Types["Commit"].Clauses,
                Is.EqualTo(new[]
                {
                    new ApiClauseEdge("5", ApiCrossReferenceDocument.TitleMethod),
                    new ApiClauseEdge("6", ApiCrossReferenceDocument.DefinitionMethod),
                }));
        }

        [Test]
        public void Clauses_are_ordered_numerically_not_as_text()
        {
            var document = this.builder.Build("1.0", Pim, null, [ServiceClause, DataModelClause, ProjectClause]);

            Assert.That(
                document.Types["Project"].Clauses,
                Is.EqualTo(new[]
                {
                    new ApiClauseEdge("7.1", ApiCrossReferenceDocument.TitleMethod),
                    new ApiClauseEdge("7.2", ApiCrossReferenceDocument.DefinitionMethod),
                    new ApiClauseEdge("7.10", ApiCrossReferenceDocument.BoldMentionMethod),
                }));
        }

        [Test]
        public void An_operation_is_linked_to_the_clause_that_lists_its_signature_under_its_service()
        {
            var document = this.builder.Build("1.0", Pim, null, [ServiceClause]);

            var signature = new[] { new ApiClauseEdge("7.10", ApiCrossReferenceDocument.SignatureMethod) };

            Assert.Multiple(() =>
            {
                Assert.That(document.Operations["ProjectService.getProjects"].Clauses, Is.EqualTo(signature));
                Assert.That(document.Operations["ProjectService.createProject"].Clauses, Is.EqualTo(signature));
                Assert.That(document.Operations["ProjectService.getProjects"].Service, Is.EqualTo("ProjectService"));

                // Owned by the service, but the clause gives no signature for it.
                Assert.That(document.Operations["ProjectService.getById"].Clauses, Is.Empty);
            });
        }

        [Test]
        public void A_signature_is_not_linked_when_the_clause_does_not_name_the_service_in_bold()
        {
            var document = this.builder.Build(
                "1.0",
                Pim,
                null,
                [new ApiClause("7.11", "Elsewhere", "The ElementService is plain here.\ngetElements(): Element")]);

            Assert.That(document.Operations["ElementService.getElements"].Clauses, Is.Empty);
        }

        [Test]
        public void Every_operation_of_every_service_is_keyed_by_service_and_name()
        {
            var document = this.builder.Build("1.0", Pim, null, []);

            Assert.That(
                document.Operations.Keys,
                Is.EqualTo(new[]
                {
                    "ElementService.getById", "ElementService.getElements", "ProjectService.createProject",
                    "ProjectService.getById", "ProjectService.getProjects", "ProjectService.mergeProject",
                }));
        }

        [Test]
        public void The_rest_model_mapping_table_is_read_row_by_row()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.That(
                document.ModelMapping,
                Is.EqualTo(new[]
                {
                    new ApiModelMappingRow("Project", "ProjectModel", "8.1"),
                    new ApiModelMappingRow("Commit", "Commit", "8.1"),
                    new ApiModelMappingRow("MergeResult.conflict", "Conflict", "8.1"),
                    new ApiModelMappingRow("Branch", "Branch [0..*]", "8.1"),
                }));
        }

        [Test]
        public void A_table_of_the_same_build_under_the_oslc_header_is_not_read()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.ModelMapping.Select(row => row.Psm), Has.None.StartsWith("Oslc"));

                // ... and neither is what follows the blank line that ends the table.
                Assert.That(document.ModelMapping.Select(row => row.Pim), Has.None.EqualTo("Stray"));
            });
        }

        [Test]
        public void A_clause_with_only_the_oslc_table_yields_no_rows()
        {
            var document = this.builder.Build(
                "1.0",
                Pim,
                OpenApi,
                [new ApiClause("9.1", "Other", "**PIM Concept OSLC PSM Resource**\nProject ProjectModel")]);

            Assert.Multiple(() =>
            {
                Assert.That(document.ModelMapping, Is.Empty);
                Assert.That(document.Types["Project"].Schema!.Response, Is.Null);
            });
        }

        [Test]
        public void The_model_mapping_table_also_ends_at_a_bold_line()
        {
            var document = this.builder.Build(
                "1.0",
                Pim,
                null,
                [
                    new ApiClause(
                        "8.1",
                        "Mapping",
                        "**PIM Concept REST/HTTP PSM Model**\nProject ProjectModel\n**Table 2. Caption**\nCommit Wrong"),
                ]);

            Assert.That(document.ModelMapping, Is.EqualTo(new[] { new ApiModelMappingRow("Project", "ProjectModel", "8.1") }));
        }

        [Test]
        public void A_whole_type_row_of_the_mapping_table_names_the_response_schema()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["Project"].Schema!.Response, Is.EqualTo("ProjectModel"));
                Assert.That(
                    document.Types["Project"].Schema!.ResponseMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.MappingTableMethod));

                // Stated by the table, so the table is credited even though the names also agree.
                Assert.That(
                    document.Types["Commit"].Schema!.ResponseMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.MappingTableMethod));
            });
        }

        [Test]
        public void A_member_row_or_a_row_with_a_multiplicity_names_no_schema()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                // "MergeResult.conflict Conflict" maps a member: MergeResult itself gets no schema.
                Assert.That(document.Types["MergeResult"].Schema, Is.Null);

                // "Branch Branch [0..*]" is kept as written but is not what links Branch: its shared
                // name is.
                Assert.That(document.Types["Branch"].Schema!.Response, Is.EqualTo("Branch"));
                Assert.That(
                    document.Types["Branch"].Schema!.ResponseMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.SameNameMethod));
            });
        }

        [Test]
        public void Without_the_mapping_table_a_schema_of_the_same_name_is_the_fallback()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, []);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Types["Commit"].Schema,
                    Is.EqualTo(new ApiSchemaEdge("Commit", ApiCrossReferenceDocument.SameNameMethod, null, null)));

                // A metamodel schema that happens to carry the name is not the API's model of the type.
                Assert.That(document.Types["DataIdentity"].Schema, Is.Null);
            });
        }

        [Test]
        public void The_request_schema_is_the_one_fixed_to_the_type_not_one_named_after_it()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["Project"].Schema!.Request, Is.EqualTo("ProjectDraft"));
                Assert.That(
                    document.Types["Project"].Schema!.RequestMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.TypeConstantMethod));

                // CommitRequest is named for it but not fixed to it; CommitView is fixed to it but is
                // never used in a request body.
                Assert.That(document.Types["Commit"].Schema!.Request, Is.Null);
                Assert.That(document.Types["Commit"].Schema!.RequestMethod, Is.Null);

                // The response schema is not offered again as its own request twin.
                Assert.That(document.Types["Branch"].Schema!.Request, Is.Null);
            });
        }

        [Test]
        public void A_type_with_only_a_request_schema_still_gets_an_edge()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, []);

            // Without the table nothing states ProjectModel, and no schema is called Project.
            Assert.That(
                document.Types["Project"].Schema,
                Is.EqualTo(new ApiSchemaEdge(null, null, "ProjectDraft", ApiCrossReferenceDocument.TypeConstantMethod)));
            Assert.That(document.Types["Project"].Members, Is.Null);
        }

        [Test]
        public void Members_compare_the_effective_pim_attributes_with_the_two_models()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.That(
                document.Types["Project"].Members,
                Is.EqualTo(new[]
                {
                    new ApiMemberEntry("commits", "[0..*]", true, null, false, false),
                    new ApiMemberEntry("id", "[1..1]", false, "Record", true, false),
                    new ApiMemberEntry("name", "[1..1]", false, null, true, true),
                }));
        }

        [Test]
        public void Members_of_the_response_model_that_are_not_pim_attributes_are_listed_as_psm_only()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["Project"].PsmOnly, Is.EqualTo(new[] { "@id", "@type", "extra" }));
                Assert.That(document.Types["Commit"].PsmOnly, Is.EqualTo(new[] { "@id" }));
            });
        }

        [Test]
        public void A_type_without_a_response_schema_has_no_members_to_compare()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [ModelMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["MergeResult"].Members, Is.Null);
                Assert.That(document.Types["MergeResult"].PsmOnly, Is.Null);
                Assert.That(document.Types["ChangeKind"].Members, Is.Null);
            });
        }

        [Test]
        public void An_endpoint_row_maps_an_operation_with_or_without_the_space_after_the_method()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [EndpointMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Operations["ProjectService.getProjects"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("GET", "/projects", "listProjects") }));
                Assert.That(
                    document.Operations["ProjectService.getById"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("GET", "/projects/{projectId}", "readProject") }));
            });
        }

        [Test]
        public void A_row_under_another_service_is_not_attributed()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [EndpointMappingClause]);

            Assert.Multiple(() =>
            {
                // Both services own a getById; each gets the row under its own heading only.
                Assert.That(
                    document.Operations["ElementService.getById"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("GET", "/elements/{elementId}", "readElement") }));
                Assert.That(document.Operations["ProjectService.getById"].Endpoints, Has.Count.EqualTo(1));

                // "getProjects GET /wrong" stands under ElementService, which owns no getProjects.
                Assert.That(
                    document.Operations["ProjectService.getProjects"].Endpoints.Select(endpoint => endpoint.Path),
                    Has.None.EqualTo("/wrong"));
            });
        }

        [Test]
        public void The_operation_identifier_is_null_when_the_openapi_document_has_no_such_endpoint()
        {
            var with = this.builder.Build("1.0", Pim, OpenApi, [EndpointMappingClause]);
            var without = this.builder.Build("1.0", Pim, null, [EndpointMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(
                    with.Operations["ProjectService.createProject"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("POST", "/projects", null) }));
                Assert.That(
                    without.Operations["ProjectService.getProjects"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("GET", "/projects", null) }));
            });
        }

        [Test]
        public void An_operation_mapped_over_several_lines_points_at_the_clause_and_carries_no_endpoint()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, [EndpointMappingClause]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Operations["ProjectService.mergeProject"].Endpoints, Is.Empty);
                Assert.That(document.Operations["ProjectService.mergeProject"].MappingClause, Is.EqualTo("8.2"));

                // Named there and mapped on one row: both are given.
                Assert.That(document.Operations["ProjectService.getProjects"].MappingClause, Is.EqualTo("8.2"));

                // Not named in the table at all.
                Assert.That(document.Operations["ElementService.getElements"].MappingClause, Is.Null);
                Assert.That(document.Operations["ElementService.getElements"].Endpoints, Is.Empty);
            });
        }

        [Test]
        public void Endpoint_rows_outside_the_endpoint_mapping_table_clause_are_not_read()
        {
            var document = this.builder.Build(
                "1.0",
                Pim,
                OpenApi,
                [new ApiClause("9.2", "Other", "**PIM Service OSLC PSM Operation**\n**ProjectService**\ngetProjects GET /projects")]);

            Assert.Multiple(() =>
            {
                Assert.That(document.Operations["ProjectService.getProjects"].Endpoints, Is.Empty);
                Assert.That(document.Operations["ProjectService.getProjects"].MappingClause, Is.Null);
            });
        }

        [Test]
        public void Without_the_openapi_index_no_type_has_a_schema()
        {
            var document = this.builder.Build("1.0", Pim, null, AllClauses);

            Assert.Multiple(() =>
            {
                Assert.That(document.Sources.OpenApi, Is.False);
                Assert.That(document.Sources.Specification, Is.True);
                Assert.That(document.Types.Values.Select(type => type.Schema), Has.All.Null);
                Assert.That(document.Types.Values.Select(type => type.Members), Has.All.Null);
                Assert.That(document.Counts.TypesWithSchema, Is.Zero);

                // The table is still the specification's, and is still kept as written.
                Assert.That(document.ModelMapping, Has.Count.EqualTo(4));
            });
        }

        [Test]
        public void Without_clauses_no_type_has_a_clause_and_no_operation_an_endpoint()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, []);

            Assert.Multiple(() =>
            {
                Assert.That(document.Sources.Specification, Is.False);
                Assert.That(document.Sources.OpenApi, Is.True);
                Assert.That(document.Types.Values.SelectMany(type => type.Clauses), Is.Empty);
                Assert.That(document.Operations.Values.SelectMany(operation => operation.Clauses), Is.Empty);
                Assert.That(document.Operations.Values.SelectMany(operation => operation.Endpoints), Is.Empty);
                Assert.That(document.ModelMapping, Is.Empty);
                Assert.That(document.Counts.TypesWithClause, Is.Zero);
                Assert.That(document.Counts.OperationsWithEndpoint, Is.Zero);
            });
        }

        [Test]
        public void The_counts_say_how_much_lines_up()
        {
            var document = this.builder.Build("1.0", Pim, OpenApi, AllClauses);

            // 11 types; Project, Commit, Branch, DataIdentity and the two services have a clause;
            // Project, Commit and Branch have a schema; 6 operations, of which mergeProject and
            // getElements have no single-row endpoint.
            Assert.That(document.Counts, Is.EqualTo(new ApiCrossReferenceCounts(11, 6, 3, 6, 4)));
        }

        [Test]
        public void A_pim_with_absent_collections_yields_an_empty_document()
        {
            var document = this.builder.Build("1.0", new PimGraph(null, null, null, null), null, []);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types, Is.Empty);
                Assert.That(document.Operations, Is.Empty);
                Assert.That(document.Counts, Is.EqualTo(new ApiCrossReferenceCounts(0, 0, 0, 0, 0)));
            });
        }

        [Test]
        public void No_clause_text_ever_reaches_the_rendered_document()
        {
            var json = ApiJson.Render(this.builder.Build("1.0", Pim, OpenApi, AllClauses));

            Assert.Multiple(() =>
            {
                // One invented word per clause body, and the two titles that name no type.
                foreach (var word in new[]
                         {
                             "wombat", "quokka", "xylophone", "narwhal", "axolotl", "capybara", "Figure", "friends",
                             "Zanzibar", "Yonder", "Hither", "sentence", "introduces",
                         })
                {
                    Assert.That(json, Does.Not.Contain(word), word);
                }

                // No whole line of prose from any clause, and no member that could carry one.
                foreach (var line in AllClauses
                             .SelectMany(clause => clause.Text.Split('\n'))
                             .Select(line => line.Trim())
                             .Where(line => line.Contains(' ', StringComparison.Ordinal) && line.EndsWith('.')))
                {
                    Assert.That(json, Does.Not.Contain(line), line);
                }

                Assert.That(json, Does.Not.Contain("\"text\""));
                Assert.That(json, Does.Not.Contain("\"title\""));
                Assert.That(json, Does.Not.Contain("**"));

                // What it does carry: identifiers.
                Assert.That(json, Does.Contain("\"clause\": \"7.10\""));
                Assert.That(json, Does.Contain("\"method\": \"definition-paragraph\""));
            });
        }

        [Test]
        public void The_rendered_document_is_the_same_whatever_order_the_inputs_arrive_in()
        {
            var forwards = ApiJson.Render(this.builder.Build("1.0", Pim, OpenApi, AllClauses));
            var backwards = ApiJson.Render(this.builder.Build("1.0", Pim, OpenApi, Enumerable.Reverse(AllClauses).ToList()));

            Assert.Multiple(() =>
            {
                Assert.That(backwards, Is.EqualTo(forwards));
                Assert.That(forwards, Does.Not.Contain("\r"));
                Assert.That(forwards, Does.EndWith("}\n"));
            });
        }

        [Test]
        public void A_clause_extracted_with_windows_line_endings_reads_the_same()
        {
            var windows = AllClauses
                .Select(clause => clause with
                {
                    Text = clause.Text.Replace("\r\n", "\n", StringComparison.Ordinal)
                        .Replace("\n", "\r\n", StringComparison.Ordinal),
                })
                .ToList();
            var unix = AllClauses
                .Select(clause => clause with { Text = clause.Text.Replace("\r\n", "\n", StringComparison.Ordinal) })
                .ToList();

            Assert.That(
                ApiJson.Render(this.builder.Build("1.0", Pim, OpenApi, windows)),
                Is.EqualTo(ApiJson.Render(this.builder.Build("1.0", Pim, OpenApi, unix))));
        }

        private static PimAttribute Owned(string name) => new(name, 1, 1, false, null);

        private static OpenApiProperty Member(string name, string type = "string") => new(name, type, false, false, null);

        private static OpenApiIndexSchema ApiSchema(string role, params OpenApiProperty[] properties) => new(
            "https://example.org/spec/SystemsModelingAPI/20990101/Schema",
            OpenApiNames.ApiNamespace,
            OpenApiNames.ObjectKind,
            role,
            "schemas/Schema.md",
            properties,
            null);

        private static OpenApiEndpoint Endpoint(string method, string path, string operationId) =>
            new(method, path, operationId, string.Empty, [], [], null, []);
    }
}
