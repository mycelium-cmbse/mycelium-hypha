// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseOperationLock.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Serialises operations that change the installed releases of one repository root, across
    /// processes: every hypha that reads the same folder takes the same lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The lock is the file <c>.release-operation.lock</c> in the repository root, created
    /// exclusively and holding <c>{"pid": …, "token": …}</c>. A lock whose owner process is gone is
    /// stale and is recovered, under the exclusively created <c>.release-operation.lock.recovery</c>;
    /// a lock that cannot be read, or an existing recovery file, is never removed automatically. The
    /// Hypha VS Code extension up to 0.1 took the same lock from TypeScript, on its own storage folder,
    /// and this protocol is the one it used; since it hands Copilot the MCP server it takes none.
    /// </para>
    /// <para>
    /// The CLI's own verbs (<c>fetch</c>, <c>generate</c>, <c>use</c>, <c>remove</c>) do not take it.
    /// That was so that the extension could run them while holding the lock itself; no client does
    /// that any more, so the reason is gone and the verbs could take it. Today it is taken by the
    /// operations that change releases on their own, such as an install driven by an MCP tool.
    /// </para>
    /// <para>
    /// By default a held lock is refused at once. With
    /// <see cref="HyphaKnowledgeOptions.ReleaseLockWait"/> set, it is tried again until the wait is
    /// over, which also covers a lock held by another operation of this same process: two overlapping
    /// tool calls of one MCP server take it one after the other.
    /// </para>
    /// </remarks>
    public interface IReleaseOperationLock
    {
        /// <summary>
        /// Takes the lock, recovering a stale one first if its owner is gone, and waiting for it while
        /// <see cref="HyphaKnowledgeOptions.ReleaseLockWait"/> lasts.
        /// </summary>
        /// <param name="cancellationToken">Cancels before the lock is taken, the wait included.</param>
        /// <returns>The held lock; disposing it releases the lock.</returns>
        /// <exception cref="ReleaseOperationLockException">
        /// Once the wait is over: another process or VS Code window holds the lock (or this process does,
        /// through a lease it has not released yet, since the lock is not re-entrant), another recovery is
        /// in progress or was interrupted, or the existing lock cannot be read or created.
        /// </exception>
        /// <exception cref="System.OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<ReleaseOperationLease> AcquireAsync(CancellationToken cancellationToken = default);
    }
}
