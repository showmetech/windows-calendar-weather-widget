# Product OAuth configuration

Normal Google connection calls the product's OAuth flow directly. It never opens a file picker. Without product configuration it shows an explicit developer-configuration error; it does not open an unrelated Google login page or claim that the account is connected.

The separate **AvanÃ§ado: configurar Google** action imports a Desktop client for support/development only. It does not authorize an account. Existing per-user protected client settings and existing account refresh credentials remain compatible.

## Required product provisioning

Use the already-authorized Desktop client belonging to the dedicated `showmetech-calendar-weather` product project. Do not create another client, copy user tokens, or reuse credentials from another application. Confirm the project/client ownership in Google Cloud before provisioning.

The release builder accepts these MSBuild/environment properties:

- `CalendarGoogleDesktopClientId`
- `CalendarGoogleDesktopClientSecret`

GitHub Actions maps `vars.CALENDAR_GOOGLE_DESKTOP_CLIENT_ID` and `secrets.CALENDAR_GOOGLE_DESKTOP_CLIENT_SECRET` to those properties. This change does not create/set either value. Both must be provided by the product developer using the existing product client. No actual values are checked into this repository.

Only the **Desktop public client** configuration belongs here. Google documents installed apps as public clients whose client secret cannot be confidential in the distributed binary. Never supply a web-server secret, service-account key, account access/refresh token, personal OAuth JSON or protected user store. The build emits the two public Desktop-client settings as assembly metadata; the user is not asked to find a JSON file for the normal connection.

Google consent-screen publication/testing/verification must permit the intended user audience and the Calendar read-only scopes. Configuring metadata is not Google verification or user consent. Each user still completes first consent in the system browser. The app uses PKCE, state and a loopback redirect and stores resulting account credentials with per-user DPAPI.

The remaining developer provisioning is the existing product Desktop client's configuration in the release build, plus confirmation of its allowed Google audience. A protected client file from one user's Windows profile is not a portable distribution configuration. No such file or account token is copied by this change.

Official flow: https://developers.google.com/identity/protocols/oauth2/native-app . Real login, account confirmation, events and persistence after reopening remain required acceptance tests.
