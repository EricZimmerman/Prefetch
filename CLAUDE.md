# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A `netstandard2.0` NuGet library that parses Windows Prefetch (`.pf`) files, format versions 17 (XP/2003) through 30/31 (Windows 10/11). Its main consumer is PECmd (https://github.com/EricZimmerman/PECmd), so changes to the public API (`PrefetchFile`, `IPrefetch`, the `Other/` types) affect it. Input may be corrupt or hostile: parse everything that is there and report problems rather than giving up or silently normalizing.

## Commands

The SDK is pinned in `global.json` (.NET 10). The library targets `netstandard2.0`; tests target `net10.0`. Both use `LangVersion 10`, so avoid newer C# syntax. Tests use NUnit 5 + FluentAssertions 7 (pinned to `[7.0.0,8.0.0)`, since 8.x requires a commercial license).

```bash
dotnet build                                                  # builds Prefetch.slnx
dotnet test                                                   # CI runs: dotnet test --configuration Release
dotnet test --filter "FullyQualifiedName~RobustnessTests"     # one fixture
dotnet test --filter "Name=TestWin10ChromePfProperties"       # one test
dotnet pack --configuration Release                           # package includes README.md and icon.png from the repo root
```

## Architecture

- **Entry point:** `PrefetchFile.Open(path)` / `Open(stream, name)` reads all bytes. A `MAM` file (Windows 10/11) is decompressed first: byte 3 holds the format (low nibble must be 4, XPRESS Huffman; the high bit means a CRC32 follows), and bytes 4-7 hold the decompressed size (capped at 64 MB). Then it checks the `SCCA` signature and dispatches on the version DWORD to `Version17` / `Version23` / `Version26` / `Version30or31` (versions 30 and 31 share one class).
- **`Versions/PrefetchBase`** does all the parsing. The version classes only supply a `Layout`: file information size, run time offset/count, run count offset, and the entry sizes of file metrics (20/32), trace chains (12/8) and volume entries (40/104/104/96). A format change usually means a new `Layout`, not new parsing code.
- Sections: header (84 bytes) → file information → file metrics → trace chains → filename strings (UTF-16, NUL-separated) → volume information. Each volume has a device path, file references and directory strings, all at offsets relative to the start of the volume information.
- File references: an 8-byte header (version 1, used in prefetch 17), or a 16-byte header (version 3, prefetch 23+) with 8 unknown bytes before the array. `MFTInformation` is a 48-bit entry number plus a 16-bit sequence number, where sequence 0 becomes `null`.
- **`XpressStream/Xpress2`** is a managed LZ77+Huffman decoder ([MS-XCA] 2.2.4). It throws `InvalidDataException` on corrupt data, which `PrefetchFile` wraps in `PrefetchParseException`. The Win10 samples are its main correctness check.
- **`PrefetchHash`** calculates the hash in the file name/header (`Header.HashValue`) from an upper-cased NT device path: an XP algorithm for version 17 and a Vista algorithm for 23+. Windows 10/11 store `\VOLUME{...}` paths, so you have to rebuild `\DEVICE\HARDDISKVOLUMEn` paths to check them. Hosting processes and packaged apps also hash part of their command line or package, so their hash can't be reproduced from the path (see `PrefetchHashTests`).
- `Open(stream, ...)` checks the first 8 bytes (`MAM` or `SCCA`) before reading the rest, and rejects input over 64 MB. PECmd relies on this to probe arbitrary alternate data streams cheaply.
- Logging goes through Serilog's static `Log`; the library doesn't configure sinks.

### Error-handling model (important)
- `PrefetchFile.Open` throws `PrefetchParseException` (with `Offset` and `Field`) only when the data isn't usable at all: it's too small, has a bad signature or an unknown version, or can't be decompressed.
- The `PrefetchBase` constructor never throws. If the header or file information can't be read, parsing stops there. Every other section, every volume, and the references and directory strings within each volume are parsed independently. A failure is added to `ParseErrors` (which also sets `ParsingError`) and leaves that list empty or partly filled. List properties are never null.
- Use the bounds-checked readers in `Helpers` (`EnsureRange`, `Slice`, `ReadInt32`, ...) for anything driven by offsets or counts in the file. They throw `PrefetchParseException` with a field name, and they check the length before allocating, so huge counts can't cause huge allocations. Out-of-range FILETIMEs go through `Helpers.FromFileTime` (null means it is reported and skipped, or becomes `DateTimeOffset.MinValue` for volume creation times).

## Tests

- `TestVersion*.cs`: assertions on real samples in `Prefetch.Test/TestFiles/<OS>/` (copied to the test output; `TestPrefetchMain` holds the paths). Tests run on Linux and Windows.
- `RobustnessTests.cs`: checks every sample, plus in-memory files from the `Build(17|23)` helper that exercise malformed input, and hand-built XPRESS Huffman streams. Add new robustness and regression tests here.

## Versioning and release

- MinVer sets the version from the git tag (bare tags like `2026.10.0`); there is no `<Version>` in the csproj. Untagged commits build as `x.y.z-alpha.0.N`.
- To release: push to `master`, then `git tag <ver> && git push origin <ver> && gh release create <ver> --verify-tag --generate-notes`.
- `.github/workflows/publish.yml` (identical to GuidMapping's) packs, validates and tests on PRs and on pushes to `master`. When a GitHub Release is published, it pushes to NuGet.org using trusted publishing (`NuGet/login` with the `NUGET_USER` repo variable; there is no API key secret).
- Dependabot (`.github/dependabot.yml`) opens monthly PRs for GitHub Actions and NuGet updates.
