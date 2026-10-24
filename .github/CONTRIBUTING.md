# How to contribute

We would like to start with saying thank you for wanting to contribute to the mycelium-hypha codebase. We want to keep it as easy as possible to contribute changes that get things working in your environment. There are a few guidelines that we need contributors to follow so that we have a chance of keeping on top of things.

- [Making Changes](#making-changes)
  - [Handling Updates from Upstream/Development](#handling-updates-from-upstreamdevelopment)
  - [Sending a Pull Request](#sending-a-pull-request)
- [Building and Testing](#building-and-testing)
  - [Prerequisites](#prerequisites)
  - [.NET](#net)
  - [Python](#python)
- [Style Guidelines](#style-guidelines)

## Making Changes

1. [Fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo) on GitHub
1. Clone your fork locally
1. Configure the upstream repo (`git remote add upstream https://github.com/mycelium-cmbse/mycelium-hypha.git`)
1. Checkout development
1. Create a local branch named after the issue it addresses (`git checkout -b GH<issue-number>`) from development
1. Work on your feature
1. Rebase if required (see below)
1. Push the branch up to GitHub (`git push origin GH<issue-number>`)
1. Send a Pull Request on GitHub into `development`

You should **never** work on a clone of main or development, and you should **never** send a pull request from main or development – always from a branch. The reasons for this are detailed below.

### Handling Updates from Upstream/Development

While you're working away in your branch it's quite possible that your upstream development (most likely the canonical mycelium-hypha version) may be updated. If this happens you should:

1. [Stash](https://git-scm.com/book/en/v2/Git-Tools-Stashing-and-Cleaning) any un-committed changes you need to
1. `git checkout development`
1. `git pull upstream development`
1. `git checkout GH<issue-number>`
1. `git rebase development GH<issue-number>`
1. `git push origin development` – (optional) this makes sure your remote development is up to date

This ensures that your history is "clean" i.e. you have one branch off from development followed by your changes in a straight line. Failing to do this ends up with several "messy" merges in your history, which we don't want. This is the reason why you should always work in a branch and you should never be working in, or sending pull requests from, development.

If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

### Sending a Pull Request

While working on your feature you may well create several branches, which is fine, but before you send a pull request you should ensure that you have rebased back to a single "Feature branch". We care about your commits, and we care about your feature branch; but we don't care about how many or which branches you created while you were working on it :smile:.

When you're ready to go you should confirm that you are up to date and rebased with upstream/development (see "Handling Updates from Upstream/Development" above), and then:

1. `git push origin GH<issue-number>`
1. Send a descriptive [Pull Request](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request) on GitHub into `development` – making sure you have selected the correct branch in the GitHub UI!
1. Wait for a maintainer to review and merge your changes.

Commit subjects follow `[Verb] <summary>; fixes #<n>`, for example `[Fix] emit operation bodyCondition OCL in the metamodel knowledge base; fixes #143`.

And remember; **A pull-request with tests is a pull-request that's likely to be pulled in.** :grin: Bonus points if you document your feature in our [wiki](https://github.com/mycelium-cmbse/mycelium-hypha/wiki) once it has been pulled in.

## Building and Testing

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) for `tools/metamodel-gen`, `tools/knowledge-gen` and `tools/hypha-cli`
- Python 3.12 or later for `tools/spec-extract`

### .NET

From the repository root:

```sh
dotnet build mycelium-hypha.sln
dotnet test mycelium-hypha.sln
```

A normal test run never reaches the network and leaves the working tree clean. The `Live` category really fetches from GitHub; it is `[Explicit]` and opt-in, so set `GITHUB_TOKEN` before running it with `--filter "TestCategory=Live"`.

### Python

From `tools/spec-extract/`:

```sh
python -m venv .venv && . .venv/Scripts/activate   # or source .venv/bin/activate
pip install -e .[dev]
ruff check .
pytest
```

## Style Guidelines

- **C#:** block-scoped namespaces with `using`s inside the namespace, one public type per file, NUnit tests. Every source file opens with the Apache-2.0 copyright header (Starion Group S.A.).
- **Python:** ruff-clean; each file carries the `# Copyright … / # SPDX-License-Identifier: Apache-2.0` header.
- **Generated artifacts** must be byte-stable: ordinal sorting, LF line endings, and no timestamps, machine paths or culture-sensitive formatting.
- **Prose and docs** use a spaced en dash (` – `), not an em dash.
