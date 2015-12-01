using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(DebugOptionsWindow))]
    [Guid(GuidList.guidDebugAttachManagerPkgString)]
    public sealed class DebugAttachManagerPackage : Package
    {
        public static EnvDTE80.DTE2 DTE;
        public static IServiceProvider ServiceProvider;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DebugAttachManagerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(DebugOptionsWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void AttachSmartDebugCommand(object sender, EventArgs e)
        {
            ToolWindowPane window = this.FindToolWindow(typeof(DebugOptionsWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var attachWindow = window as DebugOptionsWindow;
            if (attachWindow != null)
            {
                if (!attachWindow.AttachToProcesses())
                {
                    var windowFrame = attachWindow.Frame as IVsWindowFrame;
                    if (windowFrame != null)
                    {
                        windowFrame.Show();
                    }
                }                
            }
        }

        private void SmartRun(object sender, EventArgs e)
        {
            bool success = false;
            for (int i = 1; i <= DTE.Solution.Projects.Count; i++)
            {
                Project project = DTE.Solution.Projects.Item(i);
                                                
                if (project.Properties == null)
                {
                    continue;
                }

                List<string> developmentServerCommandLines = GetDevelopmentCommandLine(project);
                foreach (string developmentServerCommandLine in developmentServerCommandLines)
                {
                    if (!string.IsNullOrEmpty(developmentServerCommandLine))
                    {
                        var iisRunner = new IISExpressRunner(developmentServerCommandLine);
                        if (!iisRunner.Run())
                        {
                            MessageBox.Show("Unable to start IISExpress", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        else
                        {
                            success = true;
                        }
                    }
                }                
            }
            if (!success)
            {
                DTE.ExecuteCommand("Debug.StartWithoutDebugging");
            }    
        }

        private static List<string> GetDevelopmentCommandLine(Project project)
        {
            bool? useIISExpress = null;
            string developmentServerCommandLine = string.Empty;
            foreach (Property prop in project.Properties)
            {
                try
                {
                    if (string.Equals(prop.Name, "Extender"))
                    {
                        List<string> developmentServerCommandLines = new List<string>();
                        for (int j = 1; j <= project.ProjectItems.Count; j++)
                        {
                            ProjectItem proj = project.ProjectItems.Item(j);
                            if (proj.SubProject != null)
                            {
                                List<string> developmentCommandLines = GetDevelopmentCommandLine(proj.SubProject);
                                if (developmentCommandLines.Any())
                                {
                                    developmentServerCommandLines.AddRange(developmentCommandLines);
                                }
                            }
                        }
                        return developmentServerCommandLines;
                    }

                    if (string.Equals(prop.Name, "WebApplication.UseIISExpress"))
                    {
                        if (!useIISExpress.HasValue || !useIISExpress.Value)
                        {
                            useIISExpress = (bool) prop.Value;
                        }
                    }
                    else if (string.Equals(prop.Name, "WebApplication.UseIIS"))
                    {
                        if (!useIISExpress.HasValue || !useIISExpress.Value)
                        {
                            useIISExpress = !(bool) prop.Value;
                        }
                    }
                    else if (string.Equals(prop.Name, "WebApplication.DevelopmentServerCommandLine"))
                    {
                        developmentServerCommandLine = (string) prop.Value;
                    }
                }
                catch (Exception)
                {
                }
            }
            return useIISExpress.GetValueOrDefault(false) ? new List<string>(new [] { developmentServerCommandLine }) : new List<string>();
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidDebugAttachManagerCmdSet, (int)PkgCmdIDList.cmdidSmartDebugAttacher);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
                toolwndCommandID = new CommandID(GuidList.guidDebugAttachManagerCmdSet, (int)PkgCmdIDList.cmdidAttachSmartDebug);
                menuToolWin = new MenuCommand(AttachSmartDebugCommand, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
                toolwndCommandID = new CommandID(GuidList.guidDebugAttachManagerCmdSet, (int)PkgCmdIDList.cmdidSmartRun);
                menuToolWin = new MenuCommand(SmartRun, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
            DTE = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
            ServiceProvider = this;
        }
        #endregion

    }
}
