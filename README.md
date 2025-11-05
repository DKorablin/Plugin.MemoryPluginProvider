# Memory Plugin Provider

[![Build, Release](https://github.com/DKorablin/Plugin.MemoryPluginProvider/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.MemoryPluginProvider/actions/workflows/release.yml)

A plugin provider for SAL (Software Abstraction Layer) that loads assemblies into memory before loading them into the current AppDomain, avoiding file system locks and enabling dynamic plugin updates.

## Overview

This plugin provider reads assemblies from the file system into memory and then loads them into the current AppDomain. This approach offers several advantages over traditional file-based plugin loading:

- **No file locks**: Assemblies are loaded from memory, allowing plugin files to be updated without restarting the application
- **Dynamic updates**: Monitor plugin directories for changes and automatically reload updated plugins
- **Hot-swapping**: When a plugin is updated, the old reference is removed and the new plugin is loaded automatically

## Features

- ✅ Load plugins from multiple directories
- ✅ Automatic monitoring of plugin directories for changes
- ✅ Hot-swapping of updated plugins
- ✅ Custom assembly resolution for dependencies
- ✅ Support for both command-line and configuration-based paths
- ✅ Multi-target framework support (.NET Framework 3.5, .NET Standard 2.0)

## Installation
To install the Memory Plugin Provider Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.Winlogon/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.WorkerService](https://dkorablin.github.io/Flatbed-WorkerService)

## Usage

### Configuration

The plugin provider supports multiple methods for specifying plugin paths:

#### 1. Command Line Arguments

Use the `SAL_Path` parameter with pipe-separated paths:

```bash
YourApplication.exe /SAL_Path:"C:\Plugins|D:\MorePlugins"
```

#### 2. Application Configuration

Add to your `app.config` or `web.config`:

```xml
<configuration>
  <appSettings>
    <add key="SAL_Path" value="C:\Plugins|D:\MorePlugins" />
  </appSettings>
</configuration>
```

#### 3. Default Behavior

If no path is specified, the provider searches for plugins in the current directory.

## How It Works

1. **Initialization**: On connection, the provider scans all configured directories for DLL files
2. **Loading**: Each assembly is read into a byte array and loaded into memory using `Assembly.Load(byte[])`
3. **Monitoring**: `FileSystemWatcher` instances monitor plugin directories for changes
4. **Hot-Swapping**: When a file changes, the provider automatically loads the updated version
5. **Resolution**: The provider maintains a cache of loaded assemblies to resolve dependencies

## Path Configuration Details

The provider determines plugin paths in the following priority order:

1. **AppSettings** (`SAL_Path` key in configuration file)
2. **Command Line** (`/SAL_Path:` argument)
3. **Current Directory** (fallback if no path specified)
4. **Assembly Location** (last resort fallback)

**Path Delimiter**: Use the pipe symbol (`|`) to separate multiple paths.

## Supported File Extensions

- `.dll` - Dynamic Link Libraries

## Limitations

⚠️ **Important Considerations**

### DLL Hell Warning

When duplicate assemblies exist in separate folders, references inside the AppDomain may be broken. Be careful with versioning and assembly placement to avoid conflicts.

### Managed C++ Assemblies

The plugin **cannot load assemblies written in Managed C++** (C++/CLI). This limitation is due to:

- Managed C++ assemblies contain both managed and native code
- Native code requires the assembly to be physically present on the file system
- The Win32 API `LoadLibrary` function cannot load libraries from memory

**Workaround**: Place Managed C++ assemblies in a location where they can be loaded using the standard file-based plugin provider.

### Referenced Dependencies

Dependencies referenced by plugins are loaded into memory, but this may not work for all scenarios. Test thoroughly with your specific plugin architecture.

## Comparison with BasicFileProvider

This plugin provider shares most logic with the BasicFileProvider, with the following key differences:

| Feature | MemoryPluginProvider | BasicFileProvider |
|---------|---------------------|-------------------|
| Assembly Loading | Loads into memory first (`Assembly.Load(byte[])`) | Loads directly from file system |
| File Locks | No file locks | Files are locked while loaded |
| Hot-Swapping | Supported | Not supported without restart |
| Managed C++ Support | ❌ Not supported | ✅ Supported |
| Memory Usage | Higher (assemblies cached in memory) | Lower |

## Target Frameworks

- .NET Framework 3.5
- .NET Standard 2.0

## Tracing and Diagnostics

The plugin provider includes built-in tracing support. It will log:

- Assembly resolution failures
- Plugin loading errors
- Bad image format exceptions

Traces are written to the SAL trace listeners configured in your application.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.