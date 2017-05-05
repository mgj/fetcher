del *.nupkg
nuget setapikey 624f71d2-0d25-4c7f-8221-5bfa162cd79d

nuget pack artm.MvxPlugins.Fetcher.nuspec -symbols
nuget pack artm.MvxPlugins.Logger.nuspec -symbols


for /r %%i in (*.nupkg) do (call :pushpackage "%%i")
pause

:pushpackage
  set np=%1
  if "%np%"=="%np:symbols=%" (
	nuget push %np% 
  )