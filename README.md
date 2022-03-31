# ClickOnceApplicationDeployment

DLL designed to ease transition of ClickOnce applications from .NetFramwork to .Net by providing a unified library to both .NetFramework and .Net5 projects that utilize ClickOnce.

Provider has been designed to mimick System.Deployment.ApplicationDeployment class. Depending on the target framework, different methods will be used by the dll to provide the core ClickOnce functionality.

.Net compatibility provided by: https://github.com/derskythe/WpfSettings  
.NetFramework utilizes system.deployment.dll

Targets: Net472, Net48, .NetCoreApp3.1, and .Net5 

Available as a NuGet package here:
https://www.nuget.org/packages/ClickOnceApplicationDeploymentWrapper/

