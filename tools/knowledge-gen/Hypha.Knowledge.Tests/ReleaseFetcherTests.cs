// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseFetcherTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for fetching a release, against a stubbed transport - no live calls.
    /// </summary>
    [TestFixture]
    public class ReleaseFetcherTests
    {
        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"fetch-{Guid.NewGuid():N}"));
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Lists_blob_paths_only()
        {
            using var client = new HttpClient(StubHandler.WithTree(
                """
                {"truncated":false,"tree":[
                    {"type":"blob","path":"bnf/SysML-textual-bnf.kebnf"},
                    {"type":"blob","path":"kerml/Kernel/Connections.kerml"},
                    {"type":"tree","path":"bnf"}]}
                """));

            var paths = await new ReleaseFetcher(client).ListTreeAsync(Upstream.ReleaseRepository, "2026-05");

            Assert.That(paths, Is.EqualTo(new[]
            {
                "bnf/SysML-textual-bnf.kebnf", "kerml/Kernel/Connections.kerml",
            }));
        }

        [Test]
        public void Refuses_a_truncated_listing()
        {
            // A truncated tree would silently drop inputs and still look like a complete release.
            using var client = new HttpClient(StubHandler.WithTree("""{"truncated":true,"tree":[]}"""));
            var fetcher = new ReleaseFetcher(client);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => fetcher.ListTreeAsync(Upstream.ReleaseRepository, "2026-05"));

            Assert.That(exception!.Message, Does.Contain("truncated"));
        }

        [Test]
        public async Task Downloads_a_file_to_its_destination()
        {
            using var client = new HttpClient(StubHandler.WithContent("content"));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "nested", "x.kebnf"));

            await new ReleaseFetcher(client).DownloadAsync("owner/repo", "2026-05", "bnf/x.kebnf", destination);

            Assert.That(await File.ReadAllTextAsync(destination.FullName), Is.EqualTo("content"));
        }

        [Test]
        public async Task Skip_existing_makes_a_rerun_resume()
        {
            var handler = StubHandler.WithContent("fresh");
            using var client = new HttpClient(handler);

            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "x.kebnf"));
            await File.WriteAllTextAsync(destination.FullName, "already here");

            await new ReleaseFetcher(client).DownloadAsync(
                "owner/repo", "2026-05", "bnf/x.kebnf", destination, skipExisting: true);

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("already here"));
                Assert.That(handler.Requests, Is.Empty, "an existing file must not be re-downloaded");
            });
        }

        [Test]
        public async Task Skip_existing_still_fetches_a_zero_byte_file()
        {
            // A zero-byte file is the signature of an interrupted write, not a completed download.
            using var client = new HttpClient(StubHandler.WithContent("real"));

            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "x.kebnf"));
            await File.WriteAllBytesAsync(destination.FullName, []);

            await new ReleaseFetcher(client).DownloadAsync(
                "owner/repo", "2026-05", "bnf/x.kebnf", destination, skipExisting: true);

            Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("real"));
        }

        [Test]
        public async Task Fetches_both_metamodel_files_and_never_the_shared_primitives()
        {
            using var client = new HttpClient(StubHandler.WithContent("xmi"));

            var written = await new ReleaseFetcher(client).FetchMetamodelAsync("2026-05", this.workspace);

            Assert.Multiple(() =>
            {
                Assert.That(
                    written.Select(file => file.Name).OrderBy(name => name, StringComparer.Ordinal),
                    Is.EqualTo(new[] { "KerML_only_xmi.uml", "SysML_only_xmi.uml" }));
                Assert.That(written.Select(file => file.Name), Does.Not.Contain(ReleaseInputs.SharedXmi));
                Assert.That(written.All(file => file.Directory!.Name == "xmi"), Is.True);
            });
        }

        [Test]
        public async Task Fetches_textual_sources_preserving_the_upstream_layout()
        {
            var handler = StubHandler.WithTree(
                """
                {"truncated":false,"tree":[
                    {"type":"blob","path":"bnf/SysML-textual-bnf.kebnf"},
                    {"type":"blob","path":"kerml/Kernel/Connections.kerml"}]}
                """);

            using var client = new HttpClient(handler);
            var written = await new ReleaseFetcher(client).FetchTextualAsync("2026-05", this.workspace);

            var root = Path.Combine(this.workspace.FullName, "2026-05", "textual");
            var relative = written
                .Select(file => Path.GetRelativePath(root, file.FullName).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal);

            Assert.That(relative, Is.EqualTo(new[]
            {
                "bnf/SysML-textual-bnf.kebnf", "kerml/Kernel/Connections.kerml",
            }));
        }

        [Test]
        public async Task Writes_the_specifications_only_into_the_git_ignored_folder()
        {
            using var client = new HttpClient(StubHandler.WithContent("%PDF"));

            var written = await new ReleaseFetcher(client).FetchSpecificationsAsync("2026-05", this.workspace);

            Assert.Multiple(() =>
            {
                Assert.That(written, Has.Count.EqualTo(ReleaseInputs.SpecificationPdfs.Count));
                Assert.That(written.All(file => file.Directory!.Name == "specs"), Is.True);
            });
        }

        [Test]
        public async Task Downloads_concurrently_without_exceeding_the_limit()
        {
            var handler = StubHandler.WithContent("xmi");
            using var client = new HttpClient(handler);

            await new ReleaseFetcher(client, maxConcurrency: 2)
                .FetchSpecificationsAsync("2026-05", this.workspace);

            Assert.That(handler.PeakConcurrency, Is.LessThanOrEqualTo(2));
        }

        [Test]
        public void Rejects_a_concurrency_below_one()
        {
            using var client = new HttpClient(StubHandler.WithContent("x"));

            Assert.Throws<ArgumentOutOfRangeException>(() => new ReleaseFetcher(client, maxConcurrency: 0));
        }

        [Test]
        public async Task Progress_is_reported_once_per_target_and_reaches_the_full_total()
        {
            using var client = new HttpClient(StubHandler.WithContent("%PDF"));

            // A plain thread-safe collector, not Progress<T>: with no SynchronizationContext (as in a
            // console app / this test), Progress<T> still marshals through the thread pool, so multiple
            // callbacks from the concurrent downloads below could run at once - collecting into
            // something itself thread-safe is what makes the assertions below reliable.
            var reports = new ConcurrentBag<FetchProgress>();
            var progress = new SynchronousProgress<FetchProgress>(reports.Add);

            var written = await new ReleaseFetcher(client, maxConcurrency: 3)
                .FetchSpecificationsAsync("2026-05", this.workspace, progress: progress);

            Assert.Multiple(() =>
            {
                Assert.That(reports, Has.Count.EqualTo(written.Count));
                Assert.That(reports.Select(report => report.Total), Is.All.EqualTo(written.Count));
                Assert.That(reports.Select(report => report.Done), Is.EquivalentTo(
                    Enumerable.Range(1, written.Count)));
                Assert.That(reports.Select(report => report.Kind), Is.All.EqualTo("specifications"));
            });
        }

        [Test]
        public async Task Progress_still_counts_a_skipped_file_as_one_completed()
        {
            // Pre-populate both metamodel destinations non-empty, so skipExisting resumes both without
            // a single HTTP request - progress must still see two files "done", not zero.
            var written = await new ReleaseFetcher(new HttpClient(StubHandler.WithContent("xmi")))
                .FetchMetamodelAsync("2026-05", this.workspace);
            foreach (var file in written)
            {
                Assert.That(await File.ReadAllTextAsync(file.FullName), Is.EqualTo("xmi"));
            }

            var handler = StubHandler.WithContent("xmi");
            var reports = new ConcurrentBag<FetchProgress>();

            await new ReleaseFetcher(new HttpClient(handler)).FetchMetamodelAsync(
                "2026-05", this.workspace, skipExisting: true,
                progress: new SynchronousProgress<FetchProgress>(reports.Add));

            Assert.Multiple(() =>
            {
                Assert.That(handler.Requests, Is.Empty, "nothing should have been re-downloaded");
                Assert.That(reports, Has.Count.EqualTo(2), "both files count as done, skipped or not");
                Assert.That(reports.Select(report => report.Done), Is.EquivalentTo(new[] { 1, 2 }));
            });
        }

        /// <summary>
        /// An <see cref="IProgress{T}"/> that invokes its callback synchronously and directly, rather
        /// than through <see cref="Progress{T}"/>'s <see cref="System.Threading.SynchronizationContext"/>
        /// marshalling - simpler to reason about in a test that only cares what was reported, not when.
        /// </summary>
        private sealed class SynchronousProgress<T> : IProgress<T>
        {
            private readonly Action<T> callback;

            public SynchronousProgress(Action<T> callback) => this.callback = callback;

            public void Report(T value) => this.callback(value);
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly string body;
            private readonly Lock peak = new();
            private int active;

            private StubHandler(string body) => this.body = body;

            public ConcurrentBag<string> Requests { get; } = [];

            public int PeakConcurrency { get; private set; }

            public static StubHandler WithTree(string json) => new(json);

            public static StubHandler WithContent(string content) => new(content);

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Add(request.RequestUri!.ToString());

                var current = Interlocked.Increment(ref this.active);
                lock (this.peak)
                {
                    this.PeakConcurrency = Math.Max(this.PeakConcurrency, current);
                }

                try
                {
                    await Task.Delay(5, cancellationToken);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(this.body),
                    };
                }
                finally
                {
                    Interlocked.Decrement(ref this.active);
                }
            }
        }
    }
}
