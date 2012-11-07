using System.Collections.Generic;
using System.Configuration;

namespace Karpach.DebugAttachManager.Properties
{
    internal sealed class Settings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValue("")]
        public Dictionary<int,bool> Processes
        {
            get
            {
                return (Dictionary<int, bool>)(this["Processes"] ?? (this["Processes"] = new Dictionary<int, bool>()));
            }
            set
            {
                this["Processes"] = value;
            }
        }        

        private static Settings _default;
        public static Settings Default 
        { 
            get
            {
                if (_default == null)
                {
                    _default = new Settings();
                    _default.Reload();
                }
                return _default;
            }            
        }
    }
}