// ------------------------------------------------------------------------------------------------
// <copyright file="SpecExtractProject.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Toolchain
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Finds the <c>spec-extract</c> Python project, unpacking the copy this assembly carries when the
    /// knowledge folder has none of its own.
    /// </summary>
    /// <remarks>
    /// A source checkout has <c>tools/spec-extract</c>, and that copy is used as it is, so a maintainer runs
    /// what they are editing. A knowledge folder anywhere else (<c>HYPHA_HOME</c>, which every client
    /// shares) has no <c>tools/</c> folder, so the project is unpacked into the toolchain folder, next to
    /// the <c>uv</c> and Python that run it, and kept in step with the copy in the binary: a newer hypha
    /// replaces what an older one unpacked. The <c>.venv</c> that <c>uv</c> builds inside the project is left
    /// alone, and <c>uv</c> rebuilds it when the lock file changed.
    /// </remarks>
    public sealed class SpecExtractProject : ISpecExtractProject
    {
        /// <summary>The folder the project is unpacked into, under the toolchain folder.</summary>
        public const string FolderName = "spec-extract";

        /// <summary>What every embedded file's resource name starts with; the rest is its path in the project.</summary>
        private const string ResourcePrefix = "Hypha.Knowledge.SpecExtract/";

        /// <summary>The file that marks a folder as the project.</summary>
        private const string ProjectFile = "pyproject.toml";

        /// <summary>Where a source checkout keeps the project.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>The toolchain folder, which the unpacked copy goes under.</summary>
        private readonly DirectoryInfo toolchainRoot;

        /// <summary>Initializes a new instance of the <see cref="SpecExtractProject"/> class.</summary>
        /// <param name="layout">Where a source checkout keeps the project.</param>
        /// <param name="toolchainRoot">The toolchain folder, which the unpacked copy goes under.</param>
        /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
        public SpecExtractProject(IKnowledgeLayout layout, DirectoryInfo toolchainRoot)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(toolchainRoot);

            this.layout = layout;
            this.toolchainRoot = toolchainRoot;
        }

        /// <summary>Makes sure the project is on disk, and says where.</summary>
        /// <param name="cancellationToken">Cancels unpacking the project.</param>
        /// <returns>
        /// The checkout's own <c>tools/spec-extract</c> when the knowledge folder has one; otherwise
        /// <c>spec-extract</c> under the toolchain folder, holding exactly the files this assembly carries.
        /// </returns>
        /// <exception cref="InvalidOperationException">This build of hypha carries no copy, which is a build fault.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task<DirectoryInfo> EnsureAsync(CancellationToken cancellationToken = default)
        {
            var checkout = new DirectoryInfo(Path.Combine(this.layout.Root.FullName, "tools", FolderName));

            if (File.Exists(Path.Combine(checkout.FullName, ProjectFile)))
            {
                return checkout;
            }

            var assembly = typeof(SpecExtractProject).Assembly;
            var resources = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
                .ToDictionary(name => name[ResourcePrefix.Length..].Replace('\\', '/'), name => name, StringComparer.Ordinal);

            if (!resources.ContainsKey(ProjectFile))
            {
                throw new InvalidOperationException($"this build of hypha does not carry the {FolderName} project");
            }

            var target = new DirectoryInfo(Path.Combine(this.toolchainRoot.FullName, FolderName));

            foreach (var (relative, name) in resources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Every file of the project is text, so it is compared and written as text.
                await using var resource = assembly.GetManifestResourceStream(name)!;
                using var reader = new StreamReader(resource);
                var content = await reader.ReadToEndAsync(cancellationToken);

                var file = new FileInfo(Path.Combine(target.FullName, relative.Replace('/', Path.DirectorySeparatorChar)));

                if (file.Exists && string.Equals(await File.ReadAllTextAsync(file.FullName, cancellationToken), content, StringComparison.Ordinal))
                {
                    continue;
                }

                await ReaderSafeFile.WriteAllTextAsync(file.FullName, content, cancellationToken);
            }

            RemoveStale(target, resources.Keys);

            return target;
        }

        /// <summary>
        /// Deletes the Python sources an older hypha unpacked that this one no longer carries, so a module
        /// removed from the project cannot still be imported.
        /// </summary>
        /// <param name="target">The unpacked project.</param>
        /// <param name="carried">The paths this assembly carries, relative to the project, <c>/</c>-separated.</param>
        private static void RemoveStale(DirectoryInfo target, IEnumerable<string> carried)
        {
            var sources = new DirectoryInfo(Path.Combine(target.FullName, "src"));

            if (!sources.Exists)
            {
                return;
            }

            var keep = carried.ToHashSet(StringComparer.Ordinal);

            foreach (var file in sources.EnumerateFiles("*.py", SearchOption.AllDirectories))
            {
                if (!keep.Contains(Path.GetRelativePath(target.FullName, file.FullName).Replace('\\', '/')))
                {
                    file.Delete();
                }
            }
        }
    }
}
