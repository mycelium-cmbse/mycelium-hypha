# Security and deployment profile

This profile is for administrators who evaluate and approve hypha's MCP server, in particular through the VS Code extension. It describes what each published version does, as read from its code and checked in its published packages. It is not a certification, and it states no guarantee beyond what it says each version does.

Each answer is marked:

- **Verified (code):** read in the source at the release tag named.
- **Verified (package):** checked in the published package.
- **Not verified:** stated by the design or by a third party's documentation, and not checked for this version.
- **Not applicable** or **Not available:** said plainly, rather than implied.

To report a vulnerability, see the [security policy](../.github/SECURITY.md).

**Profile for:** VS Code extension **1.0.0**, bundling hypha tools **2.0.0** (`tools-v2.0.0`). Last reviewed 2026-11-18.

## 1. Identity and exact release

| Item | Value |
|---|---|
| MCP server name | `hypha` |
| VS Code extension | `mycelium-cmbse.mycelium-hypha`, publisher `mycelium-cmbse` |
| MCP server definition provider ID | `mycelium-hypha.server` |
| Owner | Starion Group S.A., through the repository [mycelium-cmbse/mycelium-hypha](https://github.com/mycelium-cmbse/mycelium-hypha) |
| Support | GitHub issues on that repository; vulnerabilities through the [security policy](../.github/SECURITY.md). Best effort, no stated esponse time. |
| Status | 1.0.0 is a release, not a Marketplace pre-release. The extension's manifest still marks it `preview`. |
| Intended scope | One user's machine, or the remote host of one user's VS Code session. Not a shared or network service. |
| Extension version | 1.0.0, published 2026-11-15 |
| Bundled CLI | `hypha` tools 2.0.0, built from tag `tools-v2.0.0` (commit `648e29c`) |
| Extension source | commit `6213c41` |
| Platforms | `win32-x64`, `linux-x64`, `linux-arm64` |

The extension has no release tag of its own; its version and the commit it was published from are the ones above. **Verified (package)**: every package's `out/cli/target.json` names tools 2.0.0. 

### Package hashes

The Visual Studio Marketplace and Open VSX hold **different builds** of 1.0.0. The Marketplace packages were built and published by the repository's workflow. Open VSX took 1.0.0 by hand, before its trusted publishing could be set up, from packages built on a maintainer's machine at the same commit. The two builds have the same hypha code and the same text files, but differ in line endings and in the .NET runtime they carry: **10.0.15** in the Marketplace packages, **10.0.11** in the Open VSX ones. An approval should name the registry its users install from.

SHA-256 of each package, as downloaded on 2026-11-18. **Verified (package).**

| Registry | Platform | VSIX | Bundled executable |
|---|---|---|---|
| Marketplace | `win32-x64` | `5ad6106aaa2a1c4b736b4e4da54484a16f2775b00d08235d310ef0589fe5eb8a` | `hypha.exe` `ac5edbfd2fb75ec3f8ca83b17cdcaff62ba510da717a1fe0116f7b25d7b27ae9` |
| Marketplace | `linux-x64` | `157beb60f0523ac2e3da7b6dca8ac50578d0db292c877354981950a42519d74a` | `hypha` `5b05427f3afe898ef09fc5226e3270502c495f1bd06161ff7785dca69a66d3f7` |
| Marketplace | `linux-arm64` | `2d27585ecbb8648ec03eab52ef26ef2354d5c3c87555fe4d3dc330c25ec31f66` | `hypha` `1e534b01f613ad5eb541109e9e5991c3c5633318f5cd70fb4354dd2363599ca3` |
| Open VSX | `win32-x64` | `e707de18d09feb3ec527931113fb2c0a2b4a77c3572647fe10badb0ffd58a867` | `hypha.exe` `3dc67c7be4df8410eb4356d40c8ff9d0dcfb5a494f76c04864512c4b82b77131` |
| Open VSX | `linux-x64` | `0fcd19660d3200730e412fa672d7134acd7210ce9f3607180434e960926cc402` | `hypha` `0db2abe64f568672f02d6fb8b9cb7699b436b5f86a7df64aa259c7770c5caeb5` |
| Open VSX | `linux-arm64` | `fd211f89de62c087a7404cf8585e560fc7de4d110f92e46988ddb26bdf905ac9` | `hypha` `7bc572851e990668430e997ffdf56635d8230c6bb5fdca6ada651f0ac4cf7fcc` |

The Open VSX VSIX hashes match the `.sha256` files Open VSX publishes beside each package. The bundled executable is the .NET apphost; the rest of the program is the `.dll` files beside it in `out/cli/`, which these hashes do not cover.

## 2. Launch and deployment definition

**Verified (code)**, `extensions/vscode/src/extension.ts` at `6213c41`:

| Item | 1.0.0 |
|---|---|
| Command | `<extension folder>/out/cli/hypha` (`hypha.exe` on Windows) |
| Arguments | `serve` |
| Environment | none of its own: it inherits the extension host's environment |
| Working directory | the extension's own folder |
| Provider ID / server name | `mycelium-hypha.server` / `hypha` |

`<extension folder>` is where VS Code installs extensions on the host that runs them, by default `~/.vscode/extensions/mycelium-cmbse.mycelium-hypha-1.0.0-<platform>/` (or `.vscode-server/extensions/` on a remote host). It changes with the user, the version, the platform and the host.

**Which host runs it:** the extension runs on the workspace extension host. Locally, that is the user's machine. Under Remote SSH, WSL or a dev container, it is the remote host, the WSL distribution or the container: the server runs there and keeps its data there, and nothing of it runs on the local workstation. The extension does not run in VS Code for the Web.

**Changes between versions:**
- *Next version (unreleased):* the definition sets **no working directory**; the editor chooses it. `hypha serve` does not read its working directory in tools 2.0.0 (**Verified (code)**: `KnowledgeServices.DiscoverRoot` returns no root under `serve`), so this changes nothing in how   it behaves. Copilot could not migrate a definition with a working directory.

## 3. Transport and listening services

- **Transport:** stdio. The extension registers a `McpStdioServerDefinition`, and `hypha serve` hosts the server with the MCP SDK's stdio transport only. **Verified (code).**
- **Listeners:** none. The code registers no HTTP transport and opens no socket to listen on; the only child process, `uv` (section 9), is run to completion and does not listen either, as far as hypha uses it. **Verified (code)** for hypha; **Not verified** for `uv`'s internals.

## 4. Capabilities and side effects

**Tools** (17), with the MCP annotations the server declares. **Verified (code)** at `tools-v2.0.0`.

| Tool | Reads | Changes state | Network |
|---|---|---|---|
| `hypha_release_status` | installed knowledge | no | no |
| `hypha_check_releases` | | no | GitHub API (section 6) |
| `hypha_install_release` | | writes a release into `HYPHA_HOME`; runs `uv` for the specification text | GitHub, and OMG for an API version |
| `hypha_use_release` | | changes the default release in the manifest | no |
| `hypha_remove_release` | | **deletes** an installed release's files (declared destructive) | no |
| `hypha_metaclass`, `hypha_feature`, `hypha_typed_by`, `hypha_package`, `hypha_syntax`, `hypha_spec`, `hypha_search`, `hypha_search_spec`, `hypha_resolve`, `hypha_lookup`, `hypha_diff`, `hypha_api` | installed knowledge | no | no |

- **Prompts** (5): `overview`, `explain_metaclass`, `cite_specification`, `validate_notation`, `api_lookup`. They return text that names tools to call; they do nothing themselves.
- **Resources:** one template, `hypha://{tag}/{kind}/{name}`, which returns a generated Markdown page.
- **Safeguards:** the three tools that change state run one at a time under a release lock shared by every hypha process that uses the same `HYPHA_HOME`. The server asks for no confirmation of its own; confirmation is what the MCP client does with the tool annotations (Copilot asks before running a tool unless the user allowed it). **Not verified:** how a given client uses the annotations.

## 5. Filesystem access and data lifecycle

**Verified (code)** unless marked.

- **`HYPHA_HOME`:** every file hypha writes is under one folder: the `HYPHA_HOME` environment variable when set (it must be an absolute path), otherwise `hypha` in the per-user data folder: `%LOCALAPPDATA%\hypha` on Windows, `$XDG_DATA_HOME/hypha` or `~/.local/share/hypha` on Linux, `~/Library/Application Support/hypha` on macOS.
- **What it holds:** `sources/<tag>/` (downloaded inputs: model files, textual sources, specification PDFs), `knowledge/<tag>/` (the generated knowledge, Markdown and JSON), the manifests that say what is installed, and `toolchain/` (`uv`, the Python it installs, its package cache, and the
 specification extractor hypha unpacks there).
- **Read elsewhere:** once, the first time hypha finds an empty `HYPHA_HOME`, it copies in releases that earlier versions of the clients installed in their own folders: the extension's global storage (`<VS Code config folder>/User/globalStorage/mycelium-cmbse.mycelium-hypha/`) and the Claude plugin's folder under `~/.claude`. It only reads there.
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

- **Required or optional:** the network is needed to install or check releases, never to answer from an installed one. An offline install fails, or leaves out the specification text, and says so.
- **`uv`'s own traffic:** which hosts `uv` uses for Python and packages is `uv`'s behaviour, by default GitHub (for Python builds) and the Python Package Index. **Not verified** for this version.
- **Proxies:** hypha uses .NET's default HTTP handler, which follows the system proxy settings and the `HTTPS_PROXY` / `NO_PROXY` variables. **Not verified** against a proxy.

## 7. Authentication and credentials

**Verified (code).**

- **Required:** no. Anonymous GitHub API access works, within GitHub's limit of 60 requests an hour.
- **Supported:** `GITHUB_TOKEN`, or `GH_TOKEN` when `GITHUB_TOKEN` is unset or empty, read from the environment the server inherits. The extension sets neither.
- **Where it is sent:** as a `Bearer` authorization header on hypha's GitHub requests (`api.github.com`, `raw.githubusercontent.com`, and the `uv` download from `github.com`). It is not sent to `www.omg.org`, which hypha downloads from with a separate client that carries no credential. **Not verified:** whether the header is dropped on a redirect to another GitHub host (.NET's handler normally drops it).
- **Storage and logging:** hypha stores no credential and writes the token to no file; no log statement in its code includes it.

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

