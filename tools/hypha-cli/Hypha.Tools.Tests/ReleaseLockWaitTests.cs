// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseLockWaitTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.DependencyInjection;

    // Several MCP servers share one knowledge folder, so under serve a release change waits for another's
    // to finish; every other verb refuses at once, so a script is not left hanging.
    [TestFixture]
    public class ReleaseLockWaitTests
    {
        private const string Tag = "2026-05";

        private TemporaryKnowledge knowledge = null!;

        [SetUp]
        public void SetUp() => this.knowledge = new TemporaryKnowledge().Install(Tag);

        [TearDown]
        public void TearDown() => this.knowledge.Dispose();

        [Test]
        public async Task Under_serve_a_release_change_waits_for_the_lock_another_holds()
        {
            await using var holder = this.Compose("list");
            await using var held = await holder.GetRequiredService<IReleaseOperationLock>().AcquireAsync();
            await using var serve = this.Compose("serve");
            using var stop = new CancellationTokenSource();

            var waiting = serve.GetRequiredService<IReleaseOperationLock>().AcquireAsync(stop.Token);
            var first = await Task.WhenAny(waiting, Task.Delay(TimeSpan.FromSeconds(2)));
            await stop.CancelAsync();

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.Not.SameAs(waiting), "serve gave up instead of waiting");
                Assert.That(async () => await waiting, Throws.InstanceOf<OperationCanceledException>());
            });
        }

        [TestCase("list")]
        [TestCase("use")]
        public async Task Any_other_verb_is_refused_at_once_while_another_holds_the_lock(string verb)
        {
            await using var holder = this.Compose("list");
            await using var held = await holder.GetRequiredService<IReleaseOperationLock>().AcquireAsync();
            await using var other = this.Compose(verb);

            var attempt = other.GetRequiredService<IReleaseOperationLock>().AcquireAsync();
            var first = await Task.WhenAny(attempt, Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.SameAs(attempt), $"{verb} waited instead of refusing");
                Assert.That(async () => await attempt, Throws.InstanceOf<ReleaseOperationLockException>());
            });
        }

        private ServiceProvider Compose(string verb) =>
            KnowledgeServices.Build(
                Program.BuildRootCommand().Parse($"{verb} --repository-root \"{this.knowledge.Root.FullName}\""));
    }
}
