using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Caching;

namespace Karpach.DebugAttachManager
{
    public class ProcessExt
    {
        private static readonly MemoryCache Cache = MemoryCache.Default;
        public const string TitlePrefix = "~|~";        

        public ProcessExt(string processName, int processId, string serverName, long? portNumber)
        {
            if (processName.Contains("\\"))
            {
                ProcessName = processName.Substring(processName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                Title = GetTitle(ProcessName, processId, serverName);
                if (string.IsNullOrEmpty(Title))
                {
                    Title = processName;
                }
            }
            else
            {
                ProcessName = processName;
                Title = GetTitle(processName, processId, serverName);
            }
            ServerName = serverName;
            PortNumber = portNumber;
        }


        public ProcessExt(string processName, string title, string serverName, long? portNumber)
        {
            Title = title;
            ProcessName = processName;
            ServerName = serverName;
            PortNumber = portNumber;
        }

        public string ProcessName { get; }

        public string Title { get; }

        public string ServerName { get; set; }

        public long? PortNumber { get; set; }
        

        public int Hash => string.Concat(ProcessName, Title, ServerName, PortNumber).GetHashCode();

        private static IEnumerable<WmiProcess> GetWmiProcesses(string serverName = null)
        {
            string key = serverName ?? "localhost";
            if (Cache.Contains(key))
            {
                return Cache[key] as IEnumerable<WmiProcess>;
            }            

            ManagementScope scope = null;

            if (serverName != null)
            {
                ConnectionOptions options = new ConnectionOptions
                {
                    Impersonation = ImpersonationLevel.Impersonate,
                    EnablePrivileges = true,
                    Authentication = AuthenticationLevel.PacketPrivacy
                };

                scope = new ManagementScope($"\\\\{serverName}\\root\\cimv2", options);
                try
                {
                    scope.Connect();
                }
                catch
                {
                    Cache[key] = null;
                    return null;
                }
            }

            ObjectQuery sq = new ObjectQuery("Select CommandLine, ProcessID from Win32_Process");

            var result = new List<WmiProcess>();

            using (ManagementObjectSearcher searcher = serverName == null ? new ManagementObjectSearcher(sq) : new ManagementObjectSearcher(scope, sq))
            {
                ManagementObjectCollection objectCollection = searcher.Get();
                foreach (ManagementBaseObject obj in objectCollection)
                {
                    IQueryable<PropertyData> properties = obj.Properties.Cast<PropertyData>().AsQueryable();
                    PropertyData data = properties.FirstOrDefault(p => string.Equals(p.Name, "ProcessId", StringComparison.InvariantCultureIgnoreCase));                    
                    if (data == null)
                    {
                        continue;
                    }
                    result.Add(new WmiProcess
                    {
                      ProcessId  = (uint)data?.Value,
                      CommandLine = properties.FirstOrDefault(p => string.Equals(p.Name, "CommandLine"))?.Value?.ToString()
                    });                    
                }

            }
            Cache.Add(key, result, DateTime.Now.AddSeconds(5));
            return result;
        }        

        private static string GetTitle(string processName, int processId, string serverName = null)
        {            
            if (string.IsNullOrEmpty(processName))
            {
                return string.Empty;
            }
            WmiProcess process = GetWmiProcesses(serverName).FirstOrDefault(p => p.ProcessId == processId);
            if (process == null)
            {
                return String.Empty;
            }
            if (processName.StartsWith("w3wp", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(process.CommandLine))
            {
                int startIndex = process.CommandLine.IndexOf("-ap ", StringComparison.Ordinal) + 5; //remove the -ap as well as the space and the "
                if (startIndex == -1)
                {
                    return process.CommandLine;
                }
                int endIndex = process.CommandLine.IndexOf("-", startIndex, StringComparison.Ordinal) - 2; //remove the closing "                        
                return TitlePrefix + process.CommandLine.Substring(startIndex, endIndex - startIndex);
            }
            if (string.Equals(processName, "iisexpress.exe", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(process.CommandLine))
            {
                int startIndex = process.CommandLine.IndexOf("/site:", StringComparison.Ordinal) + 7; //remove the /site: as well as the "
                if (startIndex == -1)
                {
                    return string.Empty;
                }
                int endIndex = process.CommandLine.IndexOf("\"", startIndex + 7, StringComparison.Ordinal); //remove the closing "                                                
                return TitlePrefix + process.CommandLine.Substring(startIndex, endIndex - startIndex);
            }
            if (processName.Contains("WebDev") && !string.IsNullOrEmpty(process.CommandLine))
            {
                var startIndex = process.CommandLine.IndexOf("/port:", StringComparison.Ordinal) + 6; //remove the /site: as well as the "
                if (startIndex == -1)
                {
                    return string.Empty;
                }
                var endIndex = process.CommandLine.IndexOf(" ", startIndex, StringComparison.Ordinal); //remove the closing "                                                
                return TitlePrefix + process.CommandLine.Substring(startIndex, endIndex - startIndex);
            }
            return process.CommandLine;
        }        

        private class WmiProcess
        {
            public uint ProcessId { get; set; }            
            public string CommandLine { get; set; }
        }
    }
}