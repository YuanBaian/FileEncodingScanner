@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
pushd "%SCRIPT_DIR%" >nul

set "DOTNET_CLI_HOME=%SCRIPT_DIR%.dotnet"
set "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
set "CONFIGURATION=Debug"
set "TARGET_FRAMEWORK=net8.0-windows"
set "PROJECT_NAME=FileEncodingScanner"

if not exist "%DOTNET_CLI_HOME%" (
    mkdir "%DOTNET_CLI_HOME%"
)

:parse_args
if "%~1"=="" goto build
if /I "%~1"=="-c" (
    set "CONFIGURATION=%~2"
    shift
    shift
    goto parse_args
)
if /I "%~1"=="--configuration" (
    set "CONFIGURATION=%~2"
    shift
    shift
    goto parse_args
)
set "ARG=%~1"
if /I "%ARG:~0,16%"=="--configuration=" (
    set "CONFIGURATION=%ARG:~16%"
)
shift
goto parse_args

:build
dotnet build "%PROJECT_NAME%.csproj" %*
set "EXIT_CODE=%ERRORLEVEL%"

:copy_output
if not "%EXIT_CODE%"=="0" goto end

set "BUILD_OUTPUT_DIR=%SCRIPT_DIR%bin\%CONFIGURATION%\%TARGET_FRAMEWORK%"

for %%F in (
    "%PROJECT_NAME%.exe"
    "%PROJECT_NAME%.dll"
    "%PROJECT_NAME%.deps.json"
    "%PROJECT_NAME%.runtimeconfig.json"
    "%PROJECT_NAME%.pdb"
) do (
    if exist "%BUILD_OUTPUT_DIR%\%%~F" (
        copy /Y "%BUILD_OUTPUT_DIR%\%%~F" "%SCRIPT_DIR%" >nul
    )
)

:end
popd >nul
exit /b %EXIT_CODE%
