using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Lock
    {
        // bool <- 커널
        ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            // 따로 실행한다면 문제가 발생. (멀티 스레드 환경)
            //_available.WaitOne();   // 입장 시도 
            //_available.Reset();     //  -> false로 차단 

            _available.WaitOne();   // 입장 시도 
        }
        public void Release()
        {
            _available.Set();   // 입장을 다시 허용
        }
    }

    class Program
    {
        static int _num = 0;
        static Lock _lock = new Lock();

        static void Thread_1()
        {
            for(int i =0; i<100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(_num);
        }
    }
}
