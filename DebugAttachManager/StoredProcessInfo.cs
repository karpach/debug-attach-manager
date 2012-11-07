using System;

namespace Karpach.DebugAttachManager
{
    [Serializable]
    public class StoredProcessInfo
    {
        public int Hash { get; set; }
        public bool Selected { get; set; }
    }
}