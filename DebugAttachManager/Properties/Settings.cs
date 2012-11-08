using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace Karpach.DebugAttachManager.Properties
{
    internal sealed class Settings
    {
        private Dictionary<int, bool> _processes;
        private WritableSettingsStore _settingsStore;

        public Settings(IServiceProvider provider)
        {
            SettingsManager settingsManager = new ShellSettingsManager(provider);
            _settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!_settingsStore.CollectionExists("Processes"))
            {
                _settingsStore.CreateCollection("Processes");
            }
            IEnumerable<string> services = _settingsStore.GetSubCollectionNames("Processes");            
            foreach (var s in services)
            {
                //_settingsStore.SetString();
            }
        }

        public Dictionary<int,bool> Processes
        {
            get
            {
                return _processes ?? (_processes = new Dictionary<int, bool>());
            }
            set
            {
                _processes = value;
            }
        }        
    }
}