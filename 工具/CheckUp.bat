

@ECHO OFF
cd /d %~dp0
start  %1 SimpleUpdater.exe /startupdate /cv "1.0" /url "http://192.168.1.100:8091/ERPInit/{0}" /infofile "update_c.xml" /autokill /forceupdate /p "UP4.exe

exit