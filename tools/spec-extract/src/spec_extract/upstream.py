# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Where hypha's inputs come from.

Release discovery, commit resolution and fetching now live in ``Hypha.Knowledge`` on the .NET side
(see ``tools/knowledge-gen``). What is left here is the repository name itself, which the generators
still need in order to link images and sources back to the tag they were rendered from.
"""

from __future__ import annotations

RELEASE_REPO = "Systems-Modeling/SysML-v2-Release"
"""Specification PDFs, grammar, textual sources and example models."""

PILOT_REPO = "Systems-Modeling/SysML-v2-Pilot-Implementation"
"""The metamodel UML/XMI that ``metamodel-gen`` consumes."""
