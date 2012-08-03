using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace comScoreInc.DebugAttachManager
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("422cb21c-c839-43b4-98fe-5c2b1ef87fc8")]
    public class DebugOptionsWindow : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public DebugOptionsWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.            
            base.Content = new DebugOptionsControl();
        }

        public void AttachToProcesses()
        {
            DebugOptionsControl control = base.Content as DebugOptionsControl;
            if (control != null)
            {
                control.AttachToProcesses();
            }
        }
    }
}
