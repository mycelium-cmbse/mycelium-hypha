// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperationLockTests.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    [TestFixture]
    public class ReleaseOperationLockTests
    {
        private DirectoryInfo workspace = null!;
        private DirectoryInfo root = null!;
        private FakeProcesses processes = null!;
        private ReleaseOperationLock releaseLock = null!;

        private string LockPath => Path.Combine(this.root.FullName, ReleaseOperationLock.FileName);

        private string RecoveryPath => Path.Combine(this.root.FullName, ReleaseOperationLock.RecoveryFileName);

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-lock-{Guid.NewGuid():N}"));

            // Not created: acquiring must create the root itself, as the extension does.
            this.root = new DirectoryInfo(Path.Combine(this.workspace.FullName, "storage"));
            this.processes = new FakeProcesses();
            this.releaseLock = new ReleaseOperationLock(
                new KnowledgeLayout(this.root), this.processes, NullLogger<ReleaseOperationLock>.Instance);
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
        public async Task Acquiring_creates_the_root_and_writes_the_owner_the_extension_reads()
        {
            await using var lease = await this.releaseLock.AcquireAsync();

            Assert.That(File.Exists(this.LockPath), Is.True);

            using var document = JsonDocument.Parse(File.ReadAllText(this.LockPath));
            var properties = document.RootElement.EnumerateObject().Select(property => property.Name).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(properties, Is.EqualTo(new[] { "pid", "token" }));
                Assert.That(document.RootElement.GetProperty("pid").GetInt32(), Is.EqualTo(Environment.ProcessId));
                Assert.That(document.RootElement.GetProperty("token").GetString(), Is.Not.Empty);
                Assert.That(File.ReadAllText(this.LockPath), Does.Not.Contain(" "));
            });
        }

        [Test]
        public async Task Releasing_deletes_the_lock()
        {
            var lease = await this.releaseLock.AcquireAsync();

            await lease.DisposeAsync();

            Assert.That(File.Exists(this.LockPath), Is.False);
        }

        [Test]
        public async Task A_lock_held_by_a_running_owner_is_refused_and_left_alone()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            this.processes.Running.Add(4242);

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => this.releaseLock.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("already running in another process or VS Code window"));
                Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo("""{"pid":4242,"token":"abc"}"""));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public async Task A_lock_the_extension_left_behind_is_recovered_once_its_owner_is_gone()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");

            await using var lease = await this.releaseLock.AcquireAsync();

            using var document = JsonDocument.Parse(File.ReadAllText(this.LockPath));

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty("pid").GetInt32(), Is.EqualTo(Environment.ProcessId));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("""{"pid":0}""")]
        [TestCase("""{"pid":-3}""")]
        [TestCase("""{"pid":"7"}""")]
        [TestCase("""{"pid":1.5}""")]
        public async Task An_unreadable_lock_is_refused_and_left_exactly_as_it_was(string content)
        {
            this.WriteLock(content);

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => this.releaseLock.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("unreadable or still being created"));
                Assert.That(exception.Message, Does.Contain(ReleaseOperationLock.FileName));
                Assert.That(exception.Message, Does.Contain(this.root.FullName));
                Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo(content));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public async Task An_existing_recovery_file_is_refused_and_nothing_is_deleted()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            File.WriteAllText(this.RecoveryPath, string.Empty);

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => this.releaseLock.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain(ReleaseOperationLock.RecoveryFileName));
                Assert.That(exception.Message, Does.Contain(this.root.FullName));
                Assert.That(File.Exists(this.LockPath), Is.True);
                Assert.That(File.Exists(this.RecoveryPath), Is.True);
            });
        }

        [Test]
        public async Task The_lock_file_is_the_exact_bytes_JSON_stringify_would_write()
        {
            await using var lease = await this.releaseLock.AcquireAsync();

            var bytes = File.ReadAllBytes(this.LockPath);
            using var document = JsonDocument.Parse(bytes);
            var token = document.RootElement.GetProperty("token").GetString();
            var expected = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                .GetBytes($$"""{"pid":{{Environment.ProcessId}},"token":"{{token}}"}""");

            Assert.Multiple(() =>
            {
                Assert.That(bytes[0], Is.EqualTo((byte)'{'), "no byte order mark: the extension's JSON.parse would refuse it");
                Assert.That(bytes, Is.EqualTo(expected));
            });
        }

        [Test]
        public async Task The_lock_is_taken_in_the_repository_root_not_the_output_root()
        {
            var output = new DirectoryInfo(Path.Combine(this.workspace.FullName, "output"));
            var separate = new ReleaseOperationLock(
                new KnowledgeLayout(this.root, output), this.processes, NullLogger<ReleaseOperationLock>.Instance);

            await using var lease = await separate.AcquireAsync();

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(this.LockPath), Is.True);
                Assert.That(File.Exists(Path.Combine(output.FullName, ReleaseOperationLock.FileName)), Is.False);
            });
        }

        [Test]
        public async Task A_second_acquisition_in_the_same_process_is_refused_as_such()
        {
            await using var first = await this.releaseLock.AcquireAsync();

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => this.releaseLock.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("already running in this process"));
                Assert.That(File.Exists(this.LockPath), Is.True);
            });
        }

        [Test]
        public async Task Releasing_leaves_a_lock_this_process_took_again_under_another_token()
        {
            var lease = await this.releaseLock.AcquireAsync();
            var other = $$"""{"pid":{{Environment.ProcessId}},"token":"other"}""";
            File.WriteAllText(this.LockPath, other);

            await lease.DisposeAsync();

            Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo(other));
        }

        [Test]
        public async Task Releasing_after_the_root_is_removed_does_nothing()
        {
            var lease = await this.releaseLock.AcquireAsync();
            this.root.Delete(recursive: true);

            Assert.DoesNotThrow(lease.Dispose);
        }

        [Test]
        public async Task Releasing_a_lock_that_cannot_be_removed_does_not_throw()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Ignore("Only Windows refuses to open a file another handle holds exclusively.");
            }

            var lease = await this.releaseLock.AcquireAsync();

            using (new FileStream(this.LockPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Assert.DoesNotThrow(lease.Dispose);
            }

            Assert.That(File.Exists(this.LockPath), Is.True);
        }

        [Test]
        public async Task Acquiring_gives_up_after_two_recoveries_when_stale_locks_keep_reappearing()
        {
            var stale = """{"pid":4242,"token":"abc"}""";
            this.WriteLock(stale);
            var reappearing = new ReleaseOperationLock(
                new KnowledgeLayout(this.root),
                this.processes,
                new RecreatingLogger(() => File.WriteAllText(this.LockPath, stale)));

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => reappearing.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("Could not acquire"));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public async Task Releasing_leaves_a_lock_another_owner_took_in_the_meantime()
        {
            var lease = await this.releaseLock.AcquireAsync();
            File.WriteAllText(this.LockPath, """{"pid":4242,"token":"someone-else"}""");

            await lease.DisposeAsync();

            Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo("""{"pid":4242,"token":"someone-else"}"""));
        }

        [Test]
        public async Task Releasing_twice_or_after_the_lock_is_gone_does_nothing()
        {
            var lease = await this.releaseLock.AcquireAsync();
            File.Delete(this.LockPath);

            Assert.DoesNotThrow(lease.Dispose);
            await Assert.DoesNotThrowAsync(async () => await lease.DisposeAsync());
        }

        [Test]
        public async Task The_lock_is_released_when_the_operation_fails()
        {
            async Task Operation()
            {
                await using var lease = await this.releaseLock.AcquireAsync();
                throw new InvalidOperationException("the operation failed");
            }

            await Assert.ThrowsAsync<InvalidOperationException>(Operation);
            Assert.That(File.Exists(this.LockPath), Is.False);

            await using var next = await this.releaseLock.AcquireAsync();
            Assert.That(File.Exists(this.LockPath), Is.True);
        }

        [Test]
        public async Task A_cancelled_acquisition_takes_no_lock()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => this.releaseLock.AcquireAsync(cancellation.Token));
            Assert.That(File.Exists(this.LockPath), Is.False);
        }

        [Test]
        public async Task Waiting_takes_a_lock_once_its_running_owner_releases_it()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            this.processes.Running.Add(4242);
            var waiting = this.Waiting(TimeSpan.FromSeconds(30));

            var acquiring = waiting.AcquireAsync();
            await Task.Delay(200);
            Assert.That(acquiring.IsCompleted, Is.False, "the lock is still held");

            File.Delete(this.LockPath);
            await using var lease = await acquiring;

            Assert.That(File.ReadAllText(this.LockPath), Does.StartWith($"{{\"pid\":{Environment.ProcessId},"));
        }

        [Test]
        public async Task A_lock_still_held_when_the_wait_is_over_is_refused_as_held_and_left_alone()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            this.processes.Running.Add(4242);
            var waiting = this.Waiting(TimeSpan.FromMilliseconds(300));
            var started = DateTime.UtcNow;

            var exception = await Assert.ThrowsAsync<ReleaseOperationLockException>(() => waiting.AcquireAsync());

            Assert.Multiple(() =>
            {
                Assert.That(DateTime.UtcNow - started, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(200)));
                Assert.That(exception!.Message, Does.Contain("already running in another process or VS Code window"));
                Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo("""{"pid":4242,"token":"abc"}"""));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public void Waiting_on_a_running_owner_reads_the_lock_without_creating_the_recovery_file_on_every_poll()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            this.processes.Running.Add(4242);
            var underRecovery = 0;
            var looks = 0;
            this.processes.OnCheck = () =>
            {
                looks++;
                if (File.Exists(this.RecoveryPath))
                {
                    underRecovery++;
                }
            };

            Assert.That(
                async () => await this.Waiting(TimeSpan.FromMilliseconds(500)).AcquireAsync(),
                Throws.InstanceOf<ReleaseOperationLockException>());

            // The first and the last try run the protocol, two attempts each; every poll in between
            // only reads the lock.
            Assert.Multiple(() =>
            {
                Assert.That(looks, Is.GreaterThan(4), "the owner is checked on every poll");
                Assert.That(underRecovery, Is.LessThanOrEqualTo(4));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [TestCase("")]
        [TestCase("{\"pid\":")]
        public async Task Waiting_takes_a_lock_once_it_is_no_longer_being_created(string partial)
        {
            this.WriteLock(partial);
            var acquiring = this.Waiting(TimeSpan.FromSeconds(30)).AcquireAsync();
            await Task.Delay(200);
            Assert.That(acquiring.IsCompleted, Is.False, "the lock is still being created");

            // Its owner finished writing it, and is gone by now.
            File.WriteAllText(this.LockPath, """{"pid":4242,"token":"abc"}""");
            await using var lease = await acquiring;

            Assert.That(File.ReadAllText(this.LockPath), Does.StartWith($"{{\"pid\":{Environment.ProcessId},"));
        }

        [Test]
        public async Task Waiting_takes_a_lock_once_another_recovery_is_over()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            File.WriteAllText(this.RecoveryPath, string.Empty);
            var acquiring = this.Waiting(TimeSpan.FromSeconds(30)).AcquireAsync();
            await Task.Delay(200);
            Assert.That(acquiring.IsCompleted, Is.False, "another recovery is in progress");

            File.Delete(this.RecoveryPath);
            await using var lease = await acquiring;

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(this.LockPath), Does.StartWith($"{{\"pid\":{Environment.ProcessId},"));
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public async Task Cancelling_stops_the_wait_and_takes_no_lock()
        {
            this.WriteLock("""{"pid":4242,"token":"abc"}""");
            this.processes.Running.Add(4242);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

            await Assert.ThrowsAsync(
                Is.InstanceOf<OperationCanceledException>(),
                () => this.Waiting(TimeSpan.FromSeconds(30)).AcquireAsync(cancellation.Token));

            Assert.That(File.ReadAllText(this.LockPath), Is.EqualTo("""{"pid":4242,"token":"abc"}"""));
        }

        [Test]
        public async Task Overlapping_operations_take_the_lock_one_after_the_other()
        {
            // Each operation has its own lock object over the same root, as two MCP servers would,
            // and they all start at once, so they also meet a lock that is still being created or
            // released, and a recovery in progress.
            const int Operations = 8;
            var holders = 0;
            var mostAtOnce = 0;
            var held = 0;
            using var start = new ManualResetEventSlim();

            var operations = Enumerable.Range(0, Operations).Select(_ => Task.Run(async () =>
            {
                start.Wait();

                await using (await this.Waiting(TimeSpan.FromSeconds(60)).AcquireAsync())
                {
                    var now = Interlocked.Increment(ref holders);
                    InterlockedMax(ref mostAtOnce, now);
                    await Task.Delay(20);
                    Interlocked.Decrement(ref holders);
                    Interlocked.Increment(ref held);
                }
            })).ToArray();

            start.Set();
            await Task.WhenAll(operations).WaitAsync(TimeSpan.FromMinutes(2));

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.EqualTo(Operations));
                Assert.That(mostAtOnce, Is.EqualTo(1));
                Assert.That(File.Exists(this.LockPath), Is.False);
                Assert.That(File.Exists(this.RecoveryPath), Is.False);
            });
        }

        [Test]
        public void A_negative_wait_is_refused()
        {
            Assert.That(
                () => this.Waiting(TimeSpan.FromMilliseconds(-1)),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int current;
            while ((current = Volatile.Read(ref target)) < value
                && Interlocked.CompareExchange(ref target, value, current) != current)
            {
            }
        }

        private ReleaseOperationLock Waiting(TimeSpan wait) =>
            new(new KnowledgeLayout(this.root), this.processes, NullLogger<ReleaseOperationLock>.Instance, wait);

        private void WriteLock(string content)
        {
            this.root.Create();
            File.WriteAllText(this.LockPath, content);
        }

        private sealed class FakeProcesses : IProcessProbe
        {
            public HashSet<int> Running { get; } = new();

            public Action? OnCheck { get; set; }

            public bool IsRunning(int processId)
            {
                this.OnCheck?.Invoke();
                return this.Running.Contains(processId);
            }
        }

        // Logs right after each stale lock is removed; recreating the lock there stands in for another
        // process that keeps leaving a stale lock behind.
        private sealed class RecreatingLogger(Action onLog) : ILogger<ReleaseOperationLock>
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                onLog();
            }
        }
    }
}
