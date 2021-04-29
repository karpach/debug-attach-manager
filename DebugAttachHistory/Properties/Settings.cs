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
        private bool[] _processesColumns;
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

            if (_settingsStore.PropertyExists("DebugAttachManagerProcesses", "RemotePort"))
            {
                RemotePort = _settingsStore.GetString("DebugAttachManagerProcesses", "RemotePort");
            }

            if (_settingsStore.PropertyExists("DebugAttachManagerProcesses", "RemoteUserName"))
            {
                RemoteUserName = _settingsStore.GetString("DebugAttachManagerProcesses", "RemoteUserName");
            }

            for (int i = 0; i < Constants.NUMBER_OF_OPTIONAL_COLUMNS; i++)
            {
                string columnName = $"Column{i}";
                if (_settingsStore.PropertyExists("DebugAttachManagerProcesses", columnName))
                {
                    ProcessesColumns[i] = _settingsStore.GetBoolean("DebugAttachManagerProcesses", columnName);
                }
                else
                {
                    if (i == 0)
                    {
                        // This is a hack, so we display PID by default
                        ProcessesColumns[i] = true;
                    }
                }
            }            
        }

        public Dictionary<int,StoredProcessInfo> Processes => _processes ?? (_processes = new Dictionary<int, StoredProcessInfo>());

        public bool[] ProcessesColumns => _processesColumns ?? (_processesColumns = new bool[Constants.NUMBER_OF_OPTIONAL_COLUMNS] );

        public string RemoteServer { get; set; }

        public string RemotePort { get; set; }

        public string RemoteUserName { get; set; }

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
            if (!string.IsNullOrEmpty(RemotePort))
            {
                if (!_settingsStore.CollectionExists("DebugAttachManagerProcesses"))
                {
                    _settingsStore.CreateCollection("DebugAttachManagerProcesses");
                }
                _settingsStore.SetString("DebugAttachManagerProcesses", "RemotePort", RemotePort);
            }
            if (!string.IsNullOrEmpty(RemoteUserName))
            {
                if (!_settingsStore.CollectionExists("DebugAttachManagerProcesses"))
                {
                    _settingsStore.CreateCollection("DebugAttachManagerProcesses");
                }
                _settingsStore.SetString("DebugAttachManagerProcesses", "RemoteUserName", RemoteUserName);
            }
            for (i = 0; i < Constants.NUMBER_OF_OPTIONAL_COLUMNS; i++)
            {
                string columnName = $"Column{i}";
                _settingsStore.SetBoolean("DebugAttachManagerProcesses", columnName, _processesColumns[i]);
            }
        }
    }    
}