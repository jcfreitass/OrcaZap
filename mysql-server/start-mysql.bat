@echo off
cd /d "%~dp0"
start "OrcaZap MySQL" bin\mysqld.exe --defaults-file="%~dp0my.ini"
