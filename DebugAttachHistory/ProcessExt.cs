using System;
using System.Management;
using EnvDTE;
using EnvDTE80;
using Process = System.Diagnostics.Process;

namespace Karpach.DebugAttachManager
{
    public class ProcessExt
    {            
        public ProcessExt(Process process)
        {
            ProcessName = process.ProcessName;            
            Title = GetAppPoolName(process);
            DefaultDebugMode = GetDebugMode(process);
        }


        public ProcessExt(string processName, string title)
        {
            Title = title;
            ProcessName = processName;
            DefaultDebugMode = string.Empty;
        }

        public string ProcessName { get; }

        public string Title { get; }

        public string DefaultDebugMode { get; }   

        public int Hash => string.Concat(ProcessName, Title).GetHashCode();

        private string GetDebugMode(Process process)
        {
            Processes localProcesses = (DebugAttachManagerPackage.DTE.Debugger as Debugger2).LocalProcesses;
            foreach (Process2 p in localProcesses)
            {
                if (p.ProcessID == process.Id)
                {
                    if (p.Transport.Engines.Count > 0)
                    {                        
                        return p.Transport.Engines.Item("Default").Name;
                    }                    
                    break;
                }
            }            
            return string.Empty;
        }

        private string GetAppPoolName(Process process)
        {
            if (process == null)
            {
                return string.Empty;
            }
            if (string.Equals(process.ProcessName, "w3wp", StringComparison.OrdinalIgnoreCase))
            {                            
                ObjectQuery sq = new ObjectQuery("Select CommandLine from Win32_Process Where ProcessID = '" + process.Id + "'");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(sq))
                {
                    ManagementObjectCollection objectCollection = searcher.Get();
                    foreach (ManagementObject oReturn in objectCollection)
                    {
                        var startIndex = oReturn["CommandLine"].ToString().IndexOf("-ap ") + 5; //remove the -ap as well as the space and the "
                        if (startIndex == -1)
                        {
                            return string.Empty;
                        }
                        var endIndex = oReturn["CommandLine"].ToString().IndexOf("-", startIndex) - 2; //remove the closing "                        
                        return oReturn["CommandLine"].ToString().Substring(startIndex, endIndex - startIndex);                    
                    }                
                }
            }
            if (string.Equals(process.ProcessName, "iisexpress", StringComparison.OrdinalIgnoreCase))
            {
                ObjectQuery sq = new ObjectQuery("Select CommandLine from Win32_Process Where ProcessID = '" + process.Id + "'");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(sq))
                {
                    ManagementObjectCollection objectCollection = searcher.Get();
                    foreach (ManagementObject oReturn in objectCollection)
                    {
                        var startIndex = oReturn["CommandLine"].ToString().IndexOf("/site:") + 7; //remove the /site: as well as the "
                        if (startIndex == -1)
                        {
                            return string.Empty;
                        }
                        var endIndex = oReturn["CommandLine"].ToString().IndexOf("\"", startIndex+7); //remove the closing "                                                
                        return oReturn["CommandLine"].ToString().Substring(startIndex, endIndex - startIndex);
                    }
                }
            }
            if (process.ProcessName.Contains("WebDev"))
            {
                ObjectQuery sq = new ObjectQuery("Select CommandLine from Win32_Process Where ProcessID = '" + process.Id + "'");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(sq))
                {
                    ManagementObjectCollection objectCollection = searcher.Get();
                    foreach (ManagementObject oReturn in objectCollection)
                    {
                        var startIndex = oReturn["CommandLine"].ToString().IndexOf("/port:") + 6; //remove the /site: as well as the "
                        if (startIndex == -1)
                        {
                            return string.Empty;
                        }
                        var endIndex = oReturn["CommandLine"].ToString().IndexOf(" ", startIndex); //remove the closing "                                                
                        return oReturn["CommandLine"].ToString().Substring(startIndex, endIndex - startIndex);
                    }
                }
            }
            return process.MainWindowTitle;
        }

    }
}