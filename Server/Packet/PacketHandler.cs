using ServerCore;
using Server;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        // **기존 방식
        //clientSession.Room.Broadcast(clientSession, chatPacket.chat);

        // **Job Queue 방식
        //  : 실행 시점이 뒤로 밀렸기에 null 참조가 될 수 있음.
        GameRoom room = clientSession.Room;

        room.Push(
            () => room.Broadcast(clientSession, chatPacket.chat)
            );
    }
}
