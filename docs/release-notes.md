## SMT Calendar Widget 1.8.1

- Redesigned account settings with a named Google connection button, cancellation, retry messages and a separate flow for calendar subscriptions and iCalendar files.
- Google connection uses the product's dedicated Desktop client, PKCE and a local browser return. Normal sign-in no longer asks users to locate a developer JSON file.
- Compact settings tabs with Fluent icons, complete error messages and a height limit on large displays; location, refresh intervals and calendar selection remain within the tested panel.
- Fixed callback response closure, repeated connection attempts, refresh races after location/account changes, empty Google calendar lists and reopening settings during closing animations.
- Consistent SMT Calendar Widget public naming while preserving package, startup and tray identities and existing preferences.
- Self-contained Windows 11 x64 and ARM64 packages; isolated regression tests run on x64 and native ARM64 CI.

Each user authorizes their own Google account. Account tokens and calendar caches are not distributed. Google consent-screen verification, Microsoft Store certification and availability on individual computers are separate processes; this release does not certify them as completed.
