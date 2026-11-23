# Quality review — 1.8.1

This is a release candidate, not a claim of universal compatibility or complete user acceptance.

## Reproduced defects and regression evidence

| Defect | Cause | Fix | Regression |
|---|---|---|---|
| Connect calendar opens Visual instead of Accounts | Generic settings action always selects page zero | Dedicated account navigation | connectOpensAccountsPage: false before, true after |
| Location denial/status is invisible | RefreshState unconditionally collapses the status | Bounded visible status and full tooltip | locationFailureVisible: false before, true after |
| Rapid reopen leaves settings closed | Closing animation outlives a new open request | Complete the previous close before reopening; protect stale callbacks | settingsCanReopenDuringClosingAnimation: false before, true after |
| City change/manual refresh is dropped during an active request | Busy returns immediately; response uses mutable settings | Queue forced refresh, capture requested coordinates/name, discard stale responses | refreshDoesNotLoseNewCityRequest: false before, true after |
| Google account with an empty calendar list fails | Assumes the optional items array is present | Treat missing items as an empty list | emptyGoogleCalendarList: false before, true after |
| ICS without URL looks interactive; event action lacks keyboard control | Every event has a mouse-only action | HTTPS-only action rendered as a named Button; plain cards otherwise | eventLinksUseHttpsOnly; native keyboard review pending |
| Installed names differ from Store | Saved legacy displayName, release fallback and tray tooltip | Versioned name overrides saved displayName without rewriting the configuration; app title, callback and tooltip aligned | Legacy-file and environment regressions, three manifest names, identity/Publisher/TaskId preservation; installed update pending |

Weather HTTP 503 retains the previous cache byte-for-byte and shows a failure state. Tests use controlled offline handlers, not real Google credentials.

## Coverage and release gates

| Surface / flow | Automated coverage | Remaining native / real-service gate |
|---|---|---|
| Main weather panel, seven-day forecast | Offline contract, cache retention and refresh race | Real refresh on the final installed build |
| Event list | Current event, all-day, timezone, two cards, light/dark renders | Long names, keyboard action and real account events on final build |
| Visual settings | Theme renders, opacity default, settings layout | Native focus order, contrast/high-contrast and opacity interaction |
| Location settings | Visible failure, city label, manual-refresh race | Permission allow/deny and real city search on final build |
| Refresh settings | Both intervals, five-minute default, persistence | Native control interaction |
| Accounts settings | Correct entry page, seven fixture accounts, pagination | OAuth import/cancel/browser return and ICS file chooser |
| Calendar settings | Nineteen fixture calendars and pagination without scrolling | Real account/calendar selection persistence |
| Settings overlay | Above-widget geometry, interrupted close/reopen, return | Z-order with other apps, small work areas, focus after dialogs |
| Tray / lifecycle | Hover dispatch, menu dispatch, reopen, intentional exit and isolated crash recovery | Physical tray input and next Windows login |
| ICS | Recurrences, exceptions, cancellation, DST, all-day exclusive end, URL validation and protected feed address | User-authorized subscription on final installation |
| Google | Read-only GETs, protected-account refresh mock, empty calendar list | Per-machine client and first user consent; real reads and reopening |
| Distribution | Self-contained x64/ARM64, private-file rejection, MSIX semantic validation | Native ARM64 run, Store certification, install/update and rollback readback |

The notebook's installed 1.8.0.0 separately passed its isolated regression suite and 17 runtime-handler probes. These results do not certify 1.8.1. The public identity, startup TaskId, tray GUID and data paths are preserved.

## Review boundaries

- No personal OAuth JSON, account tokens, data caches, private certificate or debug symbols belong in source or distribution.
- Do not copy protected credentials between computers. First consent belongs to the account owner.
- Keep existing cache during network failure. Never call a mock authenticated Google verification.
- No Windows session restart, global DPI/theme change, registry/tray reset or other-app process termination is part of this review.
- Use the Codex browser for Partner Center. If unavailable, do not represent a local MSIX or GitHub commit as a Store submission.

Design review follows Fluent 2 structure, meaningful text, keyboard/focus and contrast guidance: https://fluent2.microsoft.design/accessibility . Rendered fixture images are evidence of the exercised layout, not of the user's full desktop composition.
