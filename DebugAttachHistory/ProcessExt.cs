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

        public ProcessExt(string processName, int processId, string serverName, long? portNumber, string userName, string password, bool useWmi)
        {
	        UseWmi = useWmi;
            if (processName.Contains("\\"))
            {
                ProcessName = processName.Substring(processName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                Title = GetTitle(ProcessName, processId, serverName, userName, password, useWmi);
                if (string.IsNullOrEmpty(Title))
                {
                    Title = processName;
                }
            }
            else
            {
                ProcessName = processName;
                Title = GetTitle(processName, processId, serverName, userName, password, useWmi);
            }
            ServerName = serverName;
            PortNumber = portNumber;
			ProcessId = processId;
            UserName = userName;
            Password = password;
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

        public string UserName { get; set; }

        public string Password { get; set; }
        
        public bool UseWmi { get; private set; }

        public string CommandLine
		{
			get
			{
				string result = string.Empty;
				if (!string.IsNullOrEmpty(Title))
				{
					if (!Title.StartsWith(TitlePrefix))
					{
						return Title;
					}
				}
				return result;
			}
		}

		public int? ProcessId { get; set; }

        public int Hash => string.Concat(ProcessName, Title, ServerName, PortNumber).GetHashCode();

        private static IEnumerable<WmiProcess> GetWmiProcesses(string serverName, string userName, string password)
        {
            string key = serverName ?? "localhost";
            if (Cache.Contains(key))
            {
                return Cache[key] as IEnumerable<WmiProcess>;
            }            

            ManagementScope scope = null;

            if (serverName != null)
            {
                ConnectionOptions options;
                if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
                {
                    options = new ConnectionOptions
                    {
                        Impersonation = ImpersonationLevel.Impersonate,
                        EnablePrivileges = true,
                        Authentication = AuthenticationLevel.PacketPrivacy
                    };
                }
                else
                {
                    options = new ConnectionOptions
                    {
                        Impersonation = ImpersonationLevel.Identify,
                        EnablePrivileges = true,
                        Username = userName,
                        Password = password,
                        Authentication = AuthenticationLevel.PacketPrivacy
                    };
                }

                scope = new ManagementScope($"\\\\{serverName}\\root\\cimv2", options);
                try
                {
                    scope.Connect();
                }
                catch
                {
	                if (Cache.Contains(key))
	                {
		                Cache[key] = null;
                    }
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

        private static string GetTitle(string processName, int processId, string serverName, string userName, string password, bool useWmi)
        {            
            if (string.IsNullOrEmpty(processName) || !useWmi)
            {
                return string.Empty;
            }
            WmiProcess process = GetWmiProcesses(serverName, userName, password)?.FirstOrDefault(p => p.ProcessId == processId);
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
                int endIndex = process.CommandLine.IndexOf("\" -", startIndex, StringComparison.Ordinal); //remove the closing "                        
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