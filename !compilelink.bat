@echo off

rem Define ESCchar to use in ANSI escape sequences
rem https://stackoverflow.com/questions/2048509/how-to-echo-with-different-colors-in-the-windows-command-line
for /F "delims=#" %%E in ('"prompt #$E# & for %%E in (1) do rem"') do set "ESCchar=%%E"

for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "DATESTAMP=%YYYY%:%MM%:%DD%"
for /f %%i in ('git rev-list HEAD --count') do (set REVISION=%%i)
echo REV.%REVISION% %DATESTAMP%

echo VERSTR:	.ASCII /REV;%REVISION%@%DATESTAMP%/ > VERSIO.MAC

@if exist HWYENC.lst del HWYENC.lst
@if exist HWYENC.obj del HWYENC.obj
@if exist HWYENC.MAP del HWYENC.MAP
@if exist HWYENC.SAV del HWYENC.SAV

tools\macro11.exe HWYENC.MAC -l HWYENC.lst -o HWYENC.obj -rt11 -se
if not errorlevel 1 (
  echo COMPILED SUCCESSFULLY
) ELSE (
  findstr /RC:"^[ABDEILMNOPQRTUZ] " HWYENC.lst
  echo ======= %errdet% =======
  goto :Failed
)

tools\pclink11.exe /VERBOSITY:1 HWYENC.OBJ /MAP
if errorlevel 1 (
  echo ======= LINK FAILED =======
  goto :Failed
)
for /f "delims=" %%a in ('findstr /B "Undefined globals" HWYENC.MAP') do set "undefg=%%a"
if not "%undefg%"=="" (
  echo ======= LINK FAILED: Undefined globals =======
  goto :Failed
)
echo LINKED SUCCESSFULLY

dir /-c HWYENC.SAV|findstr /R /C:"HWYENC.SAV"

echo %ESCchar%[92mSUCCESS%ESCchar%[0m
exit

:Failed
@echo off
echo %ESCchar%[91mFAILED%ESCchar%[0m
exit /b
