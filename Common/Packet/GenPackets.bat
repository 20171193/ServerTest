@echo off
ECHO ���� ���: %CD%

START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml

XCOPY /Y /I GenPacket.cs "../../DummyClient/Packet"
XCOPY /Y /I GenPacket.cs "../../Client/Assets/01.Scripts/Packet"
IF %ERRORLEVEL% NEQ 0 ECHO XCOPY GenPacket.cs ����!

XCOPY /Y /I GenPacket.cs "../../Server/Packet"
IF %ERRORLEVEL% NEQ 0 ECHO XCOPY GenPacket.cs ����!

XCOPY /Y /I ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y /I ClientPacketManager.cs "../../Client/Assets/01.Scripts/Packet"
IF %ERRORLEVEL% NEQ 0 ECHO XCOPY PacketManager.cs ����!

XCOPY /Y /I ServerPacketManager.cs "../../Server/Packet"
IF %ERRORLEVEL% NEQ 0 ECHO XCOPY PacketManager.cs ����!

PAUSE
