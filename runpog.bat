@echo off

set target="D:\gebruiker\Documents\My Web Sites\pog\"
set source="C:\Users\gebruiker\Source\Repos\POGApp\POGApp\bin\Debug\"

echo stop server if running
taskkill /F /FI "IMAGENAME eq POGApp.exe"

echo copy files
xcopy /s %source%*.* %target%\. /Y

echo start new instance
cd /d %target%
start POGApp.exe