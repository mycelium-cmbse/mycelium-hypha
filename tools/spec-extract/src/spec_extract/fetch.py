# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Fetch one release's inputs from the two upstreams into ``sources/<tag>/``.

Which files come from where:

* **metamodel XMI** – ``org.omg.sysml/model/{KerML,SysML}_only_xmi.uml`` from the Pilot repo.
  ``PrimitiveTypes.xmi`` is deliberately *not* fetched: it is the OMG UML primitives library
  (``…/PrimitiveTypes/20161101``), published by neither upstream and identical for every release, so
  it stays shared at ``sources/PrimitiveTypes.xmi``.
* **grammar + textual models** – ``bnf/*.kebnf``, ``bnf/*.kgbnf`` and the ``kerml/`` and ``sysml/``
  model trees from the Release repo.
* **specification PDFs** – ``doc/*.pdf`` from the Release repo. These are OMG-copyrighted: they are
  written into a git-ignored folder and are never committed or redistributed. Fetching them is the
  same act the README otherwise asks the user to perform by hand, so it is opt-in.

Path selection is a pure function of a repository listing, so it is unit-tested without the network.
"""

from __future__ import annotations

import json
import time
import urllib.error
import urllib.parse
import urllib.request
from collections.abc import Callable
from pathlib import Path

from spec_extract.versions import PILOT_REPO, RELEASE_REPO, api_headers

_RAW = "https://raw.githubusercontent.com/{repo}/{tag}/{path}"
_TREE = "https://api.github.com/repos/{repo}/git/trees/{tag}?recursive=1"

#: Upstream path -> the name it is stored under in ``sources/<tag>/xmi/``.
XMI_FILES = {
    "org.omg.sysml/model/KerML_only_xmi.uml": "KerML_only_xmi.uml",
    "org.omg.sysml/model/SysML_only_xmi.uml": "SysML_only_xmi.uml",
}

#: The OMG UML primitives library, shared by every version rather than fetched per tag.
SHARED_XMI = "PrimitiveTypes.xmi"

GRAMMAR_SUFFIXES = (".kebnf", ".kgbnf")
TEXTUAL_ROOTS = ("kerml/", "sysml/")
TEXTUAL_SUFFIXES = (".kerml", ".sysml")

SPEC_PDFS = (
    "doc/1-Kernel_Modeling_Language.pdf",
    "doc/2a-OMG_Systems_Modeling_Language.pdf",
    "doc/3-Systems_Modeling_API_and_Services.pdf",
)


def select_textual_paths(tree_paths: list[str]) -> list[str]:
    """The grammar and model files hypha needs, from a full Release-repo listing.

    Pure and order-stable: the grammar under ``bnf/`` (but not its HTML rendering, CSS or SVGs), and
    the ``.kerml`` / ``.sysml`` models under ``kerml/`` and ``sysml/``.
    """
    grammar = [
        path for path in tree_paths if path.startswith("bnf/") and path.endswith(GRAMMAR_SUFFIXES)
    ]
    models = [
        path
        for path in tree_paths
        if path.startswith(TEXTUAL_ROOTS) and path.endswith(TEXTUAL_SUFFIXES)
    ]
    return sorted(grammar) + sorted(models)


def local_textual_path(upstream_path: str) -> str:
    """Where an upstream textual path is stored under ``sources/<tag>/textual/`` (layout preserved)."""
    return upstream_path


def list_tree(repo: str, tag: str, *, token: str | None = None) -> list[str]:
    """Every blob path in ``repo`` at ``tag``. Networked."""
    request = urllib.request.Request(_TREE.format(repo=repo, tag=tag), headers=api_headers(token))
    with urllib.request.urlopen(request, timeout=60) as response:  # noqa: S310 - fixed https host
        payload = json.loads(response.read().decode("utf-8"))

    if payload.get("truncated"):
        raise RuntimeError(f"tree listing for {repo}@{tag} was truncated by the API")

    return [entry["path"] for entry in payload["tree"] if entry["type"] == "blob"]


def read_url(url: str, timeout: int = 120) -> bytes:
    """Fetch one URL's bytes. Networked; injected into :func:`download` so retries stay testable."""
    request = urllib.request.Request(url, headers={"User-Agent": "mycelium-hypha"})
    with urllib.request.urlopen(request, timeout=timeout) as response:  # noqa: S310 - fixed https host
        return response.read()


def download(
    repo: str,
    tag: str,
    path: str,
    destination: Path,
    *,
    retries: int = 4,
    skip_existing: bool = False,
    reader: Callable[[str], bytes] = read_url,
    sleep: Callable[[float], None] = time.sleep,
) -> Path:
    """Download one file from ``repo`` at ``tag`` to ``destination``.

    A release carries a few hundred files, and fetching them back to back gets the connection reset
    by the host, so transient failures are retried with exponential backoff rather than aborting the
    whole run. ``skip_existing`` makes a re-run resume instead of starting over.
    """
    if skip_existing and destination.is_file() and destination.stat().st_size > 0:
        return destination

    destination.parent.mkdir(parents=True, exist_ok=True)
    url = _RAW.format(repo=repo, tag=tag, path=urllib.parse.quote(path))

    for attempt in range(retries):
        try:
            destination.write_bytes(reader(url))
            return destination
        except (urllib.error.URLError, ConnectionError, TimeoutError):
            if attempt == retries - 1:
                raise
            sleep(2.0**attempt)

    return destination  # unreachable: the loop either returns or raises


def fetch_metamodel(tag: str, sources_root: Path, *, skip_existing: bool = False) -> list[Path]:
    """Fetch the metamodel XMI for ``tag`` into ``sources_root/<tag>/xmi/``.

    Needs no token: the file paths are known, so this never calls the rate-limited API.
    """
    return [
        download(PILOT_REPO, tag, upstream, sources_root / tag / "xmi" / name, skip_existing=skip_existing)
        for upstream, name in sorted(XMI_FILES.items())
    ]


def fetch_textual(
    tag: str, sources_root: Path, *, token: str | None = None, skip_existing: bool = False
) -> list[Path]:
    """Fetch the grammar and textual models for ``tag`` into ``sources_root/<tag>/textual/``."""
    paths = select_textual_paths(list_tree(RELEASE_REPO, tag, token=token))
    root = sources_root / tag / "textual"
    return [
        download(RELEASE_REPO, tag, path, root / local_textual_path(path), skip_existing=skip_existing)
        for path in paths
    ]


def fetch_specs(tag: str, sources_root: Path, *, skip_existing: bool = False) -> list[Path]:
    """Fetch the OMG specification PDFs for ``tag`` into the **git-ignored** ``.../specs/`` folder.

    The PDFs are copyrighted and must never be committed; this only automates the download the user
    is otherwise told to perform by hand.
    """
    root = sources_root / tag / "specs"
    return [
        download(RELEASE_REPO, tag, path, root / Path(path).name, skip_existing=skip_existing)
        for path in SPEC_PDFS
    ]
