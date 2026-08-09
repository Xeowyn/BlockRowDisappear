@echo off
echo Rebuilding Block Row Disappear...
wsl -e bash -lc "export PATH=$HOME/.dotnet:$PATH && cd /mnt/c/projects/BlockRowDisappear/source && dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableWindowsTargeting=true -o /mnt/c/projects/BlockRowDisappear"
echo.
echo Done. BlockRowDisappear.exe has been updated.
pause
