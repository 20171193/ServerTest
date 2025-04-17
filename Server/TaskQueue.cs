using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    interface ITask
    {
        void Execute();
    }

    // 직접 각 Task를 클래스화 하는 방식
    class BroadcastTask : ITask
    {
        private GameRoom _room;
        private ClientSession _session;
        string _chat;

        BroadcastTask(GameRoom room, ClientSession session, string chat)
        {
            _room = room;
            _session = session;
            _chat = chat;
        }

        public void Execute() 
        {
            _room.Broadcast(_session, _chat);
        }   
    }

    class TaskQueue
    {
        private Queue<ITask> _queue = new Queue<ITask>();
        
    }
}
