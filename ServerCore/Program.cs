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
        // 스레드 지역
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My name is {Thread.CurrentThread.ManagedThreadId}";});

        // 전역
        static string ThreadName2;

        static void WhoAmI()
        {
            /*********** 전역 ************
             *****************************/
            //ThreadName2 = $"My name is {Thread.CurrentThread.ManagedThreadId}";
            //Thread.Sleep(1000);
            //Console.WriteLine(ThreadName2);


            /********* 스레드 지역 *******
             * 각 스레드가 가지는 TLS 영역
             *****************************/
            bool repeat = ThreadName.IsValueCreated;
            if(repeat)
                Console.WriteLine($"{ThreadName.Value} is repeat.");
            else
                Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            // 각 태스크를 부여하여 실행
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose();
        }
    }
}
