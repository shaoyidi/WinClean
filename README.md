<p align="center">
  <img src="src/WinClean/Resources/app.ico" width="80" alt="WinClean">
</p>

<h1 align="center">WinClean</h1>

<p align="center">
  <strong>Clear Clutter, Claim Space.</strong><br>
  轻量级 Windows 磁盘清理工具
</p>

<p align="center">
  <a href="https://github.com/shaoyidi/WinCleanCat/releases"><img src="https://img.shields.io/github/v/release/shaoyidi/WinCleanCat?style=flat-square&color=7B1FA2" alt="Release"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-blue?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=flat-square" alt=".NET">
  <img src="https://img.shields.io/badge/license-MIT-green?style=flat-square" alt="License">
  <img src="https://img.shields.io/github/downloads/shaoyidi/WinCleanCat/total?style=flat-square&color=orange" alt="Downloads">
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

Download the latest version from [GitHub Releases](https://github.com/shaoyidi/WinCleanCat/releases).

- **Installer**: `WinClean_Setup_x.x.x.exe` — Full installer with shortcuts
- **Portable**: `WinClean.exe` — No installation required

> **Requirements**: Windows 10/11 x64. Self-contained, no .NET runtime needed.

## 🔨 Build

```bash
# Clone the repository
git clone https://github.com/shaoyidi/WinCleanCat.git
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

If WinClean is helpful to you, consider supporting its development! / 如果 WinClean 对你有帮助，欢迎支持！

### 🇨🇳 微信赞赏

<p align="center">
  <img src="docs/images/weixin_szm.png" alt="WeChat QR Code" width="200">
</p>

### 🌐 International Payment \[Coming Soon\]

> International payment channel is under preparation. Stay tuned!
>
> 海外支付渠道正在筹备中，敬请期待！

### Other ways to support / 其他支持方式

- ⭐ **Star** this repository / 给仓库点 Star
- 📢 **Share** WinClean with friends / 分享给朋友
- 🐛 **Report** bugs or suggest features via [Issues](https://github.com/shaoyidi/WinCleanCat/issues) / 提交 Bug 或建议

## 📄 License

[MIT License](LICENSE) © 2026 WinClean
