# Security policy

## Reporting a vulnerability

Please report a vulnerability privately, through GitHub's private vulnerability reporting: open the repository's **Security** tab and choose **Report a vulnerability**, or go to <https://github.com/mycelium-cmbse/mycelium-hypha/security/advisories/new>.

Do not open a public issue, pull request or discussion for a vulnerability. A report made privately reaches the maintainers only, and stays private until a fix is published.

A useful report says:

- which part is affected (see "Scope" below) and which version, as `hypha --version`, the extension's version in VS Code or the plugin's version shows it;
- the platform, and whether hypha ran locally, over Remote SSH, in WSL or in a container;
- what an attacker can do, and the steps or a proof of concept that show it;
- whether the issue is already public, or known to anyone else.

## What happens next

The maintainers handle security reports on a best-effort basis; there is no stated response time. A report is acknowledged in the advisory, discussed there with the reporter, and fixed in a private fork where that is needed. Once a fixed version is released, the advisory is published, with a CVE where one applies, and the reporter is credited unless they ask not to be. The disclosure date is agreed with the reporter.

## Supported versions

Security fixes are made in the newest release of each part only. To get a fix, update to that release.

| Part | Released as | Supported |
|---|---|---|
| Claude plugin, with its launcher (`bin/`) | `v<major>.<minor>.<patch>` tags | the newest release |
| `hypha` CLI and MCP server (`hypha serve`) | `tools-v<major>.<minor>.<patch>` tags and the `Hypha.Tools` .NET tool | the newest release |
| VS Code extension (`mycelium-cmbse.mycelium-hypha`) | Visual Studio Marketplace and Open VSX | the newest release |

## Scope

In scope: the code in this repository, as it is released.

- the `hypha` CLI and the MCP server it hosts, including how it downloads, verifies, stores and removes releases, the toolchain it provisions, and the processes it starts;
- the launcher, which downloads and verifies the `hypha` executable for the plugin and the other MCP clients;
- the VS Code extension;
- the Claude plugin's skills, commands and manifests;
- the GitHub Actions workflows that build and publish the releases.

Out of scope, and best reported to their own maintainers:

- the OMG specifications, models and sources hypha reads, and what they say;
- uv, Python and the Python packages uv installs, VS Code, GitHub Copilot, Claude and other MCP clients;
- the answers a language model gives from hypha's tools. A tool that returns content it should not, or acts beyond what its description says, is in scope.