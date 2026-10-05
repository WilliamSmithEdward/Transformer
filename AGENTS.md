# Notes for agents

<!-- repo-standards:begin. Copied from WilliamSmithEdward/repo-standards, templates/agents/AGENTS-block.md. Change it there; the weekly rescan fails a copy that differs. -->
## Releases, CI and security

These rules are the same in every WilliamSmithEdward repository.

- **How a release happens here:** pushing a `vX.Y.Z` tag runs Publish, which builds the release files in CI and creates the GitHub release with them, their signed provenance and the security reports. Any other step, such as a marketplace upload, is described elsewhere in this file.
- **Starting a workflow by hand never releases anything.** Publish and every
  release report are dry runs when started with `gh workflow run` or the Run
  workflow button. They build, scan and assemble the release files exactly
  as a release would, and upload them as the `release-preview` artifact
  instead. Run one after changing anything on the release path:
  `gh workflow run <file> --ref main`, then
  `gh run download <run-id> -n release-preview`.
- **Do not create, publish, edit or delete a release or a `v*` tag** unless
  the owner asks for it. A `v*` tag cannot be moved or deleted once pushed.
- **Every change to `main` goes through a pull request** that passes CI
  passed, Security passed and Malware scan passed. No one can push to `main`
  directly or skip the checks, admins included. Push a branch, open a pull
  request, and let it merge itself: `gh pr merge --auto --squash <number>`.
- **Pins.** Actions by full commit SHA with the version as a comment. Images
  by digest, in `.github/security/<tool>/Dockerfile`. Python tools from the
  hash-locked `.github/requirements/<purpose>.txt`, compiled from the `.in`
  beside it with
  `uv pip compile <purpose>.in --universal --generate-hashes --python-version 3.12 -o <purpose>.txt`.
  Runners are named releases, never `-latest`.
- **Updates merge themselves.** Dependabot and the Update YARA rules workflow
  open pull requests that merge once the three checks pass, except a
  third-party major version, which waits for the owner. Leave them alone
  unless asked.
- **A scanner finding is fixed or accepted with a written reason** in the
  repository's accepted list. Never silence a scanner without one.
<!-- repo-standards:end -->

## This repository

Transformer is a .NET library of extension methods that convert values
between types, convert collections, turn a list of objects into a
`DataTable` and print one. It is published to nuget.org for net8.0, net9.0
and net10.0. What an agent working here must not break:

- **The package id is not the repository name.** The repository is
  `Transformer`, and so are the namespace, the assembly (`Transformer.dll`)
  and the project folder, but the package on nuget.org is
  `WilliamSmithE.Transformer` (`PackageId` in the csproj). The package
  called `Transformer` on nuget.org belongs to someone else: never
  reference, install or push to it. Every name on the release path uses the
  package id: Publish's package check, the `.nupkg` it pushes, the release
  file names, the sigstore bundle, nuget.org URLs and the README badges.
  Repository URLs use `Transformer`.
- **The release path.** A release starts from a `vX.Y.Z` tag that matches
  `PackageVersion` in `Transformer/Transformer.csproj` (keep
  `AssemblyVersion` the same); Publish refuses any other. Its notes are the
  version's section of `CHANGELOG.md` (`## [X.Y.Z] - date`), written before
  the tag is pushed; without one the release fails. The package goes to
  nuget.org through trusted publishing: nuget.org's policy for
  `WilliamSmithE.Transformer` is bound to `publish.yml` and the `nuget`
  environment, so both keep their names, and no API key is stored anywhere.
- **One README for GitHub and NuGet.** The root `README.md` is packed
  directly as the nuget.org readme. Keep links and image URLs absolute,
  use NuGet-supported image hosts, and serve the Scorecard badge through
  `img.shields.io`. Do not add a separate package README.
  Compile and run a changed README sample against the library before
  committing it. Check what it prints.
- **The lock files.** Restores run with `--locked-mode` against each
  project's `packages.lock.json`. The library has no package dependencies,
  so its lock file holds three empty framework sections; keep them. A new or
  changed package reference is restored without locked mode once, and the
  updated lock file committed with it. Every lock file must keep a section
  for each of the three target frameworks: a Dependabot NuGet update once
  rewrote Exceleration's with one framework only, which broke every locked
  restore; regenerate it with `dotnet restore Transformer.sln
  --force-evaluate` when that happens.
- **Three target frameworks.** The library targets net8.0, net9.0 and
  net10.0, and CI checks the package holds each one's dll and XML docs.
  Microsoft ends support for net8.0 and net9.0 on 2026-11-10. Change the
  list only on the owner's decision, and update `ci.yml`, `publish.yml`, the
  READMEs and this file with it.
- **Tests.** `Transformer.Tests` is an xUnit v3 project run by
  Microsoft.Testing.Platform (`global.json` opts `dotnet test` in), on
  net8.0, net9.0 and net10.0:
  `dotnet test --solution Transformer.sln -c Release --fail-skips on`.
  Conversions read text in the current culture, so a test that depends on
  one sets it with `CultureScope` (in `TestSupport.cs`), such as de-DE or
  tr-TR, rather than inheriting the machine's. Nothing touches the network.
  CI runs them with `--fail-skips on`. A fix comes with a test that fails
  without it.
- **XML docs.** CI builds with warnings as errors, so every public member
  needs an XML doc comment.
