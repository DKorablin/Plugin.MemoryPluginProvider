using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("80c7eab5-8c33-407f-9c6f-20a9349b547f")]
[assembly: ComVisible(false)]
[assembly: System.CLSCompliant(true)]

#if NETSTANDARD || NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=107")]
#else

[assembly: AssemblyTitle("Plugin.MemoryPluginProvider")]
[assembly: AssemblyProduct("Plugin loader from file system to local memory")]
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2011-2024")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

#endif