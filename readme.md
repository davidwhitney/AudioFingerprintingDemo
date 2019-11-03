# Audio Indexer

A proof of concept pocket sized Shazam, courtesy of the amazing `SoundFingerprinting` nuget package.

This is designed to fingerprint a small selection of audio tracks (from /App_Data), persisting the fingerprints in memory.
The API is then called from the embedded static page at /index.html, and will return match the audio you're hearing from your mic to any known tracks.

Obviously, this won't scale to a huge volume of tracks as we're converting mp3 => wav on startup. Plenty of room for optimisation, but valid for a small selection of audio.

# Requirements

* .NET Core 3.0 - https://dotnet.microsoft.com/download/dotnet-core/3.0 (cross platform)

# Running

From this very directory

> dotnet run

... or F5 in Visual Studio / VSCode

# Hosting

Kude deploy this repository to Azure app services and it should "just work".

# Notes

There's no auth, no security, no things.
This is a demo.