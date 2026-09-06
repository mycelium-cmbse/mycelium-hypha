# hooks/native/

Populated by `.github/workflows/hook-binaries.yml`, not by hand: run that workflow (manual
`workflow_dispatch`) after any change to `tools/hypha-cli/Hypha.Tools.Hook`, and it commits

```
hooks/native/
├── win-x64/hypha-hook.exe
├── linux-x64/hypha-hook
├── osx-x64/hypha-hook
└── osx-arm64/hypha-hook
```

back onto the branch it ran against. Until that has run at least once, this folder stays empty and the
plugin's second `SessionStart` hook (the dispatch shim in `.claude-plugin/plugin.json`) silently finds
nothing to `exec` — see `tools/hypha-cli/README.md`'s "Automatic sync" section for what these binaries
do and why they are the one compiled artifact this repository commits.

The dispatch shim is a POSIX shell one-liner (`case "$(uname -s)-$(uname -m)" in ... esac`), and the
hook explicitly pins `"shell": "bash"` so Claude Code always runs it under a real bash rather than
silently falling back to PowerShell, which can't parse it. **On Windows this means Git Bash is
required** – install [Git for Windows](https://gitforwindows.org/), which bundles it, if the
`SessionStart` hook reports a missing-shell error.
