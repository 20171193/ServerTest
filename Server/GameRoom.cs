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

            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가
            _session.Add(session);
            session.Room = this;

            // 신규 플레이어에게 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach(ClientSession s in _session)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write());

            // 신규 플레이어 입장을 브로드캐스팅
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }
        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _session.Remove(session);

            // 브로드캐스팅
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 변경
            session.PosX = packet.posX;
            session.PosY = packet.posY; 
            session.PosZ = packet.posZ;

            // 브로드캐스팅
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
        }
    }
}