- **Distribution:** the Visual Studio Marketplace and Open VSX. The Marketplace packages are published by the repository's *Publish VS Code extension* workflow through Trusted Publishing, with no stored token; Open VSX took 1.0.0 by hand (section 1). **Verified (code)** for the workflow.
- **Updates:** through the editor's extension updates. hypha does not update itself.
- **Signing and provenance:** **Not available.** Neither the packages nor the executable are signed by the publisher, and no build provenance or attestation is published. The Marketplace's own
  signing of packages is the Marketplace's, and **Not verified** here.
- **SBOM:** **Not available** for the extension: the VSIX carries no SBOM. **Verified (package).** The tools release archives and the `Hypha.Tools` .NET tool package carry an SPDX 2.2 SBOM under `_manifest/`. They are built from the same project as the bundled CLI, but are not the bundled CLI, so their SBOM describes it only indirectly.
- **Dependency and vulnerability scanning:** dependencies are kept current by Dependabot. No scan results are published. **Known:** the Open VSX 1.0.0 packages carry .NET runtime 10.0.11, older than the Marketplace's 10.0.15; check the runtime's own security advisories for the patches in between.
- **Vulnerability reporting:** the [security policy](../.github/SECURITY.md), through GitHub's private vulnerability reporting.
- **Licenses:** hypha is Apache-2.0. Each package carries `LICENSE.txt` and a `NOTICE` with the third-party notices. The OMG content hypha downloads stays under OMG's own terms and is never  redistributed by hypha.

## Approval integration

- **Stable identifiers:** the extension ID `mycelium-cmbse.mycelium-hypha`, the provider ID `mycelium-hypha.server` and the server name `hypha` stay the same across versions and platforms. The
  executable's path does not: it contains the version and the platform.
- **How an allowlist matches:** whether VS Code or GitHub Copilot match an MCP policy on the extension ID, the provider ID, the server name or the executable path is decided by them. **Not verified** for this extension: check the current VS Code and Copilot policy documentation, and test the rule.
- **No tested guidance:** no enterprise deployment of hypha has been tested yet. Reports of what an approval needed are welcome as issues.

## How this profile is kept

The profile is updated, and its "Profile for" line moved, whenever a version changes any of: the tools, prompts or resources; the launch definition; the network destinations; what is written or
deleted; the child processes or the toolchain pins; or how packages are built and published. Each version's profile is the one in the repository at that version's commit.