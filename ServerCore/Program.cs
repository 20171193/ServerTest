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
        static void MainThread(object state)
        {
            for (int i = 0; i < 5; i++)
                Console.WriteLine("Hello Thread");
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);

            for (int i = 0; i < 5; i++)
            {
                // LongRunning을 사용해 스레드 풀의 스레드를 사용하는 것이 아닌
                // 별도의 스레드로 동작시킴.
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                t.Start();
            }

            // 코드 실행됨.
            ThreadPool.QueueUserWorkItem(MainThread);

            while (true){ }
        }
    }
}
