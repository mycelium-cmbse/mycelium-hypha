# Hypha in other MCP clients

Clients without a plugin, extension or bundle of their own start the hypha MCP server from a
configuration file you write once. This page gives that configuration for JetBrains Junie, Devin
Desktop (formerly Windsurf), Zed and Cline, and says where each one reads skills from.

Every client that starts the server shares the same installed releases, in the per-user `hypha` data
folder (`HYPHA_HOME`), so a release installed from one answers in all of them.

## 1. Get the launcher and the skills

Clone the plugin at its release tag, into a folder of your own (here `~/hypha`):

```sh
git clone --depth 1 --branch vX.Y.Z https://github.com/mycelium-cmbse/mycelium-hypha.git ~/hypha
```

Replace `vX.Y.Z` with the newest plugin release. Two folders in it matter:

- `bin/` holds the launcher: `hypha-launch.exe` on Windows, the script `hypha-launch` on macOS and
  Linux. On its first start it downloads the `hypha` version this release names and keeps it in the
  `hypha` data folder, so nothing else needs installing.
- `skills/` holds the skills, one folder each (`metamodel-lookup`, `spec-citation`, ...).

A clone keeps the launchers' executable bit; a downloaded zip archive may not.

**If `hypha` is already installed** (`dotnet tool install --global Hypha.Tools`, or the
self-contained executable), the server can run without the launcher: use `hypha` as the command and
`["serve"]` as its arguments in the snippets below. You then update `hypha` yourself.

## 2. Configure the client

The configuration names the launcher by its absolute path, since a client does not expand `~`.
Below, `<launcher>` stands for one of:

| Platform | `<launcher>` |
| --- | --- |
| Windows | `C:\\Users\\<you>\\hypha\\bin\\hypha-launch.exe` (JSON needs each `\` doubled) |
| macOS, Linux | `/Users/<you>/hypha/bin/hypha-launch` or `/home/<you>/hypha/bin/hypha-launch` |

The launcher needs no arguments: it starts `hypha serve`.

### JetBrains Junie

Add the server in **Settings | Tools | Junie | MCP Settings**, or write it to
`~/.junie/mcp/mcp.json` (Windows: `%USERPROFILE%\.junie\mcp\mcp.json`) for every project, or to
`.junie/mcp/mcp.json` in one project:

```json
{
  "mcpServers": {
    "hypha": { "command": "<launcher>", "args": [] }
  }
}
```

**Skills:** copy each folder of `skills/` into `~/.junie/skills/` (Windows:
`%USERPROFILE%\.junie\skills\`), or into the shared `~/.agents/skills/`. `/skills` lists what Junie
found.

### Devin Desktop (formerly Windsurf)

Write the server to `~/.config/devin/mcp_config.json` (Windows: `%APPDATA%\devin\mcp_config.json`)
for yourself, or to `.devin/mcp_config.json` in one project:

```json
{
  "mcpServers": {
    "hypha": { "command": "<launcher>", "args": [] }
  }
}
```

A Windsurf build from before the rename reads `~/.codeium/windsurf/mcp_config.json`, in the same
format.

**Skills:** copy each folder of `skills/` into `~/.config/devin/skills/` or the shared
`~/.agents/skills/` (Windsurf: `~/.codeium/windsurf/skills/`).

### Zed

Add the server under `context_servers` in Zed's `settings.json` (**Settings | AI | MCP Servers**
shows whether it started):

```json
{
  "context_servers": {
    "hypha": { "command": "<launcher>", "args": [], "env": {} }
  }
}
```

**Skills:** copy each folder of `skills/` directly into `~/.agents/skills/`, so that each `SKILL.md`
is one level down (`~/.agents/skills/metamodel-lookup/SKILL.md`): Zed does not look in nested
folders. Skills apply to the Zed Agent, not to external agents.

### Cline

Open **MCP Servers | Configure | Configure MCP Servers**, which opens `cline_mcp_settings.json`
wherever your install keeps it, and add:

```json
{
  "mcpServers": {
    "hypha": { "command": "<launcher>", "args": [], "disabled": false }
  }
}
```

**Skills:** copy each folder of `skills/` into `~/.cline/skills/` (Windows:
`%USERPROFILE%\.cline\skills\`), or into `.cline/skills/` in one project.

### Roo Code

Roo Code was discontinued in May 2026; Cline, above, is the client its maintainers recommended
moving to.

## 3. First use

Ask, for example, "what SysML v2 releases are available?" The server reports that nothing is
installed yet, names what is offered upstream and asks which release to install; installing one
downloads and generates it, which takes a few minutes. Without skills, the server's own instructions
tell the client which tool answers what and how far each answer can be trusted.

## Updating

Pull the newer plugin tag into the same folder (`git -C ~/hypha fetch --depth 1 origin tag vX.Y.Z`,
then `git -C ~/hypha checkout vX.Y.Z`) and copy the skills again. The launcher then downloads the
`hypha` that release names on its next start. The installed releases are kept.
