# Memory Plugin Provider
Most of the All logic is the same as in plugin BasicFileProvider. Except reading assemblies from file system into memory and after that loading it into current AppDomain.

Plugin is using command line argument SAL_Path for searching for plugins in different folders. As a delimiter for path array symbol «;» is used. If command line parameter SAL_Path is not specified, then current directory will be used to search for plugins.

After loading all the plugins from the folder, the folder is added to monitor the update of old plugins or the appearance of new plugins, so that when new plugins appear in the folder, they will be loaded automatically. When updating a loaded plugin, the link to the old plugin will be deleted and a link to the new plugin will appear.

Warning

In addition to the possibility of appearance DLL Hell if duplicate assembly will be found in separate folders and references inside AppDomain will be broken. Plugin can't load assemblies written in Managed C++. This is due to a limitation when working with Win32 API and LoadLibrary function, which require the physical presence of the assembly on the file system.