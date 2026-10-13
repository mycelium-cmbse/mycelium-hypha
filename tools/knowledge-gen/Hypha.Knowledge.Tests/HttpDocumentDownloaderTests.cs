// ------------------------------------------------------------------------------------------------
// <copyright file="HttpDocumentDownloaderTests.cs" company="Starion Group S.A.">
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
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;

    /// <summary>
    /// Tests for downloading a published document, against a stubbed transport - no live calls.
    /// </summary>
    [TestFixture]
    public class HttpDocumentDownloaderTests
    {
        private static readonly Uri Address = new("https://documents.example.invalid/spec/model.xmi");

        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"document-{Guid.NewGuid():N}"));
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
        public async Task Downloads_a_document_to_its_destination()
        {
            var handler = StubHandler.WithContent("content");
            using var client = new HttpClient(handler);
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));

            var written = await new HttpDocumentDownloader(client).DownloadAsync(Address, destination);

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("content"));
                Assert.That(written.FullName, Is.EqualTo(destination.FullName));
                Assert.That(written.Exists, Is.True, "the returned file reflects what is now on disk");
                Assert.That(handler.Requests, Is.EqualTo(new[] { Address.ToString() }));
            });
        }

        [Test]
        public async Task Creates_the_folder_the_document_lands_in()
        {
            // sources/api/<tag>/xmi/ does not exist before the first fetch.
            using var client = new HttpClient(StubHandler.WithContent("content"));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "api", "2026-04", "xmi", "model.xmi"));

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination);

            Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("content"));
        }

        [Test]
        public async Task Writes_the_bytes_it_was_served_unchanged()
        {
            // The document is hashed into the generated knowledge, so not a byte may be re-encoded.
            byte[] served = [0xEF, 0xBB, 0xBF, 0x3C, 0x0D, 0x0A, 0x00, 0xFF];
            using var client = new HttpClient(StubHandler.WithBytes(served));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination);

            Assert.That(File.ReadAllBytes(destination.FullName), Is.EqualTo(served));
        }

        [Test]
        public async Task Skip_existing_makes_a_rerun_resume()
        {
            var handler = StubHandler.WithContent("fresh");
            using var client = new HttpClient(handler);
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));
            await File.WriteAllTextAsync(destination.FullName, "already here");

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination, skipExisting: true);

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
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));
            await File.WriteAllBytesAsync(destination.FullName, []);

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination, skipExisting: true);

            Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("real"));
        }

        [Test]
        public async Task Skip_existing_still_fetches_a_file_that_is_not_there()
        {
            using var client = new HttpClient(StubHandler.WithContent("real"));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination, skipExisting: true);

            Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("real"));
        }

        [Test]
        public async Task An_existing_file_is_replaced_unless_told_to_skip()
        {
            using var client = new HttpClient(StubHandler.WithContent("fresh"));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));
            await File.WriteAllTextAsync(destination.FullName, "a much longer stale document");

            await new HttpDocumentDownloader(client).DownloadAsync(Address, destination);

            Assert.That(File.ReadAllText(destination.FullName), Is.EqualTo("fresh"));
        }

        [Test]
        public void A_failed_download_is_reported_and_leaves_no_file_behind()
        {
            // A document server answering 404 must not look like a document that was fetched.
            using var client = new HttpClient(StubHandler.WithStatus(HttpStatusCode.NotFound));
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));

            Assert.That(
                () => new HttpDocumentDownloader(client).DownloadAsync(Address, destination),
                Throws.InstanceOf<HttpRequestException>());

            Assert.That(File.Exists(destination.FullName), Is.False);
        }

        [Test]
        public void Missing_arguments_are_refused()
        {
            using var client = new HttpClient(StubHandler.WithContent("x"));
            var downloader = new HttpDocumentDownloader(client);
            var destination = new FileInfo(Path.Combine(this.workspace.FullName, "model.xmi"));

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => _ = new HttpDocumentDownloader(null!));
                Assert.That(() => downloader.DownloadAsync(null!, destination), Throws.ArgumentNullException);
                Assert.That(() => downloader.DownloadAsync(Address, null!), Throws.ArgumentNullException);
            });
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly byte[] body;
            private readonly HttpStatusCode status;

            private StubHandler(byte[] body, HttpStatusCode status)
            {
                this.body = body;
                this.status = status;
            }

            public ConcurrentQueue<string> Requests { get; } = [];

            public static StubHandler WithContent(string content) =>
                new(System.Text.Encoding.UTF8.GetBytes(content), HttpStatusCode.OK);

            public static StubHandler WithBytes(byte[] bytes) => new(bytes, HttpStatusCode.OK);

            public static StubHandler WithStatus(HttpStatusCode status) => new([], status);

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Enqueue(request.RequestUri!.ToString());

                return Task.FromResult(new HttpResponseMessage(this.status)
                {
                    Content = new ByteArrayContent(this.body),
                });
            }
        }
    }
}
