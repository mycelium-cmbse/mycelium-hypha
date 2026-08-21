// ------------------------------------------------------------------------------------------------
// <copyright file="RemoveCommandTests.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Moq;

    /// <summary>
    /// Suite of tests for <see cref="RemoveCommand"/>.
    /// </summary>
    [TestFixture]
    public class RemoveCommandTests
    {
        private RootCommand root = null!;
        private Mock<IReleaseRemover> remover = null!;
        private RemoveCommand.Handler handler = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new RemoveCommand());

            this.remover = new Mock<IReleaseRemover>();
            this.handler = new RemoveCommand.Handler(this.remover.Object);
        }

        [Test]
        public async Task A_release_is_removed()
        {
            var result = await this.Invoke("remove --tag 2026-04");

            Assert.That(result, Is.EqualTo(0));

            this.remover.Verify(
                mock => mock.RemoveAsync("2026-04", false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Force_is_passed_through()
        {
            await this.Invoke("remove --tag 2026-04 --force");

            this.remover.Verify(
                mock => mock.RemoveAsync("2026-04", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task An_uninstalled_tag_is_a_usage_error()
        {
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-06", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("'2026-06' is not installed"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("remove --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("not installed"));
            });
        }

        [Test]
        public async Task The_default_release_is_a_usage_error()
        {
            this.remover
                .Setup(mock => mock.RemoveAsync("2026-05", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "'2026-05' is the default release - run 'hypha use --tag <other>' before removing it"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("remove --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("hypha use"));
            });
        }

        [Test]
        public void The_tag_is_required()
        {
            Assert.That(this.root.Parse("remove").Errors, Is.Not.Empty);
        }

        private Task<int> Invoke(string commandLine) =>
            this.handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
    }
}
