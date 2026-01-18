# Deployment Guide

## ?? Table of Contents
- [Prerequisites](#prerequisites)
- [Deployment Methods](#deployment-methods)
- [Installation Steps](#installation-steps)
- [Configuration](#configuration)
- [Backup & Restore](#backup--restore)
- [Troubleshooting](#troubleshooting)
- [System Requirements](#system-requirements)

## ? Prerequisites

### For End Users
- Windows 10 (version 1809) or later / Windows 11
- .NET 8 Runtime (included in self-contained deployment)
- Minimum 4 GB RAM
- 200 MB free disk space
- Administrator rights (for installation only)

### For Developers
- Visual Studio 2022 or later
- .NET 8 SDK
- Git (for version control)

## ?? Deployment Methods

### Method 1: Self-Contained Executable (Recommended for End Users)

This method packages everything including .NET runtime, so users don't need to install anything separately.

#### Step-by-Step Instructions:

1. **Open PowerShell** or Command Prompt

2. **Navigate to project directory**:
   ```powershell
   cd D:\MyPos99\MyPOS99
   ```

3. **Publish the application**:
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
   ```

4. **Locate the executable**:
   - Find `MyPOS99.exe` in the `./publish` folder
   - This is a single file that includes everything needed

5. **Distribute**:
   - You can copy this file to any Windows PC and run it
   - No installation required!

---

### Method 2: Framework-Dependent Deployment (Smaller Size)

This method requires users to install .NET 8 Runtime separately.

#### Step-by-Step Instructions:

1. **Publish framework-dependent**:
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained false -o ./publish-fdd
   ```

2. **Users must install** [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

### Method 3: Create an Installer (Professional Distribution)

#### Using Inno Setup (Free & Easy):

1. **Download & Install** [Inno Setup](https://jrsoftware.org/isinfo.php)

2. **Create installer script** (`setup-script.iss`):

```iss
[Setup]
AppName=MyPOS99
AppVersion=1.0.0
AppPublisher=Your Name
DefaultDirName={autopf}\MyPOS99
DefaultGroupName=MyPOS99
OutputDir=.\installer
OutputBaseFilename=MyPOS99-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\MyPOS99"; Filename: "{app}\MyPOS99.exe"
Name: "{commondesktop}\MyPOS99"; Filename: "{app}\MyPOS99.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\MyPOS99.exe"; Description: "Launch MyPOS99"; Flags: nowait postinstall skipifsilent
```

3. **Compile the script** in Inno Setup

4. **Get your installer**: `MyPOS99-Setup.exe` will be created in `./installer` folder

---

## ?? Installation Steps

### For End Users:

#### Option A: Using the Installer
1. **Download** `MyPOS99-Setup.exe`
2. **Double-click** to run
3. **Follow** the installation wizard
4. **Launch** from Desktop or Start Menu

#### Option B: Using the Standalone Executable
1. **Download** `MyPOS99.exe`
2. **Create a folder** (e.g., `C:\MyPOS99`)
3. **Copy** the executable to that folder
4. **Double-click** to run
5. **(Optional)** Create a desktop shortcut

### First Launch:

1. **Application will start** and create database automatically
2. **Login with default credentials**:
   - Username: `admin`
   - Password: `admin123`
3. **?? IMPORTANT**: Change the default password immediately!

---

## ?? Configuration

### Database Location

The SQLite database is automatically created at:
```
C:\Users\[YourUsername]\AppData\Local\MyPOS99\pos.db
```

### Custom Database Location (Advanced)

If you want to change the database location:

1. **Edit** (or create) `appsettings.json` next to `MyPOS99.exe`:

```json
{
  "DatabasePath": "C:\\CustomFolder\\pos.db"
}
```

2. **Restart** the application

---

## ?? Backup & Restore

### Manual Backup

#### Windows PowerShell:
```powershell
# Create backup
$source = "$env:LOCALAPPDATA\MyPOS99\pos.db"
$destination = "D:\Backups\pos-backup-$(Get-Date -Format 'yyyyMMdd').db"
Copy-Item $source $destination
Write-Host "Backup created: $destination"
```

#### Windows Command Prompt:
```cmd
copy "%LOCALAPPDATA%\MyPOS99\pos.db" "D:\Backups\pos-backup.db"
```

### Automated Backup Script

Create `backup-pos.ps1`:

```powershell
# MyPOS99 Automated Backup Script
$sourcePath = "$env:LOCALAPPDATA\MyPOS99\pos.db"
$backupFolder = "D:\MyPOS99Backups"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = "$backupFolder\pos-backup-$timestamp.db"

# Create backup folder if it doesn't exist
if (-not (Test-Path $backupFolder)) {
    New-Item -ItemType Directory -Path $backupFolder
}

# Copy database
Copy-Item $sourcePath $backupFile

# Keep only last 30 backups
Get-ChildItem $backupFolder -Filter "pos-backup-*.db" | 
    Sort-Object CreationTime -Descending | 
    Select-Object -Skip 30 | 
    Remove-Item

Write-Host "? Backup completed: $backupFile"
```

**Schedule it** with Windows Task Scheduler to run daily.

### Restore from Backup

1. **Close** MyPOS99 application
2. **Navigate** to `C:\Users\[YourUsername]\AppData\Local\MyPOS99`
3. **Replace** `pos.db` with your backup file
4. **Restart** the application

---

## ?? Troubleshooting

### Application Won't Start

**Problem**: Nothing happens when clicking the executable

**Solutions**:
1. **Check** if .NET 8 Runtime is installed (for framework-dependent version)
2. **Run as Administrator** (right-click ? Run as administrator)
3. **Check** Windows Event Viewer for error details
4. **Verify** antivirus isn't blocking the application

---

### Database Locked Error

**Problem**: "Database is locked" message appears

**Solutions**:
1. **Close all instances** of MyPOS99
2. **Check Task Manager** for background processes
3. **Restart** your computer
4. **Restore** from a backup if corruption is suspected

---

### Missing DLL Errors

**Problem**: Application shows "missing DLL" errors

**Solutions**:
1. **Use self-contained deployment** (includes all dependencies)
2. **Install** .NET 8 Runtime from Microsoft
3. **Install** Visual C++ Redistributable

---

### Performance Issues

**Problem**: Application is slow

**Solutions**:
1. **Run VACUUM** on database (advanced users):
   ```sql
   VACUUM;
   ```
2. **Archive old data** (sales older than 2 years)
3. **Check disk space** (database needs room to grow)
4. **Upgrade to SSD** for better performance

---

### PDF Export Not Working

**Problem**: Cannot generate PDF reports

**Solutions**:
1. **Check** write permissions in the export folder
2. **Ensure** enough disk space
3. **Verify** no antivirus blocking PDF creation

---

## ?? System Requirements

### Minimum Requirements
- **OS**: Windows 10 (1809) or later
- **Processor**: Intel/AMD Dual-core 2.0 GHz
- **RAM**: 4 GB
- **Storage**: 200 MB free space (HDD acceptable)
- **Display**: 1366x768 resolution

### Recommended Requirements
- **OS**: Windows 11
- **Processor**: Intel Core i5 / AMD Ryzen 5 or better
- **RAM**: 8 GB or more
- **Storage**: 500 MB free space on SSD
- **Display**: 1920x1080 resolution or higher

### Optimal Performance
- **OS**: Windows 11 Pro
- **Processor**: Intel Core i7 / AMD Ryzen 7 or better
- **RAM**: 16 GB
- **Storage**: 1 GB on NVMe SSD
- **Display**: 1920x1080 or higher

---

## ?? Network Deployment (Multi-PC Setup)

### Shared Database Setup

1. **Place database** on network share:
   ```
   \\SERVER\Shared\MyPOS99\pos.db
   ```

2. **Update all clients** to point to network database

3. **?? Warning**: SQLite has limitations with concurrent access
   - Suitable for 1-3 simultaneous users
   - For more users, consider upgrading to SQL Server or PostgreSQL

---

## ?? Deployment Checklist

Before deploying to production:

- [ ] Test on clean Windows installation
- [ ] Verify all features work correctly
- [ ] Change default admin password
- [ ] Create initial backup
- [ ] Set up automated backup schedule
- [ ] Test backup restore procedure
- [ ] Configure user accounts and permissions
- [ ] Train users on the system
- [ ] Document any custom configurations
- [ ] Plan for regular database maintenance

---

## ?? Support

### Getting Help

1. **Check** [README.md](README.md) for general information
2. **Review** [DATABASE_SERVICE_GUIDE.md](Data/DATABASE_SERVICE_GUIDE.md) for database issues
3. **Search** [GitHub Issues](https://github.com/Geethanjana99/MyPOS99/issues)
4. **Create** a new issue with:
   - Windows version
   - Application version
   - Steps to reproduce the problem
   - Error messages or screenshots

---

## ?? Version History

| Version | Release Date | Notes |
|---------|--------------|-------|
| 1.0.0   | Jan 2025     | Initial release |

---

**Need Help?** Open an issue on [GitHub](https://github.com/Geethanjana99/MyPOS99/issues)

**Built with ?? using .NET 8 and WPF**
