namespace Karpach.DebugAttachManager.Models
{    
    public class StoredProcessInfo
    {
        public string ProcessName { get; set; }
        public string Title { get; set; }
        public string RemoteServerName { get; set; }
        public long? RemotePortNumber { get; set; }
        public bool Selected { get; set; }
        public string DebugMode { get; set; }

        public int Hash
        {
            get { return string.Concat(ProcessName, Title, RemoteServerName, RemotePortNumber).GetHashCode(); }
        }
    }
}