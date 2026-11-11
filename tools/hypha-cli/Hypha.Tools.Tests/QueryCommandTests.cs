// ------------------------------------------------------------------------------------------------
// <copyright file="QueryCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;
    using Hypha.Tools.Commands;

    using Moq;

    [TestFixture]
    public class QueryCommandTests
    {
        private const string Tag = "2026-05";

        private static readonly Grounding Graph = new("DERIVED", Tag, $"knowledge/{Tag}/metamodel/metamodel.json");

        private RootCommand root = null!;

        private QueryCommand query = null!;

        private Mock<IKnowledgeQuery> knowledge = null!;

        private Mock<IApiQuery> api = null!;

        [SetUp]
        public void SetUp()
        {
            this.query = new QueryCommand();
            this.root = new RootCommand { this.query };
            this.knowledge = new Mock<IKnowledgeQuery>();
            this.api = new Mock<IApiQuery>();
        }

        [Test]
        public void Every_query_has_a_verb()
        {
            static string Verb(string method) => Regex.Replace(method, "(?<=[a-z])(?=[A-Z])", "-").ToLowerInvariant();

            var verbs = this.query.Subcommands.Select(command => command.Name).ToList();
            var apiVerbs = this.query.Subcommands.Single(command => command.Name == "api").Subcommands.Select(command => command.Name);

            Assert.Multiple(() =>
            {
                Assert.That(verbs, Is.SupersetOf(typeof(IKnowledgeQuery).GetMethods().Select(method => Verb(method.Name)).Distinct()));
                Assert.That(apiVerbs, Is.EquivalentTo(typeof(IApiQuery).GetMethods().Select(method => Verb(method.Name))));
                Assert.That(this.query.Verbs, Has.Count.EqualTo(typeof(IKnowledgeQuery).GetMethods().Length + typeof(IApiQuery).GetMethods().Length));
            });
        }

        [Test]
        public async Task A_verb_passes_its_options_and_prints_the_result_with_its_grounding()
        {
            this.knowledge.Setup(mock => mock.Subclasses("Usage", true, Tag)).Returns(new QueryResult<IReadOnlyList<string>>(["PartUsage"], Graph));
            using var console = new RecordedConsole();

            var result = await this.Run($"query subclasses --of Usage --concrete-only --tag {Tag} --json");
            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(document.RootElement.GetProperty("value")[0].GetString(), Is.EqualTo("PartUsage"));
                Assert.That(document.RootElement.GetProperty("grounding").GetProperty("tier").GetString(), Is.EqualTo("DERIVED"));
                Assert.That(document.RootElement.GetProperty("grounding").GetProperty("source").GetString(), Is.EqualTo(Graph.Source));
                Assert.That(console.Output.TrimEnd('\r', '\n').Split('\n'), Has.Length.EqualTo(1));
            });
        }

        // Every verb, with a distinct value for each option so a swapped argument shows, and the one
        // call it must make. Options left out show their defaults.
        public static IEnumerable<TestCaseData> KnowledgeVerbs()
        {
            static TestCaseData Case(string commandLine, Expression<Action<IKnowledgeQuery>> call) =>
                new TestCaseData(commandLine, call).SetArgDisplayNames(commandLine);

            yield return Case("query releases", mock => mock.Releases());
            yield return Case("query diff --from A --to B --element E", mock => mock.Diff("A", "B", "E"));
            yield return Case("query diff --from A --to B", mock => mock.Diff("A", "B", null));
            yield return Case("query subclasses --of U -t T", mock => mock.Subclasses("U", false, "T"));
            yield return Case("query subclasses --of U --concrete-only", mock => mock.Subclasses("U", true, null));
            yield return Case("query ancestors --class C -t T", mock => mock.Ancestors("C", "T"));
            yield return Case("query specializes --specific S --general G -t T", mock => mock.Specializes("S", "G", "T"));
            yield return Case("query constraints --class C", mock => mock.Constraints("C", true, null));
            yield return Case("query constraints --class C --own-only -t T", mock => mock.Constraints("C", false, "T"));
            yield return Case("query operations --class C", mock => mock.Operations("C", true, null));
            yield return Case("query operations --class C --own-only -t T", mock => mock.Operations("C", false, "T"));
            yield return Case("query literals --enumeration E -t T", mock => mock.Literals("E", "T"));
            yield return Case("query neighbours --class C -t T", mock => mock.Neighbours("C", "T"));
            yield return Case("query package --name P -t T", mock => mock.Package("P", "T"));
            yield return Case("query element --name N -t T", mock => mock.Element("N", "T"));
            yield return Case("query classes-with-feature-typed --type Y -t T", mock => mock.ClassesWithFeatureTyped("Y", false, "T"));
            yield return Case("query classes-with-feature-typed --type Y --composite-only", mock => mock.ClassesWithFeatureTyped("Y", true, null));
            yield return Case("query features --class C --name N -t T", mock => mock.Features("C", "N", "T"));
            yield return Case("query features --class C", mock => mock.Features("C", null, null));
            yield return Case("query effective-features --class C -t T", mock => mock.EffectiveFeatures("C", "T"));
            yield return Case("query redefinitions --feature F --owner O -t T", mock => mock.Redefinitions("F", "T", "O"));
            yield return Case("query subsettings --feature F --owner O -t T", mock => mock.Subsettings("F", "T", "O"));
            yield return Case("query containers --element E --class C -t T", mock => mock.Containers("E", "C", "T"));
            yield return Case("query features-accepting --type Y --class C -t T", mock => mock.FeaturesAccepting("Y", "C", "T"));
            yield return Case("query opposite --class C --feature F -t T", mock => mock.Opposite("C", "F", "T"));
            yield return Case("query feature-constraints --class C --feature F -t T", mock => mock.FeatureConstraints("C", "F", "T"));
            yield return Case("query contributors --class C --feature F -t T", mock => mock.Contributors("C", "F", "T"));
            yield return Case("query cross-reference --element E -t T", mock => mock.CrossReference("E", "T"));
            yield return Case("query feature-assignments --feature F --operator += -t T", mock => mock.FeatureAssignments("F", "+=", "T"));
            yield return Case("query feature-assignments", mock => mock.FeatureAssignments(null, null, null));
            yield return Case("query built-by --production P --grammar G -t T", mock => mock.BuiltBy("P", "G", "T"));
            yield return Case("query keyword --word W --grammar G -t T", mock => mock.Keyword("W", "G", "T"));
            yield return Case("query elements-in-clause --document D --clause 8.3 -t T", mock => mock.ElementsInClause("D", "8.3", false, "T"));
            yield return Case("query elements-in-clause --document D --clause 8.3 --include-subclauses", mock => mock.ElementsInClause("D", "8.3", true, null));
            yield return Case("query declaration --name Q::n -t T", mock => mock.Declaration("Q::n", "T"));
            yield return Case("query clause --document D --clause 7.4 -t T", mock => mock.Clause("D", "7.4", "T"));
            yield return Case("query search --text X", mock => mock.Search("X", null, 20, null));
            yield return Case("query search --text X --kind K --limit 5 -t T", mock => mock.Search("X", "K", 5, "T"));
            yield return Case("query resolve --address hypha://T/metamodel/N", mock => mock.Resolve(new HyphaAddress("T", "metamodel", "N")));
        }

        public static IEnumerable<TestCaseData> ApiVerbs()
        {
            static TestCaseData Case(string commandLine, Expression<Action<IApiQuery>> call) =>
                new TestCaseData(commandLine, call).SetArgDisplayNames(commandLine);

            yield return Case("query api pim-class --name N -t V", mock => mock.PimClass("N", "V"));
            yield return Case("query api type --name N --tag V", mock => mock.Type("N", "V"));
            yield return Case("query api operation --name N -t V", mock => mock.Operation("N", "V"));
            yield return Case("query api clause --clause 7.1", mock => mock.Clause("7.1", null));
        }

        [TestCaseSource(nameof(KnowledgeVerbs))]
        public async Task A_release_verb_passes_each_option_to_its_parameter(string commandLine, Expression<Action<IKnowledgeQuery>> call)
        {
            this.knowledge.DefaultValueProvider = new EmptyAnswers();
            using var console = new RecordedConsole();

            var result = await this.Run(commandLine);

            Assert.That(result, Is.EqualTo(0), console.Output);
            this.knowledge.Verify(call, Times.Once());
            this.knowledge.VerifyNoOtherCalls();
            this.api.VerifyNoOtherCalls();
        }

        [TestCaseSource(nameof(ApiVerbs))]
        public async Task An_api_verb_passes_each_option_to_its_parameter(string commandLine, Expression<Action<IApiQuery>> call)
        {
            this.api.DefaultValueProvider = new EmptyAnswers();
            using var console = new RecordedConsole();

            var result = await this.Run(commandLine);

            Assert.That(result, Is.EqualTo(0), console.Output);
            this.api.Verify(call, Times.Once());
            this.api.VerifyNoOtherCalls();
            this.knowledge.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Without_json_the_grounding_comes_first_then_the_answer()
        {
            this.knowledge.Setup(mock => mock.Keyword(":>>", "sysml", null))
                .Returns(new QueryResult<IReadOnlyList<KeywordUse>>([new KeywordUse("sysml", "operator", "sysml", ["REDEFINES"], [])], Graph));
            using var console = new RecordedConsole();

            var result = await this.Run("query keyword --word :>> --grammar sysml");
            var lines = console.Output.Replace("\r\n", "\n").Split('\n');

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(lines[0], Is.EqualTo($"(DERIVED, {Tag}, {Graph.Source})"));
                Assert.That(console.Output, Does.Contain("\"syntaxFrom\": \"sysml\"").And.Contain("\"REDEFINES\""));
            });
        }

        [Test]
        public async Task An_answer_that_is_not_found_or_unavailable_says_so()
        {
            this.knowledge.Setup(mock => mock.Element("Nope", null)).Returns(new QueryResult<Hypha.Knowledge.Metamodel.LookupEntry>(null, Graph));
            this.knowledge.Setup(mock => mock.Clause("kerml", "7.4.2", null))
                .Returns(new QueryResult<SpecificationClause>(null, Graph, "The specification text has not been generated."));
            using var console = new RecordedConsole();

            var results = new[] { await this.Run("query element --name Nope"), await this.Run("query clause --document kerml --clause 7.4.2") };

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.All.EqualTo(0));
                Assert.That(console.Output, Does.Contain($"Nothing in release {Tag} answers that."));
                Assert.That(console.Output, Does.Contain("The specification text has not been generated."));
            });
        }

        [Test]
        public async Task A_question_that_cannot_be_answered_is_an_error()
        {
            this.knowledge.Setup(mock => mock.Ancestors("PartUsage", "2020-01")).Throws(new KnowledgeLookupException("Release 2020-01 is not installed."));
            using var console = new RecordedConsole();

            var missing = await this.Run("query ancestors --class PartUsage --tag 2020-01 --json");
            var output = console.Output;
            var address = await this.Run("query resolve --address not-an-address");

            using var document = JsonDocument.Parse(output);

            Assert.Multiple(() =>
            {
                Assert.That(missing, Is.EqualTo(1));
                Assert.That(document.RootElement.GetProperty("error").GetString(), Is.EqualTo("Release 2020-01 is not installed."));
                Assert.That(address, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("is not a hypha address"));
            });
        }

        [Test]
        public async Task The_query_command_answers_through_the_real_knowledge()
        {
            using var knowledge = new TemporaryKnowledge().Install(Tag, "2026-04");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"query releases --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();
            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(document.RootElement.GetProperty("default").GetString(), Is.EqualTo(Tag));
                Assert.That(
                    document.RootElement.GetProperty("releases").EnumerateArray().Select(release => release.GetProperty("tag").GetString()),
                    Is.EqualTo(new[] { Tag, "2026-04" }));
            });
        }

        // Answers any query with an empty result, so a test only states the call it expects.
        private sealed class EmptyAnswers : DefaultValueProvider
        {
            protected override object GetDefaultValue(Type type, Mock mock)
            {
                if (type == typeof(ReleaseInventory))
                {
                    return new ReleaseInventory(null, [], null, []);
                }

                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(QueryResult<>)
                    ? Activator.CreateInstance(type, null, Graph, null)!
                    : throw new InvalidOperationException($"No empty answer for {type}.");
            }
        }

        private Task<int> Run(string commandLine) =>
            new QueryCommand.Handler(this.query, this.knowledge.Object, this.api.Object)
                .InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
    }
}
