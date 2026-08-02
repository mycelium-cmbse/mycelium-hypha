# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""The committed record of which releases this checkout carries.

``knowledge/versions.json`` is what the skills read to know which versions exist, which one to answer
from by default, and – for traceability only – which upstream commit each tag resolved to. The
**tag** is the identifier; the commits are provenance, and the model URI inside the XMI is not used
at all.

Pure functions plus one networked resolver, in the same shape as the rest of the pipeline.
"""

from __future__ import annotations

import json
import urllib.request
from pathlib import Path

from spec_extract.versions import PILOT_REPO, RELEASE_REPO, api_headers, sort_key

SCHEMA_VERSION = "1.0.0"

_COMMIT_API = "https://api.github.com/repos/{repo}/commits/{tag}"


def build_manifest(default: str, resolved: dict[str, dict[str, str]]) -> dict:
    """Assemble the manifest for the installed versions.

    ``resolved`` maps each tag to ``{"release": <sha>, "pilot": <sha>}``. Versions are listed newest
    first, which is also the order a chooser should offer them in.
    """
    if default not in resolved:
        raise ValueError(f"default {default!r} is not among the resolved versions")

    versions = [
        {
            "tag": tag,
            "release": {"repo": RELEASE_REPO, "commit": resolved[tag]["release"]},
            "pilot": {"repo": PILOT_REPO, "commit": resolved[tag]["pilot"]},
        }
        for tag in sorted(resolved, key=sort_key, reverse=True)
    ]

    return {"schemaVersion": SCHEMA_VERSION, "default": default, "versions": versions}


def tags(manifest: dict) -> list[str]:
    """The installed tags, newest first."""
    return [version["tag"] for version in manifest["versions"]]


def render(manifest: dict) -> str:
    """Render deterministically: builder key order, two-space indent, trailing newline."""
    return json.dumps(manifest, indent=2, ensure_ascii=False) + "\n"


def write(manifest: dict, path: str | Path) -> Path:
    """Write the manifest with LF endings."""
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(render(manifest), encoding="utf-8", newline="\n")
    return path


def read(path: str | Path) -> dict:
    """Read a manifest back."""
    return json.loads(Path(path).read_text(encoding="utf-8"))


def resolve_commit(repo: str, tag: str, *, token: str | None = None) -> str:
    """The commit SHA a tag resolves to in ``repo``. The only networked function here."""
    request = urllib.request.Request(
        _COMMIT_API.format(repo=repo, tag=tag), headers=api_headers(token)
    )
    with urllib.request.urlopen(request, timeout=30) as response:  # noqa: S310 - fixed https host
        return json.loads(response.read().decode("utf-8"))["sha"]
