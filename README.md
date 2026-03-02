<p align="center">
  <img src="src/WinClean/Resources/app.ico" width="80" alt="WinClean">
</p>

<h1 align="center">WinClean</h1>

<p align="center">
  <strong>Clear Clutter, Claim Space.</strong><br>
  轻量级 Windows 磁盘清理工具
</p>

<p align="center">
  <a href="https://github.com/shaoyidi/WinClean/releases"><img src="https://img.shields.io/github/v/release/shaoyidi/WinClean?style=flat-square&color=7B1FA2" alt="Release"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-blue?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=flat-square" alt=".NET">
  <img src="https://img.shields.io/badge/license-MIT-green?style=flat-square" alt="License">
  <img src="https://img.shields.io/github/downloads/shaoyidi/WinClean/total?style=flat-square&color=orange" alt="Downloads">
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#screenshots">Screenshots</a> •
  <a href="#download">Download</a> •
  <a href="#build">Build</a> •
  <a href="#sponsor">Sponsor</a>
</p>

---

## ✨ Features

🧹 **System Junk Cleaner** — Clean temporary files, browser caches, Windows update caches, system logs, thumbnails, and more.

🔍 **Large File Scanner** — Find the largest files on any drive with custom size thresholds and real-time disk usage display.

📋 **Duplicate File Detector** — Detect duplicate files using SHA256 hash comparison with multi-threaded scanning across multiple drives.

📊 **Disk Space Analysis** — Visualize disk usage with an interactive tree view to see exactly where your space is being used.

🌍 **6 Languages** — Simplified Chinese, Traditional Chinese, English, Japanese, Korean, and German.

🎨 **Customizable Themes** — 10+ theme colors, dark/light mode, with preferences auto-saved.

## 📸 Screenshots

<table>
  <tr>
    <td><img src="AppImage/c567d7da-0cf5-442f-bda0-10a3a938d8b9.png" alt="System Cleaner"></td>
    <td><img src="AppImage/92fd0f95-886d-4e73-b473-26c415ea4470.png" alt="Large Files"></td>
  </tr>
  <tr>
    <td align="center"><strong>System Junk Cleaner</strong></td>
    <td align="center"><strong>Large File Scanner</strong></td>
  </tr>
  <tr>
    <td><img src="AppImage/85c12df8-124a-47cb-a0de-4fa611ab19dd.png" alt="Duplicates"></td>
    <td><img src="AppImage/97549907-45c6-4024-83a1-6e6d6fdb5aef.png" alt="Disk Analysis"></td>
  </tr>
  <tr>
    <td align="center"><strong>Duplicate File Detector</strong></td>
    <td align="center"><strong>Disk Space Analysis</strong></td>
  </tr>
  <tr>
    <td><img src="AppImage/6cda7e16-06ca-4693-aa13-c5ccbc130d51.png" alt="Settings"></td>
    <td></td>
  </tr>
  <tr>
    <td align="center"><strong>Settings (Multi-language & Themes)</strong></td>
    <td></td>
  </tr>
</table>

## 📥 Download

Download the latest version from [GitHub Releases](https://github.com/shaoyidi/WinClean/releases).

- **Installer**: `WinClean_Setup_x.x.x.exe` — Full installer with shortcuts
- **Portable**: `WinClean.exe` — No installation required

> **Requirements**: Windows 10/11 x64. Self-contained, no .NET runtime needed.

## 🔨 Build

```bash
# Clone the repository
git clone https://github.com/shaoyidi/WinClean.git
cd WinClean

# Run directly
dotnet run --project src/WinClean/WinClean.csproj

# Build installer (requires Inno Setup)
build.bat

# Build MSI package (requires WiX Toolset)
build-msi.bat
```

### Tech Stack

- **Framework**: .NET 8 + WPF
- **UI Library**: Material Design In XAML
- **Architecture**: MVVM (CommunityToolkit.Mvvm)
- **DI**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog

## 💜 Sponsor

If WinClean is helpful to you, consider buying me a coffee! Your support motivates continued development.

如果 WinClean 对你有帮助，欢迎请我喝杯咖啡！你的支持是我持续开发的动力。

<p align="center">
  <a href="https://ko-fi.com/shaoyidi"><img src="https://img.shields.io/badge/Ko--fi-Support%20Me-FF5E5B?style=for-the-badge&logo=ko-fi&logoColor=white" alt="Ko-fi"></a>
  <a href="https://www.buymeacoffee.com/shaoyidi"><img src="https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support-FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black" alt="Buy Me a Coffee"></a>
  <a href="https://afdian.com/a/shaoyidi"><img src="https://img.shields.io/badge/爱发电-赞助-purple?style=for-the-badge" alt="爱发电"></a>
</p>

You can also support by:
- ⭐ **Star** this repository
- 📢 **Share** WinClean with friends
- 🐛 **Report** bugs or suggest features via [Issues](https://github.com/shaoyidi/WinClean/issues)

## 📄 License

[MIT License](LICENSE) © 2026 WinClean
