using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Karpach.DebugAttachManager
{
    public class IISExpressRunner
    {
        private readonly string _commandLineArguments;
        private readonly string defaultIISExpressPath = @"C:\Program Files (x86)\IIS Express\iisexpress.exe";

        public IISExpressRunner(string commandLineArguments)
        {
            _commandLineArguments = commandLineArguments;
        }

        public Exception Exception { get; set; }

        public bool Run()
        {
            string iisPath = GetPath();
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = iisPath ?? defaultIISExpressPath,
                    Arguments = _commandLineArguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            try
            {
                return p.Start();
            }
            catch (Exception ex)
            {
                Exception = ex;
                return false;
            }            
        }

        public string GetPath()
        {            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\IISExpress");
            if (key == null)
            {
                return null;
            }
            string[] subKeyNames = key.GetSubKeyNames();
            if (subKeyNames.Length == 0)
            {
                return null;
            }
            key = key.OpenSubKey(subKeyNames[subKeyNames.Length - 1]);
            if (key == null)
            {
                return null;
            }
            return Path.Combine((string)key.GetValue("InstallPath"), "iisexpress.exe");
        }

    }
}