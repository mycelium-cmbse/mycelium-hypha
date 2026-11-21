# Security and deployment profiles

A security and deployment profile describes, for one published version, what hypha's MCP server starts, reads, writes and downloads, the credentials it uses, the processes it starts, and the hashes of its packages, so that an administrator can evaluate and approve it. Each answer is marked verified or not.

Each version has a profile of its own, which is not rewritten when a later version is published. Link to the profile of the version you approve.

| VS Code extension | Bundled hypha tools | Claude plugin and launcher | Profile |
|---|---|---|---|
| 1.0.0 | 2.0.0 | 3.0.0 | [vscode-1.0.0.md](vscode-1.0.0.md) |

To report a vulnerability, see the [security policy](../../.github/SECURITY.md).

## When a version is published

- If it changes any of the tools, prompts or resources, the launch definition, the network destinations, what is written or deleted, the child processes or the toolchain pins, or how packages are built and published: copy the newest profile to a file named after the new version, update it, and add a row above.
- Otherwise: add a row that points to the profile that still describes it.
- Either way, take the package hashes from every registry the version is published to, after it is published.
