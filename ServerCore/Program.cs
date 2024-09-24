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
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                // 상호배제 Mutual Exclusive
                // : 정해진 영역 내부에서 사용하는 변수를
                //   다른 스레드에서 접근이 불가하게 함.
                // : 해당 영역에서는 사실상 싱글 스레드처럼 사용
                // C++ : std::mutex
                Monitor.Enter(_obj);
                { 
                    number++;
                    return;     // 잠금이 풀리기 전 return
                }
                Monitor.Exit(_obj);
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                Monitor.Enter(_obj);

                number--;

                Monitor.Exit(_obj);
            }
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
