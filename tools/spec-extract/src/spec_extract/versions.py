# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Discover the KerML / SysML v2 releases hypha can be generated for.

A hypha version is one upstream **tag name** that resolves in *both* repositories: the Release repo
(specification PDFs, grammar, textual sources) and the Pilot-Implementation repo (metamodel XMI).

The tag is the only version identifier hypha uses. The model URI carried inside the XMI
(``…/SysML/20250201``) is deliberately ignored: it tracks neither the release nor the content – the
2026-05 metamodel still declares a 2025 URI. A consequence, and an accepted one, is that the same
model may appear under several tags; each tag is still a version in its own right.

Only ``fetch_tags`` touches the network; everything else is a pure function so the selection rules
can be unit-tested without GitHub.
"""

from __future__ import annotations

import json
import os
import re
import urllib.request

RELEASE_REPO = "Systems-Modeling/SysML-v2-Release"
PILOT_REPO = "Systems-Modeling/SysML-v2-Pilot-Implementation"

# YYYY-MM, optionally a point release (2025-09.1). Everything else upstream publishes - pre-releases
# (2026-05-pre), internal drops (2021-08-internal) and letter revisions (2021-05a) - is excluded.
_RELEASE_TAG = re.compile(r"^(?P<year>\d{4})-(?P<month>\d{2})(?:\.(?P<point>\d+))?$")

_API = "https://api.github.com/repos/{repo}/tags?per_page=100&page={page}"


def is_release_tag(tag: str) -> bool:
    """True for a tag hypha will offer as a version: ``YYYY-MM`` with an optional ``.N``."""
    return _RELEASE_TAG.match(tag) is not None


def sort_key(tag: str) -> tuple[int, int, int]:
    """Chronological ordering key; a point release sorts after its base tag."""
    match = _RELEASE_TAG.match(tag)
    if match is None:
        raise ValueError(f"not a release tag: {tag}")
    return (
        int(match.group("year")),
        int(match.group("month")),
        int(match.group("point") or 0),
    )


def available_versions(release_tags: list[str], pilot_tags: list[str]) -> list[str]:
    """The tags hypha can generate: present in **both** upstreams and release-shaped, newest first.

    The two repositories are not in lockstep – some tags exist in only one of them – so this is an
    intersection, never an assumption about either side.
    """
    common = {tag for tag in release_tags if is_release_tag(tag)} & set(pilot_tags)
    return sorted(common, key=sort_key, reverse=True)


def latest(versions: list[str], count: int = 1) -> list[str]:
    """The newest ``count`` versions from an already-ordered list."""
    return versions[:count]


def default_token() -> str | None:
    """A GitHub token from the environment, if one is set.

    The anonymous API allows only 60 requests an hour, which discovery plus a couple of tag
    resolutions can exhaust; ``GITHUB_TOKEN`` / ``GH_TOKEN`` raises that to 5000. Optional: every
    repository read here is public.
    """
    return os.environ.get("GITHUB_TOKEN") or os.environ.get("GH_TOKEN") or None


def api_headers(token: str | None) -> dict[str, str]:
    """Standard GitHub API headers, authenticated when a token is available."""
    headers = {"Accept": "application/vnd.github+json", "User-Agent": "mycelium-hypha"}
    resolved = token or default_token()
    if resolved:
        headers["Authorization"] = f"Bearer {resolved}"
    return headers


def fetch_tags(repo: str, *, token: str | None = None) -> list[str]:
    """Every tag name in ``repo``, following pagination. The only networked function here."""
    headers = api_headers(token)

    names: list[str] = []
    for page in range(1, 21):  # 100 per page; upstream has well under 2000 tags
        request = urllib.request.Request(_API.format(repo=repo, page=page), headers=headers)
        with urllib.request.urlopen(request, timeout=30) as response:  # noqa: S310 - fixed https host
            batch = json.loads(response.read().decode("utf-8"))
        if not batch:
            break
        names.extend(entry["name"] for entry in batch)
    return names
