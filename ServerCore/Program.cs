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
        // Monitor
        static object _lcok = new object();

        // SpinLock
        static SpinLock _lock2 = new SpinLock();

        // Mutex
        static Mutex _lock3 = new Mutex();

        static void Main(string[] args)
        {
            bool lockTaken = false;
            try
            {
                _lock2.Enter(ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                    _lock2.Exit();
            }
        }
    }
}
