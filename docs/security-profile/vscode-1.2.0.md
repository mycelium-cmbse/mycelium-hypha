# Security and deployment profile: VS Code extension 1.2.0

This profile is for administrators who evaluate and approve hypha's MCP server, in particular through the VS Code extension. It describes what version 1.2.0 does, as read from its code and checked in its published packages. It is not a certification, and it states no guarantee beyond what it says this version does.

This file describes 1.2.0 only, and is not rewritten for later versions: each version gets a profile of its own, listed in the [index](README.md). A correction to this file is recorded in its [history](https://github.com/mycelium-cmbse/mycelium-hypha/commits/development/docs/security-profile/vscode-1.2.0.md).

Each answer is marked:

- **Verified (code):** read in the source at the commit or tag named.
- **Verified (package):** checked in the published package.
- **Verified (upstream):** checked in the upstream repository or release named.
- **Not verified:** stated by the design or by a third party's documentation, and not checked for this version.
- **Not applicable** or **Not available:** said plainly, rather than implied.

To report a vulnerability, see the [security policy](../../.github/SECURITY.md).

**Profile for:** VS Code extension **1.2.0**, bundling hypha tools **2.1.0** (`tools-v2.1.0`); section 11 covers the Claude plugin **3.1.0** and the other clients that start the same server through the launcher. **Verified (code)** marks refer to the extension's commit in section 1 unless marked. Last reviewed 2026-11-27.

**Tested environments:** the repository's CI installs each built VSIX into VS Code and starts its bundled server, on Windows and Linux runners. Remote SSH, WSL and dev containers are not tested, and 1.2.0 has not been run on a Linux x64 Remote SSH host by the maintainers. The behaviour this profile states for those setups comes from the code, not from a test.

## 1. Identity and exact release

| Item | Value |
|---|---|
| MCP server name | `hypha` |
| VS Code extension | `mycelium-cmbse.mycelium-hypha`, publisher `mycelium-cmbse` |
| MCP server definition provider ID | `mycelium-hypha.server` |
| Owner | Starion Group S.A., through the repository [mycelium-cmbse/mycelium-hypha](https://github.com/mycelium-cmbse/mycelium-hypha) |
| Support | GitHub issues on that repository; vulnerabilities through the [security policy](../../.github/SECURITY.md). Best effort, no stated response time. |
| Status | 1.2.0 is a release, not a Marketplace pre-release. The extension's manifest still marks it `preview`. |
| Intended scope | One user's machine, or the remote host of one user's VS Code session. Not a shared or network service. |
| Extension version | 1.2.0, published 2026-11-27 |
| Extension source | commit `47992c752c172308a64f1901695298e94a29b416` |
| Bundled CLI | `hypha` tools 2.1.0, built from tag `tools-v2.1.0`, commit `89715db9efa2f7708c4f64480d51e5a6ad74497c`; `tools/` is the same at the extension's commit |
| Platforms | `win32-x64`, `linux-x64`, `linux-arm64` |

The extension has no release tag of its own; its version and the commit it was published from are the ones above. **Verified (package)**: every package's `out/cli/target.json` names tools 2.1.0.

### Package hashes

The Visual Studio Marketplace and Open VSX hold **the same build** of 1.2.0: one run of the repository's *Publish VS Code extension* workflow built the packages and published them to both, and the packages downloaded from the two registries are byte for byte the same. Every package carries the .NET runtime **10.0.15**.

SHA-256 of each package, as downloaded from both registries on 2026-11-27. **Verified (package).**

| Platform | VSIX | Bundled executable |
|---|---|---|
| `win32-x64` | `af1caaab2d95680c1662fc2e806804b16531efe6f03c7a29fb217a1bc9c9870c` | `hypha.exe` `9f6ebb5ac3343285a4e5621876c086a6bc37dd1b8fc001795186b957e7483834` |
| `linux-x64` | `73910b1378af2c34a1f8a05d7622d39684751ad077a76628c9f21fb72a81bc81` | `hypha` `5b05427f3afe898ef09fc5226e3270502c495f1bd06161ff7785dca69a66d3f7` |
| `linux-arm64` | `8ddba8219a1b1436e6392f713f47b81891eaa0aa9d10268f850bce14a6b4d936` | `hypha` `1e534b01f613ad5eb541109e9e5991c3c5633318f5cd70fb4354dd2363599ca3` |

The VSIX hashes match the `.sha256` files Open VSX publishes beside each package. The bundled executable is the .NET apphost; the rest of the program is the `.dll` files beside it in `out/cli/`, which these hashes do not cover. The Linux apphosts are the same as in 1.0.0's Marketplace packages, which carried the same runtime.

## 2. Launch and deployment definition

**Verified (code)**, `extensions/vscode/src/extension.ts` at `47992c752c172308a64f1901695298e94a29b416`:

| Item | 1.2.0 |
|---|---|
| Command | `<extension folder>/out/cli/hypha` (`hypha.exe` on Windows) |
| Arguments | `serve` |
| Environment | none of its own: it inherits the extension host's environment |
| Working directory | none of its own: the editor chooses it, and `hypha serve` does not read it |
| Provider ID / server name | `mycelium-hypha.server` / `hypha` |

**Which host runs it:** the extension runs on the workspace extension host. Locally, that is the user's machine. Under Remote SSH, WSL or a dev container, it is the remote host, the WSL distribution or the container: the server runs there and keeps its data there, and nothing of it runs on the local workstation. The extension does not run in VS Code for the Web.

**Resolved paths.** `<extension folder>` is where VS Code installs extensions on the host that runs them, so the command changes with the user, the version, the platform and the host. With VS Code's default extension folders, the command and the data folder (section 5) resolve to:

| Setup | Package | Command | Data folder (`HYPHA_HOME` unset) |
|---|---|---|---|
| Windows, local | `win32-x64` | `%USERPROFILE%\.vscode\extensions\mycelium-cmbse.mycelium-hypha-1.2.0-win32-x64\out\cli\hypha.exe` | `%LOCALAPPDATA%\hypha` |
| Linux, local | `linux-x64` | `~/.vscode/extensions/mycelium-cmbse.mycelium-hypha-1.2.0-linux-x64/out/cli/hypha` | `~/.local/share/hypha` |
| Remote SSH to a Linux x64 host | `linux-x64`, on the remote host | `~/.vscode-server/extensions/mycelium-cmbse.mycelium-hypha-1.2.0-linux-x64/out/cli/hypha`, on the remote host | `~/.local/share/hypha` on the remote host |
| WSL | `linux-x64` (or `linux-arm64`), in the distribution | `~/.vscode-server/extensions/mycelium-cmbse.mycelium-hypha-1.2.0-linux-x64/out/cli/hypha`, in the distribution | `~/.local/share/hypha` in the distribution |
| Linux arm64, local or remote | `linux-arm64` | as for Linux x64, with `linux-arm64` in the folder name | as for Linux x64 |

The extension folders are **Not verified**: they are VS Code's defaults, and change with VS Code Insiders (`.vscode-insiders`, `.vscode-server-insiders`), with `--extensions-dir`, or with `$XDG_DATA_HOME` for the data folder. The data folders are **Verified (code)**.

**Changes from 1.0.0:** the server definition sets **no working directory**; 1.0.0 set the extension's own folder. `hypha serve` does not read its working directory (**Verified (code)**: `KnowledgeServices.DiscoverRoot` returns no root under `serve`), so this changes nothing in how it behaves. Copilot could not migrate a definition with a working directory to a workspace `.mcp.json`.

## 3. Transport and listening services

- **Transport:** stdio. The extension registers a `McpStdioServerDefinition`, and `hypha serve` hosts the server with the MCP SDK's stdio transport only. **Verified (code).**
- **Listeners:** none. The code registers no HTTP transport and opens no socket to listen on; the only child process, `uv` (section 9), is run to completion and does not listen either, as far as hypha uses it. **Verified (code)** for hypha; **Not verified** for `uv`'s internals.

## 4. Capabilities and side effects

**Tools** (17). The columns *Read-only*, *Destructive* and *Open world* are the MCP annotations the server declares for each tool (`readOnlyHint`, `destructiveHint`, `openWorldHint`); a client may use them to decide what to confirm. **Verified (code).**

| Tool | Read-only | Destructive | Open world | Reads | Changes state | Network |
|---|---|---|---|---|---|---|
| `hypha_release_status` | yes | no | no | installed knowledge | no (but see the start, below) | no |
| `hypha_check_releases` | yes | no | yes | | no | GitHub API (section 6) |
| `hypha_install_release` | no | no | yes | | writes a release into `HYPHA_HOME`; runs `uv` for the specification text | GitHub, and OMG for an API version |
| `hypha_use_release` | no | no | no | | changes the default release in the manifest | no |
| `hypha_remove_release` | no | yes | no | | **deletes** an installed release's files | no |
| `hypha_metaclass`, `hypha_feature`, `hypha_typed_by`, `hypha_package`, `hypha_syntax`, `hypha_spec`, `hypha_search`, `hypha_search_spec`, `hypha_resolve`, `hypha_lookup`, `hypha_diff`, `hypha_api` | yes | no | no | installed knowledge | no (but see the start, below) | no |

- **The server writes on start, once:** when `hypha serve` starts on a `HYPHA_HOME` that has no release installed and no earlier copy recorded, it looks for releases that earlier client versions installed in their own folders (section 5). If it finds some, it copies them in, beside answering the client, under the release lock, and records that in the marker file `.migrated-from-clients`, so it never copies again. If it finds none, it writes nothing. This happens whatever the client asks first, so `HYPHA_HOME` can change even when only read-only tools are used. **Verified (code)**: `Program.cs` starts `IKnowledgeMigration.MigrateOnceAsync` for every verb, `serve` included.
- **Prompts** (5): `overview`, `explain_metaclass`, `cite_specification`, `validate_notation`, `api_lookup`. They return text that names tools to call; they do nothing themselves.
- **Resources:** one template, `hypha://{tag}/{kind}/{name}`, which returns a generated Markdown page.
- **Safeguards:** the three tools that change state run one at a time under a release lock (the file `.release-operation.lock` in `HYPHA_HOME`) shared by every hypha process that uses the same `HYPHA_HOME`. The server asks for no confirmation of its own; confirmation is what the MCP client does with the tool annotations (Copilot asks before running a tool unless the user allowed it). **Not verified:** how a given client uses the annotations.

## 5. Filesystem access and data lifecycle

**Verified (code)** unless marked.

- **`HYPHA_HOME`:** every file hypha writes is under one folder: the `HYPHA_HOME` environment variable when set (it must be an absolute path), otherwise `hypha` in the per-user data folder: `%LOCALAPPDATA%\hypha` on Windows, `$XDG_DATA_HOME/hypha` or `~/.local/share/hypha` on Linux, `~/Library/Application Support/hypha` on macOS.
- **What it holds:** `sources/<tag>/` (downloaded inputs: model files, textual sources, specification PDFs), `knowledge/<tag>/` (the generated knowledge, Markdown and JSON), the manifests that say what is installed, the marker and lock files of section 4, and `toolchain/` (`uv`, the Python it installs, its package cache, and the specification extractor hypha unpacks there).
- **Read elsewhere:** once, when the server starts on an empty `HYPHA_HOME` (section 4), it copies in releases that earlier versions of the clients installed in their own folders: the extension's global storage (`<VS Code config folder>/User/globalStorage/mycelium-cmbse.mycelium-hypha/`) and the Claude plugin's folder under `~/.claude`. It only reads there.
- **Workspace:** no tool takes a file path, and the server reads no workspace file. This is how the application behaves, **not an enforced restriction**: the process runs as the user and has the user's file permissions.
- **Logs:** written to standard error, which the MCP client shows or keeps; hypha keeps no log file.
- **Retention and removal:** a release stays until `hypha_remove_release` (or `hypha remove`) deletes it. **Uninstalling the extension does not delete `HYPHA_HOME`**; delete the folder by hand to remove everything. **Not verified:** what VS Code itself deletes on uninstall beyond the extension folder.

## 6. External network access

All requests are HTTPS `GET`s, made by `hypha_install_release` and `hypha_check_releases` only. Every other tool works from the installed knowledge and needs no network. **Verified (code)** for the hosts hypha requests; redirects are followed as GitHub serves them.

| Destination | Purpose | When |
|---|---|---|
| `api.github.com` | list release tags, resolve commits, list a release's files, read the `uv` release | check, install |
| `raw.githubusercontent.com` | download the files of `Systems-Modeling/SysML-v2-Release`, `Systems-Modeling/SysML-v2-Pilot-Implementation` and `Systems-Modeling/SysML-v2-API-Services` at a tag, including the specification PDFs | install |
| `github.com` and GitHub's download hosts | the `uv` release archive and its `.sha256`, from `astral-sh/uv` | the first install that extracts specification text |
| `www.omg.org` | the Systems Modeling API PIM model (`/spec/SystemsModelingAPI/20250201/Systems-Modeling-API.xmi`) | installing an API version only |
| Python downloads and the Python package index | fetched by `uv`, not by hypha: a managed Python and the extractor's packages | the first specification extraction |

- **Specification PDFs:** in `SysML-v2-Release` at tag `2026-05`, the PDFs are ordinary files, not Git LFS objects, so `raw.githubusercontent.com` serves them itself. **Verified (upstream).** An older or newer tag may differ.
- **Required or optional:** the network is needed to install or check releases, never to answer from an installed one. An offline install fails, or leaves out the specification text, and says so.
- **`uv`'s own traffic:** which hosts `uv` uses for Python and packages is `uv`'s behaviour, by default GitHub (for Python builds) and the Python Package Index. **Not verified** for this version.
- **Proxies:** hypha uses .NET's default HTTP handler, which follows the system proxy settings and the `HTTPS_PROXY` / `NO_PROXY` variables. **Not verified** against a proxy.

## 7. Authentication and credentials

**Verified (code).**

- **Required:** no. Anonymous GitHub API access works, within GitHub's limit of 60 requests an hour.
- **Supported:** `GITHUB_TOKEN`, or `GH_TOKEN` when `GITHUB_TOKEN` is unset or empty, read from the environment the server inherits. The extension sets neither.
- **Where it is sent:** as a `Bearer` authorization header on hypha's GitHub requests (`api.github.com`, `raw.githubusercontent.com`, and the `uv` download from `github.com`). It is not sent to `www.omg.org`, which hypha downloads from with a separate client that carries no credential. **Not verified:** whether the header is dropped on a redirect to another GitHub host (.NET's handler normally drops it).
- **Storage and logging:** hypha stores no credential, writes the token to no file and does not use VS Code's secret storage; no log statement in its code includes it.

## 8. Data leaving the execution host

- **Telemetry:** none. Neither the extension nor the server collects or sends usage data, diagnostics or crash reports. **Verified (code).**
- **Server traffic:** only the requests in section 6. Their URLs name upstream repositories, tags and files; none of them carries a prompt, workspace content, a user's model or generated knowledge. **Verified (code).**
- **Tool answers:** every tool answer goes back to the MCP client over stdio, and from there to the language model the client uses. That transfer is the client's, under the client's own terms, and it carries what the tool returned: knowledge generated from the public OMG sources, including specification text.
- **Workstation and remote host:** under Remote SSH or WSL, the server's network traffic leaves the remote host, not the workstation.

## 9. Child processes and managed toolchain

**Verified (code)** unless marked.

- **`uv` 0.12.10**, pinned in the code. Downloaded from the `astral-sh/uv` GitHub release for the platform, checked against the SHA-256 in the release's `.sha256` asset, and cached under `HYPHA_HOME/toolchain/uv/0.12.10/`. That check proves the archive is intact; since the checksum comes from the same release, it does not prove who made it.
- **Command line:** `uv run --project <HYPHA_HOME>/toolchain/spec-extract python -m spec_extract --repo-root <folder> --tag <tag> --out-root <folder>`, with `UV_CACHE_DIR` and `UV_PYTHON_INSTALL_DIR` pointing into `HYPHA_HOME/toolchain/`. It runs once per installed release, to turn the specification PDFs into text, and exits.
- **Python and packages:** `uv` installs a Python that satisfies the extractor's `requires-python` (3.12 or later) and the packages its `uv.lock` pins, with their hashes. **Not verified:** that `uv` refuses a package whose hash does not match, in this mode.
- **The extractor** is embedded in hypha and unpacked into `HYPHA_HOME/toolchain/spec-extract/`.
- No other process is started by the server.

## 10. Supply chain, maintenance and licensing

- **Distribution:** the Visual Studio Marketplace and Open VSX, both published by one run of the repository's *Publish VS Code extension* workflow through each registry's Trusted Publishing, with no stored token. **Verified (code)** for the workflow, and **Verified (package)** that both registries hold the same packages (section 1).
- **Updates:** through the editor's extension updates. hypha does not update itself.
- **Signing and provenance:** **Not available.** Neither the packages nor the executable are signed by the publisher, and no build provenance or attestation is published. The Marketplace's own signing of packages is the Marketplace's, and **Not verified** here.
- **SBOM:** **Not available** for the extension: the VSIX carries no SBOM. The tools release archives and the `Hypha.Tools` .NET tool package carry an SPDX 2.2 SBOM under `_manifest/spdx_2.2/`. They are built from the same project as the bundled CLI, but are not the bundled CLI, so their SBOM describes it only indirectly.
- **Dependency and vulnerability scanning:** dependencies are kept current by Dependabot. No scan results are published. Every package carries the .NET runtime 10.0.15 (section 1); check it against the runtime's own security advisories.
- **Vulnerability reporting:** the [security policy](../../.github/SECURITY.md), through GitHub's private vulnerability reporting.
- **Licenses:** hypha is Apache-2.0. Each package carries `LICENSE.txt` and a `NOTICE` with the third-party notices. The OMG content hypha downloads stays under OMG's own terms and is never redistributed by hypha.

## 11. Other clients: the launcher

The Claude plugin, the Agent Plugins manifest (Copilot, Cursor, Codex), the Gemini CLI extension, the Claude Desktop bundle and the clients in `docs/other-clients.md` start the **same** `hypha serve`, with the same tools, files, network destinations and child processes as sections 3 to 9. What differs is how `hypha` reaches the machine: the VS Code extension bundles it, while these clients start a small **launcher** that downloads it. This section describes plugin **3.1.0** (tag `v3.1.0`, commit `47992c752c172308a64f1901695298e94a29b416`), whose launcher starts tools **2.1.0**. **Verified (code)** at that tag unless marked.

| | VS Code extension 1.2.0 | Launcher (plugin 3.1.0 and the other clients) |
|---|---|---|
| Command | `<extension folder>/out/cli/hypha serve` | `${CLAUDE_PLUGIN_ROOT}/bin/hypha-launch` (Claude Code), `./bin/hypha-launch` (Agent Plugins `mcp.json`), or the path the client's configuration names; no arguments |
| `hypha` executable | in the VSIX | downloaded once from the GitHub release `tools-v2.1.0`, then cached |
| Which `hypha` | the one in the package | the version `bin/hypha-launch.json` names (`2.1.0`), or the executable `HYPHA_BIN` names when that is set |
| Integrity check | the registry's package | the archive's SHA-256 against the checksum table in the same release's notes |
| Extra network access | none | on first start only: `api.github.com` (`repos/mycelium-cmbse/mycelium-hypha/releases/tags/tools-v2.1.0`) and the release asset from `github.com` and GitHub's download hosts |
| Credentials | as section 7 | as section 7: the launcher also reads `GITHUB_TOKEN`, then `GH_TOKEN`, for its GitHub requests |
| Files | as section 5 | as section 5, plus the cached `hypha` under `HYPHA_HOME/bin/2.1.0/<rid>/` |
| Working directory | the editor's choice; `hypha serve` does not read it | the client's choice; `hypha serve` does not read it |

- **The launcher** is a NativeAOT executable committed in the plugin's `bin/`, built by the *Launcher binaries* workflow. It starts `hypha serve` as a child process and passes every message through unchanged, both ways, so the client talks to the same server.
- **The launcher answers on its own while it downloads.** A client gives a server little time to answer `initialize`, and the first download can take longer. So on first start the launcher answers `initialize` itself, downloads `hypha` meanwhile, then starts `hypha serve`, replays the `initialize` to it, and tells the client that the tools, resources and prompts changed. Until then, and for good when the download fails, the client sees a server that offers one read-only tool, `hypha_status`, which only says why there is no `hypha` yet. **Verified (code)**: `BootstrapServer` and `FallbackServer`.
- **No self-update:** the launcher downloads only the version `bin/hypha-launch.json` names. A newer `hypha` arrives with a newer plugin version.
- **Offline:** once `hypha` is cached, the launcher starts it without the network.

Launcher binaries at `v3.1.0`, SHA-256. **Verified (code)**: hashed from the repository at that tag. The launcher's code changed only in its license headers since plugin 3.0.0, so the compiled launchers are the ones 3.0.0 shipped; the POSIX script's header changed, and with it its hash.

| File | SHA-256 |
|---|---|
| `bin/hypha-launch.exe` (Windows x64) | `e2c6a6a19603993c043ddf3c9311d409b83735cde42f15e74f39c8025a4dd5b2` |
| `bin/linux-x64/hypha-launch` | `d5973118a503336d6abd5bdc451571ff3baf6e49222663b8d45abfe1537438a8` |
| `bin/linux-arm64/hypha-launch` | `54389edd982b95c2e1ead40d90944c588fbc0fe78d7e078be88b5b14fc5ceb2f` |
| `bin/osx-arm64/hypha-launch` | `27d6a32c1d614e3c8d1629168eec3668e688be29bb7f87782378fefc3eaed0fd` |
| `bin/osx-x64/hypha-launch` | `354c9f8b4285f1ca52444a0111988f5e69b8bc0623e12b12ed4c88dd0f05dbfc` |
| `bin/hypha-launch` (POSIX script that picks the platform's launcher) | `8f9fe4125df4cbfc983dc3894f72844a5e6aeb38853feac94a6e134f02970b44` |

The archives the launcher downloads, as the `tools-v2.1.0` release notes record them. **Verified (package)**: downloaded on 2026-11-27, and each matches. Each archive carries an SPDX 2.2 SBOM under `_manifest/spdx_2.2/`.

| Archive | SHA-256 |
|---|---|
| `hypha-2.1.0-win-x64.zip` | `704f375e067f4b8780546ebee26502cde1c61b1ecb14256e7ca62a913d66ac97` |
| `hypha-2.1.0-linux-x64.zip` | `f78607d159d8a1840925b9f48bda6ff412825eca210239ec34dfc7d5273eef70` |
| `hypha-2.1.0-linux-arm64.zip` | `19eb757e81c748f2635b9530db2f017fce69b2ad9b5f2b78da151c2c5132c3be` |
| `hypha-2.1.0-osx-arm64.zip` | `5375b9d0ae2533183b0e94dfaa37b9d2f0b2b8b822b087a0fdfa2058ae7ea8de` |
| `hypha-2.1.0-osx-x64.zip` | `b958c7531e944abc1fbb1b9c5ac7e508a521d72637967a3d40747768658fdd58` |

The launcher's checksum check has the limit of section 9: the checksum comes from the same release as the archive, so it proves the download is intact, not who built it.

## Approval integration

- **Stable identifiers:** the extension ID `mycelium-cmbse.mycelium-hypha`, the provider ID `mycelium-hypha.server` and the server name `hypha` stay the same across versions and platforms. The executable's path does not: it contains the version and the platform (section 2).
- **How an allowlist matches:** whether VS Code or GitHub Copilot match an MCP policy on the extension ID, the provider ID, the server name or the executable path is decided by them. **Not verified** for this extension: check the current VS Code and Copilot policy documentation, and test the rule.
- **No tested guidance:** no enterprise deployment of hypha has been tested yet. Reports of what an approval needed are welcome as issues.

## How this profile is kept

This file is fixed to 1.2.0. A version that changes any of the tools, prompts or resources, the launch definition, the network destinations, what is written or deleted, the child processes or the toolchain pins, or how packages are built and published, gets a new profile file of its own, and the [index](README.md) lists it. A version that changes none of these is added to the index against the profile that still describes it.
