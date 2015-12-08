@echo off
set Configuration=%1
if "%Configuration%"=="" set Configuration=Debug 

del *.nupkg
pushd ..
msbuild AngleSharp.Core.sln /t:Build /p:Configuration=%Configuration%
REM SourceLink.exe index -nvg -pr CommonAddin/CommonAddin.csproj -pp Configuration Release -u "https://zperforce.cloudapp.net/swarm/view/depot/excel/addins/TestAddin/%%var2%%"
REM SourceLink.exe index -nvg -pr AngleSharp/AngleSharp.Core.csproj -pp Configuration %Configuration% -u "http://zperforce.cloudapp.net/view/depot/excel/addins/TestAddin/%%var2%%"
popd
nuget pack -IncludeReferencedProjects -Prop Configuration=%Configuration%
copy *.nupkg c:\NuGet 