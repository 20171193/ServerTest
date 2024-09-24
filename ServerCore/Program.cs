using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // Atomic - 원자성
    //  : ex) 골드를 소모해 물건을 구매하는 경우
    // 골드 소모
    //  --- 서버 다운 ---
    // 물건 구매 -> 실행되지 않음.

    class Program
    {
        static volatile int number = 0;
        static void Thread_1()
        {
            // 원자적 덧셈 : All or Nothing
            for (int i = 0; i < 10000; i++)
            {
                // 변경된 값을 받고싶은 경우 
                int afterValue = Interlocked.Increment(ref number);
            }
        }

        static void Thread_2()
        {
            // 원자적 뺄셈 : All or Nothing
            for (int i = 0; i < 10000; i++)
                Interlocked.Decrement(ref number);
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
