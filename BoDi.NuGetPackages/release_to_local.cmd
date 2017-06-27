@pushd %~dp0

set VERSION=%1
set PKGVER=%1%2
set CONFIG="/p:Configuration=Release"

@echo local NuGet publish folder: %NUGET_LOCAL_FEED%
@echo publishing version %VERSION%, pkg version %PKGVER%, %CONFIG%, OK?
@pause

cd ..

copy /Y AssemblyVersion.cs AssemblyVersion.cs.bak
powershell -Command "(gc 'AssemblyVersion.cs') -replace '1.0.0-localdev', '%PKGVER%' | Out-File 'AssemblyVersion.cs'"
powershell -Command "(gc 'AssemblyVersion.cs') -replace '1.0.0.0', '%VERSION%.0' | Out-File 'AssemblyVersion.cs'"

msbuild BoDi.sln %CONFIG%

cd BoDi.NuGetPackages

msbuild BoDi.NuGetPackages.csproj "/p:NuGetVersion=%PKGVER%" /p:NugetPublishToLocalNugetFeed=true /t:Publish /p:NugetPublishLocalNugetFeedFolder=%NUGET_LOCAL_FEED%  %CONFIG%

cd ..
move /Y AssemblyVersion.cs.bak AssemblyVersion.cs

@popd