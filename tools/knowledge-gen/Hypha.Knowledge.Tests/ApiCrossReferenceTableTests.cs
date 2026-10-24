// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceTableTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Linq;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;

    /// <summary>
    /// Suite of tests for where the <see cref="ApiCrossReferenceBuilder"/> stops reading the two
    /// mapping tables - the edges that decide whether specification wording, or a row that belongs to
    /// another service, ends up in the document.
    /// </summary>
    /// <remarks>
    /// The extracted clause text has no blank line between a table and what follows it, so the tables
    /// cannot be delimited by one. Uses the PIM and the PSM index of
    /// <see cref="ApiCrossReferenceBuilderTests"/>; every clause here is invented.
    /// </remarks>
    [TestFixture]
    public class ApiCrossReferenceTableTests
    {
        private ApiCrossReferenceBuilder builder = null!;

        [SetUp]
        public void SetUp() => this.builder = new ApiCrossReferenceBuilder();

        [Test]
        public void Prose_that_follows_the_model_table_without_a_blank_line_is_not_read_as_rows()
        {
            var clause = new ApiClause(
                "8.1",
                "Yonder Mapping",
                """
                **PIM Concept REST/HTTP PSM Model**
                Project ProjectModel
                MergeResult.conflict Conflict [0..*]
                The pangolin wrote a sentence right after the table.
                Commit Commit
                """);

            var document = this.Build(clause);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.ModelMapping,
                    Is.EqualTo(new[]
                    {
                        new ApiModelMappingRow("Project", "ProjectModel", "8.1"),
                        new ApiModelMappingRow("MergeResult.conflict", "Conflict [0..*]", "8.1"),
                    }));

                Assert.That(ApiJson.Render(document), Does.Not.Contain("pangolin"));
            });
        }

        [Test]
        public void A_model_table_that_continues_under_a_repeated_header_is_read_on()
        {
            // A table that runs over a page break repeats its header row.
            var clause = new ApiClause(
                "8.1",
                "Yonder Mapping",
                """
                **PIM Concept REST/HTTP PSM Model**
                Project ProjectModel
                **PIM Concept REST/HTTP PSM Model**
                Commit Commit
                """);

            Assert.That(
                this.Build(clause).ModelMapping.Select(row => row.Pim),
                Is.EqualTo(new[] { "Project", "Commit" }));
        }

        [Test]
        public void A_mapping_row_naming_a_schema_the_document_lacks_falls_back_to_the_shared_name()
        {
            var clause = new ApiClause(
                "8.1",
                "Yonder Mapping",
                """
                **PIM Concept REST/HTTP PSM Model**
                Commit CommitModel
                """);

            var document = this.Build(clause);

            Assert.Multiple(() =>
            {
                Assert.That(document.Types["Commit"].Schema!.Response, Is.EqualTo("Commit"));
                Assert.That(
                    document.Types["Commit"].Schema!.ResponseMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.SameNameMethod));
                Assert.That(document.ModelMapping, Has.Count.EqualTo(1), "the row is kept as the specification wrote it");
            });
        }

        [Test]
        public void A_row_before_the_first_service_heading_is_attributed_to_no_service()
        {
            var clause = new ApiClause(
                "8.2",
                "Hither Mapping",
                """
                **PIM Service REST / HTTP PSM Endpoint**
                getProjects GET /early
                **ProjectService**
                createProject POST /projects
                """);

            var document = this.Build(clause);

            Assert.Multiple(() =>
            {
                Assert.That(document.Operations["ProjectService.getProjects"].Endpoints, Is.Empty);
                Assert.That(document.Operations["ProjectService.createProject"].Endpoints, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void A_service_heading_after_an_unknown_one_takes_its_rows_again()
        {
            var clause = new ApiClause(
                "8.2",
                "Hither Mapping",
                """
                **PIM Service REST / HTTP PSM Endpoint**
                **ProjectService**
                **UnknownService**
                **ProjectService**
                getProjects GET /projects
                """);

            Assert.That(
                this.Build(clause).Operations["ProjectService.getProjects"].Endpoints.Select(endpoint => endpoint.Path),
                Is.EqualTo(new[] { "/projects" }));
        }

        [Test]
        public void A_service_heading_set_in_several_words_is_recognised()
        {
            // The specification writes some service names apart: "Project Usage Service".
            var clause = new ApiClause(
                "8.2",
                "Hither Mapping",
                """
                **PIM Service REST / HTTP PSM Endpoint**
                **Element Service**
                getById GET /elements/{elementId}
                **Project Service**
                getById GET /projects/{projectId}
                """);

            var document = this.Build(clause);

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Operations["ElementService.getById"].Endpoints.Select(endpoint => endpoint.Path),
                    Is.EqualTo(new[] { "/elements/{elementId}" }));
                Assert.That(
                    document.Operations["ProjectService.getById"].Endpoints.Select(endpoint => endpoint.Path),
                    Is.EqualTo(new[] { "/projects/{projectId}" }));
            });
        }

        [Test]
        public void A_bold_caption_inside_the_endpoint_table_ends_the_service_group()
        {
            var clause = new ApiClause(
                "8.2",
                "Hither Mapping",
                """
                **PIM Service REST / HTTP PSM Endpoint**
                **ProjectService**
                createProject POST /projects
                **Pagination**
                getProjects GET /not-a-row
                """);

            Assert.That(this.Build(clause).Operations["ProjectService.getProjects"].Endpoints, Is.Empty);
        }

        [Test]
        public void A_name_set_in_bold_in_several_words_is_a_mention_of_the_type()
        {
            var clause = new ApiClause("7.3", "Zanzibar Overview", "**Figure 4. The Merge Result explained**");

            Assert.That(
                this.Build(new ApiClause("7.4", "Other", "**Merge Result**"), clause)
                    .Types["MergeResult"].Clauses.Select(edge => edge.Clause),
                Is.EqualTo(new[] { "7.4" }));
        }

        private ApiCrossReferenceDocument Build(params ApiClause[] clauses) => this.builder.Build(
            "1.0", ApiCrossReferenceBuilderTests.Pim, ApiCrossReferenceBuilderTests.OpenApi, clauses);
    }
}
