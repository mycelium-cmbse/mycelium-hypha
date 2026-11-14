// ------------------------------------------------------------------------------------------------
// <copyright file="ToolTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text.Json;

    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;

    using Moq;

    [TestFixture]
    public class ToolTests
    {
        private const string Tag = "2026-05";

        private Mock<IKnowledgeQuery> query = null!;
        private Mock<IApiQuery> api = null!;
        private Mock<IKnowledgeLookup> lookup = null!;

        public static IEnumerable<TestCaseData> MetaclassQuestions()
        {
            yield return Case(MetaclassQuestion.Element, query => query.Element("PartUsage", Tag));
            yield return Case(MetaclassQuestion.Ancestors, query => query.Ancestors("PartUsage", Tag));
            yield return Case(MetaclassQuestion.Subclasses, query => query.Subclasses("PartUsage", true, Tag));
            yield return Case(MetaclassQuestion.Specializes, query => query.Specializes("PartUsage", "Feature", Tag));
            yield return Case(MetaclassQuestion.Neighbours, query => query.Neighbours("PartUsage", Tag));
            yield return Case(MetaclassQuestion.Constraints, query => query.Constraints("PartUsage", false, Tag));
            yield return Case(MetaclassQuestion.Operations, query => query.Operations("PartUsage", false, Tag));
            yield return Case(MetaclassQuestion.Literals, query => query.Literals("PartUsage", Tag));
            yield return Case(MetaclassQuestion.Features, query => query.Features("PartUsage", "partDefinition", Tag));
            yield return Case(MetaclassQuestion.EffectiveFeatures, query => query.EffectiveFeatures("PartUsage", Tag));
        }

        public static IEnumerable<TestCaseData> FeatureQuestions()
        {
            yield return Case(FeatureQuestion.Redefinitions, query => query.Redefinitions("ownedMember", Tag, "Namespace"));
            yield return Case(FeatureQuestion.Subsettings, query => query.Subsettings("ownedMember", Tag, "Namespace"));
            yield return Case(FeatureQuestion.Opposite, query => query.Opposite("Namespace", "ownedMember", Tag));
            yield return Case(FeatureQuestion.FeatureConstraints, query => query.FeatureConstraints("Namespace", "ownedMember", Tag));
            yield return Case(FeatureQuestion.Contributors, query => query.Contributors("Namespace", "ownedMember", Tag));
        }

        public static IEnumerable<TestCaseData> TypedByQuestions()
        {
            yield return Case(TypedByQuestion.ClassesWithFeatureTyped, query => query.ClassesWithFeatureTyped("Expression", true, Tag));
            yield return Case(TypedByQuestion.FeaturesAccepting, query => query.FeaturesAccepting("Expression", "PartUsage", Tag));
            yield return Case(TypedByQuestion.Containers, query => query.Containers("Expression", "PartUsage", Tag));
        }

        public static IEnumerable<TestCaseData> SyntaxQuestions()
        {
            yield return Case(SyntaxQuestion.CrossReference, query => query.CrossReference("PartUsage", Tag));
            yield return Case(SyntaxQuestion.FeatureAssignments, query => query.FeatureAssignments("declaredName", "=", Tag));
            yield return Case(SyntaxQuestion.BuiltBy, query => query.BuiltBy("PartUsage", "sysml", Tag));
            yield return Case(SyntaxQuestion.Keyword, query => query.Keyword("ref", "sysml", Tag));
        }

        public static IEnumerable<TestCaseData> UnreadableKnowledge()
        {
            yield return new TestCaseData(new JsonException("'n' is an invalid start of a value.")).SetArgDisplayNames("JsonException");
            yield return new TestCaseData(new IOException("The file is being used by another process.")).SetArgDisplayNames("IOException");
            yield return new TestCaseData(new UnauthorizedAccessException("Access to the path is denied.")).SetArgDisplayNames("UnauthorizedAccessException");
        }

        public static IEnumerable<TestCaseData> ApiQuestions()
        {
            yield return Case(ApiQuestion.PimClass, (IApiQuery api) => api.PimClass("Commit", "2026-04"));
            yield return Case(ApiQuestion.Type, (IApiQuery api) => api.Type("Commit", "2026-04"));
            yield return Case(ApiQuestion.Operation, (IApiQuery api) => api.Operation("Commit", "2026-04"));
            yield return Case(ApiQuestion.Schema, (IApiQuery api) => api.Schema("Commit", "2026-04"));
            yield return Case(ApiQuestion.Clause, (IApiQuery api) => api.Clause("7.1.4", "2026-04"));
        }

        [SetUp]
        public void SetUp()
        {
            this.query = new Mock<IKnowledgeQuery>();
            this.api = new Mock<IApiQuery>();
            this.lookup = new Mock<IKnowledgeLookup>();
        }

        [TestCaseSource(nameof(MetaclassQuestions))]
        public void Each_metaclass_question_asks_the_query_it_names(MetaclassQuestion question, Expression<Func<IKnowledgeQuery, object>> asked)
        {
            new MetamodelTools(this.query.Object).Metaclass(
                question, "PartUsage", general: "Feature", feature: "partDefinition", concreteOnly: true, includeInherited: false, tag: Tag);

            this.query.Verify(asked, Times.Once);
        }

        [TestCaseSource(nameof(FeatureQuestions))]
        public void Each_feature_question_asks_the_query_it_names(FeatureQuestion question, Expression<Func<IKnowledgeQuery, object>> asked)
        {
            new MetamodelTools(this.query.Object).Feature(question, "ownedMember", @class: "Namespace", tag: Tag);

            this.query.Verify(asked, Times.Once);
        }

        [TestCaseSource(nameof(TypedByQuestions))]
        public void Each_typed_by_question_asks_the_query_it_names(TypedByQuestion question, Expression<Func<IKnowledgeQuery, object>> asked)
        {
            new MetamodelTools(this.query.Object).TypedBy(question, "Expression", @class: "PartUsage", compositeOnly: true, tag: Tag);

            this.query.Verify(asked, Times.Once);
        }

        [TestCaseSource(nameof(SyntaxQuestions))]
        public void Each_syntax_question_asks_the_query_it_names(SyntaxQuestion question, Expression<Func<IKnowledgeQuery, object>> asked)
        {
            new SyntaxTools(this.query.Object).Syntax(
                question, element: "PartUsage", feature: "declaredName", @operator: "=", production: "PartUsage", word: "ref", grammar: "sysml", tag: Tag);

            this.query.Verify(asked, Times.Once);
        }

        [TestCaseSource(nameof(ApiQuestions))]
        public void Each_api_question_asks_the_query_it_names(ApiQuestion question, Expression<Func<IApiQuery, object>> asked)
        {
            new ApiTools(this.api.Object).Api(question, name: "Commit", clause: "7.1.4", version: "2026-04");

            this.api.Verify(asked, Times.Once);
        }

        [Test]
        public void The_other_tools_ask_their_queries()
        {
            var specification = new SpecificationTools(this.query.Object);
            var releases = new ReleaseTools(this.query.Object);

            specification.Spec(SpecificationQuestion.Clause, "kerml", "7.4.2", tag: Tag);
            specification.Spec(SpecificationQuestion.ElementsInClause, "sysml2", "8.3.6", includeSubclauses: true, tag: Tag);
            specification.Search("Part", kind: "metamodel", limit: 5, tag: Tag);
            specification.SearchSpec("is typed by", document: "kerml", limit: 7, tag: Tag);
            specification.Resolve("hypha://2026-05/metamodel/PartUsage");
            new MetamodelTools(this.query.Object).Package("Parts", Tag);
            releases.ReleaseStatus();
            releases.Diff("2026-02", "2026-05", "PartUsage");

            Assert.Multiple(() =>
            {
                this.query.Verify(q => q.Clause("kerml", "7.4.2", Tag), Times.Once);
                this.query.Verify(q => q.ElementsInClause("sysml2", "8.3.6", true, Tag), Times.Once);
                this.query.Verify(q => q.Search("Part", "metamodel", 5, Tag), Times.Once);
                this.query.Verify(q => q.SearchSpecification("is typed by", "kerml", 7, Tag), Times.Once);
                this.query.Verify(
                    q => q.Resolve(It.Is<HyphaAddress>(address => address.Tag == Tag && address.Kind == "metamodel" && address.Name == "PartUsage")),
                    Times.Once);
                this.query.Verify(q => q.Package("Parts", Tag), Times.Once);
                this.query.Verify(q => q.Releases(), Times.Once);
                this.query.Verify(q => q.Diff("2026-02", "2026-05", "PartUsage"), Times.Once);
            });
        }

        [Test]
        public void An_answer_is_the_result_with_its_grounding_as_camel_case_json()
        {
            this.query
                .Setup(q => q.Ancestors("PartUsage", null))
                .Returns(new QueryResult<IReadOnlyList<string>>(
                    ["Usage", "Feature"], new Grounding("DERIVED", Tag, "knowledge/2026-05/metamodel/metamodel.json")));

            using var answer = JsonDocument.Parse(new MetamodelTools(this.query.Object).Metaclass(MetaclassQuestion.Ancestors, "PartUsage"));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("value")[0].GetString(), Is.EqualTo("Usage"));
                Assert.That(answer.RootElement.GetProperty("grounding").GetProperty("tier").GetString(), Is.EqualTo("DERIVED"));
                Assert.That(answer.RootElement.GetProperty("grounding").GetProperty("release").GetString(), Is.EqualTo(Tag));
            });
        }

        [Test]
        public void A_lookup_page_is_camel_case_json_with_its_status()
        {
            this.lookup
                .Setup(l => l.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ISQBase::mass", Tag, 12, 40)))
                .Returns(new LookupResult { Release = Tag, Kind = KnowledgeKind.ModelLibrary, Status = LookupStatus.Found, Content = "attribute mass;" });

            using var answer = JsonDocument.Parse(
                new LookupTools(this.lookup.Object).Lookup(KnowledgeKind.ModelLibrary, "ISQBase::mass", startLine: 12, maxLines: 40, tag: Tag));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("status").GetString(), Is.EqualTo("found"));
                Assert.That(answer.RootElement.GetProperty("kind").GetString(), Is.EqualTo("model-library"));
                Assert.That(answer.RootElement.GetProperty("content").GetString(), Is.EqualTo("attribute mass;"));
            });
        }

        [Test]
        public void A_release_that_cannot_answer_is_a_tool_error_with_the_reason()
        {
            this.query
                .Setup(q => q.Ancestors("PartUsage", "2099-01"))
                .Throws(new KnowledgeLookupException("Release 2099-01 is not installed. Run 'hypha fetch --tag 2099-01'."));

            Assert.That(
                () => new MetamodelTools(this.query.Object).Metaclass(MetaclassQuestion.Ancestors, "PartUsage", tag: "2099-01"),
                Throws.TypeOf<McpException>().With.Message.EqualTo("Release 2099-01 is not installed. Run 'hypha fetch --tag 2099-01'."));
        }

        [TestCaseSource(nameof(UnreadableKnowledge))]
        public void A_knowledge_file_that_cannot_be_read_is_a_tool_error_with_the_reason_and_the_fix(Exception failure)
        {
            this.query.Setup(q => q.Releases()).Throws(failure);

            Assert.That(
                () => new ReleaseTools(this.query.Object).ReleaseStatus(),
                Throws.TypeOf<McpException>().With.Message.Contains("A knowledge file could not be read")
                    .And.Message.Contains(failure.Message)
                    .And.Message.Contains("fetch or generate the release again"));
        }

        [Test]
        public void A_manifest_that_is_not_a_version_manifest_is_a_tool_error_with_the_reason()
        {
            this.query.Setup(q => q.Releases()).Throws(new InvalidOperationException("knowledge/versions.json is not a readable version manifest."));

            Assert.That(
                () => new ReleaseTools(this.query.Object).ReleaseStatus(),
                Throws.TypeOf<McpException>().With.Message.EqualTo("knowledge/versions.json is not a readable version manifest."));
        }

        [Test]
        public void An_omitted_argument_takes_its_documented_default()
        {
            var metamodel = new MetamodelTools(this.query.Object);

            metamodel.Metaclass(MetaclassQuestion.Constraints, "PartUsage");
            metamodel.Metaclass(MetaclassQuestion.Operations, "PartUsage");
            metamodel.Metaclass(MetaclassQuestion.Subclasses, "PartUsage");
            metamodel.TypedBy(TypedByQuestion.ClassesWithFeatureTyped, "Expression");
            new SpecificationTools(this.query.Object).Spec(SpecificationQuestion.ElementsInClause, "sysml2", "8.3.6");
            new SpecificationTools(this.query.Object).Search("Part");
            new SpecificationTools(this.query.Object).SearchSpec("typed");
            new LookupTools(this.lookup.Object).Lookup(KnowledgeKind.Metamodel, "PartUsage");

            Assert.Multiple(() =>
            {
                this.query.Verify(q => q.Constraints("PartUsage", true, null), Times.Once);
                this.query.Verify(q => q.Operations("PartUsage", true, null), Times.Once);
                this.query.Verify(q => q.Subclasses("PartUsage", false, null), Times.Once);
                this.query.Verify(q => q.ClassesWithFeatureTyped("Expression", false, null), Times.Once);
                this.query.Verify(q => q.ElementsInClause("sysml2", "8.3.6", false, null), Times.Once);
                this.query.Verify(q => q.Search("Part", null, 20, null), Times.Once);
                this.query.Verify(q => q.SearchSpecification("typed", null, 20, null), Times.Once);
                this.lookup.Verify(
                    l => l.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", null, null, LookupRequest.DefaultMaxLines)),
                    Times.Once);
            });
        }

        [Test]
        public void A_question_without_the_argument_it_needs_is_a_tool_error_naming_it()
        {
            var metamodel = new MetamodelTools(this.query.Object);

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => metamodel.Metaclass(MetaclassQuestion.Specializes, "PartUsage"),
                    Throws.TypeOf<McpException>().With.Message.Contains("'general'"));
                Assert.That(
                    () => metamodel.Feature(FeatureQuestion.Opposite, "ownedMember"),
                    Throws.TypeOf<McpException>().With.Message.Contains("'class'"));
                Assert.That(
                    () => new SyntaxTools(this.query.Object).Syntax(SyntaxQuestion.Keyword, word: " "),
                    Throws.TypeOf<McpException>().With.Message.Contains("'word'"));
                Assert.That(
                    () => new ApiTools(this.api.Object).Api(ApiQuestion.Clause),
                    Throws.TypeOf<McpException>().With.Message.Contains("'clause'"));
            });

            this.query.VerifyNoOtherCalls();
            this.api.VerifyNoOtherCalls();
        }

        [Test]
        public void An_address_that_does_not_parse_is_a_tool_error()
        {
            Assert.That(
                () => new SpecificationTools(this.query.Object).Resolve("not an address"),
                Throws.TypeOf<McpException>());
        }

        private static TestCaseData Case<TQuestion>(TQuestion question, Expression<Func<IKnowledgeQuery, object>> asked) =>
            new TestCaseData(question, asked).SetArgDisplayNames(question!.ToString()!);

        private static TestCaseData Case<TQuestion>(TQuestion question, Expression<Func<IApiQuery, object>> asked) =>
            new TestCaseData(question, asked).SetArgDisplayNames(question!.ToString()!);
    }
}
