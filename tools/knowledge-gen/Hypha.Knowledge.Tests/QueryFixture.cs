// ------------------------------------------------------------------------------------------------
// <copyright file="QueryFixture.cs" company="Starion Group S.A.">
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
    using System.IO;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.Query;
    using Hypha.Knowledge.Releases;
    using Hypha.Knowledge.TextualNotation;

    // A scratch knowledge folder with one release and one API version installed. The metamodel is the
    // committed golden metamodel.json and index.json of Hypha.Knowledge.Metamodel.Tests, so the queries
    // run against real generator output; the other documents are small, built through the same
    // builders and renderers the generators use, so they cannot drift from the real format.
    internal sealed class QueryFixture : IDisposable
    {
        public const string Tag = "2026-05";

        public const string ApiVersion = "2026-04";

        public QueryFixture()
        {
            this.Root = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-query-{Guid.NewGuid():N}"));
            this.Layout = new KnowledgeLayout(this.Root);
            this.ApiLayout = new ApiLayout(this.Layout);
            // One cache for both queries, as AddHyphaKnowledge registers them.
            this.Documents = new DocumentCache();
            var specifications = new SpecificationReader(this.Documents);
            this.Query = new KnowledgeQuery(this.Layout, this.ApiLayout, this.Documents, specifications, new MetamodelDiffer(), new CrossReferenceDiffer());
            this.Api = new ApiQuery(this.ApiLayout, this.Layout, this.Documents, specifications);

            KnowledgeLookupTests.Install(this.Layout, Tag);
            this.InstallApiVersion();

            var golden = GoldenMetamodel();
            Directory.CreateDirectory(this.Layout.Metamodel(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), this.Layout.MetamodelGraph(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "index.json"), this.Layout.MetamodelIndex(Tag).FullName);

            // The golden pages cover a subset of the metaclasses (Feature, Usage, Element, ...), not PartUsage.
            var elements = Directory.CreateDirectory(Path.Combine(this.Layout.Metamodel(Tag).FullName, "elements"));
            foreach (var page in new DirectoryInfo(Path.Combine(golden.FullName, "elements")).EnumerateFiles("*.md"))
            {
                page.CopyTo(Path.Combine(elements.FullName, page.Name));
            }

            CrossReferenceFile.Write(CrossReferences(), this.Layout.CrossReferences(Tag).FullName);

            Directory.CreateDirectory(this.Layout.TextualNotation(Tag).FullName);
            File.WriteAllText(this.Layout.Keywords(Tag).FullName, KeywordFile.Render(Keywords(this.Layout.MetamodelGraph(Tag))));
            Directory.CreateDirectory(this.Layout.Examples(Tag).FullName);
            File.WriteAllText(this.Layout.ExamplesIndex(Tag).FullName, ExampleFile.Render(Examples()));
            File.WriteAllText(Path.Combine(this.Layout.Examples(Tag).FullName, "sysml-src-examples-vehicle-example-vehicle.md"), "```sysml\npart vehicle;\n```\n");

            Directory.CreateDirectory(this.Layout.ModelLibrary(Tag).FullName);
            File.WriteAllText(this.Layout.ModelLibraryIndex(Tag).FullName, ModelLibraryFile.Render(ModelLibrary()));
            var packages = Directory.CreateDirectory(this.Layout.ModelLibraryPackages(Tag).FullName);
            File.WriteAllText(Path.Combine(packages.FullName, "ISQBase.md"), "```sysml\nattribute mass;\n```\n");
            File.WriteAllText(Path.Combine(packages.FullName, "ScalarValues.md"), "```kerml\ndatatype Real;\n```\n");

            // The fixture XMI has no Systems Modeling API classes, so the golden metamodel stands in for
            // the PIM: the PIM's metamodel.json is the same document type, written by the same generator.
            Directory.CreateDirectory(this.ApiLayout.Pim(ApiVersion).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), this.ApiLayout.PimGraph(ApiVersion).FullName);

            Directory.CreateDirectory(this.ApiLayout.Knowledge(ApiVersion).FullName);
            File.WriteAllText(this.ApiLayout.CrossReferences(ApiVersion).FullName, ApiJson.Render(ApiCrossReferences()));
            Directory.CreateDirectory(this.ApiLayout.OpenApiIndex(ApiVersion).DirectoryName!);
            File.WriteAllText(this.ApiLayout.OpenApiIndex(ApiVersion).FullName, ApiJson.Render(OpenApiIndex()));

            // The specification text spec-extract would write: an index and one page per clause, with the
            // same front matter. The clause numbers and titles match the cross-reference fixture's.
            WriteSpecification(
                this.Layout.SpecificationCatalog(Tag, "kerml"),
                "KerML",
                ("2", "Conformance", "29", true, "02-conformance.md"),
                ("7.4.2", "Feature", "45-47", true, "07.04.02-feature.md"),
                ("8.3.3.1.8", "Feature", "212", true, "08.03.03.01.08-feature.md"));
            WriteSpecification(
                this.Layout.SpecificationCatalog(Tag, "sysml2"),
                "SysML",
                ("1", "Scope", "1-2", false, "01-scope.md"),
                ("8.3.6.3", "PartUsage", "120-121", true, "08.03.06.03-partusage.md"));
            WriteSpecification(
                this.ApiLayout.SpecificationCatalog(ApiVersion),
                "Systems Modeling API and Services",
                ("7.1.4", "Commit", "30", true, "07.01.04-commit.md"));
        }

        public DocumentCache Documents { get; }

        // A query of its own, with a cache of its own, over another layout.
        public static KnowledgeQuery NewQuery(IKnowledgeLayout layout, IApiLayout apiLayout)
        {
            var documents = new DocumentCache();

            return new KnowledgeQuery(layout, apiLayout, documents, new SpecificationReader(documents), new MetamodelDiffer(), new CrossReferenceDiffer());
        }

        public static ApiQuery NewApi(IApiLayout apiLayout, IKnowledgeLayout layout)
        {
            var documents = new DocumentCache();

            return new ApiQuery(apiLayout, layout, documents, new SpecificationReader(documents));
        }

        public static string ClauseText(string document, string clause) =>
            $"# {clause}\n\nThe text of clause {clause} of {document}.\n";

        // Written as text in the shape spec-extract's render_index_json writes (json.dumps with indent=2),
        // not through the C# contract, so a reader that drifts from the Python output fails here.
        public static void WriteSpecification(
            FileInfo index,
            string document,
            params (string Clause, string Title, string Pages, bool Normative, string File)[] clauses)
        {
            Directory.CreateDirectory(index.DirectoryName!);

            var entries = new List<string>();

            foreach (var (clause, title, pages, normative, file) in clauses)
            {
                var flag = normative ? "true" : "false";

                entries.Add(
                    $"    \"{clause}\": {{\n      \"title\": \"{title}\",\n      \"pages\": \"{pages}\",\n"
                    + $"      \"normative\": {flag},\n      \"file\": \"{file}\"\n    }}");
                File.WriteAllText(
                    Path.Combine(index.DirectoryName!, file),
                    $"---\nclause: \"{clause}\"\ntitle: \"{title}\"\ndocument: \"{document}\"\nversion: \"1.0\"\n"
                    + $"pages: \"{pages}\"\nnormative: {flag}\n---\n\n"
                    + ClauseText(document, clause));
            }

            File.WriteAllText(
                index.FullName,
                $"{{\n  \"schemaVersion\": \"1.0.0\",\n  \"document\": \"{document}\",\n  \"version\": \"1.0\",\n"
                + $"  \"clauses\": {clauses.Length},\n  \"entries\": {{\n{string.Join(",\n", entries)}\n  }}\n}}\n");
        }

        public DirectoryInfo Root { get; }

        public KnowledgeLayout Layout { get; }

        public ApiLayout ApiLayout { get; }

        public KnowledgeQuery Query { get; }

        public ApiQuery Api { get; }

        public static DirectoryInfo GoldenMetamodel()
        {
            var root = Repository.Layout?.Root
                       ?? throw new InvalidOperationException("tests must run inside the repository checkout");

            return new DirectoryInfo(Path.Combine(
                root.FullName, "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel"));
        }

        public void Dispose()
        {
            this.Root.Refresh();

            if (this.Root.Exists)
            {
                this.Root.Delete(recursive: true);
            }
        }

        private static CrossReferenceDocument CrossReferences() =>
            new CrossReferenceBuilder().Build(
                CrossReferenceBuilderTests.Inputs(
                    productions: CrossReferenceBuilderTests.PerGrammar("sysml", "PartUsage", "PartUsage", "PartUsageDeclaration"),
                    examples: new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                    {
                        ["PartUsage"] = ["examples/Vehicle.md", "examples/Wheel.md"],
                    }) with
                {
                    GrammarClauses = CrossReferenceBuilderTests.PerGrammar("kerml", "Feature", "8.3.6.10"),
                    GrammarDocuments = new Dictionary<string, string>(StringComparer.Ordinal) { ["kerml"] = "kerml" },
                    GrammarFeatures = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>>>(
                        StringComparer.Ordinal)
                    {
                        ["kerml"] = new Dictionary<string, IReadOnlyList<FeatureLink>>(StringComparer.Ordinal)
                        {
                            ["Feature"] =
                            [
                                new FeatureLink("declaredName", "=", ["Identification", "FeatureDeclaration"]),
                                new FeatureLink("isComposite", "?=", ["FeaturePrefix"]),
                            ],
                            ["PartUsage"] = [new FeatureLink("declaredName", "=", ["Identification"])],
                        },
                    },
                });

        // Two small grammars shaped like the real ones. SysML calls KerML's OwnedExpression, so its table
        // holds what KerML's expressions write: 'and' and 'new' (reserved in SysML; 'new' contextual in
        // KerML), '+', and 'true' (which KerML's Invariant writes too, but SysML never reaches it).
        // ':>>' is written by both, 'part' and 'ref' (through a helper) by SysML only, 'feature' by KerML
        // only, and SysML reserves 'variation' but writes it nowhere.
        private static KeywordDocument Keywords(FileInfo graph)
        {
            const string kerml =
                """
                RESERVED_KEYWORD =
                    'and' | 'feature' | 'inv' | 'true'

                Feature =
                    'feature' FeatureDeclaration

                FeatureDeclaration =
                    ':>>'

                Invariant : Invariant =
                    'inv' 'true'

                OwnedExpression : Expression =
                    OperatorExpression

                OperatorExpression : Expression =
                    'and' | '+' | 'new' | 'true'
                """;
            const string sysml =
                """
                RESERVED_KEYWORD =
                    'and' | 'new' | 'part' | 'ref' | 'true' | 'variation'

                PartUsage =
                    UsagePrefix 'part' ':>>' ownedRelationship += OwnedExpression

                UsagePrefix : Usage =
                    ( isReference ?= 'ref' )?
                """;

            var parser = new GrammarParser();
            var indexer = new KeywordIndexer();
            var ancestors = new KnowledgeReader().ReadAncestors(graph);

            return new KeywordDocument(
                KeywordDocument.CurrentSchemaVersion,
                Tag,
                new SortedDictionary<string, KeywordGrammar>(StringComparer.Ordinal)
                {
                    ["kerml"] = indexer.Build("kerml", parser.Parse(kerml), parser.ReservedKeywords(kerml), ancestors),
                    ["sysml"] = indexer.Build(
                        "sysml",
                        parser.Parse(sysml),
                        parser.ReservedKeywords(sysml),
                        ancestors,
                        new SharedGrammar("kerml", parser.Parse(kerml))),
                });
        }

        // One worked example, named as the generator names it: the page by its slugged path, the model by
        // its own file name.
        private static ExampleDocument Examples() =>
            new(
                ExampleDocument.CurrentSchemaVersion,
                Tag,
                new Dictionary<string, ExampleIndexEntry>(StringComparer.Ordinal)
                {
                    ["sysml-src-examples-vehicle-example-vehicle"] = new("Vehicle", "SysML", "sysml/src/examples/Vehicle Example/Vehicle.sysml", "sysml-src-examples-vehicle-example-vehicle.md", ["PartUsage"]),
                });

        private static ModelLibraryDocument ModelLibrary() =>
            new(
                ModelLibraryDocument.CurrentSchemaVersion,
                Tag,
                new ModelLibraryCounts(2, 2),
                new Dictionary<string, ModelLibraryDeclarationEntry>(StringComparer.Ordinal)
                {
                    ["ISQBase::mass"] = new("attribute", "packages/ISQBase.md", "ISQBase.sysml"),
                    ["ScalarValues::Real"] = new("datatype", "packages/ScalarValues.md", "ScalarValues.kerml"),
                });

        // An object schema of the API with its properties, a union, and a schema that mirrors a metamodel
        // element, which the index lists without a page or properties.
        private static OpenApiIndexDocument OpenApiIndex() =>
            new(
                OpenApiIndexDocument.CurrentSchemaVersion,
                ApiVersion,
                "Systems Modeling API",
                "1.0",
                "3.1.0",
                new OpenApiIndexCounts(3, 2, 0),
                new Dictionary<string, OpenApiIndexSchema>(StringComparer.Ordinal)
                {
                    ["Commit"] = new(
                        "https://www.omg.org/spec/SystemsModelingAPI/20230201/Commit",
                        "api",
                        "object",
                        "response",
                        "schemas/Commit.md",
                        [
                            new OpenApiProperty("@id", "string (uuid)", Required: true, Nullable: false, RefersTo: null),
                            new OpenApiProperty("description", "string", Required: true, Nullable: true, RefersTo: null),
                            new OpenApiProperty("previousCommit", "Identified[]", Required: true, Nullable: false, RefersTo: "Commit"),
                        ],
                        null),
                    ["DataVersion.payload"] = new(
                        "https://www.omg.org/spec/SystemsModelingAPI/20230201/Data", "api", "union", "response", "schemas/Data.md", null, ["Element", "Relationship"]),
                    ["PartUsage"] = new(
                        "https://www.omg.org/spec/SysML/20230201/PartUsage", "metamodel", "object", "response", null, null, null),
                },
                []);

        private static ApiCrossReferenceDocument ApiCrossReferences() =>
            new(
                ApiCrossReferenceDocument.CurrentSchemaVersion,
                ApiVersion,
                new ApiCrossReferenceSources(Pim: true, OpenApi: true, Specification: true),
                new ApiCrossReferenceCounts(1, 1, 1, 1, 1),
                new Dictionary<string, ApiTypeEntry>(StringComparer.Ordinal)
                {
                    ["Commit"] = new(
                        "class",
                        "Commit",
                        [new ApiClauseEdge("7.1.4", ApiCrossReferenceDocument.TitleMethod)],
                        new ApiSchemaEdge("Commit", ApiCrossReferenceDocument.SameNameMethod, null, null),
                        [new ApiMemberEntry("created", "1", Derived: false, InheritedFrom: null, Response: true, Request: false)],
                        PsmOnly: null),
                },
                new Dictionary<string, ApiOperationEntry>(StringComparer.Ordinal)
                {
                    ["ProjectService.createProject"] = new(
                        "ProjectService",
                        [new ApiClauseEdge("7.2.1", ApiCrossReferenceDocument.SignatureMethod)],
                        [new ApiEndpointEdge("POST", "/projects", "postProject")],
                        MappingClause: "8.1"),
                },
                []);

        private void InstallApiVersion() =>
            ApiVersionManifestFile.Write(
                ApiVersionManifest.Build(
                    ApiVersion,
                    [
                        new InstalledApiVersion(
                            ApiVersion,
                            new UpstreamReference(ApiUpstream.ServicesRepository, "abc"),
                            new UpstreamReference(Upstream.ReleaseRepository, "def"),
                            new UpstreamReference(Upstream.PilotRepository, "ghi"),
                            ApiUpstream.PimDocument),
                    ]),
                this.ApiLayout.VersionManifest.FullName);
    }
}
