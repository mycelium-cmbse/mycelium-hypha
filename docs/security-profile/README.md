# Security and deployment profiles

A security and deployment profile describes, for one published version, what hypha's MCP server starts, reads, writes and downloads, the credentials it uses, the processes it starts, and the hashes of its packages, so that an administrator can evaluate and approve it. Each answer is marked verified or not.

Each version has a profile of its own, which is not rewritten when a later version is published. Link to the profile of the version you approve.

| VS Code extension | Bundled hypha tools | Claude plugin and launcher | Profile |
|---|---|---|---|
| 1.0.0 | 2.0.0 | 3.0.0 | [vscode-1.0.0.md](vscode-1.0.0.md) |
| next, not published | built from `development` | built from `development` | [unreleased.md](unreleased.md) (draft) |

To report a vulnerability, see the [security policy](../../.github/SECURITY.md).

## The draft

[unreleased.md](unreleased.md) describes the code on `development`, the version that will be published next. A change that alters what it describes updates it in the same pull request; `SecurityProfileTests` fails when it disagrees with the code on the tools and their annotations, the prompts and the resource template, the network hosts, the environment variables or the `uv` pin. It is not a profile to approve from: nothing in it is checked in a published package.

## When a version is published

Following `RELEASING.md`:

- If the draft differs from the newest published profile in anything but its "at release" placeholders, copy it to a file named after the new version (`vscode-<version>.md`), fill in what it marks "at release" (the version, the commit, the tools version, the .NET runtime and the package hashes), and add a row above.
- Otherwise: add a row that points to the published profile that still describes it.
- Either way, take the package hashes from every registry the version is published to, after it is published.
