# Publishing

Publishing is a very crap and manual process today, this'll document what needs doing.

## General Notes

The `build.sh` file contains preformatted commands with variable references in for running a full publish of all things - note that this isn't actually ever going to fully succeed because MacOS/iOS builds only work on Mac and Windows builds only work on Windows.

## Windows

First amend the following files:

- Basemix.UI.csproj - The `<ApplicationDisplayVersion>` needs to be the full 4-part version string. You cannot change the last digit, so in the format `w.x.y.z` only `z` cannot be changed.
- Basemix.UI/Platforms/Windows/Package.appxmanifest - The Identity needs to be setup with the correct Name, Publisher and Version number (matching the display version in the csproj) and the `<Properties>` node needs to have suitable display names for the application and publisher.

As new versions are released, only the `<ApplicationDisplayVersion>` and `<Identity Version="">` should require amendment.

## Android

First amend the following files:

- Basemix.UI.csproj - The `<ApplicationDisplayVersion>` needs to be a 3-part version number. However, windows needs 4-part, so use that.
- Basemix.UI.csproj - The `<ApplicationVersion>` needs to be unique every publish. Increment it.

The following parameters need to be set in the csproj (or more sensibly passed into the build command using secrets). Note that the csproj has some amendments in it to set some up, and others exist in `build.sh`:

- `AndroidKeyStore` - If we should use the key store. Not sensitive. Should be `true`
- `AndroidSigningKeyAlias` - Alias for the signing key. Not sensitive. Specific to the key used. Set to `key` by default in this app.
- `AndroidSigningKeyPass` - Should be the key password. Sensitive.
- `AndroidSigningStorePass` - Should be the signing store password. Sensitive.
- `AndroidSigningKeyStore` - Path to the store. Not sensitive. Environmental dependent.

As new versions are released, only the `<ApplicationDisplayVersion>` and `<ApplicationVersion>` should require amendment.
