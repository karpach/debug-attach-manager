namespace Karpach.DebugAttachManager.Models
{
    class ProcessToBeAttached
    {
        public ProcessExt Process { get; set; }
        public bool Checked { get; set; }        
        public string DebugMode { get; set; }
    }
}
