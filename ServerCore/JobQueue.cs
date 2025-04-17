using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    // Job을 Queue로 관리하는 방식
    //  : 이 Job들을 처리할 객체가 필요.
    //  : Session의 SendQueue와 유사
    public class JobQueue : IJobQueue  
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            bool flush = false;

            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                    flush = _flush = true;
            }

            if (flush)
                Flush();
        }

        // 주문처리
        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock(_lock) 
            {
                if(_jobQueue.Count == 0 )
                {
                    _flush = false;
                    return null;
                }
                else
                {
                    return _jobQueue.Dequeue();
                }
            }
        }
    }
}
