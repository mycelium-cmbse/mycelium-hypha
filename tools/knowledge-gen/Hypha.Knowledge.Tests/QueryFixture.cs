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
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.Query;
    using Hypha.Knowledge.Releases;

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
            this.Query = new KnowledgeQuery(this.Layout, this.ApiLayout);
            this.Api = new ApiQuery(this.ApiLayout, this.Layout);

            KnowledgeLookupTests.Install(this.Layout, Tag);
            this.InstallApiVersion();

            var golden = GoldenMetamodel();
            Directory.CreateDirectory(this.Layout.Metamodel(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), this.Layout.MetamodelGraph(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "index.json"), this.Layout.MetamodelIndex(Tag).FullName);

            CrossReferenceFile.Write(CrossReferences(), this.Layout.CrossReferences(Tag).FullName);

            Directory.CreateDirectory(this.Layout.ModelLibrary(Tag).FullName);
            File.WriteAllText(this.Layout.ModelLibraryIndex(Tag).FullName, ModelLibraryFile.Render(ModelLibrary()));

            // The fixture XMI has no Systems Modeling API classes, so the golden metamodel stands in for
            // the PIM: the PIM's metamodel.json is the same document type, written by the same generator.
            Directory.CreateDirectory(this.ApiLayout.Pim(ApiVersion).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), this.ApiLayout.PimGraph(ApiVersion).FullName);

            Directory.CreateDirectory(this.ApiLayout.Knowledge(ApiVersion).FullName);
            File.WriteAllText(this.ApiLayout.CrossReferences(ApiVersion).FullName, ApiJson.Render(ApiCrossReferences()));
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
