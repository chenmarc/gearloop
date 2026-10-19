# Building gearloop with Codemagic

Codemagic builds the iPhone app on its Macs, so you don't need Xcode. The config is in
[`codemagic.yaml`](../codemagic.yaml) and has two workflows:

| Workflow | Runs | Needs Apple account | What it does |
| --- | --- | --- | --- |
| `gearloop check` | On every push | No | Runs the tests and builds the iPhone (simulator) and Mac apps without signing |
| `gearloop iOS to TestFlight` | When you start it | Yes | Builds a signed iPhone app and uploads it to TestFlight |

Do the steps in order. Steps 1 and 2 don't need an Apple Developer account, so you can start them
while Apple processes your enrollment.

## 1. Put the code on GitHub

Create an empty **private** repository called `gearloop` on github.com and push this folder to it.

## 2. Connect Codemagic and get the check build green

1. Sign up at [codemagic.io](https://codemagic.io) with your GitHub account.
2. **Add application** → GitHub → pick `gearloop` → project type **Other** → it finds `codemagic.yaml`.
3. Start the **gearloop check** workflow.

It installs .NET 10 and the MAUI workloads, runs the tests, and compiles the app. If it fails,
copy the error from the log into the thread and it gets fixed in the code.

## 3. Join the Apple Developer Program

Enroll at [developer.apple.com/programs](https://developer.apple.com/programs/) ($99 per year).
Approval can take a day or two.

## 4. Create the app in Apple's portals

1. **Bundle ID:** in [Certificates, Identifiers & Profiles](https://developer.apple.com/account/resources/identifiers/list),
   add an App ID with bundle ID `com.marcchenard.gearloop`. (To use a different one, change it in
   `codemagic.yaml` and `ApplicationId` in `src/Gearloop.App/Gearloop.App.csproj`.)
2. **App record:** in [App Store Connect](https://appstoreconnect.apple.com) → Apps → **+** → New App,
   platform iOS, name gearloop, that bundle ID, any SKU (e.g. `gearloop`).
3. Open the app's **App Information** page and copy its **Apple ID** (a 10-digit number).
   Put it in `APP_STORE_APPLE_ID` in `codemagic.yaml`.

## 5. Give Codemagic access to your Apple account

1. In App Store Connect → Users and Access → Integrations → **App Store Connect API**, create a
   key with the **App Manager** role. Download the `.p8` file (Apple lets you download it once) and
   note the **Issuer ID** and **Key ID**.
2. In Codemagic → Team settings → Team integrations → **Developer Portal** → Manage keys, add the
   key and name it exactly `gearloop-asc`.
3. In Codemagic → Team settings → codemagic.yaml settings → **Code signing identities**:
   - iOS certificates: **Generate certificate** of type *Apple Distribution* using the `gearloop-asc` key.
     Download and keep the `.p12` and its password somewhere safe.
   - iOS provisioning profiles: **Fetch profiles**, then pick the *App Store* profile for
     `com.marcchenard.gearloop`. If none exists, create one in Apple's portal
     (Profiles → **+** → App Store Connect) and fetch again.

## 6. Build and install on your iPhone

1. Start the **gearloop iOS to TestFlight** workflow.
2. In App Store Connect → your app → **TestFlight**, create an internal testing group and add
   yourself. Builds appear there after Apple finishes processing them.
3. Install the **TestFlight** app on your iPhone and install gearloop from it.

Builds for internal testers don't go through Apple review. Each TestFlight build lasts 90 days,
so re-run the workflow to refresh it. Submitting to the public App Store is optional and only
needed if other people should be able to download gearloop.
