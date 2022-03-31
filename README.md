# ClickOnceApplicationDeployment

DLL designed to ease transition of ClickOnce applications from .NetFramwork to .Net by providing a unified library to both .NetFramework and .Net5 projects that utilize ClickOnce.
Provider has been designed to mimick System.Deployment.ApplicationDeployment class. Depending on the target framework, different methods will be used by the dll to provide the core ClickOnce functionality.
Targets: Net472, Net48, .NetCoreApp3.1, and .Net5-windows 

.Net compatibility provided by: https://github.com/derskythe/WpfSettings  
.NetFramework utilizes system.deployment.dll

Available as a NuGet package here:
https://www.nuget.org/packages/ClickOnceApplicationDeploymentWrapper/

# Usage
NameSpace: RFBApplicationDeployment  

RFBApplicationDeployment.ClickOnceApplicationDeployment.EntryApplication
- This static object will act as your main interaction point. 
- If needed, other objects can be constructed that have different publish paths.
- To set the publish path (where the app will get its updates from) in .Net applications, use then SetupEntryApplication(string) method.
  - This will Recreate the static object using the new string as the update path.

The underlying providers are still accessible if needed, but the ClickOnceApplicationDeployment object wraps their methods and properties in a way to make the code-base simpler, eliminating `#if {TARGET}` statements from the caller's code. 

This also adds in the ability to use cancellation tokens with the System.Deployment.Dll methods.



