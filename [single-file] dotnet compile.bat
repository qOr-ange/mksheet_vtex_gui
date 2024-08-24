@echo off
setlocal enabledelayedexpansion 

for /f "tokens=*" %%a in ('dir /b *.csproj') do (
  set "projectName=%%~na"
  goto :continue
)
:continue

if not exist "%cd%\bin" mkdir "%cd%\bin"
if exist "%cd%\bin\%projectName%.exe" (
    set "counter=0"
    :check
    if exist "%cd%\bin\%projectName%_!counter!.exe" (
        set /a counter+=1
        goto :check
    )
    ren "%cd%\bin\%projectName%.exe" "%projectName%_!counter!.exe"
)

dotnet publish -r win-x64 -p:PublishSingleFile=True --self-contained false --output "%cd%\bin"
:: del %cd%\bin\%projectName%.pdb
pause
endlocal
