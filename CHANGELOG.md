# Changelog

All notable changes to mycelium-hypha are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-06-29

### Added
- Initial Claude plugin scaffolding (`.claude-plugin/plugin.json`, marketplace manifest).
- Skills: `metamodel-lookup`, `spec-citation`, `sysml-validation`.
- Subagents: `metamodel-navigator`, `spec-citation`, `sysml-validator`.
- Knowledge-base layout (`knowledge/`) for KerML/SysML v2 metamodel elements,
  normative spec excerpts, and textual-notation examples.
- Source-document layout (`sources/`) for OMG XMI and PDF specifications.
- Generation pipelines layout (`tools/`): C#/uml4net metamodel generator and
  Python PDF spec extractor.

### Changed
- Renamed the metamodel knowledge folder `knowledge/sysml2/` → `knowledge/metamodel/`
  (consistent content-type naming alongside `spec/` and `textual-notation/`, and no longer
  overloaded with the `knowledge/spec/sysml2/` clause folder).
