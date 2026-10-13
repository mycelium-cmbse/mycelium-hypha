// ------------------------------------------------------------------------------------------------
// <copyright file="ApiRemoveCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Tools.Commands.Api;

    using Moq;

    /// <summary>
    /// Suite of tests for <see cref="ApiRemoveCommand"/>.
    /// </summary>
    [TestFixture]
    public class ApiRemoveCommandTests
    {
        private RootCommand root = null!;
        private Mock<IApiRemover> remover = null!;
        private ApiRemoveCommand.Handler handler = null!;

        [SetUp]
        public void SetUp()
        {
            var api = new ApiCommand();
            api.Add(new ApiRemoveCommand());

            this.root = new RootCommand();
            this.root.Add(api);

            this.remover = new Mock<IApiRemover>();
            this.handler = new ApiRemoveCommand.Handler(this.remover.Object);
        }

        [Test]
        public async Task An_api_version_is_removed()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("api remove --tag 2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("Removed API version 2025-09."));
            });

            this.remover.Verify(
                mock => mock.RemoveAsync("2025-09", false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Force_is_passed_through()
        {
            await this.Invoke("api remove --tag 2025-09 --force");

            this.remover.Verify(
                mock => mock.RemoveAsync("2025-09", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task An_uninstalled_tag_is_a_usage_error()
        {
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-05", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("API version '2026-05' is not installed"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api remove --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("not installed"));
                Assert.That(console.Output, Does.Not.Contain("Removed API version"));
            });
        }

        [Test]
        public async Task The_default_version_is_a_usage_error()
        {
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-04", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "'2026-04' is the default API version - run 'hypha api use --tag <other>' before removing it"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api remove --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("hypha api use"));
            });
        }

        [Test]
        public async Task A_message_with_markup_characters_is_printed_rather_than_interpreted()
        {
            // The refusal names a placeholder in angle brackets and could as easily carry square ones,
            // which the console would otherwise try to read as styling.
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-04", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("refused [because] of the [/] default"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api remove --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("refused [because] of the [/] default"));
            });
        }

        [Test]
        public void A_failure_that_is_not_a_refusal_reaches_the_caller()
        {
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-04", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("the folder is in use"));

            Assert.That(() => this.Invoke("api remove --tag 2026-04"), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void The_tag_is_required()
        {
            Assert.That(this.root.Parse("api remove").Errors, Is.Not.Empty);
        }

        private Task<int> Invoke(string commandLine) =>
            this.handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
    }
}
