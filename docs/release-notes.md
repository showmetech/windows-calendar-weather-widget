## SMT Calendar Widget 1.9.0

- Three primary settings sections: Widget, Weather and Calendar, with native keyboard navigation and focused secondary flows.
- Fluent 2 typography, opaque surfaces, accessible accent contrast, themed switches and visible focus/hover states.
- Explicit current location, separate city search/confirmation, clear empty and failure states, and refresh progress feedback.
- Calendar labels and readable event titles. Account/calendar pagination keeps settings compact.
- Optional reduced motion, respecting Windows animation preferences. Adaptive settings placement stays within the work area on smaller screens.
- Regression coverage for light/dark themes, 320/360/540 DIP widths, 420/460 DIP heights, 19 calendars, 4,096 accent colors and mixed-DPI placement calculations.

Each user must authorize their own Google account. Offline tests and layout captures do not prove live authorization, installation, next-logon startup or Microsoft Store certification.

## SMT Calendar Widget 1.8.1

- Redesigned account settings with a named Google connection button, cancellation, retry messages and a separate flow for calendar subscriptions and iCalendar files.
- Google connection uses the product's dedicated Desktop client, PKCE and a local browser return. Normal sign-in no longer asks users to locate a developer JSON file.
- Compact settings tabs with Fluent icons, complete error messages and a height limit on large displays; location, refresh intervals and calendar selection remain within the tested panel.
- Fixed callback response closure, repeated connection attempts, refresh races after location/account changes, empty Google calendar lists and reopening settings during closing animations.
- Consistent SMT Calendar Widget public naming while preserving package, startup and tray identities and existing preferences.
- Self-contained Windows 11 x64 and ARM64 packages; isolated regression tests run on x64 and native ARM64 CI.

Each user authorizes their own Google account. Account tokens and calendar caches are not distributed. Google consent-screen verification, Microsoft Store certification and availability on individual computers are separate processes; this release does not certify them as completed.
