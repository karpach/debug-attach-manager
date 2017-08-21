using System;
using System.Collections.Generic;
using Karpach.DebugAttachManager.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace Karpach.DebugAttachManager.Properties
{
    internal sealed class Settings
    {
        private Dictionary<int,StoredProcessInfo> _processes;
        private readonly WritableSettingsStore _settingsStore;

        public Settings(IServiceProvider provider)
        {
            SettingsManager settingsManager = new ShellSettingsManager(provider);
            _settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!_settingsStore.CollectionExists("DebugAttachManagerProcesses"))
            {
                return;                
            }
            IEnumerable<string> services = _settingsStore.GetSubCollectionNames("DebugAttachManagerProcesses");            
            foreach (var s in services)
            {
                var p = new StoredProcessInfo
                            {
                                ProcessName = _settingsStore.GetString("DebugAttachManagerProcesses\\" + s, "ProcessName"),
                                Title = _settingsStore.PropertyExists("DebugAttachManagerProcesses\\" + s, "Title") ? 
                                _settingsStore.GetString("DebugAttachManagerProcesses\\" + s, "Title") : null,
                                RemoteServerName = _settingsStore.PropertyExists("DebugAttachManagerProcesses\\" + s, "RemoteServerName") ? 
                                _settingsStore.GetString("DebugAttachManagerProcesses\\" + s, "RemoteServerName") : null,
                                RemotePortNumber = _settingsStore.PropertyExists("DebugAttachManagerProcesses\\" + s, "RemotePortNumber") ?  
                                _settingsStore.GetInt64("DebugAttachManagerProcesses\\" + s, "RemotePortNumber") : (long?)null,
                                Selected = _settingsStore.GetBoolean("DebugAttachManagerProcesses\\" + s, "Selected"),
                                DebugMode = _settingsStore.PropertyExists("DebugAttachManagerProcesses\\" + s, "DebugMode") ?
                                _settingsStore.GetString("DebugAttachManagerProcesses\\" + s, "DebugMode") : null
                };
                Processes.Add(p.Hash,p);
            }

            if (_settingsStore.PropertyExists("DebugAttachManagerProcesses", "RemoteServer"))
            {
                RemoteServer = _settingsStore.GetString("DebugAttachManagerProcesses", "RemoteServer");
            }
        }

        public Dictionary<int,StoredProcessInfo> Processes
        {
            get => _processes ?? (_processes = new Dictionary<int, StoredProcessInfo>());
            set => _processes = value;
        }

        public string RemoteServer { get; set; }
       
        private static Settings _default;
        public static Settings Default => _default ?? (_default = new Settings(DebugAttachManagerPackage.ServiceProvider));

        public void Save()
        {
            int i = 1;
            if (_settingsStore.CollectionExists("DebugAttachManagerProcesses"))
            {
                _settingsStore.DeleteCollection("DebugAttachManagerProcesses");
            }            
            foreach (var p in Processes.Values)            
            {
                _settingsStore.CreateCollection("DebugAttachManagerProcesses\\Process " + i);
                if (p.Title != null)
                {
                    _settingsStore.SetString("DebugAttachManagerProcesses\\Process " + i, "Title", p.Title);
                }
                if (p.RemoteServerName != null)
                {
                    _settingsStore.SetString("DebugAttachManagerProcesses\\Process " + i, "RemoteServerName", p.RemoteServerName);
                }
                if (p.RemotePortNumber.HasValue)
                {
                    _settingsStore.SetInt64("DebugAttachManagerProcesses\\Process " + i, "RemotePortNumber", p.RemotePortNumber.Value);
                }
                _settingsStore.SetString("DebugAttachManagerProcesses\\Process " + i, "ProcessName", p.ProcessName);
                _settingsStore.SetBoolean("DebugAttachManagerProcesses\\Process " + i, "Selected", p.Selected);
                
                if (p.DebugMode != null)
                {
                    _settingsStore.SetString("DebugAttachManagerProcesses\\Process " + i, "DebugMode", p.DebugMode);
                }                
                i++;
            }
            if (!string.IsNullOrEmpty(RemoteServer))
            {
                if (!_settingsStore.CollectionExists("DebugAttachManagerProcesses"))
                {
                    _settingsStore.CreateCollection("DebugAttachManagerProcesses");
                }
                _settingsStore.SetString("DebugAttachManagerProcesses", "RemoteServer", RemoteServer);
            }            
        }
    }
}