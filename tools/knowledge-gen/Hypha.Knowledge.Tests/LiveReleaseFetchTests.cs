// ------------------------------------------------------------------------------------------------
// <copyright file="LiveReleaseFetchTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Live fetches against the real upstreams. <b>Explicit</b> - never part of a normal run or CI.
    /// </summary>
    /// <remarks>
    /// The reason for the port was that the Python fetcher was reset by the host after roughly 190 of
    /// a release's 317 files. No stub can show that is fixed, so this exists to be run by hand:
    /// <code>
    /// dotnet test --filter "FullyQualifiedName~LiveReleaseFetchTests" -- NUnit.DefaultTestNamePattern=...
    /// </code>
    /// It writes to a temporary folder, never to <c>sources/</c>, so an interrupted run cannot leave
    /// the repository half-updated. Set <c>GITHUB_TOKEN</c> first: the anonymous API allows 60
    /// requests an hour.
    /// </remarks>
    [TestFixture]
    [Explicit("Hits the network; run by hand when changing the fetcher.")]
    [Category("Live")]
    public class LiveReleaseFetchTests
    {
        private const string Tag = "2026-05";

        private DirectoryInfo workspace = null!;
        private ServiceProvider provider = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-live-{Guid.NewGuid():N}"));

            this.provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            this.provider?.Dispose();

            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Fetches_a_whole_release_without_being_throttled()
        {
            var fetcher = this.provider.GetRequiredService<ReleaseFetcher>();

            var metamodel = await fetcher.FetchMetamodelAsync(Tag, this.workspace);
            var textual = await fetcher.FetchTextualAsync(Tag, this.workspace);

            await TestContext.Out.WriteLineAsync(
                $"{metamodel.Count} metamodel + {textual.Count} textual files");

            Assert.Multiple(() =>
            {
                Assert.That(metamodel, Has.Count.EqualTo(2));
                Assert.That(textual, Has.Count.GreaterThan(200), "a release has ~300 textual files");
                Assert.That(
                    textual,
                    Has.All.Matches<FileInfo>(file => file.Exists && file.Length > 0),
                    "a zero-byte file means the transfer was cut short");
            });
        }

        [Test]
        public async Task A_rerun_with_skip_existing_downloads_nothing_twice()
        {
            var fetcher = this.provider.GetRequiredService<ReleaseFetcher>();

            var first = await fetcher.FetchMetamodelAsync(Tag, this.workspace);
            var stamps = Array.ConvertAll(first.ToArray(), file => file.LastWriteTimeUtc);

            var second = await fetcher.FetchMetamodelAsync(Tag, this.workspace, skipExisting: true);

            Assert.That(Array.ConvertAll(second.ToArray(), file => file.LastWriteTimeUtc), Is.EqualTo(stamps));
        }
    }
}
