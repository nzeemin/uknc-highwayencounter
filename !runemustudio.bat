@echo off

del x-emustudio\System.dsk
@if exist "x-emustudio\System.dsk" (
  echo.
  echo ####### FAILED to delete old disk image file #######
  exit /b
)
copy x-emustudio\sys1002ex.dsk System.dsk
C:\bin\rt11dsk a System.dsk HWYENC.SAV
move System.dsk x-emustudio\System.dsk

@if not exist "x-emustudio\System.dsk" (
  echo ####### ERROR disk image file not found #######
  exit /b
)

start x-emustudio\EmuStudio.exe
