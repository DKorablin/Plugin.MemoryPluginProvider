using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Plugin.FilePluginProvider;
using SAL.Flatbed;

namespace Plugin.MemoryPluginProvider
{
	public class Plugin : IPluginProvider
	{
		private TraceSource _trace;

		private IHost Host { get; }

		private TraceSource Trace { get => this._trace ?? (this._trace = Plugin.CreateTraceSource<Plugin>()); }

		internal FilePluginArgs Args { get; set; }

		/// <summary>Array of previously loaded assemblies</summary>
		/// <remarks>If assemblies are read in memory, a cache of assemblies must be maintained.</remarks>
		private Dictionary<String, Assembly> Assemblies { get; set; }

		/// <summary>Monitor for new plugins</summary>
		private List<FileSystemWatcher> Monitors { get; set; }

		/// <summary>Parent plugin provider</summary>
		IPluginProvider IPluginProvider.ParentProvider { get; set; }

		public Plugin(IHost host)
			=> this.Host = host ?? throw new ArgumentNullException(nameof(host));

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			this.Args = new FilePluginArgs();
			this.Monitors = new List<FileSystemWatcher>();
			this.Assemblies = new Dictionary<String, Assembly>();
			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(mode == DisconnectMode.UserClosed)
				throw new NotSupportedException("You can't unload plugin provider plugin");

			if(this.Monitors != null)
			{
				foreach(FileSystemWatcher monitor in this.Monitors)
					monitor.Dispose();
				this.Monitors = null;
			}
			return true;
		}

		void IPluginProvider.LoadPlugins()
		{
			//AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			/*if(String.IsNullOrEmpty(pluginPath))
				pluginPath = AppDomain.CurrentDomain.BaseDirectory;*/

			//System.Diagnostics.Debugger.Launch();
			foreach(String pluginPath in this.Args.PluginPath)
				if(Directory.Exists(pluginPath))
				{
					foreach(String file in Directory.GetFiles(pluginPath, "*.*"))
						if(FilePluginArgs.CheckFileExtension(file))
							this.LoadPlugin(file, ConnectMode.Startup);

					foreach(String extension in FilePluginArgs.LibraryExtensions)
					{
						FileSystemWatcher watcher = new FileSystemWatcher(pluginPath, "*" + extension);
						watcher.Changed += new FileSystemEventHandler(Monitor_Changed);
						watcher.EnableRaisingEvents = true;
						this.Monitors.Add(watcher);
					}
				}
		}

		Assembly IPluginProvider.ResolveAssembly(String assemblyName)
		{
			if(this.Assemblies.ContainsKey(assemblyName))
				return this.Assemblies[assemblyName];
			else
			{
				AssemblyName targetName = new AssemblyName(assemblyName);
				foreach(String pluginPath in this.Args.PluginPath)
					if(Directory.Exists(pluginPath))
						foreach(String file in Directory.GetFiles(pluginPath, "*.*"))
							if(FilePluginArgs.CheckFileExtension(file))
								try
								{
									AssemblyName name = AssemblyName.GetAssemblyName(file);
									if(name.FullName == targetName.FullName)
									{
										Byte[] fileBytes = File.ReadAllBytes(file);//TODO: Reference DLLs are not loaded from RAM!
										Assembly assembly = Assembly.Load(fileBytes);

										this.Assemblies.Add(assemblyName, assembly);
										return assembly;
									}
								} catch(Exception)//We skip all errors. We resolve the library, not deal with plugins.
								{
									continue;
								}
			}

			this.Trace.TraceEvent(TraceEventType.Warning, 5, "The provider {2} is unable to locate the assembly {0} in the path {1}", assemblyName, String.Join(",", this.Args.PluginPath), this.GetType());
			IPluginProvider parentProvider = ((IPluginProvider)this).ParentProvider;
			return parentProvider == null
				? null
				: parentProvider.ResolveAssembly(assemblyName);
		}

		private void LoadPlugin(String filePath, ConnectMode mode)
		{
			try
			{
				Byte[] fileBytes = File.ReadAllBytes(filePath);
				Assembly assembly = Assembly.Load(fileBytes);
				//assembly.ModuleResolve += new ModuleResolveEventHandler(Assembly_ModuleResolve);

				this.Host.Plugins.LoadPlugin(assembly, filePath, ConnectMode.Startup);
			} catch(BadImageFormatException exc)//Plugin loading error. I could read the title of the file being loaded, but I'm too lazy.
			{
				exc.Data.Add("Library", filePath);
				this.Trace.TraceData(TraceEventType.Error, 1, exc);
			} catch(Exception exc)
			{
				exc.Data.Add("Library", filePath);
				this.Trace.TraceData(TraceEventType.Error, 1, exc);
			}
		}

		/// <summary>New file available for review</summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void Monitor_Changed(Object sender, FileSystemEventArgs e)
		{
			if(e.ChangeType == WatcherChangeTypes.Changed)
				this.LoadPlugin(e.FullPath, ConnectMode.AfterStartup);
		}

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}