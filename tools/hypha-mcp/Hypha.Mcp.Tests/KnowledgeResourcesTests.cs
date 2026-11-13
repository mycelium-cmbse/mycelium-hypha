// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeResourcesTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System.IO;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;
    using Hypha.Mcp.Resources;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;

    using Moq;

    [TestFixture]
    public class KnowledgeResourcesTests
    {
        private const string Tag = "2026-05";

        private static readonly DirectoryInfo OutputRoot = new(Path.Combine(Path.GetTempPath(), "hypha-resources"));

        private Mock<IKnowledgeQuery> query = null!;
        private Mock<IKnowledgeLookup> lookup = null!;
        private Mock<IDocumentCache> documents = null!;
        private KnowledgeResources resources = null!;

        [SetUp]
        public void SetUp()
        {
            this.query = new Mock<IKnowledgeQuery>();
            this.lookup = new Mock<IKnowledgeLookup>();
            this.documents = new Mock<IDocumentCache>();

            var layout = new Mock<IKnowledgeLayout>();
            layout.Setup(l => l.OutputRoot).Returns(OutputRoot);

            this.resources = new KnowledgeResources(this.query.Object, this.lookup.Object, layout.Object, this.documents.Object);
        }

        [TestCase("metamodel", "PartUsage", "knowledge/2026-05/metamodel/elements/PartUsage.md")]
        [TestCase("kerml", "7.4.2", "knowledge/2026-05/spec/kerml/07.04.02-feature.md")]
        public void A_metaclass_or_clause_is_its_whole_page_as_markdown(string kind, string name, string page)
        {
            var address = new HyphaAddress(Tag, kind, name);
            this.query
                .Setup(q => q.Resolve(address))
                .Returns(new QueryResult<ResolvedAddress>(new ResolvedAddress(address, "title", page), new Grounding("MODEL", Tag, page)));
            this.documents
                .Setup(d => d.ReadText(It.Is<FileInfo>(file => file.FullName == Path.GetFullPath(Path.Combine(OutputRoot.FullName, page)))))
                .Returns("# the page");

            var contents = (TextResourceContents)this.resources.Page(Tag, kind, name);

            Assert.Multiple(() =>
            {
                Assert.That(contents.Text, Is.EqualTo("# the page"));
                Assert.That(contents.MimeType, Is.EqualTo("text/markdown"));
                Assert.That(contents.Uri, Is.EqualTo($"hypha://{Tag}/{kind}/{name}"));
            });
        }

        [Test]
        public void A_library_declaration_is_the_lookup_page_from_the_declaration_on()
        {
            this.Declares("ISQBase::mass");
            this.lookup
                .Setup(l => l.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ISQBase::mass", Tag, null, LookupRequest.DefaultMaxLines)))
                .Returns(Found("ISQBase::mass", "attribute mass;"));

            var contents = (TextResourceContents)this.resources.Page(Tag, "model-library", "ISQBase%3A%3Amass");

            Assert.Multiple(() =>
            {
                Assert.That(contents.Text, Is.EqualTo("attribute mass;"));
                Assert.That(contents.Uri, Is.EqualTo("hypha://2026-05/model-library/ISQBase%3A%3Amass"));
            });
        }

        [TestCase("isqbase%3A%3AMASS")]
        [TestCase("mass")]
        [TestCase("PartDefinition")]
        public void A_library_name_the_release_does_not_declare_exactly_is_refused_rather_than_matched_loosely(string name)
        {
            this.query
                .Setup(q => q.Resolve(It.IsAny<HyphaAddress>()))
                .Returns(new QueryResult<ResolvedAddress>(null, new Grounding("DERIVED", Tag, "index.json")));

            Assert.That(
                () => this.resources.Page(Tag, "model-library", name),
                Throws.TypeOf<McpException>().With.Message.Contains("exact").And.Message.Contains("hypha_search"));

            this.lookup.VerifyNoOtherCalls();
        }

        [Test]
        public void A_lookup_that_lands_on_another_declaration_is_not_served_under_this_address()
        {
            this.Declares("ISQBase::mass");
            this.lookup
                .Setup(l => l.Lookup(It.IsAny<LookupRequest>()))
                .Returns(Found("ISQSpaceTime::mass", "attribute mass;"));

            Assert.That(
                () => this.resources.Page(Tag, "model-library", "ISQBase::mass"),
                Throws.TypeOf<McpException>().With.Message.Contains("hypha generate model-library"));
        }

        [Test]
        public void An_address_that_names_nothing_is_an_error_pointing_at_the_search()
        {
            this.query
                .Setup(q => q.Resolve(It.IsAny<HyphaAddress>()))
                .Returns(new QueryResult<ResolvedAddress>(null, new Grounding("MODEL", Tag, "index.json")));

            Assert.That(
                () => this.resources.Page(Tag, "metamodel", "NoSuchThing"),
                Throws.TypeOf<McpException>().With.Message.Contains("NoSuchThing").And.Message.Contains("hypha_search"));
        }

        [Test]
        public void A_clause_whose_text_was_never_generated_says_how_to_generate_it()
        {
            this.query
                .Setup(q => q.Resolve(It.IsAny<HyphaAddress>()))
                .Returns(new QueryResult<ResolvedAddress>(null, new Grounding("NORMATIVE", Tag, "index.json"), "run 'hypha generate spec --tag 2026-05'"));

            Assert.That(
                () => this.resources.Page(Tag, "sysml2", "8.3.6"),
                Throws.TypeOf<McpException>().With.Message.Contains("hypha generate spec"));
        }

        [Test]
        public void A_kind_that_is_not_one_is_an_error()
        {
            Assert.That(() => this.resources.Page(Tag, "uml", "Class"), Throws.TypeOf<McpException>());
        }

        [Test]
        public void A_release_that_is_not_installed_is_an_error_with_the_reason()
        {
            this.query
                .Setup(q => q.Resolve(It.IsAny<HyphaAddress>()))
                .Throws(new KnowledgeLookupException("Release 2099-01 is not installed."));

            Assert.That(
                () => this.resources.Page("2099-01", "metamodel", "PartUsage"),
                Throws.TypeOf<McpException>().With.Message.EqualTo("Release 2099-01 is not installed."));
        }

        private static LookupResult Found(string name, string content) =>
            new() { Release = Tag, Kind = KnowledgeKind.ModelLibrary, Status = LookupStatus.Found, Name = name, Content = content };

        private void Declares(string name)
        {
            var address = new HyphaAddress(Tag, "model-library", name);
            this.query
                .Setup(q => q.Resolve(address))
                .Returns(new QueryResult<ResolvedAddress>(
                    new ResolvedAddress(address, "attribute", "knowledge/2026-05/model-library/ISQBase.md"), new Grounding("DERIVED", Tag, "index.json")));
        }
    }
}
