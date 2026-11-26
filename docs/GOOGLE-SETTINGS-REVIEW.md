# Google connection and settings revision

Candidate: 1.8.1. Production 1.8.0 is not evidence of this revision being installed.

## Reproduced problems and corrections

- The old installed flow exposed a client JSON picker as account login. Normal Google connection now starts product OAuth directly. Missing developer configuration reports a product error, never a file picker.
- The icon-only calendar action combined Google and subscriptions. Accounts now has a named primary Google button and a separate subscription screen with a back action, labeled inputs and an explicit iCalendar picker.
- The normal settings screen no longer exposes client import. The explicit maintenance CLI remains available to developers.
- Pending Google authorization has a cancel button, one operation at a time, disposal cancellation, recoverable errors and focus return. Account identity is checked before persistence; an existing matching account is replaced without changing its key.
- OAuth callback response used implicit framing; the new loopback integration test reproduced connection reset when the listener disposed immediately on error/cancellation. Explicit content length and connection closure fixed the test. Invalid state and denied permission cannot exchange a code or save an account.
- Settings retain five compact icon/text tabs, 4/8/16/20 DIP spacing, explicit location heading, refresh guidance, complete live status text and no general scrollbar. Existing preferences and identities remain unchanged.

## Coverage

The offline Windows executable exercises real loopback HTTP callbacks with synthetic OAuth exchange/user-info responses: PKCE challenge/verifier, state, redirect, identity, denial, invalid state, browser failure and pending cancellation. UI handler tests cover no JSON picker, cancellation, repeated clicks, separate subscription navigation and return. Existing weather/cache, startup/recovery, calendar/ICS, keyboard/navigation, animation and settings-fit tests remain required.

These fixtures do not prove real Google consent, installed account persistence, native visual behavior on a notebook, Google verification or Store delivery. Release requires the existing product Desktop client in CI, authenticated new-user consent, real calendar read and reopening, then the canonical GitHub/Store delivery path. No user tokens, private profiles or personal OAuth files belong in the public artifacts.

References: [Fluent layout](https://fluent2.microsoft.design/layout), [Google Desktop OAuth](https://developers.google.com/identity/protocols/oauth2/native-app).
