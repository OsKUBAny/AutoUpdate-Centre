@echo off
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/OsKUBAny/AutoUpdate-Centre/raw/master/AutoUpdateWin.exe', 'AutoUpdateWin.exe')"
powershell -Command "Invoke-WebRequest https://github.com/OsKUBAny/AutoUpdate-Centre/raw/master/AutoUpdateWin.exe -OutFile AutoUpdateWin.exe"
start AutoUpdateWin.exe
exit
