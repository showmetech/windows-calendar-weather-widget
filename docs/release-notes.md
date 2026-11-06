## Distribution preparation

- Windows 11 x64 and ARM64 builds, with the .NET runtime included.
- MSIX-aware startup and per-user writable maintenance files.
- A new installation asks for a location instead of silently using a preset city.
- Isolated regression tests with deterministic weather responses for CI.
- Versioned artifacts, SHA-256 hashes and Microsoft Store submission workflow.

The Microsoft Store listing, publisher identity, first certification and publishing credentials must be configured before a Store release can succeed. ARM64 is compiled; physical ARM64 validation remains required. Google Calendar authorization is performed by each user; no personal tokens or publisher client credentials are included.
