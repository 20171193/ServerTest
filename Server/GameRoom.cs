using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        private List<ClientSession> _session = new List<ClientSession>();
        // GameRoom에서 Job Queue를 사용하는 순간부터 lock을 사용할 필요는 없어짐.
        // : JobQueue에서 일감을 처리할 때 자체적인 lock을 사용하기 때문
        private JobQueue _jobQueue = new JobQueue();

        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _session)
                s.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        // 실제 게임에서 브로드캐스팅 공간을 어떻게 설정할지?
        // 과부하를 방지하는 선에서 정하는 것이 매우 어려움.
        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);

            // 병목현상이 발생할 여지가 있음.
            // 스레드가 맹목적으로 대기하는 것이 아닌 *큐*를 사용하여 일감을 관리

            // N^2 -> N
            // - 각 Send도 락을 걸고 SendQueue에 할당
            // - 요청마다 패킷을 보내기보다 뭉쳐서 한번에 보내는 것이 효율적
            //foreach (ClientSession s in _session)
            //    s.Send(segment);
        }
        public void Enter(ClientSession session)
        {
            _session.Add(session);
            session.Room = this;
        }
        public void Leave(ClientSession session)
        {
            _session.Remove(session);
        }
    }
}
