# Debug Attach Manager
This extension gives you ability to search for processes and then attach to them. It automatically saves selected processes between Visual Studio restarts. You can easily find apppool specific processes and attach to them. Extension has attach shortcut key, by default it is Ctrl+Shift+Alt+F5. You can attach to multiple processes at once. If you are web developer then you must have it.

![Screenshot1](Screenshots/screenshot1.png)

![Screenshot2](Screenshots/screenshot2.png)

The extension has non standard themes support (e.g. Dark Theme):

![Screenshot3](Screenshots/screenshot3.png)

![Screenshot4](Screenshots/screenshot4.png)

 The extension supports Visual Studio Development (Cassini) server and IISExpress. For the Visual Studio Development server it shows port number in parenthesis. For the IISExpress it shows site name in parenthesis.

 The extension uses WMI to get IIS details information. You can use WBEMTest.exe to verify that you have appropriate permissions. 
 
 Here are a few tips for WMI access:
 * Try to run Visual Studio in Admisistrator mode.
 * Try to disable UAC
 * If disabling UAC helps then you can try to add the following to your registry:
 
 HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System
 DWORD(32 bit): LocalAccountTokenFilterPolicy = 1

![Screenshot5](Screenshots/screenshot5.png)

The extension adds two buttons to the standard toolbar panel:
* First button "Smart Start" starts application without debugging. For web applications that use IISExpress the button doesn't open new page in opose to built-in Visual Studio "Start Without Debugging" command.
* The second button "Attach Smart Debug Selections" attaches selected processes.

![Screenshot6](Screenshots/screenshot6.png)

The extension also supports basic remote debugging:

![Screenshot7](Screenshots/screenshot7.png)

The logged in user needs to have access to the remote PC. 

[![Build status](https://ci.appveyor.com/api/projects/status/9mw67f51ocxiychg?svg=true)](https://ci.appveyor.com/project/karpach/debug-attach-manager)
