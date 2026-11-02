# Repeat Studio

A standalone Windows app for automatic clicking, recording and replaying mouse and keyboard actions, and finding images on screen. I've made it totally for personal use, I'm just sharing in case someone needs something like this.

## Run

Download and extract RepeatStudio-Windows-x64.zip, then open RepeatStudio.exe. The app is self-contained; no separate .NET installation is required. Built for Windows x64.

## Controls

- F8: start or finish recording.
- F9: start the selected action.
- F10: stop.
- Ctrl+V: paste a target image on the Image search page.

Recordings and pasted images last for the current session. Preferences are saved in %LOCALAPPDATA%\RepeatStudio.

## Sharing on GitHub

Use this README as the repository description and attach RepeatStudio-Windows-x64.zip to a GitHub Release so people can download the app. RepeatStudio.exe is also included here for direct use.

The current WPF source code and artwork are included in src/.
## Build from source

Building from source requires the .NET 10 SDK on Windows. [Download the .NET 10 SDK for Windows x64 (10.0.403)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-10.0.403-windows-x64-installer).

The standalone RepeatStudio.exe includes the runtime and does not require a separate .NET installation.

After installing the SDK, run from this folder:

```powershell
dotnet build src/Wpf/RepeatStudio.Wpf.csproj -c Release
```

To run the app directly from source, run this command from this folder:

```powershell
dotnet run --project src/Wpf/RepeatStudio.Wpf.csproj -c Release
```

To create a standalone Windows x64 app:

```powershell
dotnet publish src/Wpf/RepeatStudio.Wpf.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o dist
```