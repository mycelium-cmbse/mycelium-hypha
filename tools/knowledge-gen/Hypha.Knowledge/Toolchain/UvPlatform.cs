// ------------------------------------------------------------------------------------------------
// <copyright file="UvPlatform.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Toolchain
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Maps the current platform to the <c>uv</c> release asset that targets it.
    /// </summary>
    /// <remarks>
    /// <c>uv</c> publishes Rust target-triple asset names (e.g. <c>uv-x86_64-pc-windows-msvc.zip</c>),
    /// not .NET runtime identifiers - there is no shared convention with the rest of this repository's
    /// own release assets to reuse here, so this is a small, independent map. Verified against a live
    /// <c>astral-sh/uv</c> release listing: every target ships a flat archive (the executables sit at
    /// the archive root, no wrapping folder) and a companion <c>&lt;asset&gt;.sha256</c> asset.
    /// </remarks>
    public static class UvPlatform
    {
        /// <summary>The current platform's uv release target, or <c>null</c> when unsupported.</summary>
        public static string? CurrentTarget => TargetFor(RuntimeInformation.RuntimeIdentifier);

        /// <summary>The uv release target for a .NET runtime identifier, or <c>null</c> when unsupported.</summary>
        public static string? TargetFor(string runtimeIdentifier) => runtimeIdentifier switch
        {
            "win-x64" => "x86_64-pc-windows-msvc",
            "win-arm64" => "aarch64-pc-windows-msvc",
            "linux-x64" => "x86_64-unknown-linux-gnu",
            "linux-arm64" => "aarch64-unknown-linux-gnu",
            "osx-x64" => "x86_64-apple-darwin",
            "osx-arm64" => "aarch64-apple-darwin",
            _ => null,
        };

        /// <summary>True when <paramref name="target"/>'s archive is a zip (Windows); false for tar.gz.</summary>
        public static bool IsZip(string target)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(target);

            return target.Contains("windows", StringComparison.Ordinal);
        }

        /// <summary>The release asset file name for <paramref name="target"/>.</summary>
        public static string AssetName(string target)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(target);

            return $"uv-{target}.{(IsZip(target) ? "zip" : "tar.gz")}";
        }

        /// <summary>The cached executable's file name for <paramref name="target"/>.</summary>
        public static string ExecutableName(string target) => IsZip(target) ? "uv.exe" : "uv";
    }
}
