

@ECHO OFF
cd /d %~dp0
start  %1 SimpleUpdater.exe /startupdate /cv "1.0" /url "http://www.jizhuhotel.com/download/tool/{0}" /infofile "update_c.xml" /autokill /forceupdate /p "JT100.Wish.Client.exe

exit