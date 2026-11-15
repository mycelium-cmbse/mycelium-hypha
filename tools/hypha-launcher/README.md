# hypha-launcher (C#)

`hypha-launch`, the MCP server launcher every client package starts. It is not the `hypha` command
(that is [`tools/hypha-cli`](../hypha-cli/README.md)): it downloads the matching `hypha` and hands the
client over to `hypha serve`.

## What it does

Every client package starts the MCP server through one command, `bin/hypha-launch`: a package is a
git repository or a zip, which can carry a small launcher per platform but not a `hypha` of tens of MB.
The Claude plugin registers it in `.claude-plugin/plugin.json` (`mcpServers`); the other packages
follow (Step 7 of `hypha-mcp-refactor-plan.md`).

- **One path for every platform.** On Windows a client finds `bin/hypha-launch.exe` from the
  extensionless `bin/hypha-launch`; on macOS and Linux that path is a POSIX script that execs
  `bin/<rid>/hypha-launch`. A Node spawn and `cross-spawn`, which the MCP TypeScript SDK uses, both
  start the `.exe` from the extensionless path on Windows.
- **The matching `hypha`.** `bin/hypha-launch.json` names the `hypha` version the package starts. The
  launcher downloads `hypha-<version>-<rid>.zip` from the `tools-v<version>` release once, checks it
  against the checksum table in the release notes, keeps it in `HYPHA_HOME/bin/<version>/<rid>/`, and
  runs `hypha serve` on the same standard input and output, passing on any arguments it was given.
  Each download goes into a folder of its own and is renamed into place once verified, so two clients
  starting at once neither write into one folder together nor overwrite a `hypha` that is running.
- **The first start does not keep the client waiting.** A client gives a server a limited time to
  answer, and the first download can take longer, so the launcher answers at once, downloads
  meanwhile (`hypha_status` says so), then starts `hypha serve`, replays the client's `initialize` to
  it and tells the client that its tools, resources and prompts changed. From then on it passes every
  message through, so the first session gets the real tools without reconnecting.
  It resolves the package from its own location and `HYPHA_HOME` itself, never from a client's path
  variables, and the download honours `HTTPS_PROXY` and `GITHUB_TOKEN`.
- **`HYPHA_BIN`** names a `hypha` to start instead, for an offline or locked-down machine. It must be
  `hypha` 2.0.0 or later (an older one has no `serve`, and prints its help on standard output), given
  by its absolute path: a relative one would be resolved against whatever folder the client starts
  the server in, so the launcher refuses it.
- **A failure the model can see.** When there is no `hypha` to start (the download is blocked or
  cannot be written, the package is damaged, `HYPHA_HOME` cannot be used, the `hypha` found cannot be
  started), the launcher serves a minimal MCP server itself.
  Its `instructions` and its one tool, `hypha_status`, say why and how to fix it, so the model tells
  the user rather than the server failing to start.

## Building

The launcher references nothing else in the repository and carries neither Handlebars.Net nor
uml4net, so unlike `Hypha.Tools` it *is* NativeAOT-published, and the result is committed to `bin/`
rather than downloaded on demand:

```sh
dotnet publish tools/hypha-launcher/Hypha.Launcher -c Release -r win-x64 -p:PublishAot=true \
  -o publish/launch/win-x64
dotnet test tools/hypha-launcher/Hypha.Launcher.Tests/Hypha.Launcher.Tests.csproj
```

NativeAOT publishing needs a platform linker (`Desktop development with C++` on Windows, `clang` on
Linux/macOS) that a plain `dotnet build`/`dotnet test` does not; see
[the NativeAOT prerequisites](https://aka.ms/nativeaot-prerequisites). `.github/workflows/launcher-binaries.yml`
builds it on each platform's own runner and commits the results; that workflow, not a local publish,
is the normal way `bin/` gets updated. Run it after any change to `Hypha.Launcher`.

The tests start the `hypha` this solution builds (`tools/hypha-cli/Hypha.Tools`) through `HYPHA_BIN`,
since no released `hypha` before 2.0.0 has `serve`.
