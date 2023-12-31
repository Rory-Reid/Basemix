#!/bin/bash

dotnet publish src/Basemix/Basemix.csproj -c Release -f net7.0-windows10.0.19041.0 --no-restore
dotnet publish src/Basemix/Basemix.csproj -c Release -f net7.0-maccatalyst -p:BuildIpa=True
dotnet publish src/Basemix/Basemix.csproj -c Release -f net7.0-android --no-restore -p:AndroidKeyStore=True -p:AndroidSigningKeyPass="${ANDROID_SIGNING_KEY}" -p:AndroidSigningStorePass="${ANDROID_SIGNING_KEY}" -p:AndroidSigningKeyStore="${ANDROID_KEY_STORE}"
dotnet publish src/Basemix/Basemix.csproj -c Release -f net7.0-ios -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="${IOS_CODESIGN_KEY}" -p:CodesignProvision="${IOS_CODESIGN_PROVISION}"
