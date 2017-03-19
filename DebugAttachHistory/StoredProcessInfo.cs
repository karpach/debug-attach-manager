using System;

namespace Karpach.DebugAttachManager
{    
    public class StoredProcessInfo
    {
        public string ProcessName { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }
        public string DebugMode { get; set; }

        public int Hash
        {
            get { return string.Concat(ProcessName, Title).GetHashCode(); }
        }
    }
}