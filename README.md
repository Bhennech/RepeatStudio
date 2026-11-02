<div align="center">

![Repeat Studio](src/assets/logo.png)

# 🌸 Repeat Studio

**A simple, customizable Windows automation tool.**

Auto-clicking • Input Recording • Image Recognition

</div>

## 💗 About

Repeat Studio is a standalone Windows application for automating mouse clicks, recording and replaying mouse and keyboard actions, and finding images on your screen.

I originally created this application entirely for personal use. I'm sharing it in case someone else finds it useful!

## ✨ Features

* 🖱️ **Simple Clicker:** Automatically click at customizable intervals.
* 🔴 **Recorded Clicker:** Record mouse and keyboard actions and replay them.
* 🔍 **Image Search:** Find matching images on your screen.
* 🔁 **Custom Repetitions:** Repeat actions a specific number of times or indefinitely.
* ⌨️ **Global Hotkeys:** Control automation without switching windows.
* 🛑 **Emergency Stop:** Immediately stop running automation.
* 🌸 **Custom Interface:** A pastel-themed interface built using WPF.

## 📥 Installation

1. Open the **Releases** section of this repository.
2. Download `RepeatStudio-Windows-x64.zip`.
3. Extract the ZIP file.
4. Launch `RepeatStudio.exe`.

That's it!

**Requirements:**

* Windows 64-bit
* No separate .NET installation required

The distributed application is self-contained.

## ⌨️ Keyboard Shortcuts

| Shortcut | Action                           |
| -------- | -------------------------------- |
| F8       | Start/stop recording             |
| F9       | Start the selected action        |
| F10      | Emergency stop                   |
| Ctrl + V | Paste an image into Image Search |

## ⚙️ Settings

Application preferences are stored in:

`%LOCALAPPDATA%\RepeatStudio`

Recordings and pasted images are temporary and remain available only during the current session.

## 🛠️ Building from Source

Repeat Studio is built using **C# and WPF on .NET 10**.

To compile the application yourself, install the .NET 10 SDK.

Clone this repository and run:

```powershell
dotnet build src/Wpf/RepeatStudio.Wpf.csproj -c Release
```

To launch it:

```powershell
dotnet run --project src/Wpf/RepeatStudio.Wpf.csproj -c Release
```

### Creating a standalone executable

```powershell
dotnet publish src/Wpf/RepeatStudio.Wpf.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
```

The compiled application will be placed in the `dist` directory.

---

<div align="center">

Made with 💗 and very little experimenting.

**Enjoy Repeat Studio! 🌸**

</div>
