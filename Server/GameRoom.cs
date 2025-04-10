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

        // 실제 게임에서 브로드캐스팅 공간을 어떻게 설정할지?
        // 과부하를 방지하는 선에서 정하는 것이 매우 어려움.
        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();
            
            // 병목현상이 발생할 여지가 있음.
            // 스레드가 맹목적으로 대기하는 것이 아닌 *큐*를 사용하여 일감을 관리
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
