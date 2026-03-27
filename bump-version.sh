#!/bin/bash
set -euo pipefail

CSPROJ="src/Basemix/Basemix.csproj"

if [ $# -lt 1 ]; then
    current_display=$(sed -n 's/.*<ApplicationDisplayVersion>\(.*\)<\/ApplicationDisplayVersion>.*/\1/p' "$CSPROJ")
    current_build=$(sed -n 's/.*<ApplicationVersion>\(.*\)<\/ApplicationVersion>.*/\1/p' "$CSPROJ")
    echo "Current version: $current_display (build $current_build)"
    echo ""
    echo "Usage: $0 <display-version> [build-number]"
    echo "  display-version: e.g. 1.0.21 (the .0 suffix is added automatically for 4-part contexts)"
    echo "  build-number:    e.g. 21 (defaults to last numeric segment of display-version)"
    echo ""
    echo "Example: $0 1.0.21"
    exit 1
fi

DISPLAY_VERSION="$1"

# Default build number to the last numeric segment of the display version
if [ $# -ge 2 ]; then
    BUILD_NUMBER="$2"
else
    BUILD_NUMBER=$(echo "$DISPLAY_VERSION" | grep -oE '[0-9]+$')
fi

# ApplicationDisplayVersion uses 4-part format: X.Y.Z.0
DISPLAY_VERSION_FULL="${DISPLAY_VERSION}.0"

echo "Setting version to:"
echo "  ApplicationDisplayVersion: $DISPLAY_VERSION_FULL"
echo "  ApplicationVersion:        $BUILD_NUMBER"

sed -i '' "s|<ApplicationDisplayVersion>.*</ApplicationDisplayVersion>|<ApplicationDisplayVersion>${DISPLAY_VERSION_FULL}</ApplicationDisplayVersion>|" "$CSPROJ"
sed -i '' "s|<ApplicationVersion>.*</ApplicationVersion>|<ApplicationVersion>${BUILD_NUMBER}</ApplicationVersion>|" "$CSPROJ"

echo ""
echo "Updated $CSPROJ"
echo "Platform manifests will pick up the version automatically at build time."
