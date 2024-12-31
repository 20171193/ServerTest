using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static int _num = 0;

        // 뮤텍스 락

        // AutoResetEvent <-> Mutex
        // : Mutex는 다양한 정보를 담고있음.
        // : 1. 락이 잠긴 횟수를 int형으로 기록
        // : 2. 락의 소유자를 기록하여 예기치 못한 상황의 해제 방지
        //      ex) error : Thread_1(WaitOne), Thread_2(ReleaseMutex) 
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for(int i =0; i<100000; i++)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
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
