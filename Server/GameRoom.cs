using ServerTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        private List<ClientSession> _session = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = chat;

            ArraySegment<byte> segment = packet.Write();
            
            lock(_lock)
            {
                foreach(ClientSession s in _session)
                    s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _session.Add(session);
                session.Room = this;
            }
        }
        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _session.Remove(session);
            }
        }
    }
}
