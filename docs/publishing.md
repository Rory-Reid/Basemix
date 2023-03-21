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

Todo