// ------------------------------------------------------------------------------------------------
// <copyright file="TemporaryKnowledge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// A repository root in a temporary folder, with releases recorded as installed and generated
    /// files written where the generators write them. Deleted on dispose.
    /// </summary>
    internal sealed class TemporaryKnowledge : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryKnowledge"/> class: creates an empty, uniquely named
        /// folder under the system's temporary path, with nothing installed yet.
        /// </summary>
        public TemporaryKnowledge()
        {
            this.Root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-cli-tests-" + Guid.NewGuid().ToString("N")));
            this.Root.Create();
            this.Layout = new KnowledgeLayout(this.Root);
        }

        /// <summary>Gets the temporary repository root, deleted with everything under it on dispose.</summary>
        public DirectoryInfo Root { get; }

        /// <summary>Gets the knowledge layout over <see cref="Root"/>, as the handlers under test see it.</summary>
        public KnowledgeLayout Layout { get; }

        /// <summary>Records <paramref name="tags"/> as installed, the first as the default.</summary>
        /// <param name="tags">The releases to record, at least one; the first becomes the default release.</param>
        /// <returns>This instance, so set-up calls can be chained.</returns>
        public TemporaryKnowledge Install(params string[] tags)
        {
            var versions = tags.Select(tag => new InstalledVersion(
                tag, new UpstreamReference(Upstream.ReleaseRepository, "abc"), new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(VersionManifest.Build(tags[0], versions), this.Layout.VersionManifest.FullName);

            return this;
        }

        /// <summary>Writes a file under <c>knowledge/&lt;tag&gt;/</c>.</summary>
        /// <param name="tag">The release whose knowledge folder the file goes into.</param>
        /// <param name="relative">
        /// The file's path relative to that folder, with <c>/</c> separators; missing folders are created.
        /// </param>
        /// <param name="content">The file's text, written as is and replacing any existing file.</param>
        /// <returns>This instance, so set-up calls can be chained.</returns>
        public TemporaryKnowledge Write(string tag, string relative, string content)
        {
            var path = Path.Combine(this.Layout.Knowledge(tag).FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);

            return this;
        }

        /// <summary>Deletes <see cref="Root"/> and everything under it, if it still exists.</summary>
        public void Dispose()
        {
            this.Root.Refresh();
            if (this.Root.Exists)
            {
                this.Root.Delete(recursive: true);
            }
        }
    }
}
