#!/bin/bash

dotnet publish src/Basemix.UI/Basemix.UI.csproj -c Release -f net7.0-windows10.0.19041.0 --no-restore
dotnet publish src/Basemix.UI/Basemix.UI.csproj -c Release -f net7.0-maccatalyst --no-restore -p:BuildIpa=True
dotnet publish src/Basemix.UI/Basemix.UI.csproj -c Release -f net7.0-android --no-restore -p:AndroidKeyStore=True -p:AndroidSigningKeyPass="${ANDROID_SIGNING_KEY}" -p:AndroidSigningStorePass="${ANDROID_SIGNING_KEY}" -p:AndroidSigningKeyStore="${ANDROID_KEY_STORE}"
dotnet build src/Basemix.UI/Basemix.UI.csproj -c Release -f net7.0-ios --no-restore /p:buildForSimulator=True /p:packageApp=True /p:ArchiveOnBuild=False