START /WAIT ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y /I GenPacket.cs "../../DummyClient/Packet"
XCOPY /Y /I GenPacket.cs "../../Server/Packet"
XCOPY /Y /I PacketManager.cs "../../DummyClient/Packet"
XCOPY /Y /I PacketManager.cs "../../Server/Packet"