# Hypha – SysML v2 and KerML

Ask GitHub Copilot about SysML v2 and KerML using knowledge from a specific OMG release, rather
than relying on general model knowledge. Hypha looks up metamodel elements and standard-library
declarations on your machine and gives Copilot source links to cite in its answers.

## What you can do

- **Explore the metamodel** – ask about elements such as `PartUsage`, their features, inheritance,
  operations and constraints.
- **Find standard-library declarations** – look up names such as `ScalarValues::Real`.
- **Know which release answers your question** – check the default release and whether its
  generated knowledge is available.
- **Keep working across folders** – installed knowledge is shared across workspaces on the same
  VS Code host.

Hypha is a preview, published as a pre-release for Linux x64 and arm64 (including WSL and SSH
remotes).

## Get started

You need VS Code 1.100 or later and access to GitHub Copilot Chat with tools enabled.
Use a trusted local or remote workspace; browser-based VS Code and virtual workspaces are not
supported.

1. Install **Hypha – SysML v2 and KerML** from the Extensions view, or a VSIX for your platform
   with **Extensions: Install from VSIX…**.
   In WSL or SSH, install it on the remote host where your workspace runs. You do not need to
   install .NET, Python, a separate CLI or an Agent Plugin.
2. Open the Command Palette (**Ctrl+Shift+P**, or **Cmd+Shift+P** on macOS) and run
   **Hypha: Install a SysML v2 Release**.
3. Choose a release and confirm the download and generation. It becomes the default only after
  generation succeeds. The first installation needs internet access and may take several minutes.
  The first specification generation may also download `uv`, a managed Python build and the
  extraction dependencies pinned by Hypha.
4. Run **Hypha: Show Installed Releases** to check readiness. Installation progress and the data
   location appear in the **Hypha** output channel.

If you previously installed the checkout-based Hypha Agent Plugin, disable it in VS Code while
using this extension to avoid competing instructions or different data locations.

## Use Hypha in Copilot Chat

1. Open **Copilot Chat** and select **Agent** mode.
2. Open **Configure Tools** near the chat input.
3. Search for **Hypha** and enable **Hypha Release Status** and **Hypha Grounded Knowledge Lookup**
   if they are not already checked.
4. Type `#hypha` in the chat input to select a tool, or use one of the prompts below.

### Check the release and readiness

```text
Use #hyphaReleaseStatus to report the default release and artifact readiness.
Do not fetch or generate anything.
```

### Explore a metamodel element

```text
Use #hyphaLookup to explain PartUsage from the default release's metamodel.
Cite the release and source; follow pagination as needed.
```

### Find a standard-library declaration

```text
Use #hyphaLookup to find ScalarValues::Real in the default release's model library.
Cite the release and source, not general knowledge.
```

Answers should identify the release and include clickable source links. Long source pages are
returned in sections; Copilot can request the next section. If a name is ambiguous, Hypha returns
matching names rather than choosing one arbitrarily. You can also ask for a particular installed
release instead of the default.

Both tools are read-only: invoking them never downloads inputs or generates knowledge.

## Commands

Run these commands from the Command Palette:

| Command | Purpose |
| --- | --- |
| **Hypha: Check Available Releases** | Compare installed releases with upstream releases without downloading release inputs or generating knowledge. |
| **Hypha: Install a SysML v2 Release** | After confirmation, fetch and generate a release, then make it the default if generation succeeds. |
| **Hypha: Change Default Release** | Choose a different installed release as the default. |
| **Hypha: Remove a Release** | Confirm and remove one release's fetched inputs and generated knowledge from this extension host. |
| **Hypha: Show Installed Releases** | Show installed releases, the default, artifact readiness and the data location. |

## Your knowledge stays available across workspaces

Hypha stores downloaded inputs, generated knowledge and managed runtime caches in VS Code's
extension-global storage, not in your modeling workspace. Opening another folder on the same host
uses the same installed releases.

Windows, WSL, SSH hosts and other machines have separate storage; releases are not synchronized
between them. You can find the storage location with **Hypha: Show Installed Releases**.

**Uninstalling and stored data:** Uninstalling Hypha may leave downloaded releases, generated
knowledge and runtime caches on disk. Reinstalling can reuse this data. Hypha can remove individual
releases, but provides no command to clear all stored data. Use **Hypha: Show Installed Releases** to
find the data location.

The extension contains its runtime and extraction code, but no release knowledge, specification PDFs
or extracted specification text. With your confirmation, installation downloads upstream inputs and
generates knowledge locally. It may also download `uv`, a matching Python and extraction dependencies.
Source excerpts returned to Copilot are included in its chat context.

## Readiness and troubleshooting

- **No installed release:** run **Hypha: Install a SysML v2 Release**.
- **Core artifacts are incomplete:** installation may have been interrupted. Run installation again
  for the same release; existing downloaded inputs are reused.
- **Normative specification text is missing:** core lookups can still work. Check the Hypha output
  channel for extraction or download problems, then retry installation for the same release.
- **Generation cannot download its runtime:** the first specification generation needs access to
  the download sources for `uv`, managed Python and the pinned extraction dependencies. If a
  download fails or the machine is offline, generation reports the failure in the Hypha output
  channel and the new release does not become the default; retry when those sources are reachable.
- **A release-operation lock prevents changes:** wait for the other window to finish and retry.
  If a lock or its recovery was interrupted and the error persists, close all Hypha windows and
  ensure no Hypha processes are running before removing the named lock file from the data location.
- **Tools are not listed:** check that Hypha is enabled on the current workspace host, reload the
  window and open a new Agent chat. Check **Configure Tools** again.

A manifest entry alone does not mean generation finished. Status checks report artifact presence,
not a full integrity check.

## Current limits

- Hypha supports **SysML v2 and KerML**, not UML.
- Lookups provide source-backed knowledge, not compiler validation of your models.
- The lookup tool reads metamodel and model-library sources; it does not search or quote normative
  specification text, even when that text is installed.
- Package targets are Windows x64, Linux x64/arm64 and macOS x64/arm64. End-to-end validation has currently
  been completed on Linux/WSL.

For feedback and source code, visit the
[Hypha repository](https://github.com/mycelium-cmbse/mycelium-hypha).
