// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;
    using Hypha.Knowledge.Specification;

    [TestFixture]
    public class SpecificationSearchTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void A_phrase_is_found_whatever_its_case_and_wherever_its_lines_break()
        {
            this.Page("kerml", "07.04.02-feature.md", "A Feature\nis typed BY\n  a Type.");

            var result = this.fixture.Query.SearchSpecification("is typed by");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value!.Select(match => match.Address), Is.EqualTo(new[] { new HyphaAddress(QueryFixture.Tag, "kerml", "7.4.2") }));
                Assert.That(result.Value![0].Title, Is.EqualTo("Feature"));
                Assert.That(result.Value![0].Occurrences, Is.EqualTo(1));
                Assert.That(result.Value![0].Excerpt, Is.EqualTo("A Feature is typed BY a Type."));
                Assert.That(result.Grounding.Tier, Is.EqualTo(Provenance.Derived));
                Assert.That(result.Grounding.Release, Is.EqualTo(QueryFixture.Tag));
            });
        }

        [Test]
        public void The_clauses_that_say_it_most_come_first_then_by_specification_and_clause_number()
        {
            this.Page("kerml", "02-conformance.md", "A redefinition here.");
            this.Page("kerml", "07.04.02-feature.md", "Redefinition, and redefinition again.");
            this.Page("kerml", "08.03.03.01.08-feature.md", "A redefinition there.");
            this.Page("sysml2", "08.03.06.03-partusage.md", "Redefinition of a part.");

            var result = this.fixture.Query.SearchSpecification("redefinition");

            Assert.That(
                result.Value!.Select(match => $"{match.Address.Kind} {match.Address.Name} {match.Occurrences}"),
                Is.EqualTo(new[] { "kerml 7.4.2 2", "kerml 2 1", "kerml 8.3.3.1.8 1", "sysml2 8.3.6.3 1" }));
        }

        [Test]
        public void One_specification_and_a_limit_narrow_the_search()
        {
            this.Page("kerml", "02-conformance.md", "conformance");
            this.Page("kerml", "07.04.02-feature.md", "conformance conformance");
            this.Page("sysml2", "01-scope.md", "conformance");

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.SearchSpecification("conformance", document: "sysml2").Value!.Select(match => match.Address.Name),
                    Is.EqualTo(new[] { "1" }));
                Assert.That(
                    this.fixture.Query.SearchSpecification("conformance", limit: 1).Value!.Select(match => match.Address.Name),
                    Is.EqualTo(new[] { "7.4.2" }));
            });
        }

        [Test]
        public void The_excerpt_is_the_text_around_the_first_occurrence_in_whole_words()
        {
            var before = string.Join(' ', Enumerable.Repeat("before", 60));
            var after = string.Join(' ', Enumerable.Repeat("after", 60));
            this.Page("kerml", "07.04.02-feature.md", $"{before} the needle {after} needle");

            var match = this.fixture.Query.SearchSpecification("needle", document: "kerml").Value!.Single();

            Assert.Multiple(() =>
            {
                Assert.That(match.Occurrences, Is.EqualTo(2));
                Assert.That(match.Excerpt, Does.StartWith("...before ").And.EndWith(" after..."));
                Assert.That(match.Excerpt, Does.Contain("the needle after"));
                Assert.That(match.Excerpt.Length, Is.LessThan(300));
            });
        }

        [Test]
        public void Text_no_clause_contains_finds_nothing()
        {
            var result = this.fixture.Query.SearchSpecification("no clause says this");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Is.Empty);
                Assert.That(result.Available, Is.True);
            });
        }

        [Test]
        public void Without_any_specification_text_the_answer_says_how_to_generate_it()
        {
            Directory.Delete(this.Catalog("kerml").DirectoryName!, recursive: true);
            Directory.Delete(this.Catalog("sysml2").DirectoryName!, recursive: true);

            var result = this.fixture.Query.SearchSpecification("feature");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Unavailable, Does.Contain("hypha generate spec"));
            });
        }

        [Test]
        public void With_one_specification_generated_that_one_is_searched()
        {
            Directory.Delete(this.Catalog("kerml").DirectoryName!, recursive: true);
            this.Page("sysml2", "08.03.06.03-partusage.md", "A part usage.");

            var result = this.fixture.Query.SearchSpecification("part usage");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value!.Select(match => match.Address.Name), Is.EqualTo(new[] { "8.3.6.3" }));
            });
        }

        [Test]
        public void Clauses_that_say_it_as_often_are_in_clause_number_order_not_in_text_order()
        {
            QueryFixture.WriteSpecification(
                this.Catalog("sysml2"),
                "SysML",
                ("10.1", "Ten", "1", true, "10.01-ten.md"),
                ("9.2", "Nine two", "1", true, "09.02-nine-two.md"),
                ("9.10", "Nine ten", "1", true, "09.10-nine-ten.md"));
            this.Page("sysml2", "10.01-ten.md", "needle");
            this.Page("sysml2", "09.02-nine-two.md", "needle");
            this.Page("sysml2", "09.10-nine-ten.md", "needle");

            var result = this.fixture.Query.SearchSpecification("needle", document: "sysml2");

            Assert.That(result.Value!.Select(match => match.Address.Name), Is.EqualTo(new[] { "9.2", "9.10", "10.1" }));
        }

        [Test]
        public void A_phrase_typed_with_extra_white_space_is_found_as_if_typed_with_single_spaces()
        {
            this.Page("kerml", "07.04.02-feature.md", "A Feature is typed by a Type.");

            var result = this.fixture.Query.SearchSpecification("  is \t typed\n by  ");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.Select(match => match.Address.Name), Is.EqualTo(new[] { "7.4.2" }));
                Assert.That(result.Value![0].Excerpt, Is.EqualTo("A Feature is typed by a Type."));
            });
        }

        [Test]
        public void A_page_front_matter_is_not_searched()
        {
            // Page writes front matter holding 'clause: "x"', which no clause text contains.
            this.Page("kerml", "07.04.02-feature.md", "A Feature.");

            Assert.That(this.fixture.Query.SearchSpecification("clause: \"x\"").Value, Is.Empty);
        }

        [Test]
        public void Pages_missing_from_the_index_fail_the_search_naming_the_first_in_clause_order()
        {
            File.Delete(Path.Combine(this.Catalog("kerml").DirectoryName!, "07.04.02-feature.md"));
            File.Delete(Path.Combine(this.Catalog("kerml").DirectoryName!, "08.03.03.01.08-feature.md"));

            Assert.That(
                () => this.fixture.Query.SearchSpecification("feature"),
                Throws.TypeOf<KnowledgeLookupException>()
                    .With.Message.Contains("07.04.02-feature.md")
                    .And.Message.Not.Contains("08.03.03.01.08-feature.md"));
        }

        [Test]
        public void Many_clauses_are_all_searched_and_ranked_the_same_way_every_time()
        {
            const int Count = 300;
            var catalog = this.Catalog("sysml2");
            var clauses = Enumerable.Range(1, Count)
                .Select(at => ($"9.{at}", $"Clause {at}", "1", true, $"09.{at:D3}-clause.md"))
                .ToArray();
            QueryFixture.WriteSpecification(catalog, "SysML", clauses);

            foreach (var (_, _, _, _, file) in clauses)
            {
                var at = int.Parse(file[3..6], System.Globalization.CultureInfo.InvariantCulture);
                this.Page("sysml2", file, string.Join(' ', Enumerable.Repeat("needle", Occurrences(at))));
            }

            // The most occurrences first, then by clause number: the ranking the pages were written for.
            var expected = Enumerable.Range(1, Count)
                .OrderByDescending(Occurrences)
                .ThenBy(at => at)
                .Take(KnowledgeQuery.MaxSearchHits)
                .Select(at => $"9.{at} {Occurrences(at)}");

            var first = this.fixture.Query.SearchSpecification("needle", document: "sysml2", limit: KnowledgeQuery.MaxSearchHits).Value!
                .Select(match => $"{match.Address.Name} {match.Occurrences}")
                .ToList();
            var again = QueryFixture.NewQuery(this.fixture.Layout, this.fixture.ApiLayout)
                .SearchSpecification("needle", document: "sysml2", limit: KnowledgeQuery.MaxSearchHits).Value!
                .Select(match => $"{match.Address.Name} {match.Occurrences}");

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.EqualTo(expected));
                Assert.That(again, Is.EqualTo(first));
            });

            static int Occurrences(int at) => at % 7 + 1;
        }

        [Test]
        public void A_phrase_only_the_last_of_many_clauses_holds_is_found()
        {
            const int Count = 300;
            var clauses = this.ManyClauses(Count, "haystack");
            this.Page("sysml2", clauses[^1].File, "haystack and the needle");

            var result = this.fixture.Query.SearchSpecification("needle", document: "sysml2");

            Assert.That(result.Value!.Select(match => match.Address.Name), Is.EqualTo(new[] { $"9.{Count}" }));
        }

        [Test]
        public void With_many_pages_missing_the_one_named_is_the_first_missing_in_the_index()
        {
            // The missing pages start a third of the way in, so that a reader that started elsewhere in
            // the index is likely to fail first; throwing whichever failure came first would then name it.
            var clauses = this.ManyClauses(300, "needle");

            foreach (var (_, _, _, _, file) in clauses.Skip(100))
            {
                File.Delete(Path.Combine(this.Catalog("sysml2").DirectoryName!, file));
            }

            Assert.That(
                () => this.fixture.Query.SearchSpecification("needle", document: "sysml2"),
                Throws.TypeOf<KnowledgeLookupException>()
                    .With.Message.Contains(clauses[100].File)
                    .And.Message.Not.Contains(clauses[101].File)
                    .And.Message.Not.Contains(clauses[^1].File));
        }

        [Test]
        public void An_index_without_entries_is_searched_and_finds_nothing()
        {
            QueryFixture.WriteSpecification(this.Catalog("sysml2"), "SysML");

            var result = this.fixture.Query.SearchSpecification("needle", document: "sysml2");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value, Is.Empty);
            });
        }

        [Test]
        public void Clauses_are_read_several_at_a_time_but_no_more_than_eight_and_each_once()
        {
            const int Count = 200;
            var clauses = this.ManyClauses(Count, "needle");
            var documents = new DocumentCache();
            var reader = new ConcurrencyReader(new SpecificationReader(documents));
            var query = new KnowledgeQuery(
                this.fixture.Layout, this.fixture.ApiLayout, documents, reader, new MetamodelDiffer(), new CrossReferenceDiffer());

            var result = query.SearchSpecification("needle", document: "sysml2", limit: KnowledgeQuery.MaxSearchHits);

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Has.Count.EqualTo(Count));
                Assert.That(reader.MostAtOnce, Is.InRange(2, 8));
                Assert.That(reader.Reads.Keys, Is.EquivalentTo(clauses.Select(clause => clause.Clause)));
                Assert.That(reader.Reads.Values, Is.All.EqualTo(1), "each clause is read once");
            });
        }

        [Test]
        public void The_index_is_read_once_per_specification_however_many_clauses_are_searched()
        {
            const int Count = 50;
            var catalog = this.Catalog("sysml2");
            var clauses = Enumerable.Range(1, Count)
                .Select(at => ($"9.{at}", $"Clause {at}", "1", true, $"09.{at:D3}-clause.md"))
                .ToArray();
            QueryFixture.WriteSpecification(catalog, "SysML", clauses);

            foreach (var (_, _, _, _, file) in clauses)
            {
                this.Page("sysml2", file, "needle");
            }

            var documents = new DocumentCache();
            var reader = new CountingReader(new SpecificationReader(documents));
            var query = new KnowledgeQuery(
                this.fixture.Layout, this.fixture.ApiLayout, documents, reader, new MetamodelDiffer(), new CrossReferenceDiffer());

            var result = query.SearchSpecification("needle", document: "sysml2", limit: KnowledgeQuery.MaxSearchHits);

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Has.Count.EqualTo(Count));
                Assert.That(reader.IndexReads, Is.EqualTo(1));
                Assert.That(reader.ClausesReadWithTheirIndex, Is.Zero, "every clause is read from the index the search read");
            });
        }

        [TestCase(null, null, 20)]
        [TestCase("  ", null, 20)]
        [TestCase("feature", "uml", 20)]
        [TestCase("feature", null, 0)]
        [TestCase("feature", null, KnowledgeQuery.MaxSearchHits + 1)]
        public void Arguments_that_cannot_be_searched_are_refused(string? text, string? document, int limit)
        {
            Assert.That(() => this.fixture.Query.SearchSpecification(text!, document, limit), Throws.InstanceOf<ArgumentException>());
        }

        private FileInfo Catalog(string document) => this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, document);

        private void Page(string document, string file, string text) =>
            File.WriteAllText(Path.Combine(this.Catalog(document).DirectoryName!, file), $"---\nclause: \"x\"\n---\n\n{text}\n");

        // A SysML index of clauses 9.1 to 9.<count>, in that order, each page holding the text.
        private (string Clause, string Title, string Pages, bool Normative, string File)[] ManyClauses(int count, string text)
        {
            var clauses = Enumerable.Range(1, count)
                .Select(at => ($"9.{at}", $"Clause {at}", "1", true, $"09.{at:D3}-clause.md"))
                .ToArray();
            QueryFixture.WriteSpecification(this.Catalog("sysml2"), "SysML", clauses);

            foreach (var (_, _, _, _, file) in clauses)
            {
                this.Page("sysml2", file, text);
            }

            return clauses;
        }

        // The real reader, recording how many clauses are read at once and how often each is read. The
        // first read waits for a second to start, so reads made one at a time show a peak of one rather
        // than one read passing by luck before the next began; each read then lingers a little, so that
        // a search without a cap would show more than eight.
        private sealed class ConcurrencyReader : ISpecificationReader
        {
            private static readonly TimeSpan SecondReadWait = TimeSpan.FromSeconds(5);

            private readonly ISpecificationReader inner;

            private int inFlight;

            private int mostAtOnce;

            private int waited;

            public ConcurrencyReader(ISpecificationReader inner) => this.inner = inner;

            public int MostAtOnce => Volatile.Read(ref this.mostAtOnce);

            public ConcurrentDictionary<string, int> Reads { get; } = new(StringComparer.Ordinal);

            public SpecificationIndex? ReadIndex(FileInfo index, string regenerate) => this.inner.ReadIndex(index, regenerate);

            public QueryResult<SpecificationClause> Read(
                FileInfo index,
                string document,
                string clause,
                string version,
                string owner,
                string regenerate,
                string unavailable,
                Func<FileInfo, Grounding> ground) =>
                this.inner.Read(index, document, clause, version, owner, regenerate, unavailable, ground);

            public QueryResult<SpecificationClause> Read(
                FileInfo index,
                SpecificationIndex specification,
                string document,
                string clause,
                string version,
                string owner,
                string regenerate,
                Func<FileInfo, Grounding> ground)
            {
                this.Reads.AddOrUpdate(clause, 1, (_, reads) => reads + 1);
                var now = Interlocked.Increment(ref this.inFlight);

                try
                {
                    var peak = Volatile.Read(ref this.mostAtOnce);
                    while (now > peak)
                    {
                        var seen = Interlocked.CompareExchange(ref this.mostAtOnce, now, peak);

                        if (seen == peak)
                        {
                            break;
                        }

                        peak = seen;
                    }

                    if (Interlocked.Exchange(ref this.waited, 1) == 0)
                    {
                        SpinWait.SpinUntil(() => this.MostAtOnce >= 2, SecondReadWait);
                    }

                    Thread.Sleep(5);

                    return this.inner.Read(index, specification, document, clause, version, owner, regenerate, ground);
                }
                finally
                {
                    Interlocked.Decrement(ref this.inFlight);
                }
            }
        }

        // The real reader, counting how often the index is read, by ReadIndex or by the Read that reads it.
        private sealed class CountingReader : ISpecificationReader
        {
            private readonly ISpecificationReader inner;

            private int indexReads;

            private int clausesReadWithTheirIndex;

            public CountingReader(ISpecificationReader inner) => this.inner = inner;

            public int IndexReads => Volatile.Read(ref this.indexReads);

            public int ClausesReadWithTheirIndex => Volatile.Read(ref this.clausesReadWithTheirIndex);

            public SpecificationIndex? ReadIndex(FileInfo index, string regenerate)
            {
                Interlocked.Increment(ref this.indexReads);

                return this.inner.ReadIndex(index, regenerate);
            }

            public QueryResult<SpecificationClause> Read(
                FileInfo index,
                string document,
                string clause,
                string version,
                string owner,
                string regenerate,
                string unavailable,
                Func<FileInfo, Grounding> ground)
            {
                Interlocked.Increment(ref this.clausesReadWithTheirIndex);

                return this.inner.Read(index, document, clause, version, owner, regenerate, unavailable, ground);
            }

            public QueryResult<SpecificationClause> Read(
                FileInfo index,
                SpecificationIndex specification,
                string document,
                string clause,
                string version,
                string owner,
                string regenerate,
                Func<FileInfo, Grounding> ground) =>
                this.inner.Read(index, specification, document, clause, version, owner, regenerate, ground);
        }
    }
}
