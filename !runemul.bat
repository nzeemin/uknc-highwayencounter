@echo off

if exist x-ukncbtl\HWYENC.BIN del x-ukncbtl\HWYENC.BIN
rem E:\Work\MyProjects\ukncbtl-utils\Sav2Cartridge\Release\Sav2Cart.exe HWYENC.SAV HWYENC.BIN
rem move HWYENC.BIN x-ukncbtl\HWYENC.BIN

del x-ukncbtl\sys1002.dsk
@if exist "x-ukncbtl\sys1002.dsk" (
  echo.
  echo ####### FAILED to delete old disk image file #######
  exit /b
)
copy E:\Work\MyProjects-old\svn-ukncbtl\lib\disks\sys1002.dsk .
C:\bin\rt11dsk a sys1002.dsk HWYENC.SAV
move sys1002.dsk x-ukncbtl\sys1002.dsk

@if not exist "x-ukncbtl\sys1002.dsk" (
  echo ####### ERROR disk image file not found #######
  exit /b
)

start x-ukncbtl\UKNCBTL.exe /boot
