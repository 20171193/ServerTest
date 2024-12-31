using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            // 스핀락을 통한 반복적 확인
            while(true)
            {
                int expected = 0;
                int desired = 1;

                // _locked 값이 expected 값이라면
                // desired에 할당
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;  // 락 획득 시 break

                // 실패 시 ? 
                Thread.Sleep(1);    // 무조건 휴식 ==> 무조건 1ms 쉼
                Thread.Sleep(0);    // 조건부 양보 ==> 우선순위가 같거나 높은 스레드를 확인
                Thread.Yield();     // 관대한 양보 ==> 실행이 가능한 스레드가 있다면 무조건 양보
            }
        }
        public void Release()
        {
            _locked = 0;
        }
    }

    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

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
